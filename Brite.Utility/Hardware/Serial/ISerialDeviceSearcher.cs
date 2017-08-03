using System.Collections.Generic;
using System.Threading.Tasks;

namespace Brite.Utility.Hardware.Serial
{
    public interface ISerialDeviceSearcher
    {
        Task<List<SerialDeviceInfo>> GetDevicesAsync();
    }
}
