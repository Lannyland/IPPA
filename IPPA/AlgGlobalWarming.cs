using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rtwmatrix;
using System.Drawing;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;

namespace IPPA
{
    // Performs the Global Warming Search
    class AlgGlobalWarming : AlgPathPlanning
    {
        #region Members

        // Private variables
        private int ModeCount = 0;
        private List<double> arrlCurCDFs = new List<double>();
        private int GWCount;
        private int ConvCount;
        private int CTFGWCoraseLevel;
        private int CTFGWLevelCount;
        private int PFCount;

        // Variabes used for threads
        private PathPlanningResponse[] arrResponses = null;
        private List<AlgPathPlanning> lstThreads = new List<AlgPathPlanning>();
        private int bestThreadIndex = 0;

        #endregion

        #region Constructor, Destructor

        // Constructor
        public AlgGlobalWarming(PathPlanningRequest _curRequest, int _ModeCount, 
            RtwMatrix _mDistReachable, RtwMatrix _mDiffReachable, double _Efficiency_UB)
            : base (_curRequest, _mDistReachable, _mDiffReachable, _Efficiency_UB)
        {
            ModeCount = _ModeCount;
            GWCount = ProjectConstants.GWCount;
            ConvCount = ProjectConstants.ConvCount;
            CTFGWCoraseLevel = ProjectConstants.CTFGWCoraseLevel;
            CTFGWLevelCount = ProjectConstants.CTFGWLevelCount;
            PFCount = ProjectConstants.PFCount;
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
            // If GWCount is 1, then
            if (GWCount == 1)
            {
                NoGWSearch();
                return;
            }

            // If using hiararchical search
            if (curRequest.UseHierarchy)
            {
                if (ModeCount <= 1)
                {
                    // Uniform distribution or unimodal distribution or path distribution (possibly with splits)
                    NoGWSearch();
                    return;
                }
            }

            // If no Coarse-to-fine and no parallel
            if (!curRequest.UseCoarseToFineSearch && !curRequest.UseParallelProcessing)
            {
                GWExtensiveSearch();
                return;
            }

            // If yes Coarse-to-fine and no parallel
            if (curRequest.UseCoarseToFineSearch && !curRequest.UseParallelProcessing)
            {
                GWCoarseToFineSearch();
                return;
            }

            // If no Coarse-to-fine and yes parallel
            if (!curRequest.UseCoarseToFineSearch && curRequest.UseParallelProcessing)
            {
                GWParallelSearch();
                return;
            }

            // If yes Coarse-to-fine and yes parallel
            if (curRequest.UseCoarseToFineSearch && curRequest.UseParallelProcessing)
            {
                GWCoarseToFineAndParallelSearch();
                return;
            }
        }

        // Search without Global Warming
        private void NoGWSearch()
        {
            // Make copy of map
            RtwMatrix mGW = mDist.Clone();
            // Search once
            PlanPathAtCurrentGW(mGW, 0);
            // Cleaning up                        
            mGW = null;
        }

