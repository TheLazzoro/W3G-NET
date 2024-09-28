using System;
using System.Collections.Generic;
using System.Text;

namespace W3GNET.Extensions
{
    public static class StringExtension
    {
        public static byte[] FromHexToByteArray(this string hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}
