using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Brite.API.Animations.Client;
using Brite.Utility;
using Brite.Utility.IO;

namespace Brite.API.Client
{
    public class BriteChannel
    {
        private readonly BriteClient _client;
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

        internal BriteChannel(BriteClient client, uint deviceId, byte index, ushort maxSize,
            byte maxBrightness, byte animationMaxColors, float animationMinSpeed, float animationMaxSpeed,
            List<uint> supportedAnimations)
        {
            _client = client;

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
            var request = new Message(Command.RequestDeviceChannel);
            await request.Stream.WriteUInt32Async(_deviceId);
            await request.Stream.WriteUInt8Async(_index);
            await request.Stream.WriteUInt8Async((byte)priority);

            await _client.SendMessageAsync(request);
        }

        public async Task ReleaseAsync()
        {
            var request = new Message(Command.ReleaseDeviceChannel);
            await request.Stream.WriteUInt32Async(_deviceId);
            await request.Stream.WriteUInt8Async(_index);

            await _client.SendMessageAsync(request);
        }

        public async Task SetSizeAsync(ushort size)
        {

            var request = new Message(Command.DeviceSetChannelLedCount);
            await request.Stream.WriteUInt32Async(_deviceId);
            await request.Stream.WriteUInt8Async(_index);
            await request.Stream.WriteUInt16Async(size);

            await _client.SendMessageAsync(request);

            _size = size;
        }

        public async Task SetBrightnessAsync(byte brightness)
        {
            var request = new Message(Command.DeviceSetChannelBrightness);
            await request.Stream.WriteUInt32Async(_deviceId);
            await request.Stream.WriteUInt8Async(_index);
            await request.Stream.WriteUInt8Async(brightness);

            await _client.SendMessageAsync(request);

            _brightness = brightness;
        }

        public async Task SetAnimationAsync(BaseAnimation animation, bool reset = true)
        {
            // Check if the animation is supported
            var animId = animation.GetId();
            if (!_supportedAnimations.Contains(animId))
                throw new NotSupportedException("Animation is not supported");

            // Reset previous animation
            _animation?.Reset();

            // Set animation
            var request = new Message(Command.DeviceSetChannelAnimation);
            await request.Stream.WriteUInt32Async(_deviceId);
            await request.Stream.WriteUInt8Async(_index);
            await request.Stream.WriteUInt32Async(animId);

            await _client.SendMessageAsync(request);

            // Store animation
            _animation = animation;

            // Initialize animation
            _animation.Initialize(_client, reset, _deviceId, _index, _animationMaxColors, _animationMinSpeed, _animationMaxSpeed);
        }

        public async Task ResetAsync()
        {
            var request = new Message(Command.DeviceChannelReset);
            await request.Stream.WriteUInt32Async(_deviceId);
            await request.Stream.WriteUInt8Async(_index);

            await _client.SendMessageAsync(request);
        }
    }
}
