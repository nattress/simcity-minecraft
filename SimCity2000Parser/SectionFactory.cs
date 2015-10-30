using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SimCity2000Parser
{
    class SectionFactory
    {
        /// <summary>
        /// Pre-condition: stream's file position must be at the beginning of a new section, marked with 4/8 byte ascii strings
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static SaveSection ParseSaveSection(FileStream stream, SimCity2000Save save)
        {
            byte[] name = new byte[4];

            stream.Read(name, 0, 4);
            string sectionName = BytesToString(name);

            SaveSection newSection = null;

            switch (sectionName)
            {
                case "MISC":
                    newSection = new MiscSection();
                    break;
                case "ALTM":
                    newSection = new AltitudeSection();
                    break;
                case "XTER":
                    newSection = new TerrainSection();
                    break;
                case "XBLD":
                    newSection = new BuildingSection();
                    break;
                case "XZON":
                    newSection = new ZoneSection();
                    ((ZoneSection)newSection).MiscSection = save.MiscSection;
                    break;
                default:
                    newSection = new SaveSection(sectionName);
                    break;
            }
            
            newSection.ParseSection(stream);
            return newSection;
        }

        static string BytesToString(byte[] bytes)
        {
            string retString = "";
            foreach (byte b in bytes)
            {
                retString += (char)b;
            }

            return retString;
        }
    }
}
