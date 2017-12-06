using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GalaxyGen
{
    public partial class Form1 : Form
    {
        GalaxyGenerator galaxyGenerator
        {
            get { return renderer.galaxyGenerator; }
        }


        int dragX = 0, dragY = 0;
        bool draggingView = false;
        public Form1()
        {
            InitializeComponent();

            MainPictureBox.Paint += MainPictureBox_Paint;

            MainPictureBox.Refresh();

            hScrollBar1.Maximum = (GalaxyGenerator.galaxySize * renderer.cellSize);
            vScrollBar1.Maximum = (GalaxyGenerator.galaxySize * renderer.cellSize);

            vScrollBar1.Scroll += (sender, e) => { MainPictureBox.Invalidate(); };
            hScrollBar1.Scroll += (sender, e) => { MainPictureBox.Invalidate(); };

            KeyPress += Form1_KeyPress;

            MainPictureBox.MouseClick += View_MouseClick;
            MainPictureBox.MouseDown += View_MouseDown;
            MainPictureBox.MouseUp += View_MouseUp;
            MainPictureBox.MouseMove += View_MouseMove;
            MainPictureBox.MouseWheel += View_MouseWheel;


            Rand64 rand = new Rand64((ulong)new Random().Next());
            int cellX = rand.Range(0, GalaxyGenerator.galaxySize);
            int cellY = rand.Range(0, GalaxyGenerator.galaxySize);
            GalaxyCell cell = galaxyGenerator.GetCell(cellX, cellY);
            if(cell != null)
            {
                StarSystem chosenSystem = cell.starSystems[rand.Range(0, cell.starSystems.Count)];
                SelectSystem(chosenSystem);
                CenterOnSystem(chosenSystem);
            }
        }

        private void View_MouseWheel(object sender, MouseEventArgs e)
        {
            if(e.Delta > 0)
            {
                AdjustZoom(1.25f);
            }
            else if(e.Delta < 0)
            {
                AdjustZoom(1.0f / 1.25f);
            }
            ((HandledMouseEventArgs)(e)).Handled = true;
        }

        private void View_MouseUp(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
            {
                draggingView = false;
            }
        }

        private void View_MouseDown(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
            {
                draggingView = true;
                dragX = e.X;
                dragY = e.Y;
            }
        }

        private void View_MouseClick(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                float pickX = (float)(e.X + renderer.scrollX) / renderer.cellSize;
                float pickY = (float)(e.Y + renderer.scrollY) / renderer.cellSize;

                GalaxyCell cell = galaxyGenerator.GetCell((int)pickX, (int)pickY);

                if (cell != null)
                {
                    pickX -= (int)pickX;
                    pickY -= (int)pickY;

                    foreach (StarSystem system in cell.starSystems)
                    {
                        if (Math.Abs(system.x - pickX) < 0.05f && Math.Abs(system.y - pickY) < 0.05f)
                        {
                            SelectSystem(system);
                            break;
                        }
                    }
                }
            }
        }

        void CenterOnSystem(StarSystem system)
        {
            int newX = (int)(((system.id.cellX + system.x) * renderer.cellSize) - MainPictureBox.Width / 2);
            int newY = (int)(((system.id.cellY + system.y) * renderer.cellSize) - MainPictureBox.Height / 2);

            hScrollBar1.Value = Math.Max(0, Math.Min(newX, hScrollBar1.Maximum));
            vScrollBar1.Value = Math.Max(0, Math.Min(newY, vScrollBar1.Maximum));
        }

        void SelectSystem(StarSystem system)
        {
            renderer.selectedSystem = system.id;

            Region region = galaxyGenerator.GetRegion(system.regionId);

            string description = "";

            description += "Selected system:" + Environment.NewLine;
            description += system.name + Environment.NewLine;
            description += "Allegiance:" + Environment.NewLine;
            description += region.displayName + Environment.NewLine;

            textBox1.Text = description;

            Refresh();
        }

        private void View_MouseMove(object sender, MouseEventArgs e)
        {
            if(draggingView)
            {
                hScrollBar1.Value = Math.Max(0, Math.Min(hScrollBar1.Maximum, hScrollBar1.Value - (e.X - dragX)));
                vScrollBar1.Value = Math.Max(0, Math.Min(hScrollBar1.Maximum, vScrollBar1.Value - (e.Y - dragY)));
                dragX = e.X;
                dragY = e.Y;
            }
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar == '=')
            {
                AdjustZoom(1.25f);
            }
            else if(e.KeyChar == '-')
            {
                AdjustZoom(1.0f / 1.25f);
            }

        }

        void AdjustZoom(float multiplier)
        {
            float halfWidth = MainPictureBox.Width / 2;
            float halfHeight = MainPictureBox.Height / 2;
            float centerX = (halfWidth + renderer.scrollX) / renderer.cellSize;
            float centerY = (halfHeight + renderer.scrollY) / renderer.cellSize;

            renderer.zoom *= multiplier;

            hScrollBar1.Maximum = (GalaxyGenerator.galaxySize * renderer.cellSize);
            vScrollBar1.Maximum = (GalaxyGenerator.galaxySize * renderer.cellSize);

            float newX = (centerX * renderer.cellSize) - halfWidth;
            float newY = (centerY * renderer.cellSize) - halfHeight;

            hScrollBar1.Value = Math.Min(hScrollBar1.Maximum, Math.Max(0, (int)newX));
            vScrollBar1.Value = Math.Min(vScrollBar1.Maximum, Math.Max(0, (int)newY));
        }

        private void MainPictureBox_Paint(object sender, PaintEventArgs e)
        {
            Bitmap bmp = new Bitmap(MainPictureBox.Width, MainPictureBox.Height);

            MainPictureBox.Image = bmp;

            using (Graphics gfx = Graphics.FromImage(MainPictureBox.Image))
            {
                //                renderer.scrollX = (hScrollBar1.Value * renderer.cellSize * GalaxyGenerator.galaxySize) / hScrollBar1.Maximum;
                //                renderer.scrollY = (vScrollBar1.Value * renderer.cellSize * GalaxyGenerator.galaxySize) / vScrollBar1.Maximum;
                int scale = 1;// renderer.cellSize;
                renderer.scrollX = hScrollBar1.Value * scale;
                renderer.scrollY = vScrollBar1.Value * scale;
                hScrollBar1.Maximum = (GalaxyGenerator.galaxySize * renderer.cellSize) / scale;
                vScrollBar1.Maximum = (GalaxyGenerator.galaxySize * renderer.cellSize) / scale;
                renderer.Render(gfx);
            }
        }

        GalaxyRenderer renderer = new GalaxyRenderer();

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            
        }

    }
}
