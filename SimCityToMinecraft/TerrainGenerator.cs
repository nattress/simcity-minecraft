using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimCity2000Parser;
using Substrate;
using Substrate.Core;
using Substrate.Nbt;
using System.Diagnostics;
namespace SimCityToMinecraft
{
    /// <summary>
    /// Takes the output of the Sim City parser's altitude and terrain sections
    /// and generates the Minecraft world area that the city will sit on.
    /// </summary>
    internal class TerrainGenerator
    {
        TerrainSection terrain;
        AltitudeSection altitude;
        BuildingSection buildings;
        ZoneSection zones;
        AnvilWorld world;
        BuildingSource buildingSource;

        int seaLevel;

        /// <summary>
        /// Number of Minecraft blocks per Sim City map tile
        /// </summary>
        const int BlocksPerTile = 20;
        /// <summary>
        /// Minecraft block Y level that corresponds with the lowest terrain from Sim City maps.
        /// We want to strike a balance between allowing striking differences in terrain level,
        /// and being able to fit tall buildings even on high ground.  Ideally, it would be nice
        /// if sea-level in the Sim City map corresponded to Minecraft's sea-level of 65.
        /// </summary>
        const int BaseYLevel = 40;

        /// <summary>
        /// How many blocks higher we go in Minecraft for each successive elevation change
        /// in the Sim City map.
        /// </summary>
        const int TerrainHeightBlockMultiplier = 5;

        public TerrainGenerator(SimCity2000Save simCity, AnvilWorld world, BuildingSource buildingSource)
        {
            terrain = simCity.TerrainSection;
            altitude = simCity.AltitudeSection;
            buildings = simCity.BuildingSection;
            zones = simCity.ZoneSection;
            this.world = world;
            this.buildingSource = buildingSource;
            seaLevel = simCity.MiscSection.SeaLevel;
        }

