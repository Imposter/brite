namespace Brite.Micro.STKv1
{
    public enum StkDeviceCode : byte
    {
        None = 0xff,
        Pic16F628 = 1,

        ATtiny11 = 0x11,
        ATtiny12 = 0x12,
        ATtiny15 = 0x13,
        ATtiny22 = 0x20,
        ATtiny26 = 0x21,
        ATtiny28 = 0x28,
        AT90S1200 = 0x33,
        AT90S2313 = 0x40,
        AT90S2323 = 0x41,
        AT90S2333 = 0x42,
        AT90S2343 = 0x43,
        AT90S4414 = 0x50,
        AT90S4433 = 0x51,
        AT90S4434 = 0x52,
        AT90S8515 = 0x60,
        AT90S8535 = 0x61,
        ATmega8 = 0x62,
        ATmega8515 = 0x63,
        ATmega8535 = 0x64,
        ATmega161 = 0x80,
        ATmega163 = 0x81,
        ATmega16 = 0x82,
        ATmega162 = 0x83,
        ATmega169 = 0x84,
        Atmega328 = 0x86,
        ATmega323 = 0x90,
        ATmega32 = 0x91,
        ATmega64 = 0xA0,
        ATmega103 = 0xB1,
        ATmega128 = 0xB2,
        AT89551 = 0xE1,
        AT89552 = 0xE2,
        AT86RF401 = 0xD0
    }
}
