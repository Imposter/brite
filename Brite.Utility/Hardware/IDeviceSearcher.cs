using System.Collections.Generic;
using System.Threading.Tasks;

namespace Brite.Utility.Hardware
{
    public interface IDeviceSearcher
    {
        Task<List<DeviceInfo>> GetDevices();
    }
}
