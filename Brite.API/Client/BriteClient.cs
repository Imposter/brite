/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Brite.Utility;
using Brite.Utility.IO;
using Brite.Utility.Network;

namespace Brite.API.Client
{
    public class BriteClient
    {
        public const int DefaultConnectionTimeout = 5000;

        private readonly ITcpClient _client;
        private readonly string _id;
        private bool _connected;
        private BinaryStream _stream;
        private readonly Mutex _streamLock;
        private int _seqId;
        private readonly Mutex _seqMutex;
        private readonly List<BriteDevice> _devices;
        private readonly Dictionary<int, Message> _messages;

        public string Id => _id;
        public BriteDevice[] Devices => _devices.ToArray();

        public BriteClient(ITcpClient client, string id)
        {
            _client = client;
            _id = id;
            _streamLock = new Mutex();
            _seqMutex = new Mutex();
            _devices = new List<BriteDevice>();
            _messages = new Dictionary<int, Message>();
        }

        public async Task ConnectAsync(int timeout = DefaultConnectionTimeout)
        {
            // Set timeout for connection
            _client.Timeout = timeout;

            await _client.ConnectAsync();

            // Set stream
            _stream = new BinaryStream(_client.GetStream());

            // Set infinite timeout
            _client.Timeout = -1;

            // Reset message sequence id
            _seqId = 0;

            // Set as connected
            _connected = true;

            // Run handler task
#pragma warning disable 4014
            Task.Run(HandleMessages);
#pragma warning restore 4014

            try
            {
                // Identify
                {
                    var request = new Message(Command.SetId);
                    await request.Stream.WriteInt32Async(_id.Length);
                    await request.Stream.WriteStringAsync(_id);

                    await SendMessageAsync(request);
                }

                // Get devices
                {
                    var request = new Message(Command.GetDevices);
                    var response = await SendMessageAsync(request);

                    var deviceCount = await response.Stream.ReadUInt8Async();
                    for (var i = 0; i < deviceCount; i++)
                    {
                        var deviceId = await response.Stream.ReadUInt32Async();
                        var device = new BriteDevice(this, deviceId);
                        _devices.Add(device);
                    }
                }

                // Initialize devices
                foreach (var device in _devices)
                    await device.InitializeAsync();
            }
            catch
            {
                // We failed to obtain information, disconnect
                await DisconnectAsync();

                // Rethrow the exception
                throw;
            }
        }

        public async Task DisconnectAsync()
        {
            // Set as disconnected
            _connected = false;

            // Clear devices
            _devices.Clear();

            // Reject any messages that are being waited on
            foreach (var pair in _messages)
                pair.Value.SetResponse(Result.Error, null);

            // Clear messages
            _messages.Clear();

            // Disconnect client
            await _client.DisconnectAsync();

            // Deference stream
            _stream = null;
        }

        internal async Task<Message> SendMessageAsync(Message message, Result expectedResult = Result.Ok)
        {
            // Get a new ID for the message
            await _seqMutex.LockAsync();
            var id = _seqId++;
            _seqMutex.Unlock();

            // Store message
            _messages.Add(id, message);

            // Get data
            var data = ((MemoryStream)message.Stream.Stream).ToArray();

            // Send message
            try
            {
                await _streamLock.LockAsync();
                await _stream.WriteUInt8Async((byte)message.Command);
                await _stream.WriteInt32Async(id);
                await _stream.WriteInt32Async(data.Length);
                await _stream.WriteBlobAsync(data);

                var response = await message.GetResponse();
                if (response.Result != expectedResult)
                    throw new BriteException($"Unexpected result, expected {expectedResult} got {response.Result}");

                return response;
            }
            finally
            {
                _streamLock.Unlock();
            }
        }
        
        private async Task HandleMessages()
        {
            while (_connected)
            {
                // Read command
                var command = await _stream.ReadUInt8Async();
                var id = await _stream.ReadInt32Async();
                var result = await _stream.ReadUInt8Async();
                var length = await _stream.ReadInt32Async();
                var data = await _stream.ReadBlobAsync(length);

                // Find message corresponding to id
                if (id != -1)
                {
                    if (!_messages.ContainsKey(id))
                        throw new BriteException("Received response for unidentified message");

                    // Set message response
                    var message = _messages[id];
                    message.SetResponse((Result)result, new BinaryStream(new MemoryStream(data)));

                    _messages.Remove(id);
                }
            }
        }
    }
}
