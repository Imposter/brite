using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brite.Utility.IO;

namespace Brite.Micro.Operations
{
    public class DumpToStream : ProgrammerOperation
    {
        private MemoryType _type;
        private IStream _stream;

        public DumpToStream(IProgrammer programmer, MemoryType type, IStream stream) 
            : base(programmer)
        {
            _type = type;
            _stream = stream;
        }

        public override async Task Execute()
        {
            throw new NotImplementedException();
        }
    }
}
