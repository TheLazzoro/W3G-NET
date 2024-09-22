using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace W3GNET
{
    internal class BufferHelper
    {
        public static byte[] Slice(byte[] buffer, int offset, int length)
        {
            var output = buffer
                .Skip(offset)
                .Take(offset + length)
                .ToArray();

            return output;
        }
    }
}
