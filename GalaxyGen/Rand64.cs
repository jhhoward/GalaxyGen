using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalaxyGen
{
    public class Rand64
    {
        public Rand64(UInt64 seed)
        {
            current = seed + 1;
        }

        public Rand64(UInt32 x, UInt32 y)
        {
            current = 1 | (((UInt64)x) << 32) | (((UInt64)y) << 1);
        }

        public UInt64 Next()
        {
            UInt64 x = current;

            x ^= x >> 12; // a
            x ^= x << 25; // b
            x ^= x >> 27; // c
            current = x;
            return x * 0x2545F4914F6CDD1D;
        }

        public float Range(float min, float max)
        {
            float rangeSize = max - min;
            return min + ((float) Next()) / ((float)UInt64.MaxValue / rangeSize);
        }

        public int Range(int min, int max)
        {
            int rangeSize = max - min;
            return min + (int)(Next() / (UInt64.MaxValue / (UInt64)rangeSize));
        }

        UInt64 current;
    }
}
