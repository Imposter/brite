using Brite.Utility.IO;

namespace Brite.API.Server
{
    internal class Message
    {
        internal BinaryStream InternalStream { get; }

        public Command Command { get; }
        public int Id { get; }
        public BinaryStream Stream { get; }

        public Message(BinaryStream internalStream, Command command, int id, BinaryStream inputStream)
        {
            InternalStream = internalStream;
            Command = command;
            Id = id;
            Stream = inputStream;
        }
    }
}
