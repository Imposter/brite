namespace Brite
{
    public interface IUpdater
    {
        void Update(UpdaterChipset chipset, string portName, int baudRate, byte[] updateFile);
    }
}
