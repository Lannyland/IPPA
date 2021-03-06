﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using rtwmatrix;
using System.Threading;
using System.Threading.Tasks;


namespace IPPA
{
    class AlgTopNH : AlgPathPlanning
    {
        #region Members

        // Private variables
        private int ModeCount;
        private RtwMatrix mModes;                
        private List<double> arrlCurCDFs = new List<double>();

        // Variabes used for threads
        private PathPlanningResponse[] arrResponses = null;
        private List<AlgTopN> lstThreads = new List<AlgTopN>();
        private int bestThreadIndex = 0;

        // Public variables

        #endregion

        #region Constructor, Destructor

        // Constructor
        public AlgTopNH(PathPlanningRequest _curRequest, int _ModeCount, RtwMatrix _mModes, RtwMatrix _mDistReachable, 
            RtwMatrix _mDiffReachable, double _Efficiency_UB) 
            : base (_curRequest, _mDistReachable, _mDiffReachable, _Efficiency_UB)
        {
            ModeCount = _ModeCount;
            mModes = _mModes;
        }

        // Destructor
        ~AlgTopNH()
        {
            // Cleaning up
            mModes = null;
        }

        #endregion

        #region Other Functions

        // Method to perform the path planning
        protected override void DoPathPlanning()
        {
            // Sanity check: Don't do this when there is no mode or just 1 mode
            if (ModeCount < 3)
            {
                System.Windows.Forms.MessageBox.Show("Can't use TopN algorithm because there are less than 3 modes!");
                return;
            }

            // Do not exceed Max N set in project constants
            int GCount = ModeCount;
            if (GCount > ProjectConstants.Max_N)
            {
                GCount = ProjectConstants.Max_N;
            }

            // Loop through Gaussian Counts
            int counter = 0;
            for (int i = GCount; i > 1; i--)
            {
                for (int j = i; j > 1; j--)
                {
                    // Clone things for each search
                    PathPlanningRequest curRequestCopy = curRequest.DeepClone();
                    RtwMatrix mDistCopy = mDist.Clone();
                    RtwMatrix mDiffCopy = mDiff.Clone();
                    // Set appropriate parameters (always do coarse to fine and parallel)
                    curRequestCopy.AlgToUse = AlgType.TopN;
                    curRequestCopy.BatchRun = false;
                    curRequestCopy.DrawPath = false;
                    curRequestCopy.RunTimes = 1;
                    curRequestCopy.UseCoarseToFineSearch = true;
                    curRequestCopy.UseHierarchy = true;
                    curRequestCopy.UseParallelProcessing = true;
                    curRequestCopy.TopN = j;
                    MapModes curModes = new MapModes(i, ModeCount, mModes, curRequestCopy, mDist, mDiff);
                    // Create path planning object
                    AlgTopN curAlg = new AlgTopN(curModes, curRequestCopy, ModeCount, mModes, mDistCopy, mDiffCopy, Efficiency_UB);
                    curAlg.index = counter;
                    lstThreads.Add(curAlg);
                    counter++;
                }
            }

            // Allocate array space for results
            arrResponses = new PathPlanningResponse[lstThreads.Count];

            // Decide whether to do parallelization
            if (curRequest.UseParallelProcessing)
            {
                ParallelSearch();
            }
            else
            {
                ExtensiveSearch();
                FindBestPath();
            }
        }

        // Method to perform extensive search in the T space
        private void ExtensiveSearch()
        {
            for (int i = 0; i < lstThreads.Count; i++)
            {
                AlgTopN alg = lstThreads[i];

                // Remember if true CDF is better
                StoreResults(i);

                // If we already have the best path, then no need to continue
                if (Math.Abs(Efficiency_UB - CDF) < 0.001)
                {
                    break;
                }
            }
        }

        // Search multiple time allocation simutaneously
        private void ParallelSearch()
        {
            SpawnThreads();
        }

        // Spawn multi threads to do parallel path planning
        private void SpawnThreads()
        {
            // Create task array
            Task[] tasks = new Task[lstThreads.Count];

            for (int i = 0; i < lstThreads.Count; i++)
            {
                int cur_i = i;
                tasks[i] = Task.Factory.StartNew(() => StoreResults(cur_i));
                // Thread.Sleep(500);
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
            // Storing results
            arrResponses[index] = new PathPlanningResponse(
                                    lstThreads[index].index,
                                    lstThreads[index].GetCDF(),
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
                    if (ProjectConstants.DebugMode)
                    {
                        curRequest.SetLog("Path " + (i + 1) + ": CDF = " + arrResponses[i].CDF + "\n");
                    }
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
                
        // Debugging shouts
        public override void Shout()
        {
            Console.WriteLine("I am AlgTopNH!");
        }

        #endregion
    }
}
