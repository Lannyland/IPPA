using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rtwmatrix;
using System.Drawing;
using System.Collections;

namespace IPPA
{
    // Performs the Global Warming Search
    class AlgGlobalWarming : AlgPathPlanning
    {
        #region Members

        // Private variables
        private int ModeCount = 0;
        private List<float> arrlCurCDFs = new List<float>();
        
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

        // Algorithm specific implementation of the path planning
        protected override void DoPathPlanning()
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
                // GWParallelSearch();
            }

            // If yes Coarse-to-fine and yes parallel
            if (curRequest.UseCoarseToFineSearch && curRequest.UseParallelProcessing)
            {
                // GWCoarseToFineAndParallelSearch();
            }
        }

        // Search every GW
        private void GWExtensiveSearch()
        {
            // Make copy of map
            RtwMatrix mGW = mDist.Clone();

            // Find max value
            float[] minmax = mGW.MinMaxValue();
            float max = minmax[1];
            float rise = max / ProjectConstants.GWCount;

            // Loop many times
            for (int i = 0; i < ProjectConstants.GWCount; i++)
            {
                //Console.Write("i=" + i + " ");
                // Don't rise ocean for first search
                // Log("Orean rise " + i.ToString() + "\n");
                if (i > 0)
                {
                    // Ocean rises
                    OceanRises(mGW, rise);
                }

                // After ocean rises
                if (PlanPathAtCurrentGW(mGW))
                {
                    // Already found the best path, no need to continue.
                    i = ProjectConstants.GWCount;
                }
            }

            // Cleaning up                        
            mGW = null;
        }

        // Search GW intelligently
        private void GWCoarseToFineSearch()
        {
            // Make copy of map
            RtwMatrix mGW = mDist.Clone();

            // How many to search on each side?
            int sideSearch = ProjectConstants.CTFGWCoraseLevel - 1;

            // Find max value
            float[] minmax = mGW.MinMaxValue();
            float globalMax = minmax[1];
            float curMiddle = globalMax;

            float rise = curMiddle / ProjectConstants.CTFGWCoraseLevel;

            for (int i = 0; i < ProjectConstants.CTFGWLevelCount; i++)
            {
                float curLeft = curMiddle + sideSearch * rise;
                float curRight = curMiddle - sideSearch * rise;
                // Array of CDFs for all searches (left, middle, and right)
                float[] CDFs = new float[ProjectConstants.CTFGWCoraseLevel * 2 - 1];

                // Compute middle
                if (PlanPathAtCurrentGW(mGW))
                {
                    // Already found the best path, no need to continue.
                    return;
                }
                else
                {
                    arrlCurCDFs.Sort();
                    CDFs[sideSearch] = arrlCurCDFs[arrlCurCDFs.Count-1];
                    arrlCurCDFs.Clear();
                }

                // Now do left side (ocean falls)
                if (curLeft <= globalMax)
                {
                    RtwMatrix mGWLeft = mGW.Clone();
                    OneSideGWSearch(false, mGWLeft, rise, CDFs);
                    mGWLeft = null;
                }

                // Now do right side (ocean rises)
                if (curRight > 0)
                {
                    RtwMatrix mGWRight = mGW.Clone();
                    OneSideGWSearch(true, mGWRight, rise, CDFs);
                    mGWRight = null;
                }                

                // No need to do this again in the last level
                if (i < sideSearch)
                {
                    // Find best CDF 
                    int indexBest = Array.IndexOf(CDFs, CDFs.Max());
                    // Get the mGW for that best one
                    int indexDiff = indexBest - sideSearch;
                    OceanRises(mGW, indexDiff * rise);

                    curMiddle = curMiddle - indexDiff * rise;
                    rise = rise / ProjectConstants.CTFGWCoraseLevel;
                }
            }

            // Cleaning up                        
            mGW = null;
        }

        private void OneSideGWSearch(bool OceanUp, RtwMatrix mGW, float rise, float[] CDFs)
        {
            int sign = -1;
            if (OceanUp)
            {
                sign = 1;
            }

            for (int i = 0; i < ProjectConstants.CTFGWCoraseLevel - 1; i++)
            {
                OceanRises(mGW, sign * rise);
                if (PlanPathAtCurrentGW(mGW))
                {
                    // Already found the best path, no need to continue.
                    return;
                }
                else
                {
                    arrlCurCDFs.Sort();
                    CDFs[ProjectConstants.CTFGWCoraseLevel - 1 + sign + sign * i] = arrlCurCDFs[arrlCurCDFs.Count - 1];
                    arrlCurCDFs.Clear();
                }
            }

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

        // Ocean rises by rise (height of island tip decreases if rise is positive)
        private void OceanRises(RtwMatrix mGW, float rise)
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

        // At current GW do LHC
        private bool PlanPathAtCurrentGW(RtwMatrix mGW)
        {
            // Console.WriteLine("Doing PlanPathAtCurrentGW once!");
            if (curRequest.AlgToUse == AlgType.LHCGWCONV)
            {
                // If LHCGWCONV, search multiple convolution kernal sizes
                int dim = Math.Max(mDist.Rows, mDist.Columns);
                for (int j = 3; j < dim; j += (int)(dim / ProjectConstants.ConvCount))
                {
                    //Console.Write("j=" + j + "\n");
                    AlgLHCGWCONV myAlg = new AlgLHCGWCONV(curRequest, mGW, mDiff, Efficiency_LB, j);
                    myAlg.PlanPath();

                    // Remember if true CDF is better
                    RememberBestPath(myAlg);

                    // Cleaning up                        
                    myAlg = null;

                    // If we already have the best path, then no need to continue
                    if (Math.Abs(Efficiency_LB - CDF) < 0.001)
                    {
                        return true;
                    }
                }
                //// Print one GW per line (3 conv each line)
                //curRequest.SetLog("\n");
            }
            if (curRequest.AlgToUse == AlgType.LHCGWPF)
            {
                // If LHCGWPF, search three convolution kernal sizes
                int dim = Math.Max(mDist.Rows, mDist.Columns);
                int Sigma = 0;
                for (int j = 0; j < ProjectConstants.PFCount; j++)
                {
                    //Console.Write("j=" + j + "\n");
                    Sigma += Convert.ToInt16(dim / 3);
                    AlgLHCGWCONV myAlg = new AlgLHCGWCONV(curRequest, mGW, mDiff, Efficiency_LB, Sigma);
                    myAlg.PlanPath();

                    // Remember if true CDF is better
                    RememberBestPath(myAlg);

                    // Cleaning up                        
                    myAlg = null;

                    // If we already have the best path, then no need to continue
                    if (Math.Abs(Efficiency_LB - CDF) < 0.001)
                    {
                        return true;
                    }
                }
                // Print one GW per line (3 conv each line)
                //curRequest.SetLog("\n");
            }
            return false;
        }

        // Compute true CDF based on original dist map (no GW) and remember the best
        private void RememberBestPath(AlgLHC myAlg)
        {
            // I am recalculating BestCDF because the Global Warming effect lowered the probabilities
            float RealCDF = GetTrueCDF(myAlg.GetPath());
            arrlCurCDFs.Add(RealCDF);
            //// Log RealCDF for each GW run
            //curRequest.SetLog(RealCDF.ToString() + ", ");

            if (CDF < RealCDF)
            {
                CDF = RealCDF;
                Path = myAlg.GetPath();
            }
        }

        // Function to calculate true cummulative probability using original map
        private float GetTrueCDF(List<Point> curPath)
        {
            float curCDF = 0;

            RtwMatrix mCDF = mDist.Clone();
            for (int i = 0; i < curRequest.T + 1; i++)
            {
                curCDF += GetPartialDetection(curPath[i], mCDF);
                mCDF[curPath[i].Y, curPath[i].X] = VacuumProbability(curPath[i], mCDF);
            }

            // Cleaning up
            mCDF = null;

            return curCDF;
        }

        // Debugging shouts
        public override void Shout()
        {
            Console.WriteLine("I am AlgGlobalWarming!");
        }

        #endregion
    }
}
