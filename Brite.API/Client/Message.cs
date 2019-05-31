using Brite.Utility;
using Brite.Utility.IO;
using System.Threading.Tasks;

namespace Brite.API.Client
{
    internal class Message
    {
        private readonly ManualReset _reset;
        private Message _response;

        public Command Command { get; }
        public Result Result { get; }
        public BinaryStream Stream { get; }

        public Message(Command command)
        {
            _reset = new ManualReset();

            Command = command;
            Stream = new BinaryStream(new MemoryStream());
        }

        public Message(Command command, Result result, BinaryStream stream)
        {
            Command = command;
            Result = result;
            Stream = stream;
        }

        public async Task<Message> GetResponse()
        {
            if (_reset == null) return null;
            if (_response != null) return _response;
            await _reset.WaitAsync();
            return _response;
        }

        internal void SetResponse(Result result, BinaryStream stream)
        {
            _response = new Message(Command, result, stream);
            _reset.Set();
        }
    }
}
