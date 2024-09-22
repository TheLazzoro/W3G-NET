using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace W3GNET.Extensions
{
    internal static class BinaryReaderExtension
    {
        public static void SkipBytes(this BinaryReader reader, int count)
        {
            for (int i = 0; i < count; i++)
            {
                reader.ReadByte();
            }
        }
    }
}
