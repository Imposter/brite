using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brite.Utility;
using Brite.Utility.IO;

namespace Brite.API.Client
{
    public class BriteDevice
    {
        private readonly BinaryStream _stream;
        private readonly Mutex _streamLock;
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

        internal BriteDevice(BinaryStream stream, Mutex streamLock, uint id)
        {
            _stream = stream;
            _streamLock = streamLock;
            _id = id;
            _channels = new List<BriteChannel>();
            _supportedAnimations = new List<uint>();
        }

        internal async Task InitializeAsync()
        {
            // Get version
            {
                await SendCommandAsync(Command.DeviceGetVersion);
                await _stream.WriteUInt32Async(_id);

                await ReceiveResultAsync();
                _version = await _stream.ReadUInt32Async();
            }

            // Get parameters
            {
                await SendCommandAsync(Command.DeviceGetParameters);
                await _stream.WriteUInt32Async(_id);

                await ReceiveResultAsync();
                _channelCount = await _stream.ReadUInt8Async();
                _channelMaxSize = await _stream.ReadUInt16Async();
                _channelMaxBrightness = await _stream.ReadUInt8Async();
                _animationMaxColors = await _stream.ReadUInt8Async();
                _animationMinSpeed = await _stream.ReadFloatAsync();
                _animationMaxSpeed = await _stream.ReadFloatAsync();
            }

            // Get supported animations
            {
                await SendCommandAsync(Command.DeviceGetAnimations);
                await _stream.WriteUInt32Async(_id);

                await ReceiveResultAsync();
                var animations = await _stream.ReadUInt8Async();
                for (var i = 0; i < animations; i++)
                    _supportedAnimations.Add(await _stream.ReadUInt32Async());
            }

            // TODO: Create channels
        }

        public async Task SynchronizeAsync()
        {
            await SendCommandAsync(Command.DeviceSynchronize);
            await _stream.WriteUInt32Async(_id);

            await ReceiveResultAsync();
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
