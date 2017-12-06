using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalaxyGen
{
    public class RegionCell
    {
        public const int galaxyCellsPerRegion = 4;

        public List<Region> regions = new List<Region>();
        public int cellX { get; private set; }
        public int cellY { get; private set; }

        GalaxyGenerator generator;
        public RegionCell(GalaxyGenerator generator, int cellX, int cellY)
        {
            this.generator = generator;
            this.cellX = cellX;
            this.cellY = cellY;

            Rand64 random = new Rand64((ulong)(cellY * GalaxyGenerator.galaxySize + cellX + GalaxyGenerator.galaxySeed + GalaxyGenerator.galaxySize * GalaxyGenerator.galaxySize));

            int numRegions = random.Range(5, 10);

            for (int n = 0; n < numRegions; n++)
            {
                RegionId id = new RegionId(cellX, cellY, n);
                Region region = new Region(id, random);
                regions.Add(region);
            }

            ConstrainPositions();

            NameRegions(random);
        }

        void NameRegions(Rand64 random)
        { 
            for(int n = 0; n < regions.Count; n++)
            {
                regions[n].baseName = generator.GenerateName(regions[n].languageIndex, random);
                regions[n].displayName = generator.GenerateRegionName(regions[n].baseName, random);
            }
        }

        bool ApplyConstraint(Region region, float x, float y, float distanceConstraint, bool constrainIn = true)
        {
            float distance = (float)Math.Sqrt((region.x - x) * (region.x - x) + (region.y - y) * (region.y - y));

            if(distance == 0.0f)
            {
                distance = 0.01f;
            }

            if ((!constrainIn && distance < distanceConstraint) || (constrainIn && distance > distanceConstraint))
            {
                float resolve = ((distanceConstraint / distance) - 1.0f);
                float midX = (region.x + x) / 2;
                float midY = (region.y + y) / 2;
                region.x += (region.x - midX) * resolve;
                region.y += (region.y - midY) * resolve;
                return true;
            }

            return false;
        }

        void ConstrainPositions()
        {
            const int numIterations = 10;
            const float edgeConstraint = 0.2f;
            const float distanceConstraint = 0.25f;

            for (int it = 0; it < numIterations; it++)
            {
                for (int i = 0; i < regions.Count; i++)
                {
                    for (int j = 0; j < regions.Count; j++)
                    {
                        if (i != j && ApplyConstraint(regions[i], regions[j].x, regions[j].y, distanceConstraint, false))
                        {
                        }
                    }
                }

                for (int n = 0; n < regions.Count; n++)
                {
                    if (regions[n].x < edgeConstraint)
                    {
                        regions[n].x = edgeConstraint;
                    }
                    else if (regions[n].x > 1.0f - edgeConstraint)
                    {
                        regions[n].x = 1.0f - edgeConstraint;
                    }
                    if (regions[n].y < edgeConstraint)
                    {
                        regions[n].y = edgeConstraint;
                    }
                    else if (regions[n].y > 1.0f - edgeConstraint)
                    {
                        regions[n].y = 1.0f - edgeConstraint;
                    }
                }
            }
        }


    }
}
