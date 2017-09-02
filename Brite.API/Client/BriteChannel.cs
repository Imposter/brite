using Brite.API.Animations.Client;
using Brite.Utility;
using Brite.Utility.IO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Brite.API.Client
{
    public class BriteChannel
    {
        private readonly BinaryStream _stream;
        private readonly Mutex _streamLock;
        private readonly uint _deviceId;
        private readonly byte _index;
        private readonly ushort _maxSize;
        private readonly byte _maxBrightness;
        private readonly byte _animationMaxColors;
        private readonly float _animationMinSpeed;
        private readonly float _animationMaxSpeed;
        private readonly List<uint> _supportedAnimations;

        private ushort _size;
        private byte _brightness;
        private BaseAnimation _animation;

        // Properties
        public byte Index => _index;

        public ushort MaxSize => _maxSize;
        public byte MaxBrightness => _maxBrightness;
        public ushort Size => _size;
        public byte Brightness => _brightness;
        public BaseAnimation Animation => _animation;

        internal BriteChannel(BinaryStream stream, Mutex streamLock, uint deviceId, byte index, ushort maxSize,
            byte maxBrightness, byte animationMaxColors, float animationMinSpeed, float animationMaxSpeed,
            List<uint> supportedAnimations)
        {
            _stream = stream;
            _streamLock = streamLock;

            _deviceId = deviceId;
            _index = index;
            _maxSize = maxSize;
            _maxBrightness = maxBrightness;
            _animationMaxColors = animationMaxColors;
            _animationMinSpeed = animationMinSpeed;
            _animationMaxSpeed = animationMaxSpeed;
            _supportedAnimations = supportedAnimations;
        }

        public async Task RequestAsync(Priority priority = Priority.VeryLow)
        {
            try
            {
                await _streamLock.LockAsync();

                await SendCommandAsync(Command.RequestDeviceChannel);
                await _stream.WriteUInt32Async(_deviceId);
                await _stream.WriteUInt8Async(_index);
                await _stream.WriteUInt8Async((byte)priority);

                await ReceiveResultAsync();
            }
            finally
            {
                _streamLock.Unlock();
            }
        }

        public async Task ReleaseAsync()
        {
            try
            {
                await _streamLock.LockAsync();

                await SendCommandAsync(Command.ReleaseDeviceChannel);
                await _stream.WriteUInt32Async(_deviceId);
                await _stream.WriteUInt8Async(_index);

                await ReceiveResultAsync();
            }
            finally
            {
                _streamLock.Unlock();
            }
        }

        public async Task SetSizeAsync(ushort size)
        {
            _size = size;

            try
            {
                await _streamLock.LockAsync();

                await SendCommandAsync(Command.DeviceSetChannelLedCount);
                await _stream.WriteUInt32Async(_deviceId);
                await _stream.WriteUInt8Async(_index);
                await _stream.WriteUInt16Async(size);

                await ReceiveResultAsync();
            }
            finally
            {
                _streamLock.Unlock();
            }
        }

        public async Task SetBrightnessAsync(byte brightness)
        {
            _brightness = brightness;

            try
            {
                await _streamLock.LockAsync();

                await SendCommandAsync(Command.DeviceSetChannelBrightness);
                await _stream.WriteUInt32Async(_deviceId);
                await _stream.WriteUInt8Async(_index);
                await _stream.WriteUInt8Async(brightness);

                await ReceiveResultAsync();
            }
            finally
            {
                _streamLock.Unlock();
            }
        }

        public async Task SetAnimationAsync(BaseAnimation animation, bool reset = true)
        {
            // Check if the animation is supported
            var animId = animation.GetId();
            if (!_supportedAnimations.Contains(animId))
                throw new NotSupportedException("Animation is not supported");

            // Reset previous animation
            _animation?.Reset();

            // Store animation
            _animation = animation;

            // Initialize animation
            _animation.Initialize(_stream, _streamLock, reset, _deviceId, _index, _animationMaxColors, _animationMinSpeed, _animationMaxSpeed);

            try
            {
                await _streamLock.LockAsync();

                await SendCommandAsync(Command.DeviceSetChannelAnimation);
                await _stream.WriteUInt32Async(_deviceId);
                await _stream.WriteUInt8Async(_index);
                await _stream.WriteUInt32Async(animId);

                await ReceiveResultAsync();
            }
            finally
            {
                _streamLock.Unlock();
            }
        }

        public async Task ResetAsync()
        {
            try
            {
                await _streamLock.LockAsync();

                await SendCommandAsync(Command.DeviceChannelReset);
                await _stream.WriteUInt32Async(_deviceId);
                await _stream.WriteUInt8Async(_index);

                await ReceiveResultAsync();
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
