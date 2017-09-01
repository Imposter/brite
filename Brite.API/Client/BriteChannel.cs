using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brite.Utility;
using Brite.Utility.IO;

namespace Brite.API.Client
{
    public class BriteChannel
    {
        private readonly BinaryStream _stream;
        private readonly Mutex _streamLock;

        // TODO: Local public properties

        // TODO: byte index, ushort maxSize, byte maxBrightness, byte animationMaxColors, float animationMinSpeed, float animationMaxSpeed, etc, from Channel.cs


        // TODO: Public RequestChannel // Channel::Release
        // NOTE: Do we re-request channel once we've lost ownership?

        // ...
        // TODO: This should also take device ID as a parameter
        internal BriteChannel(BinaryStream stream, Mutex streamLock)
        {
            _stream = stream;
            _streamLock = streamLock;
        }
    }
}
