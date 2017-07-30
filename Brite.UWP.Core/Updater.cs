using System;

namespace Brite.UWP.Core
{
    public class Updater : IUpdater
    {
        public void Update(UpdaterChipset chipset, string portName, int baudRate, byte[] updateFile)
        {
            // TODO: Implement: https://github.com/christophediericx/ArduinoSketchUploader/blob/master/Source/ArduinoUploader/ArduinoSketchUploader.cs
            throw new NotImplementedException();
        }
    }
}
