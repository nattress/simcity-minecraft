using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using SimCity2000Parser;

namespace SimCity2000Parser
{
    /// <summary>
    /// Encapsulates a save game file
    /// </summary>
    public class SimCity2000Save
    {
        List<SaveSection> sections = new List<SaveSection>();
        int length;

        private SimCity2000Save() { }

        public static SimCity2000Save ParseSaveFile(string saveFilePath)
        {
            SimCity2000Save newSaveObject = new SimCity2000Save();

            FileStream reader = File.Open(saveFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            if (reader.Length < 8)
            {
                throw new SimCityParseException("Unexpectedly short file length");
            }

            byte[] fileMagic = new byte[4];
            
            if (reader.Read(fileMagic, 0, 4) != 4)
            {
                throw new SimCityParseException("Not a valid Sim City save file");
            }
            
            // FORM
            if (fileMagic[0] != 0x46 || fileMagic[1] != 0x4f || fileMagic[2] != 0x52 || fileMagic[3] != 0x4d)
            {
                throw new SimCityParseException("Invalid magic file identifier bytes");
            }

            newSaveObject.length = reader.Read4ByteInt();

            // Read the Scdh header.
            byte[] scdhHeader = new byte[4];

            if (reader.Read(scdhHeader, 0, 4) != 4)
            {
                throw new SimCityParseException("Not a Sim City save file");
            }
            
            // SCDH
            if (scdhHeader[0] != 0x53 || scdhHeader[1] != 0x43 || scdhHeader[2] != 0x44 || scdhHeader[3] != 0x48)
            {
                throw new SimCityParseException("Not a Sim City save file");
            }

            // Read sections
            while (reader.Position < reader.Length)
            {
                newSaveObject.sections.Add(SectionFactory.ParseSaveSection(reader, newSaveObject));
            }
            
            reader.Close();

            newSaveObject.BuildingSection.SetZoneSection(newSaveObject.ZoneSection);
            newSaveObject.ZoneSection.MiscSection = newSaveObject.MiscSection;
            return newSaveObject;
        }

        public void DiffSections(SimCity2000Save oldSave)
        {
            foreach (SaveSection section in sections)
            {
                List<SectionDifference> diffs = section.Diff(oldSave.GetSection(section.Name));

                if (diffs.Count > 0)
                {
                    Console.WriteLine(section.Name);

                    foreach (SectionDifference diff in diffs)
                    {
                        Console.WriteLine(string.Format("{0}\t\t{1}\t\t{2}", diff.FieldName, diff.OldValue, diff.NewValue));
                    }
                }
            }
        }

        internal SaveSection GetSection(string name)
        {
            foreach (SaveSection section in sections)
            {
                if (name.Equals(section.Name))
                    return section;
            }

            throw new Exception("Section not found");
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Length:\t" + length.ToString("x8"));
            sb.AppendLine("Sections:");

            foreach (SaveSection section in sections)
            {
                sb.AppendLine(section.ToString());
            }

            return sb.ToString();
        }

        public void PrintSections(List<string> sectionList = null)
        {

            if (sectionList == null || sectionList.Count == 0)
            {
                foreach (SaveSection section in sections)
                {
                    Console.WriteLine(section.ToString());
                }
            }
            else
            {
                foreach (string s in sectionList)
                {
                    SaveSection section = GetSection(s);
                    Console.WriteLine(section.ToString());
                }
            }
        }

        public MiscSection MiscSection
        {
            get
            {
                return GetSection("MISC") as MiscSection;
            }
        }

        public TerrainSection TerrainSection
        {
            get
            {
                return GetSection("XTER") as TerrainSection;
            }
        }

        public AltitudeSection AltitudeSection
        {
            get
            {
                return GetSection("ALTM") as AltitudeSection;
            }
        }

        public BuildingSection BuildingSection
        {
            get
            {
                return GetSection("XBLD") as BuildingSection;
            }
        }

        public ZoneSection ZoneSection
        {
            get
            {
                return GetSection("XZON") as ZoneSection;
            }
        }
    }

    public class SimCityParseException : Exception
    {
        public SimCityParseException(string message) : base(message) { }
    }
}
