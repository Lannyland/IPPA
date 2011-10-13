using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using rtwmatrix;

namespace IPPA
{
    public partial class frmMap : Form
    {
        #region Members

        private Bitmap Map;
        private Bitmap DisplayMap;
        private Point Start = new Point();
        private Point End = new Point();
        private MyPictureBox pbMap = new MyPictureBox();
        private frmTestModule TM;
        private bool blnCanClickSet = false;

        #endregion

        #region Constructor, Destructor

        public frmMap()
        {
            InitializeComponent();
        }

        public frmMap(frmTestModule _TM)
        {
            InitializeComponent();
            TM = _TM;
            blnCanClickSet = true;
        }

        #endregion

        #region Message Handlers

        // Load things when form loads
        private void frmMap_Load(object sender, EventArgs e)
        {
            Size PBSize = new Size();
            PBSize.Height = 800;
            PBSize.Width = 800;
            pbMap.Size = PBSize;
            pbMap.Location = new Point(2, 16);
            pbMap.SizeMode = PictureBoxSizeMode.StretchImage;
            this.Controls.Add(pbMap);
            pbMap.Click += new EventHandler(this.pbMap_Click);
            pbMap.MouseDown += new MouseEventHandler(this.pbMap_MouseDown);
        }

        // Allow user to set start end points by clicking
        private void pbMap_MouseDown(object sender, MouseEventArgs e)
        {
            if (blnCanClickSet)
            {
                Color c = new Color();
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    // Left click for starting point in Yellow
                    c = Color.FromArgb(255, 255, 0);
                    Start.X = Convert.ToInt32(Math.Round((decimal)e.X / pbMap.Width * DisplayMap.Width - 0.5m));
                    Start.Y = Convert.ToInt32(Math.Round((decimal)e.Y / pbMap.Height * DisplayMap.Height - 0.5m));                    
                }
                else if (e.Button == System.Windows.Forms.MouseButtons.Right)
                {
                    // Right click for starting point in Blue
                    c = Color.FromArgb(0, 0, 255);
                    End.X = Convert.ToInt32(Math.Round((decimal)e.X / pbMap.Width * DisplayMap.Width - 0.5m));
                    End.Y = Convert.ToInt32(Math.Round((decimal)e.Y / pbMap.Height * DisplayMap.Height - 0.5m));
                }
                // Drawing the points
                DrawingStartEndPoints();
                // Set numbers on TestModule form
                TM.SetStartEndPoints(Start, End);
            }
        }

        private void pbMap_Click(object sender, EventArgs e)        
        {
            
        }

        #endregion

        #region Other Functions

        // Function for image to be set from outside the form
        public void setImage(Bitmap BMP)
        {
            //Control.CheckForIllegalCrossThreadCalls= false;
            Map = (Bitmap)BMP.Clone();
            DisplayMap = (Bitmap)BMP.Clone();
            
            
            pbMap.Image = DisplayMap;
            //Control.CheckForIllegalCrossThreadCalls = true;
        }

        public void resetImage()
        {
            DisplayMap = (Bitmap)Map.Clone();
            pbMap.Image = DisplayMap;
        }

        public void setPoint(bool start, Point p)
        {
            if (start)
            {
                Start.X = p.X;
                Start.Y = p.Y;
            }
            else
            {
                End.X = p.X;
                End.Y = p.Y;
            }
        }

        public void DrawingStartEndPoints()
        {
            resetImage();
            Color c = new Color();
            
            // Yellow for starting point
            c = Color.FromArgb(255, 255, 0);
            DisplayMap.SetPixel(Start.X, Start.Y, c);

            // Blue for ending point 
            c = Color.FromArgb(0, 0, 255);
            DisplayMap.SetPixel(End.X, End.Y, c);

            // Showing the image
            pbMap.Image = DisplayMap;
        }

        public void DrawReachableRegion(RtwMatrix mask)
        {
            // Apply mask
            int rows = mask.Rows;
            int columns = mask.Columns;
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    if (mask[i, j] != 1)
                    {
                        Color c = new Color();
                        c = Color.FromArgb(0, 0, 0);
                        DisplayMap.SetPixel(j, i, c);
                    }
                }
            }

            // Showing the image
            pbMap.Image = DisplayMap;
        }

        public void DrawCoverage(List<Point> Path)
        {
            if (Path.Count < 2)
            {
                return;
            }

            for (int i = 0; i < Path.Count; i++)
            {
                Point p = Path[i];
                Color c = new Color();
                if (i == 0)
                {
                    // Yellow for staring point
                    c = Color.FromArgb(255, 255, 0);
                }
                else if (i != Path.Count - 1)
                {
                    // Red for path
                    c = Color.FromArgb(255, 0, 0);
                }
                else
                {
                    // Blue for ending point 
                    c = Color.FromArgb(0, 0, 255);
                }
                DisplayMap.SetPixel(p.X, p.Y, c);
                pbMap.Image = DisplayMap;
                this.Refresh();
            }
            pbMap.Image = DisplayMap;
        }

        public void DrawPath(List<Point> Path)
        {
            if (Path.Count < 2)
            {
                return;
            }

            for (int i = 0; i < Path.Count; i++)
            {
                Point p = Path[i];
                Color c = new Color();
                Color cOriginal = new Color();
                cOriginal = DisplayMap.GetPixel(p.X, p.Y);

                if (i == 0)
                {
                    // Yellow for staring point
                    c = Color.FromArgb(255, 255, 0);
                }
                else if (i != Path.Count - 1)
                {
                    // Red for path
                    c = Color.FromArgb(255, 0, 0);
                }
                else
                {
                    // Blue for ending point 
                    c = Color.FromArgb(0, 0, 255);
                }
                DisplayMap.SetPixel(p.X, p.Y, c);
                pbMap.Image = DisplayMap;
                this.Refresh();

                // Now change pixel back to original color
                if (i != 0 && i != Path.Count - 1)
                {
                    DisplayMap.SetPixel(p.X, p.Y, cOriginal);
                }
            }
            pbMap.Image = DisplayMap;
        }

        public void setPointColor(Point p, float f)
        {
            Color c = new Color();
            int i = Convert.ToInt32(f);
            c = Color.FromArgb(i, i, i);
            DisplayMap.SetPixel(p.X, p.Y, c);
        }

        public void setPointColor(Point p, Color c)
        {
            DisplayMap.SetPixel(p.X, p.Y, c);
        }

        // When I have time, add in the sprite visual effect and allow
        // setting and dragging sprites to show starting pint and ending point

        #endregion
    }
}