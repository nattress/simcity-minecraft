using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using SimCity2000Parser;
using Substrate;
using Substrate.Core;
using Substrate.Nbt;

namespace SimCityToMinecraft
{
    class Program
    {
        static void Main(string[] args)
        {
            // Todo: Add arg parsing.  For now just hard-code an input file
            // World seed:
            // 1097395251422502855
            string inputCity = @"C:\sc2000\lolol.sc2";
            //string inputCity = @"C:\sc2000\ARCOSPRI.SC2";

            SimCity2000Save save = SimCity2000Save.ParseSaveFile(inputCity);

            AnvilWorld world = AnvilWorld.Open(@"C:\Users\Simon\AppData\Roaming\.minecraft\saves\simcity");

            XmlSerializer xs = new XmlSerializer(typeof(ModelData));
            StreamReader sr = new StreamReader(@"H:\dev\SimCityToMinecraft\SimCityToMinecraft\inputModelConfig.xml");
            ModelData md = (ModelData)xs.Deserialize(sr);
            sr.Close();
            AnvilWorld inputWorld = AnvilWorld.Open(@"C:\Users\Simon\AppData\Roaming\.minecraft\saves\SimCityImportWorld");
            BuildingSource source = new BuildingSource(inputWorld, md);

            TerrainGenerator terrainGenerator = new TerrainGenerator(save, world, source);

            terrainGenerator.GenerateTerrain();
            world.Level.GameType = GameType.CREATIVE;
            world.Save();
            Console.ReadKey();
        }
    }
}
