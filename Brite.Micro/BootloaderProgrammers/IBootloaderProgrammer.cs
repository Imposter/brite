using System.Threading.Tasks;
using Brite.Micro.Hardware.Memory;
using Brite.Micro.Intel;

namespace Brite.Micro.BootloaderProgrammers
{
    public interface IBootloaderProgrammer
    {
        Task Open();
        Task Close();
        Task EstablishSync();
        Task CheckDeviceSignature();
        Task InitializeDevice();
        Task EnableProgrammingMode();
        Task LeaveProgrammingMode();
        Task ProgramDevice(MemoryBlock memoryBlock);
        Task LoadAddress(IMemory memory, int offset);
        Task ExecuteWritePage(IMemory memory, int offset, byte[] bytes);
        Task<byte[]> ExecuteReadPage(IMemory memory);
    }
}
