using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace W3GNET.Parsers
{
    internal class DataBlock
    {
        public int BlockSize;
        public int BlockDecompressedSize;
        public Stream buffer;
    }

    internal class RawParser
    {
    }
}
