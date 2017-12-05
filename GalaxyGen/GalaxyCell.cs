using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GalaxyGen
{
    public class GalaxyCell
    {
        public List<StarSystem> starSystems = new List<StarSystem>();
        public List<JumpGateLink> jumpGates = new List<JumpGateLink>();
        public int cellX { get; private set; }
        public int cellY { get; private set; }

        GalaxyGenerator generator;

        public GalaxyCell(GalaxyGenerator generator, int cellX, int cellY, bool fullGeneration = true)
        {
            this.generator = generator;
            this.cellX = cellX;
            this.cellY = cellY;

            Rand64 random = new Rand64((ulong)(cellY * GalaxyGenerator.galaxySize + cellX + GalaxyGenerator.galaxySeed));

            int numStars = random.Range(1, 10);

            for (int n = 0; n < numStars; n++)
            {
                StarSystemId id = new StarSystemId(cellX, cellY, n);
                StarSystem star = new StarSystem(id, random);
                starSystems.Add(star);
            }

            if (cellY > 0)
            {
                PickConnector(random, StarSystem.Attributes.NorthConnector);
            }
            if (cellX < GalaxyGenerator.galaxySize - 1)
            {
                PickConnector(random, StarSystem.Attributes.EastConnector);
            }
            if (cellY < GalaxyGenerator.galaxySize - 1)
            {
                PickConnector(random, StarSystem.Attributes.SouthConnector);
            }
            if (cellX > 0)
            {
                PickConnector(random, StarSystem.Attributes.WestConnector);
            }

            ConstrainPositions();

            if (fullGeneration)
            {
                if (cellY > 0)
                {
                    jumpGates.Add(new JumpGateLink(GetSystemWithAttribute(StarSystem.Attributes.NorthConnector).id,
                        GetCell(cellX, cellY - 1).GetSystemWithAttribute(StarSystem.Attributes.SouthConnector).id));
                }
                if (cellX < GalaxyGenerator.galaxySize - 1)
                {
                    jumpGates.Add(new JumpGateLink(GetSystemWithAttribute(StarSystem.Attributes.EastConnector).id,
                        GetCell(cellX + 1, cellY).GetSystemWithAttribute(StarSystem.Attributes.WestConnector).id));
                }
                if (cellY < GalaxyGenerator.galaxySize - 1)
                {
                    jumpGates.Add(new JumpGateLink(GetSystemWithAttribute(StarSystem.Attributes.SouthConnector).id,
                        GetCell(cellX, cellY + 1).GetSystemWithAttribute(StarSystem.Attributes.NorthConnector).id));
                }
                if (cellX > 0)
                {
                    jumpGates.Add(new JumpGateLink(GetSystemWithAttribute(StarSystem.Attributes.WestConnector).id,
                        GetCell(cellX - 1, cellY).GetSystemWithAttribute(StarSystem.Attributes.EastConnector).id));
                }

                if(!IsFullyConnected())
                {
                    TryConnectClosest();
                }

                int itCount = 0;
                while(!IsFullyConnected())
                {
                    bool avoidIntersections = itCount < 100;
                    TryAddJumpGate(starSystems[random.Range(0, starSystems.Count)].id, starSystems[random.Range(0, starSystems.Count)].id, avoidIntersections);
                    itCount++;
                }

                AssignRegions();

                for (int n = 0; n < starSystems.Count; n++)
                {
                    Region region = generator.GetRegion(starSystems[n].regionId);

                    if ((starSystems[n].attributes & StarSystem.Attributes.Capital) != 0)
                    {
                        starSystems[n].name = generator.GenerateCapitalName(region, random);
                    }
                    else
                    {
                        starSystems[n].name = generator.GenerateName(region.languageIndex, random);
                    }
                }
            }
        }

        void AssignRegions()
        {
            List<Region> regions = new List<Region>();

            int regionCellX = cellX / RegionCell.galaxyCellsPerRegion;
            int regionCellY = cellY / RegionCell.galaxyCellsPerRegion;

            for(int x = regionCellX - 1; x < regionCellX + 1; x++)
            {
                for(int y = regionCellY - 1; y < regionCellY + 1; y++)
                {
                    if (x >= 0 && y >= 0 && x < GalaxyGenerator.galaxySize / RegionCell.galaxyCellsPerRegion && y < GalaxyGenerator.galaxySize / RegionCell.galaxyCellsPerRegion)
                    {
                        RegionCell regionCell = generator.GetRegionCell(x, y);
                        if (regionCell != null)
                        {
                            regions.AddRange(regionCell.regions);
                        }
                    }
                }
            }

            for (int n = 0; n < starSystems.Count; n++)
            {
                StarSystem system = starSystems[n];
                Region closest = null;
                float closestDistanceSqr = 0.0f;

                for(int r = 0; r < regions.Count; r++)
                {
                    float locX, locY;
                    regions[r].GetRelativePosition(this, out locX, out locY);

                    float distSqr = (locX - system.x) * (locX - system.x) + (locY - system.y) * (locY - system.y);
                    if(closest == null || distSqr < closestDistanceSqr)
                    {
                        closest = regions[r];
                        closestDistanceSqr = distSqr;
                    }
                }
                starSystems[n].regionId = closest.id;
            }

            for (int n = 0; n < starSystems.Count; n++)
            {
                StarSystem system = starSystems[n];
                int numJumpGates = CountJumpGateLinks(system.id);

                if (numJumpGates == 1)
                {
                    for (int j = 0; j < jumpGates.Count; j++)
                    {
                        if (jumpGates[j].start == system.id)
                        {
                            system.regionId = GetStarSystem(jumpGates[j].end).regionId;
                            break;
                        }
                        else if (jumpGates[j].end == system.id)
                        {
                            system.regionId = GetStarSystem(jumpGates[j].start).regionId;
                            break;
                        }
                    }
                    system.attributes |= StarSystem.Attributes.Deadend;
                }
                else if(numJumpGates >= 4 && (system.attributes & StarSystem.Attributes.Capital) == 0)
                {
                    system.attributes |= StarSystem.Attributes.HubWorld;
                }                
            }

            // Assign capital
            RegionCell mainRegionCell = generator.GetRegionCell(regionCellX, regionCellY);
            for(int r = 0; r < mainRegionCell.regions.Count; r++)
            {
                Region region = mainRegionCell.regions[r];
                float locX, locY;
                region.GetRelativePosition(this, out locX, out locY);

                if(locX >= 0 && locY >= 0 && locX <= 1 && locY <= 1)
                {
                    StarSystem closest = null;
                    float closestDistanceSqr = 0.0f;

                    for (int n = 0; n < starSystems.Count; n++)
                    {
                        StarSystem system = starSystems[n];

                        if (system.regionId == region.id && CountJumpGateLinks(system.id) >= 3)
                        {
                            float distSqr = (locX - system.x) * (locX - system.x) + (locY - system.y) * (locY - system.y);

                            if (closest == null || distSqr < closestDistanceSqr)
                            {
                                closest = system;
                                closestDistanceSqr = distSqr;
                            }
                        }
                    }

                    if(closest != null)
                    {
                        closest.attributes |= StarSystem.Attributes.Capital;
                    }
                }
            }
        }

        void PickConnector(Rand64 random, StarSystem.Attributes connectorType)
        {
            const int iterations = 5;
            int pick = 0;

            for(int n = 0; n < iterations; n++)
            {
                pick = random.Range(0, starSystems.Count);
                if((starSystems[pick].attributes & (StarSystem.Attributes.NorthConnector | StarSystem.Attributes.EastConnector | StarSystem.Attributes.SouthConnector | StarSystem.Attributes.WestConnector)) == 0)
                {
                    break;
                }
            }
            starSystems[pick].attributes |= connectorType;
        }

        GalaxyCell GetCell(int x, int y)
        {
            if (x == cellX && y == cellY)
                return this;
            return new GalaxyCell(generator, x, y, false);
        }

        StarSystem GetStarSystem(StarSystemId id)
        {
            return GetCell(id.cellX, id.cellY).starSystems[id.systemIndex];
        }

        bool TryConnectClosest(StarSystem system, List<StarSystem> toConnect, bool avoidIntersections)
        {
            toConnect.Remove(system);
            toConnect.Sort((a, b) =>
            {
                float distA = (a.x - system.x) * (a.x - system.x) + (a.y - system.y) * (a.y - system.y);
                float distB = (b.x - system.x) * (b.x - system.x) + (b.y - system.y) * (b.y - system.y);
                return distA.CompareTo(distB);
            });

            for(int n = 0; n < toConnect.Count; n++)
            {
                if(TryAddJumpGate(system.id, toConnect[n].id, avoidIntersections))
                {
                    return true;
                }
            }

            return false;
        }

        void TryConnectClosest()
        {
            for (int n = 0; n < starSystems.Count; n++)
            {
                if(CountJumpGateLinks(starSystems[n].id) == 0)
                {
                    TryConnectClosest(starSystems[n], new List<StarSystem>(starSystems), true);
                }
            }

            StarSystemId firstConnected = jumpGates[0].start;
            HashSet<StarSystemId> connectedSystems = new HashSet<StarSystemId>();
            CheckConnections(connectedSystems, firstConnected);

            const int numIterations = 10;

            for (int it = 0; it < numIterations && connectedSystems.Count < starSystems.Count; it++)
            {
                for (int n = 0; n < starSystems.Count; n++)
                {
                    if (!connectedSystems.Contains(starSystems[n].id))
                    {
                        List<StarSystem> toConnect = new List<StarSystem>();
                        for (int i = 0; i < starSystems.Count; i++)
                        {
                            if (connectedSystems.Contains(starSystems[i].id))
                            {
                                toConnect.Add(starSystems[i]);
                            }
                        }

                        if (TryConnectClosest(starSystems[n], toConnect, it < numIterations / 2))
                        {
                            connectedSystems.Clear();
                            CheckConnections(connectedSystems, firstConnected);
                        }
                    }
                }
            }
        }


        int CountJumpGateLinks(StarSystemId system)
        {
            int count = 0;
            for (int n = 0; n < jumpGates.Count; n++)
            {
                if(jumpGates[n].start == system || jumpGates[n].end == system)
                {
                    count++;
                }
            }

            return count;
        }

        void CheckConnections(HashSet<StarSystemId> connectedSystems, StarSystemId checkSystem)
        {
            connectedSystems.Add(checkSystem);

            for(int n = 0; n < jumpGates.Count; n++)
            {
                JumpGateLink link = jumpGates[n];
                if(link.start.cellX == cellX && link.start.cellY == cellY
                    && link.end.cellX == cellX && link.end.cellY == cellY)
                {
                    if(link.start == checkSystem && !connectedSystems.Contains(link.end))
                    {
                        CheckConnections(connectedSystems, link.end);
                    }
                    else if(link.end == checkSystem && !connectedSystems.Contains(link.start))
                    {
                        CheckConnections(connectedSystems, link.start);
                    }
                }
            }
        }

        int GetConnectionCount()
        {
            HashSet<StarSystemId> connectedSystems = new HashSet<StarSystemId>();
            CheckConnections(connectedSystems, starSystems[0].id);
            return connectedSystems.Count;
        }

        bool IsFullyConnected()
        {
            return GetConnectionCount() == starSystems.Count;
        }

        bool DoIntersect(float x1, float y1, float x2, float y2, float x3, float y3, float x4, float y4)
        {
            float s1_x, s1_y, s2_x, s2_y;
            s1_x = x2 - x1; s1_y = y2 - y1;
            s2_x = x4 - x3; s2_y = y4 - y3;

            float s, t;
            s = (-s1_y * (x1 - x3) + s1_x * (y1 - y3)) / (-s2_x * s1_y + s1_x * s2_y);
            t = (s2_x * (y1 - y3) - s2_y * (x1 - x3)) / (-s2_x * s1_y + s1_x * s2_y);

            if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
            {
                return true;
            }

            return false;
        }

        bool TryAddJumpGate(StarSystemId a, StarSystemId b, bool avoidIntersections)
        {
            if (a == b)
                return false;

            // Check duplicates
            for (int n = 0; n < jumpGates.Count; n++)
            {
                if((jumpGates[n].start == a && jumpGates[n].end == b)
                    || (jumpGates[n].start == b && jumpGates[n].end == a))
                {
                    return false;
                }
            }

            bool intersected = false;

//            if (avoidIntersections)
            {
                float checkStartX, checkStartY;
                float checkEndX, checkEndY;

                GetStarSystem(a).GetRelativePosition(this, out checkStartX, out checkStartY);
                GetStarSystem(b).GetRelativePosition(this, out checkEndX, out checkEndY);

                for (int n = 0; n < jumpGates.Count; n++)
                {
                    if (jumpGates[n].start != a && jumpGates[n].start != b
                        && jumpGates[n].end != a && jumpGates[n].end != b)
                    {
                        float startX, startY;
                        float endX, endY;
                        GetStarSystem(jumpGates[n].start).GetRelativePosition(this, out startX, out startY);
                        GetStarSystem(jumpGates[n].end).GetRelativePosition(this, out endX, out endY);

                        if (DoIntersect(startX, startY, endX, endY, checkStartX, checkStartY, checkEndX, checkEndY))
                        {
                            if (avoidIntersections)
                            {
                                return false;
                            }
                            else
                            {
                                intersected = true;
                            }
                        }
                    }
                }
            }

//            int connectionCount = GetConnectionCount();

            JumpGateLink newLink = new JumpGateLink(a, b);
            newLink.didIntersect = intersected;

            jumpGates.Add(newLink);

            //if(GetConnectionCount() == connectionCount)
            //{
            //    jumpGates.RemoveAt(jumpGates.Count - 1);
            //}

            return true;
        }

        bool ApplyConstraint(StarSystem system, float x, float y, float distanceConstraint, bool constrainIn = true)
        {
            float distance = (float)Math.Sqrt((system.x - x) * (system.x - x) + (system.y - y) * (system.y - y));
            if ((!constrainIn && distance < distanceConstraint) || (constrainIn && distance > distanceConstraint))
            {
                float resolve = ((distanceConstraint / distance) - 1.0f);
                float midX = (system.x + x) / 2;
                float midY = (system.y + y) / 2;
                system.x += (system.x - midX) * resolve;
                system.y += (system.y - midY) * resolve;
                return true;
            }

            return false;
        }

        void ConstrainPositions()
        { 
            const int numIterations = 10;
            const float edgeConstraint = 0.1f;
            const float distanceConstraint = 0.2f;
            bool constrained = true;

            for(int it = 0; it < numIterations; it++)
            {
                constrained = false;

                for(int i = 0; i < starSystems.Count; i++)
                {
                    for(int j = 0; j < starSystems.Count; j++)
                    {
                        if(i != j && ApplyConstraint(starSystems[i], starSystems[j].x, starSystems[j].y, distanceConstraint, false))
                        {
                            constrained = true;
                        }
                    }

                    if((starSystems[i].attributes & StarSystem.Attributes.NorthConnector) != 0 && (starSystems[i].attributes & StarSystem.Attributes.SouthConnector) == 0)
                    {
                        ApplyConstraint(starSystems[i], 0.5f, 0.0f, 0.25f);
                    }
                    if ((starSystems[i].attributes & StarSystem.Attributes.EastConnector) != 0 && (starSystems[i].attributes & StarSystem.Attributes.WestConnector) == 0)
                    {
                        ApplyConstraint(starSystems[i], 1.0f, 0.5f, 0.25f);
                    }
                    if ((starSystems[i].attributes & StarSystem.Attributes.SouthConnector) != 0 && (starSystems[i].attributes & StarSystem.Attributes.NorthConnector) == 0)
                    {
                        ApplyConstraint(starSystems[i], 0.5f, 1.0f, 0.25f);
                    }
                    if ((starSystems[i].attributes & StarSystem.Attributes.WestConnector) != 0 && (starSystems[i].attributes & StarSystem.Attributes.EastConnector) == 0)
                    {
                        ApplyConstraint(starSystems[i], 0.0f, 0.5f, 0.25f);
                    }
                }

                for (int n = 0; n < starSystems.Count; n++)
                {
                    if(starSystems[n].x < edgeConstraint)
                    {
                        starSystems[n].x = edgeConstraint;
                    }
                    else if(starSystems[n].x > 1.0f - edgeConstraint)
                    {
                        starSystems[n].x = 1.0f - edgeConstraint;
                    }
                    if (starSystems[n].y < edgeConstraint)
                    {
                        starSystems[n].y = edgeConstraint;
                    }
                    else if (starSystems[n].y > 1.0f - edgeConstraint)
                    {
                        starSystems[n].y = 1.0f - edgeConstraint;
                    }
                }
            }
        }

        public StarSystem GetSystemWithAttribute(StarSystem.Attributes mask)
        {
            for(int n = 0; n < starSystems.Count; n++)
            {
                if((starSystems[n].attributes & mask) != 0)
                {
                    return starSystems[n];
                }
            }

            return null;
        }
    }
}
