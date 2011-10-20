using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using rtwmatrix;

namespace IPPA
{
    class EAPath : IComparable
    {
        public double CDF;
        public List<int> DirPath = new List<int>();
        public List<Point> Path = new List<Point>();

        public int CompareTo(object obj)
        {
            EAPath Compare = (EAPath)obj;
            int result = this.CDF.CompareTo(Compare.CDF);
            if (result == 0)
            {
                result = this.CDF.CompareTo(Compare.CDF);
            }
            return result;
        }
    }

    class AlgEA : AlgPathPlanning
    {
        // TODO Implement EA

        #region Members

        // Private variables
        private int ModeCount = 0;
        private List<EAPath> CurGeneration = new List<EAPath>();
        private List<EAPath> NewGeneration = new List<EAPath>();

        // Public variables

        #endregion

        #region Constructor, Destructor

        // Constructor
        public AlgEA(PathPlanningRequest _curRequest, int _ModeCount, 
            RtwMatrix _mDistReachable, RtwMatrix _mDiffReachable, double _Efficiency_UB)
            : base (_curRequest, _mDistReachable, _mDiffReachable, _Efficiency_UB)
        {
            ModeCount = _ModeCount;
        }

        // Destructor
        ~AlgEA()
        {
            // Cleaning up
        }

        #endregion

        #region Other Functions

        // Algorithm specific implementation of the path planning
        protected override void DoPathPlanning()
        {
            #region Hiararchical Search

            // If using hiararchical search
            if (curRequest.UseHiararchy)
            {
                AlgPathPlanning curAlg = null;

                if (ModeCount == 0)
                {
                    // If uniform distribution, just do Complete Coverage
                    curAlg = new AlgCC(curRequest, mDist, mDiff, Efficiency_UB);
                    curAlg.PlanPath();
                }
                else if (!curRequest.UseTaskDifficultyMap               // No task difficulty map
                        && curRequest.DetectionRate == 1                // Detection rate is 1
                        && curRequest.T >= mDist.Rows*mDist.Columns)    // Time enough to cover entire map
                {
                    // If there's plenty of time, then just do Complete Coverage
                    curAlg = new AlgCC(curRequest, mDist, mDiff, Efficiency_UB);
                    curAlg.PlanPath();
                }
                else if (curRequest.UseTaskDifficultyMap                            // Use task difficulty map
                        && curRequest.DetectionType == DType.FixAmountInPercentage  // A fixed percentage of original
                        && curRequest.DetectionRate == 1                            // Detection rate is 1
                        && curRequest.T >= mDist.Rows*mDist.Columns
                        /curRequest.DiffRates[curRequest.MaxDifficulty-1])          // Time enough to cover entire map multiple times to wipe it clean
                {
                    // If there's plenty of time, then just do Complete Coverage
                    curAlg = new AlgCC(curRequest, mDist, mDiff, Efficiency_UB);
                    curAlg.PlanPath();
                }
                // If 1 mode
                if (ModeCount == 1)
                {
                    // Unimodal distribution or path distribution (possibly with splits)
                    return;
                }

                // If lots of modes, consider using PF as seed
                if (ModeCount > 4)
                {
                    return;
                }

                CDF = curAlg.GetCDF();
                Path = curAlg.GetPath();
                return;
            }

            #endregion 

            CurGeneration = CreatePopulation();
            CurGeneration.Sort();

            /*

            // Let's make sure all population has paths
            foreach (EAPath e in CurGeneration)
            {
                if (e.Path.Count == 0)
                {
                    System.Windows.Forms.MessageBox.Show("Path is empty!");
                    break;
                }
                if (CrossoverType == 1 && e.DirPath.Count == 0)
                {
                    System.Windows.Forms.MessageBox.Show("DirPath is empty!");
                    break;
                }
            }

            // Remaining population size (e.g. 100-3=97)
            P = Population - Keep;

            // What do we have so far?
            BestCDF = CurGeneration[CurGeneration.Count - 1].CDF;
            BestPoints.Clear();
            BestPoints.AddRange(CurGeneration[CurGeneration.Count - 1].Path);

            // Iterate until converge or until certain number of iterations
            int count = 1;
            List<float> Improvement = new List<float>();
            Improvement.Add(0);
            List<float> RememberCDF = new List<float>();
            RememberCDF.Add(CurGeneration[CurGeneration.Count - 1].CDF);
            float epsilon = 1;
            int min_run = PathPlanningSVM.ProjectConstants.EA_Minimum_Run;
            int max_run = PathPlanningSVM.ProjectConstants.EA_Maximum_Run;
            int epsilon_run = PathPlanningSVM.ProjectConstants.EA_E_Epsilon_Run;
            while (epsilon > 0 && count < max_run)
            {
                // If we already have the path, then no need to continue
                if (Math.Abs(BestCDF - UpperBound) < 0.001)
                {
                    break;
                }

                // Create a new generation

                // Add the best three to mid generation 
                // (First three in new generation are the best three from last generation)
                NewGeneration = CurGeneration.GetRange(P, Keep);

                // Array to store fitness proportion
                CDF = new float[Population];

                // 1. Select based on replace rate
                SelectPopulation();

                // 2. Crossover based on crossover rate
                Crossover();

                // 3. Add best to keep to end of list and don't mutate those
                for (int i = 0; i < Keep; i++)
                {
                    EAPath eap = new EAPath();
                    eap.CDF = NewGeneration[i].CDF;
                    eap.DirPath.AddRange(NewGeneration[i].DirPath);
                    eap.Path.AddRange(NewGeneration[i].Path);
                    NewGeneration.Add(eap);
                }

                // 4. Mutate based on mutate rate
                Mutate();

                // 5. Evaluate
                EvaluateFitness();
                NewGeneration.Sort();

                // 6. Update                    
                CurGeneration.Clear();
                CurGeneration.AddRange(NewGeneration.GetRange(NewGeneration.Count - Population, Population));

                // 7. Remember best and record improvement
                RememberCDF.Add(CurGeneration[Population - 1].CDF);
                Improvement.Add(CurGeneration[Population - 1].CDF - BestCDF);
                BestCDF = CurGeneration[Population - 1].CDF;
                BestPoints.Clear();
                BestPoints.AddRange(CurGeneration[Population - 1].Path);

                if (Improvement.Count < min_run)
                {
                    epsilon = 1;
                }
                else
                {
                    epsilon = 0;
                    for (int i = 0; i < epsilon_run; i++)
                    {
                        epsilon += Improvement[Improvement.Count - 1 - i];
                    }
                    epsilon = epsilon / epsilon_run;
                }

                // 7. Increase counter
                count++;

                // In case I need to terminate thread early
                // Record stop time
                stopTime = DateTime.Now;
                // Compute execution time
                duration = stopTime - startTime;

                // If algorithm runs more than specified duration, terminate 
                if (duration >= PathPlanningSVM.ProjectConstants.Duration)
                {
                    ThreadFinish();
                    return;
                }

                ThreadStop();
            }

            // Evolution completed...
            BestPoints.Clear();
            BestPoints.AddRange(CurGeneration[CurGeneration.Count - 1].Path);

            // Report how many iterations
            Log("Algorithm ran " + count.ToString() + " iterations.\n");

            // Print out improvement log
            Log("Improvement: ");
            for (int i = 0; i < Improvement.Count; i++)
            {
                Log(Improvement[i].ToString() + " ");
            }
            Log("\n");

            // Cleaning up
            Improvement.Clear();
            Improvement = null;

            */
        }

