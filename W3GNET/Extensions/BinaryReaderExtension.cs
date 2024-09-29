using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace W3GNET.Extensions
{
    internal static class BinaryReaderExtension
    {
        internal static void SkipBytes(this BinaryReader reader, uint count)
        {
            for (int i = 0; i < count; i++)
            {
                if (reader.BaseStream.Length <= reader.BaseStream.Position)
                {
                    return;
                }

                reader.ReadByte();
            }
        }

        /// <summary>
        /// Copies the content of the current stream, returns a new stream, and resets the reader's position.
        /// </summary>
        internal static Stream SliceFromCurrentOffset(this BinaryReader reader, int length)
        {
            long position = reader.BaseStream.Position;
            var output = reader.ReadBytes(length);
            reader.BaseStream.Position = position;
            return new MemoryStream(output);
        }

        internal static string ReadZeroTermString(this BinaryReader reader, StringEncoding encoding)
        {
            List<byte> bytes = new List<byte>();

            byte b;
            while (true)
            {
                b = reader.ReadByte();
                if (b == 0)
                    break;

                bytes.Add(b);
            }

            var str = string.Empty;
            if (encoding == StringEncoding.UTF8)
            {
                str = Encoding.Default.GetString(bytes.ToArray());
            }
            else if (encoding == StringEncoding.HEX)
            {
                str = BitConverter.ToString(bytes.ToArray()).Replace("-", "");
            }
            else if (encoding == StringEncoding.ASCII)
            {
                str = System.Text.Encoding.ASCII.GetString(bytes.ToArray()).Trim();
            }

            return str;
        }
    }

    internal enum StringEncoding
    {
        UTF8,
        HEX,
        ASCII,
    }
}
