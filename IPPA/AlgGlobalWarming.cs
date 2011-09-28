using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rtwmatrix;
using System.Drawing;

namespace IPPA
{
    // Performs the Global Warming Search
    class AlgGlobalWarming
    {
        #region Members

        // Private variables
        private PathPlanningRequest curRequest;
        private int ModeCount = 0;
        private RtwMatrix mDistReachable;
        private RtwMatrix mDiffReachable;
        private double Efficiency_LB = 0;
        private double Efficiency = 0;
        private double RunTime = 0;
        private List<Point> Path;

        #endregion

        #region Constructor, Destructor

        // Constructor
        public AlgGlobalWarming(PathPlanningRequest _curRequest, int _ModeCount, 
            RtwMatrix _mDistReachable, RtwMatrix _mDiffReachable, double _Efficiency_LB)
        {
            curRequest = _curRequest;
            ModeCount = _ModeCount;
            mDistReachable = _mDistReachable;
            mDiffReachable = _mDiffReachable;
            Efficiency_LB = _Efficiency_LB;
        }

        // Destructor
        ~AlgGlobalWarming()
        {
            // Cleaning up
            curRequest = null;
            mDistReachable = null;
            mDiffReachable = null;
        }

        #endregion

        #region Other Functions

        public void Run()
        {
            // If no Coarse-to-fine and no parallel
            if (!curRequest.UseCoarseToFineSearch && !curRequest.UseParallelProcessing)
            {
                GWExtensiveSearch();
            }
            
            // If yes Coarse-to-fine and no parallel
            if (curRequest.UseCoarseToFineSearch && !curRequest.UseParallelProcessing)
            {
                GWCoarseToFineSearch();
            }

            // If no Coarse-to-fine and yes parallel
            if (!curRequest.UseCoarseToFineSearch && curRequest.UseParallelProcessing)
            {
                GWParallelSearch();
            }

            // If yes Coarse-to-fine and yes parallel
            if (curRequest.UseCoarseToFineSearch && curRequest.UseParallelProcessing)
            {
                GWCoarseToFineAndParallelSearch();
            }
        }

        // Search every GW
        private void GWExtensiveSearch()
        {
            int GWCount = IPPA.ProjectConstants.GWCount;

            // Make copy of map
            RtwMatrix mGW = mDistReachable.Clone();

            // Find max value
            float[] minmax = mGW.MinMaxValue();
            float max = minmax[1];
            float rise = max / (GWCount + 1);

            // Loop many times
            for (int i = 0; i < GWCount; i++)
            {
                // Log("Orean rise " + i.ToString() + "\n");
                if (i > 0)
                {
                    for (int a = 0; a < mGW.Rows; a++)
                    {
                        for (int b = 0; b < mGW.Columns; b++)
                        {
                            mGW[a, b] = mGW[a, b] - rise;
                            if (mGW[a, b] < 0)
                            {
                                mGW[a, b] = 0;
                            }
                        }
                    }
                }
            }
            AlgLHCGWCONV myAlg = new AlgLHCGWCONV();

        }

        // Search GW intelligently
        private void GWCoarseToFineSearch()
        {
            throw new NotImplementedException();
        }

        // Search multiple GW simutaneously
        private void GWParallelSearch()
        {
            throw new NotImplementedException();
        }

        // Search GW intelligently and simutaneously
        private void GWCoarseToFineAndParallelSearch()
        {
            throw new NotImplementedException();
        }


        #region Getters
        public double GetEfficiency()
        {
            return Efficiency;
        }
        public double GetRunTime()
        {
            return RunTime;
        }
        public List<Point> GetPath()
        {
            return Path;
        }
        #endregion

        #endregion
    }
}
