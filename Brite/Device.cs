using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Brite.Utility;
using Brite.Utility.IO;

namespace Brite
{
    public class Device
    {
        // Get logger
        private static readonly Log log = Logger.GetLog<Device>();

        // USB Device Info
        private readonly DeviceInfo _deviceInfo;

        // Serial Port
        private uint _baudRate;
        private int _timeout;
        private int _retries;
        private readonly ISerialConnection _serial;
        private TypedStream _stream;
        private Mutex _streamLock;
        private bool _isOpen;

        // Brite Info
        private uint _firmwareVersion;
        private uint _deviceId;
        private bool _bluetoothEnabled;
        private byte _channelCount;
        private ushort _channelMaxSize;
        private byte _channelMaxBrightness;
        private byte _animationMaxColors;
        private float _animationMinSpeed;
        private float _animationMaxSpeed;
        private readonly List<uint> _supportedAnimations;
        private readonly List<Channel> _channels;

        // Properties
        public DeviceInfo Info => _deviceInfo;

        public uint BaudRate => _baudRate;
        public int Timeout
        {
            get => _timeout; set
            {
                _timeout = value;
                if (_serial != null)
                    _serial.Timeout = value;
            }
        }

        public int Retries => _retries;
        public bool IsOpen => _isOpen;
        public uint FirmwareVersion => _firmwareVersion;
        public uint DeviceId => _deviceId;
        public bool BluetoothEnabled => _bluetoothEnabled;
        public byte ChannelCount => _channelCount;
        public ushort ChannelMaxSize => _channelMaxSize;
        public byte ChannelMaxBrightness => _channelMaxBrightness;
        public byte AnimationMaxColors => _animationMaxColors;
        public float AnimationMinSpeed => _animationMinSpeed;
        public float AnimationMaxSpeed => _animationMaxSpeed;
        public uint[] SupportedAnimations => _supportedAnimations.ToArray();
        public Channel[] Channels => _channels.ToArray();

        // Constructor
        public Device(ISerialConnection serial, DeviceInfo usbDeviceInfo)
        {
            _serial = serial;
            _deviceInfo = usbDeviceInfo;
            _supportedAnimations = new List<uint>();
            _channels = new List<Channel>();
        }

        public async Task Open(uint baudRate, int timeout, int retries, bool restart = false)
        {
            if (_isOpen)
                throw new InvalidOperationException("Port is already open");

            if (_serial.IsOpen)
                await _serial.Close();

            // Store config
            _baudRate = baudRate;
            _timeout = timeout;
            _retries = retries;

            // Open port
            _serial.PortName = _deviceInfo.PortName;
            _serial.BaudRate = _baudRate;
            _serial.DtrEnable = restart;
            _serial.Timeout = _timeout;
            await _serial.Open();

            // Create stream
            _stream = new TypedStream(_serial.BaseStream);

            // Create stream mutex
            _streamLock = new Mutex();

            // Wait for device to initialize
            if (restart)
                await Task.Delay(3000);

            // Get device information
            try
            {
                // Lock mutex
                await _streamLock.Lock();

                // Get firmware version
                {
                    // Wait for device to get ready
                    await SendCommand(Command.GetVersion);

                    // Read response
                    _stream.TypesEnabled = true;
                    var firmwareVersion = await _stream.ReadUInt32();

                    // Store version
                    _firmwareVersion = firmwareVersion;
                }

                // Get device ID
                {
                    // Wait for device to get ready
                    await SendCommand(Command.GetId);

                    // Read response success
                    _stream.TypesEnabled = true;
                    var result = await _stream.ReadUInt8();
                    if (result == (byte)Result.Ok)
                    {
                        var deviceId = await _stream.ReadUInt32();

                        // Store device ID
                        _deviceId = deviceId;
                    }
                    else
                    {
                        throw new Exception("Unable to read device ID");
                    }
                }

                // Get device parameters
                {
                    // Wait for device to get ready
                    await SendCommand(Command.GetParameters);

                    // Read response
                    _stream.TypesEnabled = true;
                    var bluetoothEnabled = await _stream.ReadBoolean();
                    var channelCount = await _stream.ReadUInt8();
                    var channelMaxSize = await _stream.ReadUInt16();
                    var channelMaxBrightness = await _stream.ReadUInt8();
                    var animationMaxColors = await _stream.ReadUInt8();
                    var animationMinSpeed = await _stream.ReadFloat();
                    var animationMaxSpeed = await _stream.ReadFloat();

                    // Store parameters
                    _bluetoothEnabled = bluetoothEnabled;
                    _channelCount = channelCount;
                    _channelMaxSize = channelMaxSize;
                    _channelMaxBrightness = channelMaxBrightness;
                    _animationMaxColors = animationMaxColors;
                    _animationMinSpeed = animationMinSpeed;
                    _animationMaxSpeed = animationMaxSpeed;
                }

                // Get animations
                {
                    // Wait for device to respond
                    await SendCommand(Command.GetAnimations);

                    // Read response
                    _stream.TypesEnabled = true;
                    var animationCount = await _stream.ReadUInt8();
                    var supportedAnimations = new List<uint>();
                    for (byte i = 0; i < animationCount; i++)
                        supportedAnimations.Add(await _stream.ReadUInt32());

                    // Store supported animations
                    _supportedAnimations.Clear();
                    _supportedAnimations.AddRange(supportedAnimations);
                }
            }
            finally
            {
                _streamLock.Unlock();
            }

            // Create channels
            _channels.Clear();
            for (byte i = 0; i < _channelCount; i++)
            {
                _channels.Add(new Channel(i, _channelMaxSize, _channelMaxBrightness, _animationMaxColors,
                    _animationMinSpeed, _animationMaxSpeed, _stream, _streamLock, _retries, _supportedAnimations));
            }

            _isOpen = true;
        }

        public async void Close()
        {
            if (!_isOpen)
                throw new InvalidOperationException("Port is not open");

            // Close port
            await _serial.Close();

            // Dereference stream
            _stream = null;

            _isOpen = false;
        }

        // TODO: Update using IUpdater

        public async Task Reset()
        {
            // Wait for device to respond
            await SendCommand(Command.Reset);

            // Read response
            _stream.TypesEnabled = true;
            var result = await _stream.ReadUInt8();
            if (result != (byte)Result.Ok)
                throw new Exception("Unable to reset device");
        }

        private async Task SendCommand(Command command)
        {
            var typesEnabled = _stream.TypesEnabled;

            var done = false;
            for (var i = 1; i <= _retries; i++)
            {
                try
                {
                    _stream.TypesEnabled = false;
                    await _stream.WriteUInt8((byte)command);

                    var response = await _stream.ReadUInt8();
                    if (response == (byte)command)
                    {
                        done = true;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    log.Warn($"SendCommand failed on try {i}");
                    log.Warn($"\tError: {ex}");
                }
            }

            _stream.TypesEnabled = typesEnabled;

            if (!done)
                throw new Exception("Unable to send command");
        }

        public static async Task<List<Device>> GetDevices<TSerial>(IDeviceSearcher searcher) where TSerial : ISerialConnection, new()
        {
            return (await searcher.GetDevices()).Select(deviceInfo => new Device(new TSerial(), deviceInfo)).ToList();
        }
    }
}