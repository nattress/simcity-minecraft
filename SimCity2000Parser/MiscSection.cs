using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SimCity2000Parser
{
    /// <summary>
    /// The Misc section is 1200 4-byte integers, stored big-endian
    /// </summary>
    public class MiscSection : CompressedSaveSection
    {
        List<int> Data = new List<int>();

        public MiscSection() : base("MISC") { }
        
        public override string Name
        {
            get
            {
                return "MISC";
            }
        }

        //
        // Properties that map to the Data List
        //
        public int SeaLevel
        {
            get
            {
                return Data[912];
            }
        }

        public int Rotation
        {
            get
            {
                return Data[2];
            }
        }

        internal override void ParseSection(System.IO.FileStream file)
        {
            base.ParseSection(file);

            // RawData now contains an uncompressed set of 1200 4-byte integers.  Read them
            // into Data.
            using (MemoryStream ms = new MemoryStream(RawData))
            {
                while (ms.Position < ms.Length)
                {
                    Data.Add(ms.Read4ByteInt());
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(base.ToString());

            for (int i = 0; i < Data.Count; ++i)
            {
                sb.AppendLine(string.Format("{0}\t{1}", i, Data[i]));
            }

            return sb.ToString();
        }

        public override List<SectionDifference> Diff(SaveSection oldSection)
        {
            var oldMiscSection = oldSection as MiscSection;

            List<SectionDifference> diffs = new List<SectionDifference>();
            for (int i = 0; i < Data.Count; i++)
            {
                if (Data[i] != oldMiscSection.Data[i])
                {
                    diffs.Add(new SectionDifference(i.ToString(), oldMiscSection.Data[i].ToString(), Data[i].ToString()));
                }
            }

            return diffs;
        }
    }
}
