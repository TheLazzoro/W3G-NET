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

        public static string ReadZeroTermString(this BinaryReader reader)
        {
            long pos = reader.BaseStream.Position;
            List<byte> bytes = new List<byte>();

            do
            {
                byte b = reader.ReadByte();
                bytes.Add(b);
                pos++;
            }
            while (reader.ReadByte() != 0);
            pos--;

            var str = Encoding.Default.GetString(bytes.ToArray());

            return str;
        }
    }
}
