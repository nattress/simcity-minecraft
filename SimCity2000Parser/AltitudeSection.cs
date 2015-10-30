using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SimCity2000Parser
{
    public class AltitudeSection : SaveSection
    {
        public AltitudeDescriptor[,] AltitudeData { get; set; }

        internal AltitudeSection() : base("ALTM") { }

        internal override void ParseSection(System.IO.FileStream file)
        {
            Length = file.Read4ByteInt();
            RawDataFileOffset = (int)file.Position;
            List<AltitudeDescriptor> data = new List<AltitudeDescriptor>();

            while (file.Position < RawDataFileOffset + Length)
            {
                data.Add(new AltitudeDescriptor(file));
            }

            AltitudeData = new AltitudeDescriptor[128,128];
            for (int i = 0; i < 128; i++)
            {
                for (int j = 0; j < 128; j++)
                {
                    AltitudeData[i, 127 - j] = data[i * 128 + j];
                }
            }
        }

        public override List<SectionDifference> Diff(SaveSection oldSection)
        {
            AltitudeSection oldAltitudeSection = oldSection as AltitudeSection;
            List<SectionDifference> sds = new List<SectionDifference>();

            for (int i = 0; i < 128; i++)
            {
                for (int j = 0; j < 128; j++)
                {
                    if (!oldAltitudeSection.AltitudeData[i, j].Equals(AltitudeData[i, j]))
                    {
                        SectionDifference sd = new SectionDifference(j.ToString() + ", " + i.ToString(), oldAltitudeSection.AltitudeData[i, j].Altitude.ToString() + ", Water: " + oldAltitudeSection.AltitudeData[i, j].WaterCovered.ToString(), AltitudeData[i, j].Altitude.ToString() + ", Water: " + AltitudeData[i, j].WaterCovered.ToString());
                        sds.Add(sd);
                    }
                }
            }
            return sds;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int y = 0; y < 128; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    AltitudeDescriptor td = AltitudeData[y, x];

                    sb.AppendFormat("[{0}, {1}]\t\tAlt: {2}\tWater?: {3}\n", x, y, td.Altitude, td.WaterCovered.ToString());

                }
            }

            return sb.ToString();
        }
    }

    public class AltitudeDescriptor
    {
        /// <summary>
        /// Has a value from 0 - 31 representing altitude in feet / 100
        /// </summary>
        public int Altitude { get; private set; }
        public bool WaterCovered { get; private set; }

        public int RawValue { get; private set; }

        public AltitudeDescriptor(Stream file)
        {
            RawValue = file.Read2ByteInt();

            Altitude = RawValue & 0x1F;
            WaterCovered = (RawValue & 0x80) != 0;

            if ((RawValue & 0xFF60) != 0)
            {
                System.Diagnostics.Debug.WriteLine("Altitude data had values in unknown bytes");
            }
        }

        public override bool Equals(object obj)
        {
            if (!(obj is AltitudeDescriptor))
            {
                return false;
            }

            AltitudeDescriptor other = obj as AltitudeDescriptor;


            return this.Altitude == other.Altitude && this.WaterCovered == other.WaterCovered;
        }
    }
}
