namespace Brite.Utility.IO.Serial
{
    public enum SerialHandshake
    {
        None = 0,
        RequestToSend = 1,
        XOnXOff = 2,
        RequestToSendXOnXOff = 3
    }
}
