namespace Brite.Utility.IO
{
    public class DeviceInfo
    {
        public string Name { get; }
        public string Description { get; }
        public string PortName { get; }
        public string Pnpid { get; }
        public string VendorId { get; }
        public string ProductId { get; }

        public DeviceInfo(string name, string description, string portName, string pnpId, string vendorId, string productId)
        {
            Name = name;
            Description = description;
            PortName = portName;
            Pnpid = pnpId;
            VendorId = vendorId;
            ProductId = productId;
        }
    }
}
