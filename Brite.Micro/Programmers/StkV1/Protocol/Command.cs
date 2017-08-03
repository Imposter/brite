namespace Brite.Micro.Programmers.StkV1.Protocol
{
    internal enum Command : byte
    {
        GetSync = 0x30,
        SetDeviceParameters = 0x42,
        SetDeviceParametersExt = 0x45,
        EnterProgramMode = 0x50,
        LeaveProgramMode = 0x51,
        ChipErase = 0x52,
        LoadAddress = 0x55,
        Universal = 0x56,
        ProgramPage = 0x64,
        ReadPage = 0x74,
    }
}
