using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using rtwmatrix;

namespace IPPA
{
    class AlgEA : AlgPathPlanning
    {
        // TODO Implement EA

        #region Members

        // Private variables
        private int ModeCount = 0;

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
/*            // If using hiararchical search
            if (curRequest.UseHiararchy)
            {
                AlgPathPlanning curAlg;

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
                Efficiency = curAlg.GetEfficiency();
                RunTime = curAlg.GetRunTime();
            }

            // Don't use this if T<10
            if (T < 10)
            {
                System.Windows.Forms.MessageBox.Show("Can't use this algorithm when T is smaller than 10!");
                return;
            }

            CurGeneration = CreatePopulation();
            CurGeneration.Sort();

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

        // Debugging shouts
        public override void Shout()
        {
            Console.WriteLine("I am AlgEA!");
        }

        #endregion
    }
}
