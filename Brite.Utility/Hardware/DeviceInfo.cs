namespace Brite.Utility.Hardware
{
    public class DeviceInfo
    {
        public string Name { get; }
        public string Description { get; }
        public string Pnpid { get; }
        public string VendorId { get; }
        public string ProductId { get; }

        public DeviceInfo(string name, string description, string pnpId, string vendorId, string productId)
        {
            Name = name;
            Description = description;
            Pnpid = pnpId;
            VendorId = vendorId;
            ProductId = productId;
        }
    }
}
