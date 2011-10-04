using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using rtwmatrix;
using System.Drawing;
using System.Windows.Forms;

namespace IPPA
{
    class MISCLib
    {
        // Read in a csv file and then store that into matrix
        public static RtwMatrix ReadInMap(string FileInName)
        {
            // Read file one line at a time and store to list.
            FileStream file = new FileStream(FileInName, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(file);
            string strLine;
            List<string[]> lstRow = new List<string[]>();
            // Loop through lines
            while (sr.Peek() >= 0)
            {
                strLine = sr.ReadLine().Trim();
                if (strLine.Length > 1)
                {
                    string[] splitData = strLine.Split(',');
                    lstRow.Add(splitData);
                }
            }
            sr.Close();
            file.Close();
            // Create matrix
            RtwMatrix mGrid = new RtwMatrix(lstRow.Count, lstRow[0].Length);
            for (int i = 0; i < mGrid.Rows; i++)
            {
                for (int j = 0; j < mGrid.Columns; j++)
                {
                    mGrid[i, j] = (float)(Convert.ToDouble(lstRow[i][j]));
                }
            }
            return mGrid;
        }

        public static Bitmap DrawPath(List<Point> Path)
        {
            int scale = 7;
            if (Path.Count < 2)
            {
                return null;
            }

            int width = 60 * scale;
            int height = 60 * scale;
            //
            // Create drawing objects
            //
            Bitmap bitmap = new Bitmap(width, height);
            Graphics bitmapGraphics = Graphics.FromImage(bitmap);
            Pen p = new Pen(Color.Red);
            SolidBrush sb = new SolidBrush(Color.Black);

            //
            // Draw Board background
            //
            bitmapGraphics.FillRectangle(sb, 0, 0, width, height);

            //
            // Draw Path
            //
            p.Color = Color.Red;
            p.Width = 3;

            for (int i = 1; i < Path.Count; i++)
            {
                Point prev = new Point(Path[i - 1].X * scale + scale / 2, Path[i - 1].Y * scale + scale / 2);
                Point cur = new Point(Path[i].X * scale + scale / 2, Path[i].Y * scale + scale / 2);

                bitmapGraphics.DrawLine(p, prev, cur);
            }

            //
            // Release objects
            //
            bitmapGraphics.Dispose();
            return bitmap;
        }

        // Code to show bitmap in figure forms
        public static void ShowImage(Bitmap bmp, string strFormTitle)
        {
            // Create new figure form and set all properties
            Form MyForm = new Form();
            ShowImage(bmp, strFormTitle, ref MyForm);
        }

        // Code to show bitmap in figure forms
        public static void ShowImage(Bitmap bmp, string strFormTitle, ref Form MyForm)
        {
            // MyForm.Name = "frmFigure" + counter;
            MyForm.Text = strFormTitle;
            MyForm.AutoSize = true;
            // Create new PictureBox on the figure form
            MyForm.Controls.Clear();
            PictureBox MyPB = new PictureBox();
            // Associate bitmap to PictureBox
            MyPB.Image = bmp;
            MyPB.Location = new Point(0, 0);
            // Set the size of the form and PictureBox according to the image size
            MyPB.Size = bmp.Size;
            MyForm.Size = bmp.Size;
            // Add PictureBox to form
            MyForm.Controls.Add(MyPB);
            // Display figure form
            MyForm.Show();
            MyForm.Refresh();
        }

        // Calculating Manhattan Distance
        public static int ManhattanDistance(int x1, int y1, int x2, int y2)
        {
            int distance = Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
            return distance;
        }


    }
}
