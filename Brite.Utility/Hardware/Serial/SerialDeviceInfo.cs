namespace Brite.Utility.Hardware.Serial
{
    public class SerialDeviceInfo : DeviceInfo
    {
        public string PortName { get; }

        public SerialDeviceInfo(string name, string description, string portName, string pnpId, string vendorId, string productId) 
            : base(name, description, pnpId, vendorId, productId)
        {
            PortName = portName;
        }
    }
}