        public void GenerateTerrain()
        {
            RegionChunkManager cm = world.GetChunkManager();
            
            int createdChunkCount = 0;

            // Calculate how many chunks we will need
            int chunkSizeOfMap = (int)Math.Ceiling((128d * (double)BlocksPerTile) / 16d);
            // TODO: Replace this with the above line to generate a full city (slow)
            //int chunkSizeOfMap = 20;

            for (int chunkZ = 0; chunkZ < chunkSizeOfMap; chunkZ++)
            {
                for (int chunkX = 0; chunkX < chunkSizeOfMap; chunkX++)
                {
                    ChunkRef cr = null;
                    if (cm.ChunkExists(chunkX, chunkZ))
                    {
                        cr = cm.GetChunkRef(chunkX, chunkZ);
                    }
                    else
                    {
                        cr = cm.CreateChunk(chunkX, chunkZ);
                        ++createdChunkCount;
                    }
                    System.Diagnostics.Debug.Assert(cr != null);

                    // Turn off generating caves / ores / goodies
                    cr.IsTerrainPopulated = true;

                    // Auto-lighting is horrible when batch editing thousands of blocks
                    cr.Blocks.AutoLight = false;

                    for (int y = 0; y < 16; y++)
                    {
                        for (int x = 0; x < 16; x++)
                        {
                            int tileX = NormalizeBlockX((chunkX * 16) + x) / BlocksPerTile;
                            int blockWithinTileX = NormalizeBlockX((chunkX * 16) + x) % BlocksPerTile;
                            int tileY = ((chunkZ * 16) + y) / BlocksPerTile;
                            int blockWithinTileY = ((chunkZ * 16) + y) % BlocksPerTile;

                            // Because we're iterating by chunk, if the map is not perfectly chunk-aligned, there will
                            // be some spare blocks in the outer edge chunks that we don't want to touch.  Skip them here.
                            if (tileX < 0 || tileY > 127)
                                continue;

                            int blockElevation = GetBlockElevation(tileX, tileY, blockWithinTileX, blockWithinTileY);

                            // For slopes, fill in the gaps under the slope with stone since some corner slopes expose the
                            // bedrock underneath the side.
                            for (int h = BaseYLevel + TerrainHeightBlockMultiplier * altitude.AltitudeData[tileX, tileY].Altitude; h < blockElevation; h++)
                            {
                                cr.Blocks.SetID(x, h, y, (int)BlockType.STONE);
                            }

                            int airLevel = blockElevation + 1;

                            TerrainType terrainType = terrain.Terrain[tileX, tileY].Type;

                            switch (terrainType)
                            {
                                case TerrainType.Submerged:
                                case TerrainType.PartiallySubmerged:
                                    cr.Blocks.SetID(x, blockElevation + 1, y, BlockType.WATER);
                                    break;
                                case TerrainType.DryLand:
                                    cr.Blocks.SetID(x, blockElevation, y, BlockType.DIRT);
                                    cr.Blocks.SetID(x, blockElevation - 1, y, BlockType.DIRT);
                                    break;
                                case TerrainType.BayEast:
                                case TerrainType.BayNorth:
                                case TerrainType.BaySouth:
                                case TerrainType.BayWest:
                                case TerrainType.CanalEastWest:
                                case TerrainType.CanalNorthSouth:
                                case TerrainType.SurfaceWater:
                                    cr.Blocks.SetID(x, blockElevation, y, BlockType.WATER);
                                    cr.Blocks.SetID(x, blockElevation - 1, y, BlockType.WATER);
                                    cr.Blocks.SetID(x, blockElevation - 2, y, BlockType.STONE);
                                    break;
                            }
                            
                            if (altitude.AltitudeData[tileX, tileY].Altitude < seaLevel)
                            {
                                for (int h = airLevel; h <= BaseYLevel + (TerrainHeightBlockMultiplier * seaLevel); h++)
                                {
                                    cr.Blocks.SetID(x, h, y, (int)BlockType.WATER);
                                }

                                airLevel = BaseYLevel + (TerrainHeightBlockMultiplier * seaLevel) + 1;
                            }

                            for (int h = airLevel; h < 128; h++)
                            {
                                if (cr.Blocks.GetID(x, h, y) != (int)BlockType.AIR)
                                    cr.Blocks.SetID(x, h, y, (int)BlockType.AIR);
                            }

                            //
                            // Todo:
                            //  Iterate over the building height - need an accessor in BuildingSource
                            //  Support buildings being larger than 1x1 tile - probably also needs an accessor in BuildingSource and some
                            //  code here to handle the current tile not being the starting tile for the building.
                            //
                            int buildingStartHeight = blockElevation + buildingSource.GetGroundLevel(buildings.Buildings[tileX, tileY].Type);
                            int targetHeight = buildingSource.GetHeight(buildings.Buildings[tileX, tileY].Type);

                            // For buildings that span multiple tiles, figure out which tile we're currently on
                            int multiTileBuildingTileOffsetX = 0;
                            int multiTileBuildingTileOffsetY = 0;

                            if (buildings.IsMultiTileBuilding(tileX, tileY))
                                buildings.GetMultiTileBuildingOffset(tileX, tileY, out multiTileBuildingTileOffsetX, out multiTileBuildingTileOffsetY);

                            for (int i = 0; i < targetHeight; i++)
                            {
                                AlphaBlock bd = buildingSource.GetBuildingBlock(buildings.Buildings[tileX, tileY].Type, (Math.Max(buildings.BuildingSizeX(tileX, tileY) - multiTileBuildingTileOffsetX - 1, 0) * BlocksPerTile) + BlocksPerTile - blockWithinTileX - 1, i, (BlocksPerTile * multiTileBuildingTileOffsetY) + blockWithinTileY);
                                cr.Blocks.SetBlock(x, buildingStartHeight + i, y, bd);
                            }
                        }
                    }

                    //cr.Blocks.RebuildHeightMap();
                    //cr.Blocks.RebuildBlockLight();
                    //cr.Blocks.RebuildSkyLight();
                    cm.Save();
                }

                if (createdChunkCount > 0)
                {
                    cm.RelightDirtyChunks();
                    cm.Save();
                }
                
                Console.WriteLine("{0} / {1} chunks.", (chunkZ + 1) * chunkSizeOfMap, chunkSizeOfMap * chunkSizeOfMap);
            }

            cm.RelightDirtyChunks();
            cm.Save();

            Console.WriteLine("Done");
            Console.WriteLine("Created {0} chunks.", createdChunkCount);
        }

        // Because Sim City's coordinates go from top to bottom, a 0 x Sim City tile is actually
        // 127 in Minecraft, so we have to invert all block references on the x axis
        public int NormalizeBlockX(int blockX)
        {
            return (128 * BlocksPerTile - 1) - blockX;
        }

