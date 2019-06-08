/*
 * Copyright (C) 2017 Eyaz Rehman. All Rights Reserved.
 *
 * This file is part of Brite.
 * Licensed under the GNU General Public License. See LICENSE file in the project
 * root for full license information.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Brite.API.Client;
using Brite.Utility;
using Brite.Utility.Crypto;
using Brite.Utility.IO;

namespace Brite.API.Animations.Client
{
    public abstract class BaseAnimation
    {
        private BriteClient _client;
        private uint _deviceId;
        private byte _channel;
        private byte _maxColors;
        private float _minSpeed;
        private float _maxSpeed;
        private readonly List<Color> _colors;
        private byte _colorCount;
        private float _speed;
        private bool _enabled;

        public byte Channel => _channel;
        public byte MaxColors => _maxColors;
        public float MinSpeed => _minSpeed;
        public float MaxSpeed => _maxSpeed;
        public Color[] Colors => _colors.ToArray();
        public byte ColorCount => _colorCount;
        public float Speed => _speed;
        public bool Enabled => _enabled;

        public abstract string GetName();

        public BaseAnimation()
        {
            _colors = new List<Color>();
        }

        internal void Initialize(BriteClient client, bool reset, uint deviceId, byte channel, byte maxColors, float minSpeed, float maxSpeed)
        {
            if (deviceId != _deviceId || channel != _channel)
                reset = true;

            _client = client;
            _deviceId = deviceId;
            _channel = channel;
            _maxColors = maxColors;
            _minSpeed = minSpeed;
            _maxSpeed = maxSpeed;

            if (!reset)
                return;

            // Clamp color count
            _colorCount = _colorCount.Clamp(byte.MinValue, maxColors);

            // Clamp speed between min and max
            _speed = _speed.Clamp(minSpeed, maxSpeed);

            _colors.Clear();
            _colors.AddRange(Enumerable.Repeat(Color.Black, maxColors));
        }

        internal void Reset()
        {
            _client = null;
        }

        public uint GetId()
        {
            return Hash.Fnv1A32(GetName());
        }

        public virtual async Task SetColorCountAsync(byte colorCount)
        {
            if (_client == null)
                throw new InvalidOperationException("Animation channel not set");

            var request = new Message(Command.DeviceSetChannelAnimationColorCount);
            await request.Stream.WriteUInt32Async(_deviceId);
            await request.Stream.WriteUInt8Async(_channel);
            await request.Stream.WriteUInt8Async(colorCount);

            await _client.SendMessageAsync(request);

            _colorCount = colorCount;
        }

        public virtual async Task SetColorAsync(byte index, Color color)
        {
            if (_client == null)
                throw new InvalidOperationException("Animation channel not set");

            var request = new Message(Command.DeviceSetChannelAnimationColor);
            await request.Stream.WriteUInt32Async(_deviceId);
            await request.Stream.WriteUInt8Async(_channel);
            await request.Stream.WriteUInt8Async(index);
            await request.Stream.WriteUInt8Async(color.R);
            await request.Stream.WriteUInt8Async(color.G);
            await request.Stream.WriteUInt8Async(color.B);

            await _client.SendMessageAsync(request);

            _colors[index] = color;
        }

        public virtual async Task SetSpeedAsync(float speed)
        {
            if (_client == null)
                throw new InvalidOperationException("Animation channel not set");

            var request = new Message(Command.DeviceSetChannelAnimationSpeed);
            await request.Stream.WriteUInt32Async(_deviceId);
            await request.Stream.WriteUInt8Async(_channel);
            await request.Stream.WriteFloatAsync(speed);

            await _client.SendMessageAsync(request);

            _speed = speed;
        }

        public async Task SetEnabledAsync(bool enabled)
        {
            if (_client == null)
                throw new InvalidOperationException("Animation channel not set");

            var request = new Message(Command.DeviceSetChannelAnimationEnabled);
            await request.Stream.WriteUInt32Async(_deviceId);
            await request.Stream.WriteUInt8Async(_channel);
            await request.Stream.WriteBooleanAsync(enabled);

            await _client.SendMessageAsync(request);

            _enabled = enabled;
        }

        internal delegate Task RequestCallback(BinaryStream stream);
        internal async Task SendRequestAsync(RequestCallback callback)
        {
            if (_client == null)
                throw new InvalidOperationException("Animation channel not set");

            var request = new Message(Command.DeviceSendChannelAnimationRequest);
            await request.Stream.WriteUInt32Async(_deviceId);
            await request.Stream.WriteUInt8Async(_channel);

            await callback(request.Stream);

            await _client.SendMessageAsync(request);
        }
    }
}
