using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalaxyGen
{
    public class GalaxyGenerator
    {
        public const int galaxySeed = 1;
        public const int galaxySize = 256;
        const int maxCacheSize = 1000;
        List<GalaxyCell> cachedCells = new List<GalaxyCell>();
        List<RegionCell> cachedRegionCells = new List<RegionCell>();

        List<NameGenerator> nameGenerators = new List<NameGenerator>();

        string[] capitalNamingRules =
        {
            "{0} Capital",
            "{0} City",
            "City of {0}",
            "{0} Central",
            "{0} Alpha",
            "Alpha {0}",
            "{0} Prime",
            "{0} Town",
        };


        public GalaxyGenerator()
        {
            nameGenerators.Add(new MarkovNameGenerator("english_towns.txt"));
            nameGenerators.Add(new MarkovNameGenerator("roman_place_names.txt"));
            nameGenerators.Add(new MarkovNameGenerator("german_towns.txt"));
            nameGenerators.Add(new MarkovNameGenerator("egyptian_deities.txt"));
            nameGenerators.Add(new MarkovNameGenerator("dutch_cities.txt"));
            nameGenerators.Add(new MarkovNameGenerator("american_cities.txt"));
        }


        public GalaxyCell GetCell(int x, int y)
        {
            if (x < 0 || y < 0 || x >= galaxySize || y >= galaxySize)
                return null;

            for(int n = 0; n < cachedCells.Count; n++)
            {
                if(cachedCells[n].cellX == x && cachedCells[n].cellY == y)
                {
                    GalaxyCell result = cachedCells[n];
                    cachedCells.RemoveAt(n);
                    cachedCells.Add(result);
                    return result;
                }
            }

            GalaxyCell newCell = new GalaxyCell(this, x, y);
            cachedCells.Add(newCell);
            if(cachedCells.Count > maxCacheSize)
            {
                cachedCells.RemoveAt(0);
            }

            return newCell;
        }

        public StarSystem GetStarSystem(StarSystemId id)
        {
            GalaxyCell cell = GetCell(id.cellX, id.cellY);

            if (cell != null)
            {
                return cell.starSystems[id.systemIndex];
            }
            return null;
        }

        public RegionCell GetRegionCell(int x, int y)
        {
            if (x < 0 || y < 0 || x >= galaxySize / RegionCell.galaxyCellsPerRegion || y >= galaxySize / RegionCell.galaxyCellsPerRegion)
                return null;

            for (int n = 0; n < cachedRegionCells.Count; n++)
            {
                if (cachedRegionCells[n].cellX == x && cachedRegionCells[n].cellY == y)
                {
                    RegionCell result = cachedRegionCells[n];
                    cachedRegionCells.RemoveAt(n);
                    cachedRegionCells.Add(result);
                    return result;
                }
            }

            RegionCell newCell = new RegionCell(this, x, y);
            cachedRegionCells.Add(newCell);
            if (cachedRegionCells.Count > maxCacheSize)
            {
                cachedRegionCells.RemoveAt(0);
            }

            return newCell;
        }
        public Region GetRegion(RegionId id)
        {
            RegionCell cell = GetRegionCell(id.cellX, id.cellY);
            return cell.regions[id.index];
        }

        public string GenerateName(int genType, Rand64 random)
        {
            return nameGenerators[genType % nameGenerators.Count].Generate(random);
        }

        public string GenerateCapitalName(Region region, Rand64 random)
        {
            string baseName = region.name;

            if(random.Range(0.0f, 1.0f) < 0.5f)
            {
                baseName = GenerateName(region.languageIndex, random);
            }
            if(random.Range(0.0f, 1.0f) < 1.0f)
            {
                string nameFormat = capitalNamingRules[random.Range(0, capitalNamingRules.Length)];
                return string.Format(nameFormat, baseName);
            }
            return baseName;
        }
    }
}
