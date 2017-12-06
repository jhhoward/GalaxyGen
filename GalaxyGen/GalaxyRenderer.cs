using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace GalaxyGen
{
    public class GalaxyRenderer
    {
        public StarSystemId selectedSystem;

        public float zoom = 1.0f;
        public int scrollX = 200;
        public int scrollY = 200;
        public int cellSize
        {
            get
            {
                return (int) (200 * zoom);
            }
        }


        public GalaxyGenerator galaxyGenerator;
        public GalaxyRenderer()
        {
            galaxyGenerator = new GalaxyGenerator();
        }

        public void Render(Graphics gfx)
        {
            gfx.Clear(Color.Black);
//            RenderGrid(gfx);

            int cellLeft = scrollX / cellSize;
            int cellRight = (int)(scrollX + gfx.VisibleClipBounds.Width) / cellSize;
            int cellTop = scrollY / cellSize;
            int cellBottom = (int)(scrollY + gfx.VisibleClipBounds.Height) / cellSize;

            for(int cellY = cellTop; cellY <= cellBottom; cellY++)
            {
                for(int cellX = cellLeft; cellX <= cellRight; cellX++)
                {
                    RenderCell(gfx, cellX, cellY);
                }
            }

            /*
            for(int regY = cellTop / RegionCell.galaxyCellsPerRegion; regY <= cellBottom / RegionCell.galaxyCellsPerRegion; regY++)
            {
                for(int regX = cellLeft / RegionCell.galaxyCellsPerRegion; regX <= cellRight / RegionCell.galaxyCellsPerRegion; regX++)
                {
                    RegionCell regionCell = galaxyGenerator.GetRegionCell(regX, regY);

                    if (regionCell != null)
                    {
                        foreach (Region region in regionCell.regions)
                        {
                            float x = (regX + region.x) * cellSize * RegionCell.galaxyCellsPerRegion - scrollX;
                            float y = (regY + region.y) * cellSize * RegionCell.galaxyCellsPerRegion - scrollY;

                            gfx.DrawEllipse(Pens.Red, x - 10, y - 10, 20, 20);
                        }
                    }
                }
            }*/
        }

        void RenderCell(Graphics gfx, int cellX, int cellY)
        {
            Pen pen = new Pen(Color.White, 1.0f);

            GalaxyCell cell = galaxyGenerator.GetCell(cellX, cellY);
            if (cell == null)
                return;

            int startX = (int)cellX * cellSize - scrollX;
            int startY = (int)cellY * cellSize - scrollY;

            foreach(JumpGateLink jump in cell.jumpGates)
            {
                DrawJump(gfx, jump);
            }

            foreach(StarSystem star in cell.starSystems)
            {
                float x = startX + star.x * cellSize;
                float y = startY + star.y * cellSize;

                Region region = galaxyGenerator.GetRegion(star.regionId);

                Brush systemColor = Brushes.White;

                switch (region.colorIndex % 23)
                {
                    case 0: systemColor = Brushes.Red; break;
                    case 1: systemColor = Brushes.Gold; break;
                    case 2: systemColor = Brushes.Blue; break;
                    case 3: systemColor = Brushes.Yellow; break;
                    case 4: systemColor = Brushes.Orange; break;
                    case 5: systemColor = Brushes.Magenta; break;
                    case 6: systemColor = Brushes.Cyan; break;
                    case 7: systemColor = Brushes.LightCyan; break;
                    case 8: systemColor = Brushes.LightPink; break;
                    case 9: systemColor = Brushes.LightGreen; break;
                    case 10: systemColor = Brushes.LightBlue; break;
                    case 11: systemColor = Brushes.DarkCyan; break;
                    case 12: systemColor = Brushes.DarkBlue; break;
                    case 13: systemColor = Brushes.DarkRed; break;
                    case 14: systemColor = Brushes.IndianRed; break;
                    case 15: systemColor = Brushes.DarkOrange; break;
                    case 16: systemColor = Brushes.DarkSalmon; break;
                    case 17: systemColor = Brushes.DarkSlateBlue; break;
                    case 18: systemColor = Brushes.DarkTurquoise; break;
                    case 19: systemColor = Brushes.DarkViolet; break;
                    case 20: systemColor = Brushes.DeepPink; break;
                    case 21: systemColor = Brushes.DeepSkyBlue; break;
                    case 22: systemColor = Brushes.DodgerBlue; break;
                }

                if ((star.attributes & StarSystem.Attributes.Capital) != 0)
                {
                    gfx.FillRectangle(Brushes.White, x - 4, y - 4, 8, 8);
                    gfx.FillRectangle(systemColor, x - 3, y - 3, 6, 6);
                }
                else
                {
                    gfx.FillRectangle(systemColor, x - 1, y - 1, 3, 3);
                }

                if(star.id == selectedSystem)
                {
                    gfx.DrawRectangle(Pens.White, x - 10, y - 10, 20, 20);
                }

                if (zoom > 0.6f)
                {
                    gfx.DrawString(star.name, Label.DefaultFont, Brushes.White, x + 5, y - 5);
                }
                
                //if((star.attributes & StarSystem.Attributes.NorthConnector) != 0)
                //{
                //    DrawJump(gfx, star, galaxyGenerator.GetCell(cellX, cellY - 1).GetSystemWithAttribute(StarSystem.Attributes.SouthConnector));
                //}
                //if ((star.attributes & StarSystem.Attributes.EastConnector) != 0)
                //{
                //    DrawJump(gfx, star, galaxyGenerator.GetCell(cellX + 1, cellY).GetSystemWithAttribute(StarSystem.Attributes.WestConnector));
                //}
                //if ((star.attributes & StarSystem.Attributes.SouthConnector) != 0)
                //{
                //    DrawJump(gfx, star, galaxyGenerator.GetCell(cellX, cellY - 1).GetSystemWithAttribute(StarSystem.Attributes.NorthConnector));
                //}
                //if ((star.attributes & StarSystem.Attributes.WestConnector) != 0)
                //{
                //    DrawJump(gfx, star, galaxyGenerator.GetCell(cellX, cellY - 1).GetSystemWithAttribute(StarSystem.Attributes.EastConnector));
                //}
            }
        }

        void DrawJump(Graphics gfx, JumpGateLink jump)
        {
            StarSystem a = galaxyGenerator.GetStarSystem(jump.start), b = galaxyGenerator.GetStarSystem(jump.end);

            if (a != null && b != null)
            {
                if(a.id.cellX != b.id.cellX && a.id.cellY != b.id.cellY)
                {
                    throw new Exception();
                }

                if (a.x < 0 || a.y < 0 || a.x > 1 || a.y > 1)
                    throw new Exception();
                if (b.x < 0 || b.y < 0 || b.x > 1 || b.y > 1)
                    throw new Exception();

                float x1 = (((int)a.id.cellX * cellSize) - scrollX) + (a.x * cellSize);
                float y1 = (((int)a.id.cellY * cellSize) - scrollY) + (a.y * cellSize);
                float x2 = (((int)b.id.cellX * cellSize) - scrollX) + (b.x * cellSize);
                float y2 = (((int)b.id.cellY * cellSize) - scrollY) + (b.y * cellSize);

                if(Math.Abs(x1 - x2) > cellSize * 2)
                {
                    throw new Exception();
                }
                if (Math.Abs(y1 - y2) > cellSize * 2)
                {
                    throw new Exception();
                }

                if(jump.didIntersect)
                {
                    gfx.DrawLine(new Pen(Brushes.Red), x1, y1, x2, y2);
                }
                else if (Math.Abs((int)a.id.cellX - (int)b.id.cellX) + Math.Abs((int)a.id.cellY - (int)b.id.cellY) != 1)
                {
                    gfx.DrawLine(new Pen(Brushes.Green), x1, y1, x2, y2);
                }
                else
                {
                    gfx.DrawLine(new Pen(Brushes.DarkOliveGreen), x1, y1, x2, y2);
                }
            }
        }

        void RenderGrid(Graphics gfx)
        {
            Pen pen = new Pen(Color.DarkGray, 1.0f);

            float x = cellSize - (scrollX % cellSize);
            while (x < gfx.VisibleClipBounds.Width)
            {
                gfx.DrawLine(pen, x, 0, x, gfx.VisibleClipBounds.Height);
                x += cellSize;
            }
            float y = cellSize - (scrollY % cellSize);
            while (y < gfx.VisibleClipBounds.Height)
            {
                gfx.DrawLine(pen, 0, y, gfx.VisibleClipBounds.Width, y);
                y += cellSize;
            }
        }
    }
}