        /// <summary>
        /// A tile in Sim City is represented by multiple blocks in Minecraft.  In cases of slopes
        /// the elevation must change across the blocks that build the tile.  This method returns,
        /// for a given block in a tile, what elevation that block should have (Minecraft Y-level).
        /// </summary>
        /// <param name="tileX"></param>
        /// <param name="tileY"></param>
        /// <param name="blockOffsetX"></param>
        /// <param name="blockOffsetY"></param>
        /// <returns></returns>
        internal int GetBlockElevation(int tileX, int tileY, int blockOffsetX, int blockOffsetY)
        {
            TerrainDescriptor td = terrain.Terrain[tileX, tileY];
            
            // Certain tile types don't support slopes, so just return their elevation
            if (td.Type == TerrainType.BayEast || td.Type == TerrainType.BayNorth || td.Type == TerrainType.BaySouth || td.Type == TerrainType.BayWest || td.Type == TerrainType.CanalEastWest || td.Type == TerrainType.CanalNorthSouth || td.Type == TerrainType.SurfaceWater || td.Type == TerrainType.Waterfall)
                return BaseYLevel + (TerrainHeightBlockMultiplier * altitude.AltitudeData[tileX, tileY].Altitude);

            switch (td.Slope)
            {
                case TerrainSlope.West:
                    // The Y coordinate determines the height.
                    return BaseYLevel + (TerrainHeightBlockMultiplier * altitude.AltitudeData[tileX, tileY].Altitude) + LinearInterpolate(0, TerrainHeightBlockMultiplier, BlocksPerTile, blockOffsetY);
                case TerrainSlope.North:
                    return BaseYLevel + (TerrainHeightBlockMultiplier * altitude.AltitudeData[tileX, tileY].Altitude) + LinearInterpolate(0, TerrainHeightBlockMultiplier, BlocksPerTile, blockOffsetX);
                case TerrainSlope.East:
                    return BaseYLevel + (TerrainHeightBlockMultiplier * altitude.AltitudeData[tileX, tileY].Altitude) + (4 - LinearInterpolate(0, TerrainHeightBlockMultiplier, BlocksPerTile, blockOffsetY));
                case TerrainSlope.South:
                    return BaseYLevel + (TerrainHeightBlockMultiplier * altitude.AltitudeData[tileX, tileY].Altitude) + (4 - LinearInterpolate(0, TerrainHeightBlockMultiplier, BlocksPerTile, blockOffsetX));
                case TerrainSlope.HighFlat:
                    return BaseYLevel + (TerrainHeightBlockMultiplier * altitude.AltitudeData[tileX, tileY].Altitude);
                case TerrainSlope.NorthEast:
                    return BaseYLevel + (TerrainHeightBlockMultiplier * altitude.AltitudeData[tileX, tileY].Altitude) + DiagonalSlopeNorthEast(blockOffsetX, blockOffsetY);
                case TerrainSlope.NorthWest:
                    return BaseYLevel + (TerrainHeightBlockMultiplier * altitude.AltitudeData[tileX, tileY].Altitude) + DiagonalSlopeNorthWest(blockOffsetX, blockOffsetY);
                case TerrainSlope.SouthEast:
                    return BaseYLevel + (TerrainHeightBlockMultiplier * altitude.AltitudeData[tileX, tileY].Altitude) + DiagonalSlopeSouthEast(blockOffsetX, blockOffsetY);
                case TerrainSlope.SouthWest:
                    return BaseYLevel + (TerrainHeightBlockMultiplier * altitude.AltitudeData[tileX, tileY].Altitude) + DiagonalSlopeSouthWest(blockOffsetX, blockOffsetY);
                case TerrainSlope.NorthEastTerrace:
                    return BaseYLevel + (TerrainHeightBlockMultiplier * altitude.AltitudeData[tileX, tileY].Altitude) + DiagonalSlopeNorthEastTerrace(blockOffsetX, blockOffsetY);
                case TerrainSlope.NorthWestTerrace:
                    return BaseYLevel + (TerrainHeightBlockMultiplier * altitude.AltitudeData[tileX, tileY].Altitude) + DiagonalSlopeNorthWestTerrace(blockOffsetX, blockOffsetY);
                case TerrainSlope.SouthEastTerrace:
                    return BaseYLevel + (TerrainHeightBlockMultiplier * altitude.AltitudeData[tileX, tileY].Altitude) + DiagonalSlopeSouthEastTerrace(blockOffsetX, blockOffsetY);
                case TerrainSlope.SouthWestTerrace:
                    return BaseYLevel + (TerrainHeightBlockMultiplier * altitude.AltitudeData[tileX, tileY].Altitude) + DiagonalSlopeSouthWestTerrace(blockOffsetX, blockOffsetY);
            }

            return BaseYLevel + (TerrainHeightBlockMultiplier * altitude.AltitudeData[tileX, tileY].Altitude);
        }