        // Search every GW
        private void GWExtensiveSearch()
        {
            // Make copy of map
            RtwMatrix mGW = mDist.Clone();

            // Find max value
            float[] minmax = mGW.MinMaxValue();
            float max = minmax[1];
            float rise = max / GWCount;

            // Loop many times
            for (int i = 0; i < GWCount; i++)
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
                if (PlanPathAtCurrentGW(mGW, i))
                {
                    // Already found the best path, no need to continue.
                    i = GWCount;
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
            int sideSearch = CTFGWCoraseLevel - 1;

            // Find max value
            float[] minmax = mGW.MinMaxValue();
            float globalMax = minmax[1];
            
            // Start from one side (no ocean rise)
            float curMiddle = globalMax;

            // Actual search
            float rise = curMiddle / CTFGWCoraseLevel;
            double lastBestCDF = 0;
            for (int i = 0; i < CTFGWLevelCount; i++)
            {
                float curLeft = curMiddle + sideSearch * rise;
                float curRight = curMiddle - sideSearch * rise;
                // Array of CDFs for all searches (left, middle, and right)
                double[] CDFs = new double[CTFGWCoraseLevel * 2 - 1];

                if (i == 0)
                {
                    // Compute middle
                    if (PlanPathAtCurrentGW(mGW, 0))
                    {
                        // Already found the best path, no need to continue.
                        return;
                    }
                    else
                    {
                        arrlCurCDFs.Sort();
                        CDFs[sideSearch] = arrlCurCDFs[arrlCurCDFs.Count - 1];
                        arrlCurCDFs.Clear();
                    }
                }
                else
                {
                    // No need to compute middle again. Already done from previous step.
                    CDFs[sideSearch] = lastBestCDF;
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

                // Debug: log
                for (int k = 0; k < CDFs.Length; k++)
                {
                    curRequest.SetLog(CDFs[k].ToString() + ", ");
                }
                curRequest.SetLog("\n");

                // No need to do this again in the last level
                if (i < CTFGWLevelCount - 1)
                {
                    // Find best CDF 
                    int indexBest = Array.IndexOf(CDFs, CDFs.Max());
                    lastBestCDF = CDFs[indexBest];
                    // Get the mGW for that best one
                    int indexDiff = indexBest - sideSearch;
                    OceanRises(mGW, indexDiff * rise);

                    curMiddle = curMiddle - indexDiff * rise;
                    rise = rise / CTFGWCoraseLevel;
                }
            }

            // Cleaning up                        
            mGW = null;
        }

        private void OneSideGWSearch(bool OceanUp, RtwMatrix mGW, float rise, double[] CDFs)
        {
            int sign = -1;
            if (OceanUp)
            {
                sign = 1;
            }

            for (int i = 0; i < CTFGWCoraseLevel - 1; i++)
            {
                OceanRises(mGW, sign * rise);
                if (PlanPathAtCurrentGW(mGW, 0))
                {
                    // Already found the best path, no need to continue.
                    return;
                }
                else
                {
                    arrlCurCDFs.Sort();
                    CDFs[CTFGWCoraseLevel - 1 + sign + sign * i] = arrlCurCDFs[arrlCurCDFs.Count - 1];
                    arrlCurCDFs.Clear();
                }
            }

        }

        // Search multiple GW simutaneously
        private void GWParallelSearch()
        {
            // Make copy of map
            RtwMatrix mGW = mDist.Clone();

            // Find max value
            float[] minmax = mGW.MinMaxValue();
            float max = minmax[1];
            float rise = max / GWCount;

            // Loop many times
            for (int i = 0; i < GWCount; i++)
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
                PlanPathAtCurrentGW(mGW, i);
            }

            // Cleaning up                        
            mGW = null;

            // Spawn threads to plan path
            SpawnThreads();
        }

        // Search GW intelligently and simutaneously
        private void GWCoarseToFineAndParallelSearch()
        {
            // Make copy of map
            RtwMatrix mGW = mDist.Clone();

            // How many to search on each side?
            int sideSearch = CTFGWCoraseLevel - 1;

            // Find max value
            float[] minmax = mGW.MinMaxValue();
            float globalMax = minmax[1];

            // Start from one side (no ocean rise)
            float curMiddle = globalMax;

            // Actual search
            float rise = curMiddle / CTFGWCoraseLevel;
            List<Point> MiddlePath = new List<Point>();
            for (int i = 0; i < CTFGWLevelCount; i++)
            {
                // Clear all threads first to start fresh
                lstThreads.Clear();
                // Clear all responses too
                arrResponses = null;

                // Now do the coarse to fine search
                float curLeft = curMiddle + sideSearch * rise;
                float curRight = curMiddle - sideSearch * rise;
                // Array of CDFs for all searches (left, middle, and right)
                double[] CDFs = new double[CTFGWCoraseLevel * 2 - 1];

                // Compute middle
                if (i == 0)
                {
                    PlanPathAtCurrentGW(mGW, sideSearch);
                }
                else
                {
                    // No need to compute middle again. Already done from previous step.
                    CDFs[sideSearch] = CDF;
                    MiddlePath = Path;
                }

                // Now do left side (ocean falls)
                if (curLeft <= globalMax)
                {
                    RtwMatrix mGWLeft = mGW.Clone();
                    OneSideGWSearchParallel(false, mGWLeft, rise);
                    mGWLeft = null;
                }

                // Now do right side (ocean rises)
                if (curRight > 0)
                {
                    RtwMatrix mGWRight = mGW.Clone();
                    OneSideGWSearchParallel(true, mGWRight, rise);
                    mGWRight = null;
                }

                // Now do this round of multi-thread path planning
                SpawnThreads();

                // Debug: log
                for (int k = 0; k < CDFs.Length; k++)
                {
                    curRequest.SetLog(CDFs[k].ToString() + ", ");
                }
                curRequest.SetLog("\n");

                // Since we never compute the middle again, what if middle is better than the other 6x3?
                if (CDFs[sideSearch] >= CDF)
                {
                    CDF = CDFs[sideSearch];
                    Path = MiddlePath;
                    bestThreadIndex = sideSearch;
                }

                // No need to do this again in the last level
                if (i < CTFGWLevelCount - 1)
                {
                    int indexDiff = bestThreadIndex - sideSearch;
                    OceanRises(mGW, indexDiff * rise);
                    curMiddle = curMiddle - indexDiff * rise;
                    rise = rise / CTFGWCoraseLevel;
                }
            }

            // Cleaning up                        
            mGW = null;
        }

        private void OneSideGWSearchParallel(bool OceanUp, RtwMatrix mGW, float rise)
        {
            int sign = -1;
            if (OceanUp)
            {
                sign = 1;
            }

            for (int i = 0; i < CTFGWCoraseLevel - 1; i++)
            {
                OceanRises(mGW, sign * rise);
                PlanPathAtCurrentGW(mGW, CTFGWCoraseLevel - 1 + sign + sign * i);
            }
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
        private bool PlanPathAtCurrentGW(RtwMatrix mGW, int index)
        {
            // Console.WriteLine("Doing PlanPathAtCurrentGW once!");
            if (curRequest.AlgToUse == AlgType.LHCGWCONV || curRequest.AlgToUse == AlgType.LHCGWCONV_E)
            {
                // If LHCGWCONV, search multiple convolution kernal sizes
                int dim = Math.Max(mDist.Rows, mDist.Columns);
                for (int j = 5; j < dim; j += (int)(dim / ConvCount))
                {
                    // Console.Write("j=" + j + "\n");
                    AlgLHCGWCONV myAlg = null;
                    if (curRequest.UseParallelProcessing)
                    {
                        PathPlanningRequest curRequestCopy = curRequest.DeepClone();
                        RtwMatrix mGWCopy = mGW.Clone();
                        RtwMatrix mDiffCopy = mDiff.Clone();
                        myAlg = new AlgLHCGWCONV(curRequestCopy, mGWCopy, mDiffCopy, Efficiency_UB, j);
                        myAlg.SetBeforeStart(BeforeStart);
                        // Debug code
                        myAlg.conv = j;
                        myAlg.index = index;
                        lstThreads.Add(myAlg);
                    }
                    else
                    {
                        myAlg = new AlgLHCGWCONV(curRequest, mGW, mDiff, Efficiency_UB, j);
                        myAlg.SetBeforeStart(BeforeStart);
                        myAlg.PlanPath();

                        // Remember if true CDF is better
                        RememberBestPath(myAlg);

                        // Cleaning up                        
                        myAlg = null;

                        // If we already have the best path, then no need to continue
                        if (Math.Abs(Efficiency_UB - CDF) < 0.001)
                        {
                            return true;
                        }
                    }
                }
                //// Print one GW per line (3 conv each line)
                //curRequest.SetLog("\n");
            }
            if (curRequest.AlgToUse == AlgType.LHCGWPF || curRequest.AlgToUse == AlgType.LHCGWPF_E)
            {
                // If LHCGWPF, search three convolution kernal sizes
                int dim = Math.Max(mDist.Rows, mDist.Columns);
                int Sigma = 0;
                for (int j = 0; j < PFCount; j++)
                {
                    //Console.Write("j=" + j + "\n");
                    Sigma += Convert.ToInt16(dim / 3);
                    AlgLHCGWCONV myAlg = null;
                    if (curRequest.UseParallelProcessing)
                    {
                        PathPlanningRequest curRequestCopy = curRequest.DeepClone();
                        RtwMatrix mGWCopy = mGW.Clone();
                        RtwMatrix mDiffCopy = mDiff.Clone();
                        myAlg = new AlgLHCGWCONV(curRequestCopy, mGWCopy, mDiffCopy, Efficiency_UB, Sigma);
                        myAlg.SetBeforeStart(BeforeStart);
                        // Debug code
                        myAlg.conv = j;
                        myAlg.index = index;
                        lstThreads.Add(myAlg);
                    }
                    else
                    {
                        myAlg = new AlgLHCGWCONV(curRequest, mGW, mDiff, Efficiency_UB, Sigma);
                        myAlg.SetBeforeStart(BeforeStart);
                        myAlg.PlanPath();

                        // Remember if true CDF is better
                        RememberBestPath(myAlg);

                        // Cleaning up                        
                        myAlg = null;

                        // If we already have the best path, then no need to continue
                        if (Math.Abs(Efficiency_UB - CDF) < 0.001)
                        {
                            return true;
                        }
                    }
                }
                // Print one GW per line (3 conv each line)
                //curRequest.SetLog("\n");
            }
            if (curRequest.AlgToUse == AlgType.CONV || curRequest.AlgToUse == AlgType.CONV_E)
            {
                // If CONV, search multiple convolution kernal sizes
                int dim = Math.Max(mDist.Rows, mDist.Columns);
                for (int j = 3; j < dim; j += (int)(dim / ConvCount))
                {
                    //Console.Write("j=" + j + "\n");
                    AlgCONV myAlg = null;
                    if (curRequest.UseParallelProcessing)
                    {
                        PathPlanningRequest curRequestCopy = curRequest.DeepClone();
                        RtwMatrix mGWCopy = mGW.Clone();
                        RtwMatrix mDiffCopy = mDiff.Clone();
                        myAlg = new AlgCONV(curRequestCopy, mGWCopy, mDiffCopy, Efficiency_UB, j);
                        myAlg.SetBeforeStart(BeforeStart);
                        // Debug code
                        myAlg.conv = j;
                        myAlg.index = index;
                        lstThreads.Add(myAlg);
                    }
                    else
                    {
                        myAlg = new AlgCONV(curRequest, mGW, mDiff, Efficiency_UB, j);
                        myAlg.SetBeforeStart(BeforeStart);
                        myAlg.PlanPath();

                        // Remember if true CDF is better
                        RememberBestPath(myAlg);

                        // Cleaning up                        
                        myAlg = null;

                        // If we already have the best path, then no need to continue
                        if (Math.Abs(Efficiency_UB - CDF) < 0.001)
                        {
                            return true;
                        }
                    }
                }
                //// Print one GW per line (3 conv each line)
                //curRequest.SetLog("\n");
            }
            return false;
        }

        // Compute true CDF based on original dist map (no GW) and remember the best
        private void RememberBestPath(AlgLHC myAlg)
        {
            // I am recalculating BestCDF because the Global Warming effect lowered the probabilities
            double RealCDF = GetTrueCDF(myAlg.GetPath());

            //// Debug
            //Console.WriteLine(RealCDF + ", " + myAlg.GetRunTime() + ", " + myAlg.GetEfficiency());

            arrlCurCDFs.Add(RealCDF);

            //// Log RealCDF for each GW run
            //curRequest.SetLog(RealCDF.ToString() + ", ");

            if (CDF < RealCDF)
            {
                CDF = RealCDF;
                Path = myAlg.GetPath();
            }
        }

        // Spawn multi threads to do parallel path planning
        private void SpawnThreads()
        {
            // Create task array
            Task[] tasks = new Task[lstThreads.Count];
            // Allocate array space for results
            arrResponses = new PathPlanningResponse[lstThreads.Count];

            for (int i = 0; i < lstThreads.Count; i++)
            {
                int cur_i = i;
                tasks[i] = Task.Factory.StartNew(() => StoreResults(cur_i));
            }
            Task.WaitAll(tasks);
            // Now that all threads/tasks are done, find the best one
            FindBestPath();
        }

        // Store path planning results to array
        private void StoreResults(int index)
        {
            // Plan path
            lstThreads[index].PlanPath();
            // I am recalculating BestCDF because the Global Warming effect lowered the probabilities
            double RealCDF = GetTrueCDF(lstThreads[index].GetPath());
            // Storing results
            arrResponses[index] = new PathPlanningResponse(
                                    lstThreads[index].index,
                                    RealCDF,
                                    lstThreads[index].GetRunTime(),
                                    lstThreads[index].GetEfficiency(),
                                    lstThreads[index].GetPath());
        }

        // Find best path in parallel version
        private void FindBestPath()
        {
            int bestIndex = 0;
            double bestCDF = 0;
            // Find one with best CDF
            for (int i = 0; i < arrResponses.Length; i++)
            {
                if (arrResponses[i] != null)
                {
                    if (bestCDF < arrResponses[i].CDF)
                    {
                        bestCDF = arrResponses[i].CDF;
                        bestIndex = i;
                    }
                }
            }
            // Set best one as path planning response
            CDF = arrResponses[bestIndex].CDF;
            Path = arrResponses[bestIndex].Path;
            bestThreadIndex = arrResponses[bestIndex].index;
        }

        // Getters and Setters
        public void SetGWCount(int _GWCount)
        {
            GWCount = _GWCount;
        }
        public void SetConvCount(int _ConvCount)
        {
            ConvCount = _ConvCount;
        }

        // Debugging shouts
        public override void Shout()
        {
            Console.WriteLine("I am AlgGlobalWarming!");
        }

        #endregion
    }
}
