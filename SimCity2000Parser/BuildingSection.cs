using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SimCity2000Parser
{
    public class BuildingSection : CompressedSaveSection
    {
        public BuildingDescriptor[,] Buildings { get; private set; }
        ZoneSection zoneSection;

        internal BuildingSection() : base("XBLD") {
            Buildings = new BuildingDescriptor[128, 128];
        }

        internal void SetZoneSection(ZoneSection section)
        {
            zoneSection = section;
        }

        internal override void ParseSection(System.IO.FileStream file)
        {
            base.ParseSection(file);

            List<BuildingDescriptor> buildingDescriptors = new List<BuildingDescriptor>();

            using (MemoryStream ms = new MemoryStream(RawData))
            {
                while (ms.Position < ms.Length)
                {
                    var building = new BuildingDescriptor(ms);
                    buildingDescriptors.Add(building);
                }
            }

            for (int i = 0; i < 128; i++)
            {
                for (int j = 0; j < 128; j++)
                {
                    Buildings[i, 127 - j] = buildingDescriptors[i * 128 + j];
                }
            }
        }

        public void GetMultiTileBuildingOffset(int tileX, int tileY, out int tileOffsetX, out int tileOffsetY)
        {
            if (!IsMultiTileBuilding(tileX, tileY))
            {
                tileOffsetX = 0;
                tileOffsetY = 0;
                return;
            }

            tileOffsetX = tileX - zoneSection.Zone[tileX, tileY].OriginX;
            tileOffsetY = tileY - zoneSection.Zone[tileX, tileY].OriginY;
        }

        public bool IsMultiTileBuilding(int tileX, int tileY)
        {
            return zoneSection.Zone[tileX, tileY].IsMultiTile;
        }

        public int BuildingSizeX(int tileX, int tileY)
        {
            return zoneSection.Zone[tileX, tileY].LengthX;
        }

        public int BuildingSizeY(int tileX, int tileY)
        {
            return zoneSection.Zone[tileX, tileY].LengthY;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int y = 0; y < 128; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    BuildingDescriptor bd = Buildings[y, x];

                    sb.AppendFormat("[{0}, {1}]\t\tType: {2} (Value: {3})\n", x, y, bd.Type.ToString(), (int)bd.Type);
                }
            }

            return sb.ToString();
        }
    }

    public class BuildingDescriptor
    {
        public BuildingType Type { get; private set; }

        public BuildingDescriptor(Stream file)
        {
            Type = (BuildingType)file.ReadByte();
        }
    }

    public enum BuildingType : byte
    {
        ClearTerrain = 0x0,
        Rubble1 = 0x1,
        Rubble2 = 0x2,
        Rubble3 = 0x3,
        Rubble4 = 0x4,
        RadioactiveWaste = 0x5,
        Trees1 = 0x6,
        Trees2 = 0x7,
        Trees3 = 0x8,
        Trees4 = 0x9,
        Trees5 = 0xA,
        Trees6 = 0xB,
        Trees7 = 0xC,
        SmallPark = 0xD,
        PowerLineEastWest = 0xE,
        PowerLineNorthSouth = 0xF,
        PowerLineNorthSouthSlopeSouth = 0x10,
        PowerLineEastWestSlopeWest = 0x11,
        PowerLineNorthSouthSlopeNorth = 0x12,
        PowerLineEastWestSlopeEast = 0x13,
        PowerLineSouthEastCorner = 0x14,
        PowerLineSouthWestCorner = 0x15,
        PowerLineNorthWestCorner = 0x16,
        PowerLineNorthEastCorner = 0x17,
        PowerLineTNorthEastSouth = 0x18,
        PowerLineTWestSouthEast = 0x19,
        PowerLineTNorthWestSouth = 0x1A,
        PowerLineTNorthWestEast = 0x1B,
        PowerLineAllWay = 0x1C,
        RoadEastWest = 0x1D,
        RoadNorthSouth = 0x1E,
        RoadNorthSouthSlopeSouth = 0x1F,
        RoadEastWestSlopeWest = 0x20,
        RoadNorthSouthSlopeNorth = 0x21,
        RoadEastWestSlopeEast = 0x22,
        RoadSouthEastCorner = 0x23,
        RoadSouthWestCorner = 0x24,
        RoadNorthWestCorner = 0x25,
        RoadNorthEastCorner = 0x26,
        RoadTNorthEastSouth = 0x27,
        RoadTWestSouthEast = 0x28,
        RoadTNorthWestSouth = 0x29,
        RoadTNorthWestEast = 0x2A,
        RoadAllWay = 0x2B,
        RailEastWest = 0x2C,
        RailNorthSouth = 0x2D,
        RailNorthSouthSlopeSouth = 0x2E,
        RailEastWestSlopeWes = 0x2F,
        RailNorthSouthSlopeNorth = 0x30,
        RailEastWestSlopeEast = 0x31,
        RailSouthEastCorner = 0x32,
        RailSouthWestCorner = 0x33,
        RailNorthWestCorner = 0x34,
        RailNorthEastCorner = 0x35,
        RailTNorthEastSouth = 0x36,
        RailTWestSouthEast = 0x37,
        RailTNorthWestSouth = 0x38,
        RailTNorthWestEast = 0x39,
        RailAllWay = 0x3A,
        RailNorthSouthSlopeSouthPrep = 0x3B,
        RailEastWestSlopeWestPrep = 0x3C,
        RailNorthSouthSlopeNorthPrep = 0x3D,
        RailEastWestSlopeEastPrep = 0x3E,
        TunnelTop = 0x3F,
        TunnelRight = 0x40,
        TunnelBottom = 0x41,
        TunnelLeft = 0x42,
        // Crossovers (roads / power lines)
        RoadLeftRightPowerTopBottom = 0x43,
        RoadTopBottomPowerLeftRight = 0x44,
        // Crossovers (roads / rails)
        RoadLeftRightRailTopBottom = 0x45,
        RoadTopBottomRailLeftRight = 0x46,
        // Crossovers (rails / power lines)
        RailLeftRightPowerTopBottom = 0x47,
        RailTopBottomPowerLeftRight = 0x48,
        // Highways
        HighwayEastWest = 0x49,
        HighwayNorthSouth = 0x4A,
        // Crossovers (highway / road)
        HighwayEastWestRoadNorthSouth = 0x4B,
        HighwayNorthSouthRoadEastWest = 0x4C,
        // Crossovers (highway / rail)
        HighwayEastWestRailNorthSouth = 0x4D,
        HighwayNorthSouthRailEastWest = 0x4E,
        // Crossovers (highway / power line)
        HighwayEastWestPowerNorthSouth = 0x4F,
        HighwayNorthSouthPowerEastWest = 0x50,
        // Suspension bridge pieces
        SuspensionBridge1 = 0x51,
        SuspensionBridge2 = 0x52,
        SuspensionBridge3 = 0x53,
        SuspensionBridge4 = 0x54,
        SuspensionBridge5 = 0x55,
        RoadBridge1 = 0x56,
        RoadBridge2 = 0x57,
        RoadBridge3 = 0x58,
        RoadBridge4 = 0x59,
        RailBridge1 = 0x5A,
        RailBridge2 = 0x5B,
        ElevatedPowerLine = 0x5C,
        // Highway on-ramps
        HighwayOnramp1 = 0x5D,
        HighwayOnramp2 = 0x5E,
        HighwayOnramp3 = 0x5F,
        HighwayOnramp4 = 0x60,
        // Highways
        HighwayNorthSouthSlopeSouth = 0x61,
        HighwayEastWestSlopeWest = 0x62,
        HighwayNorthSouthSlopeNorth = 0x63,
        HighwayEastWestSlopeEast = 0x64,
        HighwaySouthEastCorner = 0x65,
        HighwaySouthWestCorner = 0x66,
        HighwayNorthWestCorner = 0x67,
        HighwayNorthEastCorner = 0x68,
        HighwayCloverLeaf = 0x69,
        // Highway reinforced bridges
        HighwayReinforcedBridge1 = 0x6A,
        HighwayReinforcedBridge2 = 0x6B,
        // Subway / rail connections
        SubRailConnectionRailSouth = 0x6C,
        SubRailConnectionRailWest = 0x6D,
        SubRailConnectionRailNorth = 0x6E,
        SubRailConnectionRailEast = 0x6F,
        //
        // Buildings
        //
        // Residential 1x1
        LowerClassHome1 = 0x70,
        LowerClassHome2 = 0x71,
        LowerClassHome3 = 0x72,
        LowerClassHome4 = 0x73,
        MiddleClassHome1 = 0x74,
        MiddleClassHome2 = 0x75,
        MiddleClassHome3 = 0x76,
        MiddleClassHome4 = 0x77,
        LuxuryHome1 = 0x78,
        LuxuryHome2 = 0x79,
        LuxuryHome3 = 0x7A,
        LuxuryHome4 = 0x7B,
        // Commercial 1x1
        GasStation1 = 0x7C,
        BedAndBreakfastInn = 0x7D,
        ConvenienceStore = 0x7E,
        GasStation2 = 0x7F,
        SmallOfficeBuilding = 0x80,
        OfficeBuilding = 0x81,
        Warehouse = 0x82,
        ToyStore = 0x83,
        // Industrial 1x1
        Warehouse1 = 0x84,
        ChemicalStorage = 0x85,
        Warehouse2 = 0x86,
        IndustrialSubstation = 0x87,
        // Miscellaneous 1x1
        Construction1 = 0x88,
        Construction2 = 0x89,
        AbandonedBuilding1 = 0x8A,
        AbandonedBuilding2 = 0x8B,
        // Residential 2x2
        CheapApartments = 0x8C,
        Apartments1 = 0x8D,
        Apartments2 = 0x8E,
        LuxuryApartments1 = 0x8F,
        LuxuryApartments2 = 0x90,
        Condo1 = 0x91,
        Condo2 = 0x92,
        Condo3 = 0x93,
        // Commercial 2x2
        ShoppingMall = 0x94,
        GroceryStore = 0x95,
        BigOfficeBuilding1 = 0x96,
        Hotel = 0x97,
        BigOfficeBuilding2 = 0x98,
        OfficeRetail = 0x99,
        BigOfficeBuilding3 = 0x9A,
        BigOfficeBuilding4 = 0x9B,
        BigOfficeBuilding5 = 0x9C,
        BigOfficeBuilding6 = 0x9D,
        // Industrial 2x2
        MediumWarehouse = 0x9E,
        ChemicalProcessing = 0x9F,
        Factory1 = 0xA0,
        Factory2 = 0xA1,
        Factory3 = 0xA2,
        Factory4 = 0xA3,
        Factory5 = 0xA4,
        Factory6 = 0xA5,
        //Miscellaneous 2x2
        ConstructionMedium1 = 0xA6,
        ConstructionMedium2 = 0xA7,
        ConstructionMedium3 = 0xA8,
        ConstructionMedium4 = 0xA9,
        AbandonedBuildingMedium1 = 0xAA,
        AbandonedBuildingMedium2 = 0xAB,
        AbandonedBuildingMedium3 = 0xAC,
        AbandonedBuildingMedium4 = 0xAD,
        // Residential 3x3
        LargeApartmentBuilding1 = 0xAE,
        LargeApartmentBuilding2 = 0xAF,
        LargeCondo1 = 0xB0,
        LargeCondo2 = 0xB1,
        // Commercial 3x3
        LargeOfficePark = 0xB2,
        OfficeTower1 = 0xB3,
        MiniMall = 0xB4,
        TheatreSquare = 0xB5,
        DriveInTheatre = 0xB6,
        OfficeTower2 = 0xB7,
        OfficeTower3 = 0xB8,
        ParkingLot = 0xB9,
        HistoricOfficeBuilding = 0xBA,
        CorporateHeadQuarters = 0xBB,
        // Industrial 3x3
        LargeChemicalProcessing = 0xBC,
        LargeFactory1 = 0xBD,
        LargeIndustrialBuilding = 0xBE,
        LargeFactory2 = 0xBF,
        LargeWarehouse1 = 0xC0,
        LargeWarehouse2 = 0xC1,
        // Miscellaneous 3x3
        LargeConstruction1 = 0xC2,
        LargeConstruction2 = 0xC3,
        LargeAbandonedBuilding1 = 0xC4,
        LargeAbandonedBuilding2 = 0xC5,
        // Power plants
        HydroElectricPower1 = 0xC6,
        HydroElectricPower2 = 0xC7,
        WindPower = 0xC8,
        NaturalGasPowerPlant = 0xC9,
        OilPowerPlant = 0xCA,
        NuclearPowerPlant = 0xCB,
        SolarPowerPlant = 0xCC,
        MicrowavePowerReceiver = 0xCD,
        FusionPowerPlant = 0xCE,
        CoalPowerPlant = 0xCF,
        // City services
        CitHall = 0xD0,
        Hospital = 0xD1,
        PoliceStation = 0xD2,
        FireStation = 0xD3,
        Museum = 0xD4,
        BigPark = 0xD5,
        School = 0xD6,
        Stadium = 0xD7,
        Prison = 0xD8,
        College = 0xD9,
        Zoo = 0xDA,
        Statue = 0xDB,
        // Seaports / transportation / military bases / misc city services
        WaterPump = 0xDC,
        Runway1 = 0xDD,
        Runway2 = 0xDE,
        Pier = 0xDF,
        Crane = 0xE0,
        ControlTower1 = 0xE1,
        ControlTower2 = 0xE2,
        SeaportWarehouse = 0xE3,
        AirportBuilding1 = 0xE4,
        AirportBuilding2 = 0xE5,
        Tarmac = 0xE6,
        FighterJet = 0xE7,
        Hangar1 = 0xE8,
        SubwayStation = 0xE9,
        Radar = 0xEA,
        WaterTower = 0xEB,
        BusStation = 0xEC,
        RailStation = 0xED,
        ParkingLot1 = 0xEE,
        LoadBay = 0xF0,
        TopSecret = 0xF1,
        CargoYard = 0xF2,
        MayorsHouse = 0xF3,
        WaterTreatmentPlant = 0xF4,
        Library = 0xF5,
        Hangar2 = 0xF6,
        Church = 0xF7,
        Marina = 0xF8,
        MissileSilo = 0xF9,
        DesalinationPlant = 0xFA,
        // Arcologies!
        PlymothArco = 0xFB,
        ForestArco = 0xFC,
        DarcoArco = 0xFD,
        LaunchArco = 0xFE,
        BrainLlamaDome = 0xFF
    }

    public enum BuildingOrientation : int
    {
        EastWest,
        NorthSouth,
        NorthSouthSlopeSouth,
        EastWestSlopeWest,
        NorthSouthSlopeNorth,
        EastWestSlopeEast,
        SouthEastCorner,
        SouthWestCorner,
        NorthWestCorner,
        NorthEastCorner,
        TNorthEastSouth,
        TWestSouthEast,
        TNorthWestSouth,
        TNorthWestEast,
        AllWay
    }
}
