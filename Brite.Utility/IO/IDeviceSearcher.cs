using System.Collections.Generic;
using System.Threading.Tasks;

namespace Brite.Utility.IO
{
    public interface IDeviceSearcher
    {
        Task<List<DeviceInfo>> GetDevices();
    }
}
