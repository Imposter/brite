﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Brite.API.Client;
using Brite.Utility;
using Brite.Utility.IO;
using Brite.Utility.Network;

namespace Brite.API
{
    public class BriteClient
    {
        private readonly ITcpClient _client;
        private BinaryStream _stream;
        private readonly string _id;
        private readonly Mutex _streamLock;
        private readonly List<BriteDevice> _devices;

        public BriteClient(ITcpClient client, string id)
        {
            _client = client;
            _id = id;
            _streamLock = new Mutex();
            _devices = new List<BriteDevice>();
        }

        public async Task ConnectAsync()
        {
            await _client.ConnectAsync();

            _stream = new BinaryStream(_client.GetStream());

            try
            {
                // Lock mutex
                await _streamLock.LockAsync();

                // Identify
                {
                    await SendCommandAsync(Command.SetId);
                    await _stream.WriteInt32Async(_id.Length);
                    await _stream.WriteStringAsync(_id);

                    await ReceiveResultAsync();
                }

                // Get devices
                {
                    await SendCommandAsync(Command.GetDevices);
                    await ReceiveResultAsync();

                    var deviceCount = await _stream.ReadUInt8Async();
                    for (var i = 0; i < deviceCount; i++)
                    {
                        var deviceId = await _stream.ReadUInt32Async();
                        var device = new BriteDevice(_stream, _streamLock, deviceId);
                        _devices.Add(device);
                    }
                }

                // Initialize devices
                foreach (var device in _devices)
                    await device.InitializeAsync();
            }
            finally
            {
                // Unlock mutex
                _streamLock.Unlock();
            }
        }

        public async Task DisconnectAsync()
        {
            // Clear devices
            _devices.Clear();

            // Disconnect client
            await _client.DisconnectAsync();

            // Deference stream
            _stream = null;
        }

        private async Task SendCommandAsync(Command command)
        {
            await _stream.WriteUInt8Async((byte)command);
            var responseCommand = await _stream.ReadUInt8Async();
            if (responseCommand != (byte)command)
                throw new BriteApiException($"Unexpected command response, expected {command} got {(Command)responseCommand}");
        }

        private async Task ReceiveResultAsync(Result expected = Result.Ok)
        {
            var result = await _stream.ReadUInt8Async();
            if (result != (byte)expected)
                throw new BriteApiException($"Unexpected result, expected {expected} got {(Result)result}");
        }
    }
}
