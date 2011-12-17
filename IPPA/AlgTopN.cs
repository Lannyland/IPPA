using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using rtwmatrix;

namespace IPPA
{
    class AlgTopN : AlgPathPlanning
    {
        #region Members

        // Private variables
        private Point Start = new Point();
        private Point End = new Point();
        private MapModes myModes = null;
        private int N = 0;  // Actual number of modes we will use
        private List<Point> lstCentroids = null;


        // Public variables

        #endregion

        #region Constructor, Destructor

        // Constructor
        public AlgTopN(PathPlanningRequest _curRequest, int _ModeCount, RtwMatrix _mModes, RtwMatrix _mDistReachable,
            RtwMatrix _mDiffReachable, double _Efficiency_UB) 
            : base (_curRequest, _mDistReachable, _mDiffReachable, _Efficiency_UB)
        {
            myModes = new MapModes(_ModeCount, _mModes, curRequest.TopN);
        }

        // Destructor
        ~AlgTopN()
        {
            // Cleaning up
        }

        #endregion

        #region Other Functions

        protected override void DoPathPlanning()
        {
            // Sanity check: Don't do this when there is no mode or just 1 mode
            if (myModes.GetModeCount() < 2)
            {
                System.Windows.Forms.MessageBox.Show("Can't use TopN algorithm because there are less than 2 modes!");
                return;
            }

            //Determine whether there are TopN modes.
            N = curRequest.TopN;
            if (N > myModes.GetModeCount())
            {
                //If Yes, N is good. If No, use mode count as N.
                N = myModes.GetModeCount();
            }
                        
            // Sanity check: make sure T is enough to cover all centroids
            lstCentroids = myModes.GetModeCentroids();
            int[] distances = new int[lstCentroids.Count-1];
            int[] CentroidsIndexes = new int[lstCentroids.Count];
            for(int i=0; i<lstCentroids.Count; i++)
            {
                CentroidsIndexes[i] = i;
            }
            foreach (IEnumerable<T> permutation in PermuteUtils.Permute<T>(CentroidsIndexes, lstCentroids.Count))
            {
                foreach (T i in permutation)
                {
                    
                }
            }
            if(false)
            {
            }

            /*
            // Figuring our all points
            FigureOutAllPoints();

            // Searth different combinations and find the best one
            PlanPathSegments();

            // Get real CDF
            CDF = GetTrueCDF(Path);

            //TODO Starting from each centroid, do greedy (among modes) path planning
            //TODO Compute minimum distance to cover N current nodes
            //TODO Compute minimum distance from starting point to one of the N current nodes
            //TODO Do it until reaching T.
            //TODO Plan LHC to join them.
            //TODO Deal with flying backwards
             */
        }

        /*
        // Method to figure our all points
        private void FigureOutAllPoints()
        {
            Start = new Point(curRequest.pStart.column, curRequest.pStart.row);
            if (curRequest.UseEndPoint)
            {
                End = new Point(curRequest.pEnd.column, curRequest.pEnd.row);
            }

            // Find best two modes
            List<Point> lstCentroids = myModes.GetModeCentroids();

            // Find closest centroid and make that centriod1            
            d1 = MISCLib.ManhattanDistance(Start, lstCentroids[0]);
            d2 = MISCLib.ManhattanDistance(Start, lstCentroids[1]);
            if (d1 < d2)
            {
                Centroid1 = lstCentroids[0];
                Centroid2 = lstCentroids[1];
            }
            else
            {
                Centroid1 = lstCentroids[1];
                Centroid2 = lstCentroids[0];
                d1 = d2;
            }

            // Sanity check: Don't do this if there's not enough time to go from start to centroid 1 and then to centriod 2 (and then to end)
            d2 = MISCLib.ManhattanDistance(Centroid1, Centroid2);
            if (curRequest.UseEndPoint)
            {
                d4 = MISCLib.ManhattanDistance(Centroid2, End);
            }
            if (d1 + d2 + d3 + d4 > curRequest.T)
            {
                System.Windows.Forms.MessageBox.Show("Can't reach both centroids with given flight time!");
                return;
            }

            // Deal with copter or non-copter
            FindEndPoints();
        }
        */


        // Debugging shouts
        public override void Shout()
        {
            Console.WriteLine("I am AlgTopN!");
        }

        #endregion
    }
}
