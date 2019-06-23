using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brite.API.Client;
using Brite.Utility.Hardware.Serial;

namespace Brite.App.Win.Services
{
    public interface IBriteService : IService
    {
        Task<IList<SerialDeviceInfo>> GetHardwareDevicesAsync();

        Task<IList<BriteDevice>> GetDevicesAsync();

        Task UpdateFirmwareAsync(BriteDevice device, string firmwarePath);
    }
}
