using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace W3GNET.Util
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

                buffer.Position = offset; // reset

                return new MemoryStream(output);
            }
        }
    }
}
