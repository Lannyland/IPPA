using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rtwmatrix;
using System.Drawing;

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
        private List<Point> Path;

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
            RtwMatrix mDistReachable;
            RtwMatrix mDiffReachable;

            if (curRequest.T < curRequest.DistMap.Rows + curRequest.DistMap.Columns)
            {
                mDistReachable = curRequest.DistMap.Clone();
                mDiffReachable = curRequest.DiffMap.Clone();
                if (!ComputeReachableArea(mDistReachable, mDiffReachable))
                {
                    // Cannot plan path.
                    return;
                }
            }
            else
            {
                mDistReachable = curRequest.DistMap.Clone();
                mDiffReachable = curRequest.DiffMap.Clone();
            }

            // Then do mode count (If Diff map is used, multiply first)
            CountDistModes myCount;
            if (curRequest.UseTaskDifficultyMap)
            {

                RtwMatrix mRealModes = new RtwMatrix(mDistReachable.Rows, mDistReachable.Columns);
                for (int i = 0; i < mRealModes.Rows; i++)
                {
                    for (int j = 0; j < mRealModes.Columns; j++)
                    {
                        mRealModes[i, j] = mDistReachable[i, j] *
                            (float)curRequest.DiffRates[Convert.ToInt32(mDiffReachable[i, j])];
                    }
                }
                myCount = new CountDistModes(mRealModes);
            }
            else
            {
                myCount = new CountDistModes(mDistReachable);
            }
            
            int ModeCount = myCount.GetCount();
            RtwMatrix mModes = myCount.GetModes();
            myCount = null;
            //Console.WriteLine("ModeCount = " + ModeCount);

            // Then do efficiency lower bound
            ComputeEfficiencyUB myELB = new ComputeEfficiencyUB(curRequest, mDistReachable, mDiffReachable);
            double Efficiency_UB = myELB.GetEfficiency_UB();
            myELB = null;

            // Do the batch run of Path Planning
            List<double> AllRunTimes = new List<double>();
            List<double> AllEfficiencies = new List<double>();
            for (int i = 0; i < BatchCount; i++)
            {
                // Run them sequencially and don't use multiple threads.
                PathPlanningTask curTask = new PathPlanningTask(curRequest, ModeCount, mModes, mDistReachable, mDiffReachable, Efficiency_UB);
                curTask.Run();
                AllRunTimes.Add(curTask.GetRunTime());
                AvgRunTime += curTask.GetRunTime();
                AllEfficiencies.Add(curTask.GetEfficiency());
                AvgEfficiency += curTask.GetEfficiency();
                Path = curTask.GetPath();
                curTask = null;
            }
            AvgRunTime = AvgRunTime / BatchCount;
            StdRunTime = ComputeStDev(AllRunTimes, AvgRunTime);
            AvgEfficiency = AvgEfficiency / BatchCount;
            StdEfficiency = ComputeStDev(AllEfficiencies, AvgEfficiency);
            
            // Log path planning activities
            if (ProjectConstants.DebugMode)
            {
                curRequest.SetLog("----------------------------------------------\n");
                curRequest.SetLog("Average run time: " + AvgRunTime.ToString() + "\n");
                curRequest.SetLog("Standard deviation: " + StdRunTime.ToString() + "\n");
                curRequest.SetLog("Average efficiency: " + AvgEfficiency.ToString() + "\n");
                curRequest.SetLog("Standard deviation: " + StdEfficiency.ToString() + "\n");
                curRequest.SetLog("----------------------------------------------");
                curRequest.SetLog("----------------------------------------------\n");
            }
        }

        // Compute the reachable area and might as well compute distance to closest non-zero node
        private bool ComputeReachableArea(RtwMatrix mDistReachable, RtwMatrix mDiffReachable)
        {
            // Code is cleaner to just deal two cases seperately.
            if (!curRequest.UseEndPoint)
            {
                Point Start = new Point(curRequest.pStart.column, curRequest.pStart.row);
                int d = curRequest.T;
                for (int y = 0; y < mDistReachable.Rows; y++)
                {
                    for (int x = 0; x < mDistReachable.Columns; x++)
                    {
                        int dist = MISCLib.ManhattanDistance(x, y, Start.X, Start.Y);
                        if (dist >= curRequest.T)
                        {
                            // Wipe cell in both maps clean
                            mDistReachable[y, x] = 0;
                            mDiffReachable[y, x] = 0;
                        }
                        if (dist < d && mDistReachable[y, x] != 0)
                        {
                            d = dist;
                        }
                    }
                }
                curRequest.d = d;
            }
            else
            {
                Point Start = new Point(curRequest.pStart.column, curRequest.pStart.row);
                Point End = new Point(curRequest.pEnd.column, curRequest.pEnd.row);
                int d = curRequest.T;
                int dist = MISCLib.ManhattanDistance(Start.X, Start.Y, End.X, End.Y);

                if (dist > curRequest.T)
                {
                    // Impossible to get from A to B in allowed flight time
                    System.Windows.Forms.MessageBox.Show("Impossible! Extend flight time!");
                    return false;
                }

                if (curRequest.T % 2 != dist % 2)
                {
                    // Impossible to get from A to B in the exact allowed flight time
                    System.Windows.Forms.MessageBox.Show("Impossible to reach end point at time T! Add 1 or minus 1!");
                    return false;
                }

                for (int y = 0; y < mDistReachable.Rows; y++)
                {
                    for (int x = 0; x < mDistReachable.Columns; x++)
                    {
                        int dist_AC = MISCLib.ManhattanDistance(x, y, Start.X, Start.Y);
                        int dist_BC = MISCLib.ManhattanDistance(x, y, End.X, End.Y);
                        if ((dist_AC + dist_BC) > curRequest.T)
                        {
                            // Wipe cell in both maps clean
                            mDistReachable[y, x] = 0;
                            mDiffReachable[y, x] = 0;
                        }
                        dist = MISCLib.ManhattanDistance(x, y, Start.X, Start.Y);
                        if (dist < d && mDistReachable[y, x] != 0)
                        {
                            d = dist;
                        }
                    }
                }
                curRequest.d = d;

            }            
            return true;
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
        public double GetStdRunTime()
        {
            return StdRunTime;
        }
        public double GetAvgEfficiency()
        {
            return AvgEfficiency;
        }
        public double GetStdEfficiency()
        {
            return StdRunTime;
        }
        public List<Point> GetPath()
        {
            return Path;
        }
        
        #endregion

        #endregion      
    }
}
