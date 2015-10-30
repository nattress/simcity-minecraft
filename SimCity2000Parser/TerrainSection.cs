using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SimCity2000Parser
{
    public class TerrainSection : CompressedSaveSection
    {
        public TerrainDescriptor[,] Terrain { get; private set; }

        internal TerrainSection() : base("XTER") 
        {
            Terrain = new TerrainDescriptor[128, 128];
        }

        internal override void ParseSection(System.IO.FileStream file)
        {
            base.ParseSection(file);

            List<TerrainDescriptor> terrainDescriptors = new List<TerrainDescriptor>();

            using (MemoryStream ms = new MemoryStream(RawData))
            {
                while (ms.Position < ms.Length)
                {
                    TerrainDescriptor terrain = new TerrainDescriptor(ms);
                    terrainDescriptors.Add(terrain);
                }
            }

            for (int i = 0; i < 128; i++)
            {
                for (int j = 0; j < 128; j++)
                {
                    Terrain[i, 127 - j] = terrainDescriptors[i * 128 + j];
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int y = 0; y < 128; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    TerrainDescriptor td = Terrain[y, x];

                    sb.AppendFormat("[{0}, {1}]\t\tType: {2}\tSlope: {3}\n", x, y, td.Type.ToString(), td.Slope.ToString());

                }
            }

            return sb.ToString();
        }
    }

    public class TerrainDescriptor
    {
        public TerrainType Type { get; private set; }
        public TerrainSlope Slope { get; private set; }
        byte Data;

        public TerrainDescriptor(Stream file)
        {
            Data = (byte)file.ReadByte();

            byte slopeInfo = 0;
            if (Data >= 0 && Data <= 0xD)
            {
                Type = TerrainType.DryLand;
                slopeInfo = Data;
            }
            else if (Data >= 0x10 && Data <= 0x1D)
            {
                Type = TerrainType.Submerged;
                slopeInfo = (byte)(Data & 0xF);
            }
            else if (Data >= 0x20 && Data <= 0x2D)
            {
                Type = TerrainType.PartiallySubmerged;
                slopeInfo = (byte)(Data & 0xF);
            }
            else if (Data >= 0x30 && Data <= 0x3D)
            {
                Type = TerrainType.SurfaceWater;
                slopeInfo = (byte)(Data & 0xF);
            }
            else if (Data == 0x3E)
            {
                Type = TerrainType.Waterfall;
            }
            else if (Data == 0x40)
            {
                Type = TerrainType.CanalEastWest;
            }
            else if (Data == 0x41)
            {
                Type = TerrainType.CanalNorthSouth;
            }
            else if (Data == 0x42)
            {
                Type = TerrainType.BaySouth;
            }
            else if (Data == 0x43)
            {
                Type = TerrainType.BayEast;
            }
            else if (Data == 0x44)
            {
                Type = TerrainType.BayNorth;
            }
            else if (Data == 0x45)
            {
                Type = TerrainType.BayWest;
            }

            switch (slopeInfo)
            {
                case 0:
                    Slope = TerrainSlope.LowFlat;
                    break;
                case 1:
                    Slope = TerrainSlope.South;
                    break;
                case 2:
                    Slope = TerrainSlope.West;
                    break;
                case 3:
                    Slope = TerrainSlope.North;
                    break;
                case 4:
                    Slope = TerrainSlope.East;
                    break;
                case 5:
                    Slope = TerrainSlope.SouthWest;
                    break;
                case 6:
                    Slope = TerrainSlope.NorthWest;
                    break;
                case 7:
                    Slope = TerrainSlope.NorthEast;
                    break;
                case 8:
                    Slope = TerrainSlope.SouthEast;
                    break;
                case 9:
                    Slope = TerrainSlope.SouthWestTerrace;
                    break;
                case 10:
                    Slope = TerrainSlope.NorthWestTerrace;
                    break;
                case 11:
                    Slope = TerrainSlope.NorthEastTerrace;
                    break;
                case 12:
                    Slope = TerrainSlope.SouthEastTerrace;
                    break;
                case 13:
                    Slope = TerrainSlope.HighFlat;
                    break;

            }
        }
        
    }

    /// <summary>
    /// Describes the various types of slopes the terrain can make in Sim City 2000
    /// All slopes are from the point of view of high ground to low ground.  A
    /// SouthWest slope starts high in the north-east, sloping down towards the south-
    /// west.
    /// </summary>
    public enum TerrainSlope : byte
    {
        LowFlat,
        South,
        West,
        North,
        East,
        SouthWest,
        NorthWest,
        NorthEast,
        SouthEast,
        SouthWestTerrace,
        NorthWestTerrace,
        NorthEastTerrace,
        SouthEastTerrace,
        HighFlat
    }

    public enum TerrainType
    {
        DryLand,
        Submerged,
        PartiallySubmerged,
        SurfaceWater,
        Waterfall,
        CanalEastWest,
        CanalNorthSouth,
        BaySouth,
        BayEast,
        BayNorth,
        BayWest
    }
}
