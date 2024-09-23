using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

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

        /// <summary>
        /// Copies the content of the specified stream, and returns a new stream.
        /// </summary>
        internal static async Task<Stream> Slice(Stream buffer, int offset, int length)
        {
            using (MemoryStream ms = new MemoryStream((int)buffer.Length))
            {
                await buffer.CopyToAsync(ms);
                var reader = new BinaryReader(ms);
                reader.BaseStream.Position = offset;
                var output = reader.ReadBytes(length);

                return new MemoryStream(output);
            }
        }
    }
}
