using System.Collections.Generic;
using System.Threading.Tasks;
using Brite.Utility;
using Brite.Utility.IO;

namespace Brite.API.Client
{
    public class BriteDevice
    {
        private readonly BriteClient _client;
        private readonly uint _id;
        private readonly List<BriteChannel> _channels;
        private uint _version;
        private byte _channelCount;
        private ushort _channelMaxSize;
        private byte _channelMaxBrightness;
        private byte _animationMaxColors;
        private float _animationMinSpeed;
        private float _animationMaxSpeed;
        private readonly List<uint> _supportedAnimations;

        public uint Id => _id;
        public BriteChannel[] Channels => _channels.ToArray();
        public uint Version => _version;
        public byte ChannelCount => _channelCount;
        public ushort ChannelMaxSize => _channelMaxSize;
        public byte ChannelMaxBrightness => _channelMaxBrightness;
        public byte AnimationMaxColors => _animationMaxColors;
        public float AnimationMinSpeed => _animationMinSpeed;
        public float AnimationMaxSpeed => _animationMaxSpeed;
        public uint[] SupportedAnimations => _supportedAnimations.ToArray();

        internal BriteDevice(BriteClient client, uint id)
        {
            _client = client;
            _id = id;
            _channels = new List<BriteChannel>();
            _supportedAnimations = new List<uint>();
        }

        internal async Task InitializeAsync()
        {
            // Get version
            {
                var request = new Message(Command.DeviceGetVersion);
                await request.Stream.WriteUInt32Async(_id);

                var response = await _client.SendMessageAsync(request);
                _version = await response.Stream.ReadUInt32Async();
            }

            // Get parameters
            {
                var request = new Message(Command.DeviceGetParameters);
                await request.Stream.WriteUInt32Async(_id);

                var response = await _client.SendMessageAsync(request);
                _channelCount = await response.Stream.ReadUInt8Async();
                _channelMaxSize = await response.Stream.ReadUInt16Async();
                _channelMaxBrightness = await response.Stream.ReadUInt8Async();
                _animationMaxColors = await response.Stream.ReadUInt8Async();
                _animationMinSpeed = await response.Stream.ReadFloatAsync();
                _animationMaxSpeed = await response.Stream.ReadFloatAsync();
            }

            // Get supported animations
            {
                var request = new Message(Command.DeviceGetAnimations);
                await request.Stream.WriteUInt32Async(_id);

                var response = await _client.SendMessageAsync(request);
                var animations = await response.Stream.ReadUInt8Async();
                for (var i = 0; i < animations; i++)
                    _supportedAnimations.Add(await response.Stream.ReadUInt32Async());
            }

            // Create channels
            for (byte i = 0; i < _channelCount; i++)
                _channels.Add(new BriteChannel(_client, _id, i, _channelMaxSize, _channelMaxBrightness,
                    _animationMaxColors, _animationMinSpeed, _animationMaxSpeed, _supportedAnimations));
        }

        public async Task RequestAsync(Priority priority = Priority.VeryLow)
        {
            var request = new Message(Command.RequestDevice);
            await request.Stream.WriteUInt32Async(_id);
            await request.Stream.WriteUInt8Async((byte)priority);

            await _client.SendMessageAsync(request);
        }

        public async Task ReleaseAsync()
        {
            var request = new Message(Command.ReleaseDevice);
            await request.Stream.WriteUInt32Async(_id);

            await _client.SendMessageAsync(request);
        }

        public async Task OpenAsync()
        {
            var request = new Message(Command.OpenDevice);
            await request.Stream.WriteUInt32Async(_id);

            await _client.SendMessageAsync(request);
        }

        public async Task CloseAsync()
        {
            var request = new Message(Command.CloseDevice);
            await request.Stream.WriteUInt32Async(_id);

            await _client.SendMessageAsync(request);
        }

        public async Task SynchronizeAsync()
        {
            var request = new Message(Command.DeviceSynchronize);
            await request.Stream.WriteUInt32Async(_id);

            await _client.SendMessageAsync(request);
        }
    }
}
