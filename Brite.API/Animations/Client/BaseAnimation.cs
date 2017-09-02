using Brite.Utility;
using Brite.Utility.Crypto;
using Brite.Utility.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Brite.API.Animations.Client
{
    public abstract class BaseAnimation
    {
        private BinaryStream _stream;
        private Mutex _streamLock;
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

        internal void Initialize(BinaryStream stream, Mutex streamLock, bool reset, uint deviceId, byte channel, byte maxColors, float minSpeed, float maxSpeed)
        {
            if (deviceId != _deviceId || channel != _channel)
                reset = true;

            _stream = stream;
            _streamLock = streamLock;
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
            _stream = null;
            _streamLock = null;
        }

        public uint GetId()
        {
            return Hash.Fnv1A32(GetName());
        }

        public async Task SetColorCountAsync(byte colorCount)
        {
            if (_stream == null)
                throw new InvalidOperationException("Animation channel not set");

            _colorCount = colorCount;

            try
            {
                await _streamLock.LockAsync();

                await SendCommandAsync(Command.DeviceSetChannelAnimationColorCount);
                await _stream.WriteUInt32Async(_deviceId);
                await _stream.WriteUInt8Async(_channel);
                await _stream.WriteUInt8Async(colorCount);

                await ReceiveResultAsync();
            }
            finally
            {
                _streamLock.Unlock();
            }
        }

        public async Task SetColorAsync(byte index, Color color)
        {
            if (_stream == null)
                throw new InvalidOperationException("Animation channel not set");

            _colors[index] = color;

            try
            {
                await _streamLock.LockAsync();

                await SendCommandAsync(Command.DeviceSetChannelAnimationColor);
                await _stream.WriteUInt32Async(_deviceId);
                await _stream.WriteUInt8Async(_channel);
                await _stream.WriteUInt8Async(index);
                await _stream.WriteUInt8Async(color.R);
                await _stream.WriteUInt8Async(color.G);
                await _stream.WriteUInt8Async(color.B);

                await ReceiveResultAsync();
            }
            finally
            {
                _streamLock.Unlock();
            }
        }

        public async Task SetSpeedAsync(float speed)
        {
            if (_stream == null)
                throw new InvalidOperationException("Animation channel not set");

            _speed = speed;

            try
            {
                await _streamLock.LockAsync();

                await SendCommandAsync(Command.DeviceSetChannelAnimationSpeed);
                await _stream.WriteUInt32Async(_deviceId);
                await _stream.WriteUInt8Async(_channel);
                await _stream.WriteFloatAsync(speed);

                await ReceiveResultAsync();
            }
            finally
            {
                _streamLock.Unlock();
            }
        }

        public async Task SetEnabledAsync(bool enabled)
        {
            if (_stream == null)
                throw new InvalidOperationException("Animation channel not set");

            _enabled = enabled;

            try
            {
                await _streamLock.LockAsync();

                await SendCommandAsync(Command.DeviceSetChannelAnimationEnabled);
                await _stream.WriteUInt32Async(_deviceId);
                await _stream.WriteUInt8Async(_channel);
                await _stream.WriteBooleanAsync(enabled);

                await ReceiveResultAsync();
            }
            finally
            {
                _streamLock.Unlock();
            }
        }

        internal delegate Task RequestCallback(BinaryStream stream);
        internal async Task SendRequestAsync(RequestCallback callback)
        {
            if (_stream == null)
                throw new InvalidOperationException("Animation channel not set");

            try
            {
                await _streamLock.LockAsync();

                await SendCommandAsync(Command.DeviceSendChannelAnimationRequest);
                await _stream.WriteUInt32Async(_deviceId);
                await _stream.WriteUInt8Async(_channel);

                await ReceiveResultAsync();

                await callback(_stream);
            }
            finally
            {
                _streamLock.Unlock();
            }
        }

        private async Task SendCommandAsync(Command command)
        {
            await _stream.WriteUInt8Async((byte)command);
            var responseCommand = await _stream.ReadUInt8Async();
            if (responseCommand != (byte)command)
                throw new BriteException($"Unexpected command response, expected {command} got {(Command)responseCommand}");
        }

        private async Task ReceiveResultAsync(Result expected = Result.Ok)
        {
            var result = await _stream.ReadUInt8Async();
            if (result != (byte)expected)
                throw new BriteException($"Unexpected result, expected {expected} got {(Result)result}");
        }
    }
}
