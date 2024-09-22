using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;

namespace W3GNET
{
    // Looks like this is just a C# StreamReader. 

    //internal class StatefulBufferParser
    //{
    //    private byte[] buffer;
    //    internal int offset;

    //    public void Initialize(byte[] buffer)
    //    {
    //        this.buffer = buffer;
    //        offset = 0;
    //    }

    //    public string ReadStringOfLength_Hex(int length)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public string ReadStringOfLength_UTF8(int length)
    //    {
    //        var output = BufferHelper.Slice(buffer, offset, length);
    //        var result = Encoding.UTF8.GetString(output);
    //        this.offset += length;
    //        return result;
    //    }

    //    private (string, int) ReadZeroTermString(byte[] input, int startAt = 0)
    //    {
    //        int pos = startAt;
    //        while (input[pos] != 0)
    //        {
    //            pos++;
    //        }
    //        var sliced = BufferHelper.Slice(input, startAt, pos);
    //        string encoded = Encoding.UTF8.GetString(sliced);
    //        int posDifference = pos - startAt + 1;
    //        this.offset += posDifference;

    //        return (encoded, posDifference);
    //    }

    //    public int ReadUInt32LE()
    //    {
    //        var val = this.buffer.readUInt32LE(this.offset);
    //        this.offset += 4;
    //        return val;
    //    }

    //    public int readUInt16LE()
    //    {
    //        var val = this.buffer.readUInt16LE(this.offset);
    //        this.offset += 2;
    //        return val;
    //    }

    //    public byte readUInt8()
    //    {
    //        var val = this.buffer.readUInt8(this.offset);
    //        this.offset += 1;
    //        return val;
    //    }

    //    public int readFloatLE()
    //    {
    //        var val = this.buffer.readFloatLE(this.offset);
    //        this.offset += 4;
    //        return val;
    //    }
    //}
}