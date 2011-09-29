using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rtwmatrix;
using System.Drawing;

namespace IPPA
{
    // Performs the Global Warming Search
    class AlgGlobalWarming : AlgPathPlanning
    {
        #region Members

        // Private variables
        private int ModeCount = 0;
        private double BestCDF = 0;
        private List<Point> BestPath;

        #endregion

        #region Constructor, Destructor

        // Constructor
        public AlgGlobalWarming(PathPlanningRequest _curRequest, int _ModeCount, 
            RtwMatrix _mDistReachable, RtwMatrix _mDiffReachable, double _Efficiency_LB)
            : base (_curRequest, _mDistReachable, _mDiffReachable, _Efficiency_LB)
        {
            ModeCount = _ModeCount;
        }

        // Destructor
        ~AlgGlobalWarming()
        {
            // Cleaning up
        }

        #endregion

        #region Other Functions

        public override void PlanPath()
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
            RtwMatrix mGW = mDist.Clone();

            // Find max value
            float[] minmax = mGW.MinMaxValue();
            float max = minmax[1];
            float rise = max / (GWCount + 1);

            // Loop many times
            for (int i = 0; i < GWCount; i++)
            {
                // Don't rise ocean for first search
                // Log("Orean rise " + i.ToString() + "\n");
                if (i > 0)
                {
                    // Ocean rises
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

                // After ocean rises
                if (curRequest.AlgToUse == AlgType.LHCGWCONV)
                {
                    // If LHCGWCONV, search three convolution kernal sizes
                    int dim = Math.Max(mDist.Rows, mDist.Columns);
                    for (int j = 3; j < dim; j += (int)(dim / ProjectConstants.ConvCount))
                    {
                        // Log("Using kernel size " + j.ToString() + "x" + j.ToString() + "\n");
                        AlgLHCGWCONV myAlg = new AlgLHCGWCONV(curRequest,mDist,mDiff,Efficiency_LB,j);
                        myAlg.PlanPath();
                        // Log(myAlg.NodesExpanded.ToString() + " nodes expanded.\n");
                        // Log("1 full path explored.\n");
                        // Log("Theoretical upper bound is " + Efficiency_LB.ToString() + "\n");
                        // I am recalculating BestCDF because the Global Warming effect lowered the probabilities
                        double RealCDF = GetTrueCDF(myAlg.GetPath());
                        if (CDF < RealCDF)
                        {
                            CDF = RealCDF;
                            Path = myAlg.GetPath();
                        }
                        // Log("Cumulative probability is " + CDF.ToString() + "\n");
                        // float Efficiency = lhc.BestCDF / UpperBound * 100;
                        // Log("Efficiency is " + Efficiency.ToString() + "%\n");

                        // Cleaning up                        
                        myAlg = null;

                        // If we already have the path, then no need to continue
                        if (Math.Abs(Efficiency_LB - CDF) < 0.001)
                        {
                            i = GWCount;
                            j = dim;
                        }
                    }
                }

            }

            // Cleaning up                        
            mGW = null;
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

        // TODO Turn this into a seperate file so we can consider task-difficulty map and detection type
        // Function to calculate true cummulative probability using original map
        private double GetTrueCDF(List<Point> curPath)
        {
            double curCDF = 0;

            // Duplicate original map
            RtwMatrix mCDF = mDist.Clone();

            // Fly through the map
            for (int i = 0; i < curRequest.T + 1; i++)
            {
                curCDF += mCDF[curPath[i].Y, curPath[i].X];
                // Assuming 100% detection rate
                mCDF[curPath[i].Y, curPath[i].X] = 0;
            }

            // Cleaning up
            mCDF = null;

            return curCDF;
        }

        #endregion
    }
}