        // Function to create initial population of paths
        private List<EAPath> CreatePopulation()
        {
            List<EAPath> AllPaths = new List<EAPath>();

            // One option is to use other algorithms to generate initial population
            #region Add CC
            for (int i = 0; i < ProjectConstants.Count_CC; i++)
            {
                EAPath eap = new EAPath();
                AlgCC cc = new AlgCC(curRequest, mDiff, mDist, Efficiency_UB);
                cc.PlanPath();
                eap.CDF = cc.GetCDF();
                eap.Path.AddRange(cc.GetPath());
                cc = null;

                // If later crossover using flying directions, then we better draw direction path
                // Note: Only T directions (versus T+1 points in path)
                if (CrossoverType == 1)
                {
                    for (int j = 1; j < T + 1; j++)
                    {
                        eap.DirPath.Add(GetDirection(eap.Path[j], eap.Path[j - 1]));
                    }
                }

                // Add EAPath to population
                AllPaths.Add(eap);

                // If we already have the path, then no need to continue
                BestCDF = eap.CDF;
                if (Math.Abs(BestCDF - UpperBound) < 0.001)
                {
                    return AllPaths;
                }
            }
            #endregion

            #region Add LHC1
            for (int i = 0; i < 3; i++)
            {
                EAPath eap = new EAPath();
                GlobalWarming lhc = new GlobalWarming(null, null, null,
                    Start, mMap % mReachableRegion, mReachableRegion,
                    T, UpperBound, 1);
                lhc.PlanPath();
                eap.CDF = lhc.BestCDF;
                eap.Path.AddRange(lhc.BestPoints);
                lhc = null;

                // If later crossover using flying directions, then we better draw direction path
                // Note: Only T directions (versus T+1 points in path)
                if (CrossoverType == 1)
                {
                    for (int j = 1; j < T + 1; j++)
                    {
                        eap.DirPath.Add(GetDirection(eap.Path[j], eap.Path[j - 1]));
                    }
                }

                // Add EAPath to population
                AllPaths.Add(eap);

                // If we already have the path, then no need to continue
                BestCDF = eap.CDF;
                if (Math.Abs(BestCDF - UpperBound) < 0.001)
                {
                    return AllPaths;
                }
            }
            #endregion

            // Commented Code
            #region Add LHC2
            /*
                for (int i = 0; i < 1; i++)
                {
                    EAPath eap = new EAPath();
                    GlobalWarming lhc = new GlobalWarming(null, null, null,
                        Start, mMap % mReachableRegion, mReachableRegion,
                        T, UpperBound, 2);
                    lhc.PlanPath();
                    eap.CDF = lhc.BestCDF;
                    eap.Path.AddRange(lhc.BestPoints);
                    lhc = null;

                    // If later crossover using flying directions, then we better draw direction path
                    // Note: Only T directions (versus T+1 points in path)
                    if (CrossoverType == 1)
                    {
                        for (int j = 1; j < T + 1; j++)
                        {
                            eap.DirPath.Add(GetDirection(eap.Path[j], eap.Path[j - 1]));
                        }
                    }

                    // Add EAPath to population
                    AllPaths.Add(eap);
                  
                    // If we already have the path, then no need to continue
                    BestCDF = eap.CDF;
                    if (Math.Abs(BestCDF - UpperBound) < 0.001)
                    {
                        return AllPaths;
                    }
                }
                */
            #endregion

            #region Add Potential Field
            for (int i = 0; i < 1; i++)
            {
                EAPath eap = new EAPath();
                PFWrapper pfw = new PFWrapper(null, null, null,
                    Start, mMap, mReachableRegion,
                    T, UpperBound);
                pfw.PlanPath();
                eap.CDF = pfw.BestCDF;
                eap.Path.AddRange(pfw.BestPoints);
                pfw = null;

                // If later crossover using flying directions, then we better draw direction path
                // Note: Only T directions (versus T+1 points in path)
                if (CrossoverType == 1)
                {
                    for (int j = 1; j < T + 1; j++)
                    {
                        eap.DirPath.Add(GetDirection(eap.Path[j], eap.Path[j - 1]));
                    }
                }

                // Add EAPath to population
                AllPaths.Add(eap);

                // If we already have the path, then no need to continue
                BestCDF = eap.CDF;
                if (Math.Abs(BestCDF - UpperBound) < 0.001)
                {
                    return AllPaths;
                }
            }
            #endregion

            #region Rest try random paths
            // Loop through all population
            for (int i = AllPaths.Count; i < Population; i++)
            {
                // Generate each EAPath
                EAPath eap = new EAPath();
                eap.CDF = 0;

                // Create matrix to record repeated visit
                RtwMatrix mVisited = new RtwMatrix(DIM, DIM);
                mMap = mOriginalMap.Clone();

                // First add starting node to path
                Point start = new Point(Start.X, Start.Y);
                eap.Path.Add(start);
                eap.CDF += mMap[start.Y, start.X];
                mMap[start.Y, start.X] = 0;
                mVisited[start.Y, start.X] = 1;

                for (int j = 0; j < T; j++)
                {
                    List<Point> neighbors = new List<Point>();
                    Point parent;
                    Point me;
                    Point child;

                    // Find parent
                    if (eap.Path.Count < 2)
                    {
                        // It only has starting point
                        parent = start;
                        me = start;
                    }
                    else
                    {
                        parent = eap.Path[eap.Path.Count - 2];
                        me = eap.Path[eap.Path.Count - 1];
                    }


                    // Loop through all four directions (N, E, S, W)
                    for (int k = 0; k < 4; k++)
                    {
                        // Expand children
                        child = MISCLib.get_delta(k, me);
                        NodesExpanded++;

                        // Check if it's valid children (no repeat first)
                        if (MISCLib.ValidMove(parent, me, child, mReachableRegion, false, mVisited))
                        {
                            neighbors.Add(child);
                        }
                    }

                    // If no valid child (meaning have to repeat) then
                    if (neighbors.Count < 1)
                    {
                        // Loop through all four directions (N, E, S, W)
                        for (int k = 0; k < 4; k++)
                        {
                            // Expand children
                            child = MISCLib.get_delta(k, me);
                            NodesExpanded++;

                            // Check if it's valid children (allow repeat now)
                            if (MISCLib.ValidMove(parent, me, child, mReachableRegion, true, mVisited))
                            {
                                neighbors.Add(child);
                            }
                        }
                    }

                    // Decide which way to go.
                    int indexOfNext = PickRandomNode(neighbors.Count);
                    // Add node to path and then collect probability (zero it out)
                    Point next = neighbors[indexOfNext];
                    eap.Path.Add(next);
                    eap.CDF += mMap[next.Y, next.X];
                    mMap[next.Y, next.X] = 0;
                    mVisited[next.Y, next.X] = 1;
                }

                // If later crossover using flying directions, then we better draw direction path
                // Note: Only T directions (versus T+1 points in path)
                if (CrossoverType == 1)
                {
                    for (int j = 1; j < T + 1; j++)
                    {
                        eap.DirPath.Add(GetDirection(eap.Path[j], eap.Path[j - 1]));
                    }
                }

                // Add EAPath to population
                AllPaths.Add(eap);
            }
            #endregion

            return AllPaths;
        }

        // Debugging shouts
        public override void Shout()
        {
            Console.WriteLine("I am AlgEA!");
        }

        #endregion
    }
}
