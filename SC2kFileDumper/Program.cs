using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using SimCity2000Parser;

namespace SC2kFileDumper
{
    class Program
    {

        static void PrintUsage()
        {
            Console.WriteLine("Sc2kFileDumper.exe <File> [Options]");
            Console.WriteLine(" Dumps all the Sim City save file sections of <File> unless specific");
            Console.WriteLine(" sections are selected with [Options].");
            Console.WriteLine("");
            Console.WriteLine("Sc2kFiledumper.exe /Diff <File1> <File2> [Options]");
            Console.WriteLine(" Performs a diff of two Sim City save files listing the differences");
            Console.WriteLine(" in all sections, unless specific sections are selected with [Options].");
            Console.WriteLine("");
            Console.WriteLine("Options:");
            Console.WriteLine(" /Misc - Dumps / diffs the Misc data section which stores a list");
            Console.WriteLine("         of integers for things like sea level, total cash, population.");
            Console.WriteLine(" /AltM - Dumps / diffs the terrain section which stores tile elevations");
            Console.WriteLine("         and whether tiles have water on them.");
            Console.WriteLine(" /Xter - Dumps / diffs the terrain section which describes tile geometry");
            Console.WriteLine("         such as slopes, waterfalls, canals, etc.");
            Console.WriteLine(" /Xzon - Dumps / diffs the zone section which desribes area zoning and");
            Console.WriteLine("         corners of buildings (useful for multi-tile buildings");
            Console.WriteLine(" /Xbld - Dumps / diffs the buildings section which desribes structures");
        }

        static void Main(string[] args)
        {
            bool doDiff = false;
            string file1 = null;
            string file2 = null;
            List<string> sections = new List<string>();

            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }

            int optionsFirstIndex = 0;

            if (args[0].Equals("/diff", StringComparison.OrdinalIgnoreCase))
            {
                doDiff = true;

                if (args.Length < 3)
                {
                    Console.WriteLine("Expected two files to diff");
                }

                file1 = args[1];
                file2 = args[2];
                optionsFirstIndex = 3;
            }
            else
            {
                file1 = args[0];
                optionsFirstIndex = 1;
            }

            if (!File.Exists(file1))
            {
                Console.WriteLine("File not found: {0}", file1);
                return;
            }

            if (file2 != null && !File.Exists(file2))
            {
                Console.WriteLine("File not found: {0}", file2);
                return;
            }

            if (args.Length > optionsFirstIndex)
            {
                for (int i = optionsFirstIndex; i < args.Length; i++)
                {
                    if (args[i].Equals("/misc", StringComparison.OrdinalIgnoreCase))
                    {
                        sections.Add("MISC");
                    }
                    else if (args[i].Equals("/xter", StringComparison.OrdinalIgnoreCase))
                    {
                        sections.Add("XTER");
                    }
                    else if (args[i].Equals("/altm", StringComparison.OrdinalIgnoreCase))
                    {
                        sections.Add("ALTM");
                    }
                    else if (args[i].Equals("/xzon", StringComparison.OrdinalIgnoreCase))
                    {
                        sections.Add("XZON");
                    }
                    else if (args[i].Equals("/xbld", StringComparison.OrdinalIgnoreCase))
                    {
                        sections.Add("XBLD");
                    }
                    else
                    {
                        Console.WriteLine("Invalid option");
                        return;
                    }
                }
            }

            if (doDiff)
            {
                SimCity2000Save save = null;
                SimCity2000Save save2 = null;
                try
                {
                    save = SimCity2000Save.ParseSaveFile(file1);
                    save2 = SimCity2000Save.ParseSaveFile(file2);

                    save2.DiffSections(save);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to parse Sim City data file: " + e.Message);
                }
            }
            else
            {
                SimCity2000Save save = null;
                try
                {
                    save = SimCity2000Save.ParseSaveFile(file1);
                    save.PrintSections(sections);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to parse Sim City data file: " + e.Message);
                }
            }

            //Console.Read();
            
        }
    }
}
