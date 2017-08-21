using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brite.Utility;
using Brite.Utility.Crypto;
using Brite.Utility.IO;

namespace Brite
{
    public abstract class Animation
    {
        private TypedStream _stream;
        private Mutex _streamLock;
        private int _retries;
        private byte _channel;
        private byte _maxColors;
        private float _minSpeed;
        private float _maxSpeed;
        private readonly List<Color> _colors;
        private byte _colorCount;
        private float _speed;
        private bool _enabled;

        public int Retries => _retries;
        public byte Channel => _channel;
        public byte MaxColors => _maxColors;
        public float MinSpeed => _minSpeed;
        public float MaxSpeed => _maxSpeed;
        public Color[] Colors => _colors.ToArray();
        public byte ColorCount => _colorCount;
        public float Speed => _speed;
        public bool Enabled => _enabled;

        public abstract string GetName();

        protected virtual void OnInitialize()
        {
        }

        protected Animation()
        {
            _colors = new List<Color>();
        }

        internal void Initialize(byte channel, byte maxColors, float minSpeed, float maxSpeed, TypedStream stream, Mutex streamLock, int retries, bool reset)
        {
            if (channel != _channel)
                reset = true;

            _enabled = false;

            _channel = channel;
            _maxColors = maxColors;
            _minSpeed = minSpeed;
            _maxSpeed = maxSpeed;

            _stream = stream;
            _streamLock = streamLock;
            _retries = retries;

            if (!reset)
                return;

            // Clamp color count
            _colorCount = _colorCount.Clamp(byte.MinValue, maxColors);

            // Clamp speed between min and max
            _speed = _speed.Clamp(minSpeed, maxSpeed);

            _colors.Clear();
            _colors.AddRange(Enumerable.Repeat(Color.Black, maxColors));

            OnInitialize();
        }

        internal void Reset()
        {
            _stream = null;
            _streamLock = null;
        }

        private async Task SendCommandAsync(Command command)
        {
            var typesEnabled = _stream.TypesEnabled;

            for (var i = 0; i < _retries; i++)
            {
                try
                {
                    _stream.TypesEnabled = false;
                    await _stream.WriteUInt8Async((byte)command);

                    var response = await _stream.ReadUInt8Async();
                    if (response == (byte)command)
                        break;
                }
                catch (Exception ex)
                {
                    if (i == _retries - 1)
                        throw new Exception("Unable to send command", ex);
                }
            }

            _stream.TypesEnabled = typesEnabled;
        }

        public uint GetId()
        {
            return Hash.Fnv1A32(GetName());
        }

        public virtual async Task SetColorCountAsync(byte colorCount)
        {
            if (_stream == null)
                throw new InvalidOperationException("Animation channel not set");

            // Store color count
            _colorCount = colorCount;

            // Notify device
            try
            {
                // Lock mutex
                await _streamLock.LockAsync();

                // Wait for device to get ready
                await SendCommandAsync(Command.SetChannelAnimationColorCount);

                // Send parameters
                _stream.TypesEnabled = true;
                await _stream.WriteUInt8Async(_channel);
                await _stream.WriteUInt8Async(colorCount);

                // Read response
                _stream.TypesEnabled = true;
                var result = await _stream.ReadUInt8Async();
                if (result != (byte)Result.Ok)
                    throw new Exception("Unable to set animation color count");
            }
            finally
            {
                _streamLock.Unlock();
            }
        }

        public virtual async Task SetColorAsync(byte index, Color color)
        {
            if (_stream == null)
                throw new InvalidOperationException("Animation channel not set");

            // Store color
            _colors[index] = color;

            // Notify device
            try
            {
                // Lock mutex
                await _streamLock.LockAsync();

                // Wait for device to get ready
                await SendCommandAsync(Command.SetChannelAnimationColor);

                // Send parameters
                _stream.TypesEnabled = true;
                await _stream.WriteUInt8Async(_channel);
                await _stream.WriteUInt8Async(index);
                await _stream.WriteUInt8Async(color.R);
                await _stream.WriteUInt8Async(color.G);
                await _stream.WriteUInt8Async(color.B);

                // Read response
                _stream.TypesEnabled = true;
                var result = await _stream.ReadUInt8Async();
                if (result != (byte)Result.Ok)
                    throw new Exception("Unable to set animation color");
            }
            finally
            {
                _streamLock.Unlock();
            }
        }

        public virtual async Task SetSpeedAsync(float speed)
        {
            if (_stream == null)
                throw new InvalidOperationException("Animation channel not set");

            // Store speed
            _speed = speed;

            // Notify device
            try
            {
                // Lock mutex
                await _streamLock.LockAsync();

                // Wait for device to get ready
                await SendCommandAsync(Command.SetChannelAnimationSpeed);

                // Send parameters
                _stream.TypesEnabled = true;
                await _stream.WriteUInt8Async(_channel);
                await _stream.WriteFloatAsync(speed);

                // Read response
                _stream.TypesEnabled = true;
                var result = await _stream.ReadUInt8Async();
                if (result != (byte)Result.Ok)
                    throw new Exception("Unable to set animation speed");
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

            // Store enabled
            _enabled = enabled;

            // Notify device
            try
            {
                // Lock mutex
                await _streamLock.LockAsync();

                // Wait for device to get ready
                await SendCommandAsync(Command.SetChannelAnimationEnabled);

                // Send parameters
                _stream.TypesEnabled = true;
                await _stream.WriteUInt8Async(_channel);
                await _stream.WriteBooleanAsync(enabled);

                // Read response
                _stream.TypesEnabled = true;
                var result = await _stream.ReadUInt8Async();
                if (result != (byte)Result.Ok)
                    throw new Exception("Unable to set animation enabled");
            }
            finally
            {
                _streamLock.Unlock();
            }
        }

        internal delegate Task RequestCallback(TypedStream stream);
        internal async Task SendRequestAsync(RequestCallback callback)
        {
            if (_stream == null)
                throw new InvalidOperationException("Animation channel not set");

            // Notify device
            try
            {
                // Lock mutex
                await _streamLock.LockAsync();

                // Wait for device to get ready
                await SendCommandAsync(Command.SendChannelAnimationRequest);

                // Send parameters
                _stream.TypesEnabled = true;
                await _stream.WriteUInt8Async(_channel);
                await _stream.WriteUInt32Async(GetId());

                // Read response
                var result = await _stream.ReadUInt8Async();
                if (result != (byte)Result.Ok)
                    throw new Exception("Unable to send animation request");

                _stream.TypesEnabled = false;
                await callback(_stream);
            }
            finally
            {
                _streamLock.Unlock();
            }
        }
    }
}
