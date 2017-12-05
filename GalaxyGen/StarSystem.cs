using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalaxyGen
{
    public struct JumpGateLink
    {
        public JumpGateLink(StarSystemId start, StarSystemId end)
        {
            this.start = start;
            this.end = end;
            didIntersect = false;
        }
        public StarSystemId start;
        public StarSystemId end;
        public bool didIntersect;
    }

    public struct StarSystemId
    {
        public StarSystemId(int cellX, int cellY, int systemIndex)
        {
            this.cellX = cellX;
            this.cellY = cellY;
            this.systemIndex = systemIndex;
        }

        public int cellX, cellY;
        public int systemIndex;

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(StarSystemId a, StarSystemId b)
        {
            return a.cellX == b.cellX && a.cellY == b.cellY && a.systemIndex == b.systemIndex;
        }

        public static bool operator !=(StarSystemId a, StarSystemId b)
        {
            return !(a == b);
        }
    }

    public class StarSystem
    {
        [FlagsAttribute]
        public enum Attributes : short
        {
            NorthConnector = 1,
            EastConnector = 2,
            SouthConnector = 4,
            WestConnector = 8,
            HubWorld = 16,
            Capital = 32,
            Deadend = 64
        };

        public string name;
        public float x, y;
        public Attributes attributes;
        public StarSystemId id;
        public RegionId regionId;

        public StarSystem(StarSystemId id, Rand64 random)
        {
            this.id = id;
            x = random.Range(0.0f, 1.0f);
            y = random.Range(0.0f, 1.0f);
        }

        public void GetRelativePosition(GalaxyCell relativeCell, out float outX, out float outY)
        {
            outX = (float)(id.cellX - relativeCell.cellX) + x;
            outY = (float)(id.cellY - relativeCell.cellY) + y;
        }
    }
}
