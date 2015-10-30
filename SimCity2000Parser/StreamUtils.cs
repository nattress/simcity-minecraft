using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SimCity2000Parser
{
    static class StreamUtils
    {
        public static int Read4ByteInt(this Stream stream)
        {
            byte[] buffer = new byte[4];
            buffer[0] = (byte)stream.ReadByte();
            buffer[1] = (byte)stream.ReadByte();
            buffer[2] = (byte)stream.ReadByte();
            buffer[3] = (byte)stream.ReadByte();

            int retVal = buffer[0] << 24;
            retVal += buffer[1] << 16;
            retVal += buffer[2] << 8;
            retVal += buffer[3];

            return retVal;
        }

        public static int Read2ByteInt(this Stream stream)
        {
            byte[] buffer = new byte[2];
            buffer[0] = (byte)stream.ReadByte();
            buffer[1] = (byte)stream.ReadByte();
            
            int retVal = buffer[0] << 8;
            retVal += buffer[1];

            return retVal;
        }
    }
}
