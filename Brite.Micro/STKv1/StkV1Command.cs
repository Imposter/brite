namespace Brite.Micro.STKv1
{
    public enum StkV1Command : byte
    {
        GetSync = 0x30,
        GetSignOn = 0x31,
        SetParameterValue = 0x40,
        GetParameterValue = 0x41,
        SetDeviceParameters = 0x42,
        SetDeviceParametersExt = 0x45,
        EnterProgramMode = 0x50,
        LeaveProgramMode = 0x51,
        ChipErase = 0x52,
        LoadAddress = 0x55,
        Universal = 0x56,
        UniversalExt = 0x57,
        ProgramFlashMemory = 0x60,
        ProgramDataMemory = 0x61,
        ProgramFuseBits = 0x62,
        ProgramLockBits = 0x63,
        ProgramPage = 0x64,
        ProgramFuseBitsExt = 0x65,
        ReadFlashMemory = 0x70,
        ReadDataMemory = 0x71,
        ReadFuseBits = 0x72,
        ReadLockBits = 0x73,
        ReadPage = 0x74,
        ReadSignatureBytes = 0x75,
        ReadOscillatorCalibrationByte = 0x76,
        ReadFuseBitsExt = 0x77,
        ReadOscillatorCalibrationByteExt = 0x78
    }
}
