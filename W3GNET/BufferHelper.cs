using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace W3GNET
{
    internal class BufferHelper
    {
        internal static byte[] Slice(byte[] buffer, int offset, int length)
        {
            var output = buffer
                .Skip(offset)
                .Take(offset + length)
                .ToArray();

            return output;
        }
    }
}
