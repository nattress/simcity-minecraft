using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SimCity2000Parser;
using Substrate;
using Substrate.Core;
using Substrate.Nbt;

namespace SimCityToMinecraft
{
    class BuildingSource
    {
        AnvilWorld world;
        ModelData modelData;
        Dictionary<BuildingType, BuildingBlocks> Buildings;

        public BuildingSource(AnvilWorld inputWorld, ModelData md)
        {
            world = inputWorld;
            modelData = md;
            Buildings = new Dictionary<BuildingType, BuildingBlocks>();
        }

        public AlphaBlock GetBuildingBlock(BuildingType buildingType, int x, int y, int z)
        {
            BuildingBlocks bb = GetBuildingByType(buildingType);

            return bb.GetBlock(x, y, z);
        }

        public int GetGroundLevel(BuildingType buildingType)
        {
            BuildingBlocks bb = GetBuildingByType(buildingType);

            return bb.GroundLevelOffset;
        }

        public int GetHeight(BuildingType buildingType)
        {
            return GetBuildingByType(buildingType).Height;
        }

        private BuildingBlocks GetBuildingByType(BuildingType buildingType)
        {
            if (!Buildings.ContainsKey(buildingType))
            {
                Buildings.Add(buildingType, new BuildingBlocks(world, modelData, buildingType));
            }

            System.Diagnostics.Debug.Assert(Buildings.ContainsKey(buildingType));
            return Buildings[buildingType];
        }
    }

    struct IntVector3
    {
        public int X;
        public int Y;
        public int Z;
    }

    struct BlockData
    {
        public int Id { get; set; }
        public int Data { get; set; }
        public TileEntity Entity { get; set; }
        public AlphaBlock Ab { get; set; }
    }

    class BuildingBlocks
    {
        AlphaBlock[, ,] blocks;
        /// <summary>
        /// Specifies the level relative to ground level that the building is placed on.
        /// Negative values result in the bottom of the structure being below-ground.
        /// 0 results in the structure being placed at ground level.
        /// Positive values result in the structure being placed in the air.
        /// </summary>
        public int GroundLevelOffset { get; private set; }
        public int Height { get; private set; }

        public BuildingBlocks(AnvilWorld world, ModelData md, BuildingType buildingType)
        {
            BlockManager bm = world.GetBlockManager();
            ModelDataModel mdm = md.GetModelByTypeId((int)buildingType);

            if (mdm == null)
            {
                // This can happen until we have models for everything in the config file
                Height = 0;
                GroundLevelOffset = 0;
                return;
            }
            
            GroundLevelOffset = mdm.GroundLevelOffset;

            IntVector3 corner1 = new IntVector3();
            corner1.X = mdm.Corner1[0].x;
            corner1.Y = mdm.Corner1[0].y;
            corner1.Z = mdm.Corner1[0].z;

            IntVector3 corner2 = new IntVector3();
            corner2.X = mdm.Corner2[0].x;
            corner2.Y = mdm.Corner2[0].y;
            corner2.Z = mdm.Corner2[0].z;

            // Handle rotation
            /*
                * Block entities aren't drawing right.  We're flipping the Z-axis here which might be the cause..
                * 
            blocks = new AlphaBlock[Math.Abs(corner1.X - corner2.X) + 1, Math.Abs(corner1.Y - corner2.Y) + 1, Math.Abs(corner1.Z - corner2.Z) + 1];

            for (int x = corner1.X; x <= corner2.X; x++)
            {
                for (int y = corner1.Y; y <= corner2.Y; y++)
                {
                    for (int z = 0; z <= Math.Abs(corner2.Z - corner1.Z); z++)
                    {
                        blocks[x - corner1.X, y - corner1.Y, z] = bm.GetBlock(x, y, corner2.Z - z);
                    }
                }
            }
            */

            blocks = new AlphaBlock[Math.Abs(corner1.X - corner2.X) + 1, Math.Abs(corner1.Y - corner2.Y) + 1, Math.Abs(corner1.Z - corner2.Z) + 1];

            for (int x = corner1.X; x <= corner2.X; x++)
            {
                for (int y = corner1.Y; y <= corner2.Y; y++)
                {
                    for (int z = corner1.Z; z <= corner2.Z; z++)
                    {
                        blocks[x - corner1.X, y - corner1.Y, z - corner1.Z] = bm.GetBlock(x, y, z);
                    }
                }
            }

            Height = Math.Abs(corner1.Y - corner2.Y) + 1;
        }

        public AlphaBlock GetBlock(int x, int y, int z)
        {
            return blocks[x, y, z];
        }
   } 
}
