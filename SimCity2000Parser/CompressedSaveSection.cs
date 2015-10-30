using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SimCity2000Parser
{
    /// <summary>
    /// The data is formatted in many sections as a repeating set of blobs with length
    /// information and data as follows:
    /// 0   Number that ranges from 0x80 upwards.  This looks like it may be a 
    ///     bit-field / descriptor as it's non-unique
    /// 1-2 2-byte length field that describes the length of the remaining data in the blob
    /// 3+  Data stored in the blob
    /// </summary>
    public class CompressedSaveSection : SaveSection
    {
        protected CompressedSaveSection(string s) : base(s) {}

        internal override void ParseSection(System.IO.FileStream file)
        {
            // Call base parser to populate RawData, then we can parse the structures
            Length = file.Read4ByteInt();
            RawDataFileOffset = (int)file.Position;
            
            List<byte> bytes = new List<byte>();
            while (file.Position < RawDataFileOffset + Length)
            {
                // Read a byte, n.  If it's < 128, read the next n bytes into the buffer.
                // If it's > 128, subtract 127 and then read the next byte and duplicate n - 127 times
                byte b = (byte)file.ReadByte();

                if (b > 0 && b < 128)
                {
                    //byte nextByte = (byte)file.ReadByte();
                    for (int i = 0; i < b; ++i)
                    {
                        bytes.Add((byte)file.ReadByte());
                    }
                }
                else if (b > 128)
                {
                    byte nextByte = (byte)file.ReadByte();
                    for (int i = 0; i < b - 127; ++i)
                    {
                        bytes.Add(nextByte);
                    }
                }
                else
                {
                    throw new Exception("Bad run-length encoding");
                }
            }

            RawData = bytes.ToArray();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
