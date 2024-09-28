using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace W3GNET.Extensions
{
    public static class StreamExtension
    {
        public static byte[] Copy(this Stream stream, int maxLength)
        {
            _ = stream ?? throw new ArgumentNullException(nameof(stream));

            var output = new byte[maxLength];

            var bytesRead = 0;
            var lengthRemaining = maxLength;
            while (lengthRemaining > 0)
            {
                var read = stream.Read(output, bytesRead, lengthRemaining);
                if (read == 0)
                {
                    break;
                }

                bytesRead += read;
                lengthRemaining -= read;
            }

            if (lengthRemaining > 0)
            {
                Array.Resize(ref output, bytesRead);
            }

            return output;
        }
    }
}
