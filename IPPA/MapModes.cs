using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using rtwmatrix;

namespace IPPA
{
    class MapModes
    {
        #region Members

        // Private variables
        private int ModeCount;
        private RtwMatrix mModes;
        private int N;
        private List<Point> lstCentroids = new List<Point>();

        // Public variables

        #endregion

        #region Constructor, Destructor

        // Constructor
        public MapModes(int _ModeCount, RtwMatrix _mModes, int _TopN)
        {
            ModeCount = _ModeCount;
            mModes = _mModes;
            N = _TopN;
            FindTopNModes();
        }

        // Destructor
        ~MapModes()
        {
            // Cleaning up
            mModes = null;
        }

        #endregion

        #region Other Functions

        // Method to to find top N modes
        private void FindTopNModes()
        {
            // Debug code: show modes
            // Show mode nodes
            for (int i = 0; i < mModes.Rows; i++)
            {
                for (int j = 0; j < mModes.Columns; j++)
                {
                    if (mModes[i, j] > 0)
                    {
                        mModes[i, j] = 255;
                    }
                }
            }
            // Convert matrix to image
            Bitmap CurBMP = new Bitmap(mModes.Columns, mModes.Rows);
            ImgLib.MatrixToImage(ref mModes, ref CurBMP);
            // Showing map in map form
            frmMap myModesForm = new frmMap();
            myModesForm.Text = "Modes Map";
            myModesForm.setImage(CurBMP);
            myModesForm.Show();



            // Sanity check: make sure we do have that many modes
            if (N > ModeCount)
            {
                System.Windows.Forms.MessageBox.Show("You want top " + N + " modes, but there are only " + ModeCount + " modes!");
            }

            // Identify centroids for all modes
            FindModeCentroids();

            //TODO Compute goodness of modes


            // Find best N modes and erase other modes

        }

        // Method to find mode centroids for all modes
        private void FindModeCentroids()
        {
            //TODO Method to find mode centroids for all modes
            //TODO For now each mode has one node
            for (int i = 0; i < mModes.Rows; i++)
            {
                for (int j = 0; j < mModes.Columns; j++)
                {
                    if (mModes[i, j] > 0)
                    {
                        lstCentroids.Add(new Point(j, i));
                    }
                }
            }
        }

        // Getters
        public int GetModeCount()
        {
            return ModeCount;
        }
        public List<Point> GetModeCentroids()
        {
            return lstCentroids;
        }

        #endregion
    }
}
