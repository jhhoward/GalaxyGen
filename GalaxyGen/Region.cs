using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalaxyGen
{
    public struct RegionId
    {
        public RegionId(int cellX, int cellY, int index)
        {
            this.cellX = cellX;
            this.cellY = cellY;
            this.index = index;
        }
        public int cellX, cellY;
        public int index;

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(RegionId a, RegionId b)
        {
            return a.cellX == b.cellX && a.cellY == b.cellY && a.index == b.index;
        }

        public static bool operator !=(RegionId a, RegionId b)
        {
            return !(a == b);
        }
    }

    public class Region
    {
        public enum Type
        {
            Empire,
            Kingdom,
            Federation,
            House,
            Union,
            Corporation,
            Republic,
            Family,
        }

        public string name;
        public RegionId id { get; private set; }
        public int colorIndex { get; private set; }
        public int languageIndex { get; private set; }
        public float x, y;

        public Region(RegionId id, Rand64 random)
        {
            this.id = id;
            colorIndex = random.Range(0, 1000);
            languageIndex = random.Range(0, 1000);
            x = random.Range(0.0f, 1.0f);
            y = random.Range(0.0f, 1.0f);
        }

        public void GetRelativePosition(GalaxyCell relativeCell, out float outX, out float outY)
        {
            outX = (float)(id.cellX * RegionCell.galaxyCellsPerRegion - relativeCell.cellX) + x * RegionCell.galaxyCellsPerRegion;
            outY = (float)(id.cellY * RegionCell.galaxyCellsPerRegion - relativeCell.cellY) + y * RegionCell.galaxyCellsPerRegion;
        }
    }
}