        # region Terrain slope calculations
        internal int LinearInterpolate(int minVal, int maxVal, int interpolateLength, int currentVal)
        {
            return (int)((double)((maxVal - minVal) / (double)interpolateLength) * currentVal);
        }

        internal int DiagonalSlopeNorthEastTerrace(int offsetX, int offsetY)
        {
            if ((BlocksPerTile - 1 - offsetX) + offsetY >= BlocksPerTile)
                return 0;

            return TerrainHeightBlockMultiplier - 1 - (int)((offsetY + (BlocksPerTile - 1 - offsetX)) / (double)BlocksPerTile * (double)TerrainHeightBlockMultiplier);
        }

        internal int DiagonalSlopeNorthWestTerrace(int offsetX, int offsetY)
        {
            if ((BlocksPerTile - 1 - offsetX) + (BlocksPerTile - 1 - offsetY) >= BlocksPerTile)
                return 0;

            return TerrainHeightBlockMultiplier - 1 - (int)(((BlocksPerTile - 1 - offsetY) + (BlocksPerTile - 1 - offsetX)) / (double)BlocksPerTile * (double)TerrainHeightBlockMultiplier);
        }

        internal int DiagonalSlopeSouthEastTerrace(int offsetX, int offsetY)
        {
            if (offsetX + offsetY >= BlocksPerTile)
                return 0;

            return TerrainHeightBlockMultiplier - 1 - (int)((offsetY + offsetX) / (double)BlocksPerTile * (double)TerrainHeightBlockMultiplier);
        }

        internal int DiagonalSlopeSouthWestTerrace(int offsetX, int offsetY)
        {
            if (offsetX + (BlocksPerTile - 1 - offsetY) >= BlocksPerTile)
                return 0;

            return TerrainHeightBlockMultiplier - 1 - (int)(((BlocksPerTile - 1 - offsetY) + offsetX) / (double)BlocksPerTile * (double)TerrainHeightBlockMultiplier);
        }

        internal int DiagonalSlopeNorthEast(int offsetX, int offsetY)
        {
            if (offsetX + (BlocksPerTile - 1 - offsetY) >= BlocksPerTile)
                return TerrainHeightBlockMultiplier;

            return (int)(((BlocksPerTile - 1 - offsetY) + offsetX + 1) / (double)BlocksPerTile * (double)TerrainHeightBlockMultiplier);
        }

        internal int DiagonalSlopeNorthWest(int offsetX, int offsetY)
        {
            if (offsetX + offsetY >= BlocksPerTile)
                return TerrainHeightBlockMultiplier;

            return (int)((offsetY + offsetX + 1) / (double)BlocksPerTile * (double)TerrainHeightBlockMultiplier);
        }

        internal int DiagonalSlopeSouthEast(int offsetX, int offsetY)
        {
            if ((BlocksPerTile - 1 - offsetX) + (BlocksPerTile - 1 - offsetY) >= BlocksPerTile)
                return TerrainHeightBlockMultiplier;

            return (int)(((BlocksPerTile - 1 - offsetY) + (BlocksPerTile - 1 - offsetX) + 1) / (double)BlocksPerTile * (double)TerrainHeightBlockMultiplier);
        }

        internal int DiagonalSlopeSouthWest(int offsetX, int offsetY)
        {
            if ((BlocksPerTile - 1 - offsetX) + offsetY >= BlocksPerTile)
                return TerrainHeightBlockMultiplier;

            return (int)((offsetY + (BlocksPerTile - 1 - offsetX) + 1) / (double)BlocksPerTile * (double)TerrainHeightBlockMultiplier);
        }
        #endregion
    }
}
