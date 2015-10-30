using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SimCity2000Parser
{
    public class ZoneSection : CompressedSaveSection
    {
        public ZoneDescriptor[,] Zone {get; private set; }
        internal MiscSection MiscSection { get; set; }

        internal ZoneSection() : base("XZON")
        {
            Zone = new ZoneDescriptor[128, 128];
        }

        internal override void ParseSection(System.IO.FileStream file)
        {
            base.ParseSection(file);

            List<ZoneDescriptor> zoneDescriptors = new List<ZoneDescriptor>();

            using (MemoryStream ms = new MemoryStream(RawData))
            {
                while (ms.Position < ms.Length)
                {
                    var zone = new ZoneDescriptor(ms, this);
                    zoneDescriptors.Add(zone);
                }
            }

            for (int i = 0; i < 128; i++)
            {
                for (int j = 0; j < 128; j++)
                {
                    Zone[i, 127 - j] = zoneDescriptors[i * 128 + j];
                }
            }

            //
            // For each zone we want to know where its origin tile is to make rendering
            // multi-tile buildings easier.  Iterate over all zone tiles finding "Top-Left"
            // tiles and then fill in all other tiles' origins that are part of that zone.
            //
            for (int i = 0; i < 128; i++)
            {
                for (int j = 0; j < 128; j++)
                {
                    if (Zone[i, j].IsTopLeft() &&
                        !Zone[i, j].IsBottomRight())
                    {
                        int originX = i;
                        int originY = j;

                        int bottomLeftX = -1;
                        int bottomRightY = -1;

                        // Find the top-right
                        for (bottomLeftX = i; bottomLeftX < 128; bottomLeftX++)
                        {
                            if (Zone[bottomLeftX, j].IsBottomLeft())
                            {
                                break;
                            }
                        }

                        // Find the bottom-right
                        for (bottomRightY = j; bottomRightY < 128; bottomRightY++)
                        {
                            if (Zone[bottomLeftX, bottomRightY].IsBottomRight())
                            {
                                break;
                            }
                        }

                        // Now fill in all tiles between
                        for (int i2 = i; i2 <= bottomLeftX; i2++)
                        {
                            for (int j2 = j; j2 <= bottomRightY; j2++)
                            {
                                Zone[i2, j2].IsMultiTile = true;
                                Zone[i2, j2].OriginX = originX;
                                Zone[i2, j2].OriginY = originY;
                                Zone[i2, j2].LengthX = bottomLeftX - originX + 1;
                                Zone[i2, j2].LengthY = bottomRightY - originY + 1;
                            }
                        }
                    }
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
                    ZoneDescriptor td = Zone[y, x];

                    sb.AppendFormat("[{0}, {1}]\t\tType: {2}\tCorners: {3}\n", x, y, td.ZoneType.ToString(), CornerInfoToString(td.CornerInfo));
                }
            }

            sb.AppendLine("");
            sb.AppendLine("Corner Map");

            for (int y = 0; y < 128; y++)
            {
                for (int x = 0; x < 128; x++)
                {
                    ZoneDescriptor td = Zone[y, x];

                    if (td.IsTopRight() && td.IsTopLeft() && td.IsBottomLeft() && td.IsBottomRight())
                    {
                        sb.Append("1 ");
                    }
                    else if (td.IsTopLeft())
                    {
                        sb.Append("A ");
                    }
                    else if (td.IsTopRight())
                    {
                        sb.Append("B ");
                    }
                    else if (td.IsBottomLeft())
                    {
                        sb.Append("C ");
                    }
                    else if (td.IsBottomRight())
                    {
                        sb.Append("D ");
                    }
                    else
                    {
                        sb.Append("  ");
                    }
                }
                sb.AppendLine("");
            }
            return sb.ToString();
        }

        public string CornerInfoToString(ZoneCorners corners)
        {
            StringBuilder sb = new StringBuilder();

            if ((corners & ZoneCorners.TopLeft) != 0)
            {
                sb.Append(" TopLeft");
            }

            if ((corners & ZoneCorners.TopRight) != 0)
            {
                sb.Append(" TopRight");
            }

            if ((corners & ZoneCorners.BottomLeft) != 0)
            {
                sb.Append(" BottomLeft");
            }

            if ((corners & ZoneCorners.BottomRight) != 0)
            {
                sb.Append(" BottomRight");
            }

            return sb.ToString();
        }
    }

    public class ZoneDescriptor
    {
        public ZoneType ZoneType { get; private set; }
        public ZoneCorners CornerInfo { get; private set; }
        public int OriginX { get; set; }
        public int OriginY { get; set; }
        public bool IsMultiTile { get; set; }
        public int LengthX { get; set; }
        public int LengthY { get; set; }

        public ZoneDescriptor(Stream s, ZoneSection zs)
        {
            byte data = (byte)s.ReadByte();

            if ((data & 0x80) != 0)
            {
                CornerInfo |= TransposeCorner(zs.MiscSection, ZoneCorners.TopLeft);
            }

            if ((data & 0x40) != 0)
            {
                CornerInfo |= TransposeCorner(zs.MiscSection, ZoneCorners.BottomLeft);
            }

            if ((data & 0x20) != 0)
            {
                CornerInfo |= TransposeCorner(zs.MiscSection, ZoneCorners.BottomRight);
            }

            if ((data & 0x10) != 0)
            {
                CornerInfo |= TransposeCorner(zs.MiscSection, ZoneCorners.TopRight);
            }

            ZoneType = (ZoneType)(data & 0xF);
            IsMultiTile = false;
        }

        private ZoneCorners TransposeCorner(MiscSection ms, ZoneCorners corner)
        {
            if (ms.Rotation == 1)
            {
                switch (corner)
                {
                    case ZoneCorners.BottomLeft:
                        return ZoneCorners.BottomRight;
                    case ZoneCorners.BottomRight:
                        return ZoneCorners.TopRight;
                    case ZoneCorners.TopLeft:
                        return ZoneCorners.BottomLeft;
                    case ZoneCorners.TopRight:
                        return ZoneCorners.TopLeft;
                }
            }
            else if (ms.Rotation == 2)
            {
                switch (corner)
                {
                    case ZoneCorners.BottomLeft:
                        return ZoneCorners.TopRight;
                    case ZoneCorners.BottomRight:
                        return ZoneCorners.TopLeft;
                    case ZoneCorners.TopLeft:
                        return ZoneCorners.BottomRight;
                    case ZoneCorners.TopRight:
                        return ZoneCorners.BottomLeft;
                }
            }
            else if (ms.Rotation == 3)
            {
                switch (corner)
                {
                    case ZoneCorners.BottomLeft:
                        return ZoneCorners.TopLeft;
                    case ZoneCorners.BottomRight:
                        return ZoneCorners.BottomLeft;
                    case ZoneCorners.TopLeft:
                        return ZoneCorners.TopRight;
                    case ZoneCorners.TopRight:
                        return ZoneCorners.BottomRight;
                }
            }

            return corner;
            
        }

        public bool IsTopLeft()
        {
            return (CornerInfo & ZoneCorners.TopLeft) != 0;
        }

        public bool IsTopRight()
        {
            return (CornerInfo & ZoneCorners.TopRight) != 0;
        }

        public bool IsBottomLeft()
        {
            return (CornerInfo & ZoneCorners.BottomLeft) != 0;
        }

        public bool IsBottomRight()
        {
            return (CornerInfo & ZoneCorners.BottomRight) != 0;
        }
    }

    public enum ZoneType : int
    {
        None,
        LightResidential,
        DenseResidential,
        LightCommercial,
        DenseCommercial,
        LightIndustrial,
        DenseIndustrial,
        MilitaryBase,
        Airport,
        Seaport
    }

    [Flags]
    public enum ZoneCorners : int
    {
        TopLeft = 1,
        BottomLeft = 2,
        BottomRight = 4,
        TopRight = 8
    }
}
