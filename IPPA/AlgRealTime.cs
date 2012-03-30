using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rtwmatrix;
using System.Threading;
using System.Threading.Tasks;

namespace IPPA
{
    class AlgRealTime : AlgPathPlanning
    {
        #region Members

        // Private variables
        private int ModeCount = 0;
        private RtwMatrix mModes;

        // Variabes used for threads
        private PathPlanningResponse[] arrResponses = null;
        private List<AlgPathPlanning> lstThreads = new List<AlgPathPlanning>();

        // Public variables

        #endregion

        #region Constructor, Destructor

        // Constructor
        public AlgRealTime(PathPlanningRequest _curRequest, int _ModeCount, RtwMatrix _mModes,
            RtwMatrix _mDistReachable, RtwMatrix _mDiffReachable, double _Efficiency_UB)
            : base(_curRequest, _mDistReachable, _mDiffReachable, _Efficiency_UB)
        {
            ModeCount = _ModeCount;
            mModes = _mModes;
        }

        // Destructor
        ~AlgRealTime()
        {
            // Cleaning up
            mModes = null;
        }

        #endregion

        #region Other Functions

        // Algorithm specific implementation of the path planning
        protected override void DoPathPlanning()
        {
            // Take care of business without hierarchical search

            // Always use parallelization at this level no matter what

            if (!curRequest.UseEndPoint)
            {
                // For the no end point path planning requests
                // Always use CC
                PathPlanningRequest newRequest = curRequest.DeepClone();
                newRequest.AlgToUse = AlgType.CC;
                AlgPathPlanning myAlg = new AlgCC(newRequest, mDist, mDiff, Efficiency_UB);
                lstThreads.Add(myAlg);

                //// TopTwo works best when there are two modes (n>=2)
                //if (ModeCount >= 2)
                //{
                //    newRequest = curRequest.DeepClone();
                //    newRequest.AlgToUse = AlgType.TopTwo;
                //    myAlg = new AlgTopTwo(newRequest, ModeCount, mModes, mDist, mDiff, Efficiency_UB);
                //    lstThreads.Add(myAlg);
                //}
                
                //// Use TopN if there are more than two modes
                //if (ModeCount >= 3)
                //{
                //    newRequest = curRequest.DeepClone();
                //    newRequest.AlgToUse = AlgType.TopN;
                //    if (ModeCount <= 5)
                //    {
                //        newRequest.TopN = ModeCount;
                //    }
                //    // This is really hierarchical search
                //    for (int i = 3; i < ModeCount + 1; i++)
                //    {
                //        PathPlanningRequest newR = newRequest.DeepClone();
                //        newR.TopN = i;
                //        myAlg = new AlgTopN(newR, ModeCount, mModes, mDist, mDiff, Efficiency_UB);
                //        lstThreads.Add(myAlg);
                //    }
                //}

                // TopTwo works best when there are two modes (n>=2)
                if (ModeCount == 2)
                {
                    newRequest = curRequest.DeepClone();
                    newRequest.AlgToUse = AlgType.TopTwo;
                    RtwMatrix mDistCopy = mDist.Clone();
                    RtwMatrix mDiffCopy = mDiff.Clone();
                    myAlg = new AlgTopTwo(newRequest, ModeCount, mModes, mDistCopy, mDiffCopy, Efficiency_UB);
                    lstThreads.Add(myAlg);
                }

                // Use TopN if there are more than two modes
                if (ModeCount >= 3)
                {
                    newRequest = curRequest.DeepClone();
                    newRequest.AlgToUse = AlgType.TopN;
                    if (ModeCount <= 5)
                    {
                        newRequest.TopN = ModeCount;
                    }
                    // This is really hierarchical search
                    for (int i = ModeCount; i < ModeCount + 1; i++)
                    {
                        PathPlanningRequest newR = newRequest.DeepClone();
                        newR.TopN = i;
                        RtwMatrix mDistCopy = mDist.Clone();
                        RtwMatrix mDiffCopy = mDiff.Clone();
                        myAlg = new AlgTopN(newR, ModeCount, mModes, mDistCopy, mDiffCopy, Efficiency_UB);
                        lstThreads.Add(myAlg);
                    }
                }

                SpawnThreads();
            }
            else
            {

            }

            //if (HierarchicalSearch())
            //{
            //    return;
            //}
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
            DateTime startTime2 = DateTime.Now;
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
            DateTime stopTime2 = DateTime.Now;
            TimeSpan duration2 = stopTime2 - startTime2;
            double RunTime2 = duration2.TotalSeconds;
            curRequest.SetLog(lstThreads[index].GetCurRequest().AlgToUse + " took " + RunTime2 + " seconds.\n");
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
        }
        
        #endregion
    }
}
