using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SimCity2000Parser
{
    public class SaveSection
    {
        /// <summary>
        /// Section name ie XTER
        /// </summary>
        public virtual string Name { get; protected set; }
        public int Length { get; protected set; }

        public byte[] RawData { get; protected set; }
        public int RawDataFileOffset { get; protected set; }

        /// <summary>
        /// This paramaterized constructor should go away and this class should be abstract
        /// once I know more about the file lay-out
        /// </summary>
        /// <param name="name"></param>
        internal SaveSection(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Pre-condition: The section name has already been parsed
        /// </summary>
        /// <param name="file"></param>
        internal virtual void ParseSection(FileStream file)
        {
            Length = file.Read4ByteInt();
            RawDataFileOffset = (int)file.Position;
            RawData = new byte[Length];
            file.Read(RawData, 0, Length);
        }

        /// <summary>
        /// Base implementation of diffing that just works on the raw data.
        /// Eventually, each specializing class should understand the data and diff
        /// actual fields
        /// </summary>
        /// <param name="oldSection"></param>
        /// <returns></returns>
        public virtual List<SectionDifference> Diff(SaveSection oldSection)
        {
            List<SectionDifference> diffs = new List<SectionDifference>();

            if (RawData.Length != oldSection.RawData.Length)
            {
                diffs.Add(new SectionDifference("Length", oldSection.Length.ToString("x8"), Length.ToString("x8")));
            }

            for (int i = 0; i < RawData.Length; i++)
            {
                // Handle this data section being larger
                if (i >= oldSection.RawData.Length)
                {
                    diffs.Add(new SectionDifference((i + RawDataFileOffset).ToString("x8"), "", RawData[i].ToString("x2")));
                }
                else if (RawData[i] != oldSection.RawData[i])
                {
                    diffs.Add(new SectionDifference((i + RawDataFileOffset).ToString("x8"), oldSection.RawData[i].ToString("x2"), RawData[i].ToString("x2")));
                }
            }

            // Handle oldSection's data section being larger
            if (oldSection.RawData.Length > RawData.Length)
            {
                for (int i = oldSection.RawData.Length; i < oldSection.RawData.Length; i++)
                {
                    diffs.Add(new SectionDifference((i + oldSection.RawDataFileOffset).ToString("x2"), oldSection.RawData[i].ToString("x2"), ""));
                }
            }

            return diffs;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Name:\t" + Name);
            sb.AppendLine("Length:\t" + Length.ToString("x8"));

            return sb.ToString();
        }
    }
}
