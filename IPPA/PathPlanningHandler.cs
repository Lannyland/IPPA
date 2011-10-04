using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rtwmatrix;

namespace IPPA
{
    // One PathPlanningHandler per PathPlanningRequest
    // Meaning it can handle batch runs and compute mean and standard deviation
    // Also deals with counting modes, computing reachable areas and efficiency lower bound 
    class PathPlanningHandler
    {
        #region Members

        // Private members
        private PathPlanningRequest curRequest;
        private double AvgRunTime = 0;
        private double StdRunTime = 0;
        private double AvgEfficiency = 0;
        private double StdEfficiency = 0;            

        #endregion

        #region Constructor, Destructor

        // Constructor
        public PathPlanningHandler(PathPlanningRequest _curRequest)
        {
            curRequest = _curRequest;
        }

        // Destructor
        ~PathPlanningHandler()
        {
            // Cleaning up
            curRequest = null;
        }

        #endregion

        #region Other Functions

        public void Run()
        {
            int BatchCount = 1;

            if (curRequest.BatchRun)
            {
                BatchCount = curRequest.RunTimes;
            }
            
            // First do reachable area (Dist and Diff)
            // TODO Implement method to compute reachable areas
            RtwMatrix mDistReachable = curRequest.DistMap;
            RtwMatrix mDiffReachable = curRequest.DiffMap;

            // Then do mode count (If Diff is used, multiply first)
            // TODO Implenment Diff map            
            CountDistModes myCount = new CountDistModes(ref mDistReachable);
            int ModeCount = myCount.GetCount();
            myCount = null;

            // Then do efficiency lower bound
            ComputeEfficiencyLB myELB = new ComputeEfficiencyLB(curRequest);
            double Efficiency_LB = myELB.GetEfficiency_LB();
            myELB = null;

            // Do the batch run of Path Planning
            List<double> AllRunTimes = new List<double>();
            List<double> AllEfficiencies = new List<double>();
            for (int i = 0; i < BatchCount; i++)
            {
                // Run them sequencially and don't use multiple threads.
                PathPlanningTask curTask = new PathPlanningTask(curRequest, ModeCount, mDistReachable, mDiffReachable, Efficiency_LB);
                curTask.Run();
                AllRunTimes.Add(curTask.GetRunTime());
                AvgRunTime += curTask.GetRunTime();
                AllEfficiencies.Add(curTask.GetEfficiency());
                AvgEfficiency += curTask.GetEfficiency();
                curTask = null;
            }
            AvgRunTime = AvgRunTime / BatchCount;
            StdRunTime = ComputeStDev(AllRunTimes, AvgRunTime);
            AvgEfficiency = AvgEfficiency / BatchCount;
            StdEfficiency = ComputeStDev(AllEfficiencies, AvgEfficiency);
            
            // Log path planning activities
            curRequest.SetLog("----------------------------------------------\n");
            curRequest.SetLog("Average run time: " + AvgRunTime.ToString() + "\n");
            curRequest.SetLog("Standard deviation: " + StdRunTime.ToString() + "\n");
            curRequest.SetLog("Average efficiency: " + AvgEfficiency.ToString() + "\n");
            curRequest.SetLog("Standard deviation: " + StdEfficiency.ToString() + "\n");
            curRequest.SetLog("----------------------------------------------");
            curRequest.SetLog("----------------------------------------------\n");
        }

        // Compute standard deviation given a list and average
        private double ComputeStDev(List<double> list, double avg)
        {
            double StDev = 0;
            foreach (double f in list)
            {
                StDev += Math.Pow(f - avg, 2); 
            }
            StDev = StDev / list.Count;
            StDev = Math.Sqrt(StDev);
            return StDev;
        }

        #region Getters
        public double GetAvgRunTime()
        {
            return AvgRunTime;
        }
        public double GetStdEfficiency()
        {
            return StdRunTime;
        }        
        #endregion

        #endregion




    }
}
