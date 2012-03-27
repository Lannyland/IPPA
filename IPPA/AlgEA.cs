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
        #region Members

        // Private variables
        private int ModeCount = 0;
        private List<EAPath> CurGeneration = new List<EAPath>();
        private List<EAPath> NewGeneration = new List<EAPath>();
        int PRemain = 0;
        private double[] ProbabilityCDF = new double[ProjectConstants.EA_Population];
        Random r = new Random((int)DateTime.Now.Ticks); 

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
            if (HiararchicalSearch())
            {
                return;
            }

            // Generate initial population
            CurGeneration = CreatePopulation();
            // Sort based on CDF. Best at the end.
            CurGeneration.Sort();
            // Debug
            CheckPathValidity(CurGeneration, "After CreatePopulation()");

            // Remaining population size (e.g. 100-3=97)
            PRemain = ProjectConstants.EA_Population - ProjectConstants.EA_BestToKeep;

            // What's the best we have so far?
            CDF = CurGeneration[CurGeneration.Count - 1].CDF;
            Path = new List<Point>();
            Path.AddRange(CurGeneration[CurGeneration.Count - 1].Path);

            // Prepare lists to store improvements
            List<double> Improvement = new List<double>();
            Improvement.Add(0);
            List<double> RememberCDF = new List<double>();
            RememberCDF.Add(CurGeneration[CurGeneration.Count - 1].CDF);

            // Iterate until converge or until certain number of iterations
            int count = 1;
            double epsilon = 1;
            while (epsilon > 0 && count < ProjectConstants.EA_Maximum_Run)
            {
                // If we already have the path, then no need to continue
                if (Math.Abs(CDF - Efficiency_UB) < 0.001)
                {
                    break;
                }

                // Create a new generation

                // 0. Keep the best three to mid generation 
                // (First three in new generation are the best three from last generation)
                NewGeneration = CurGeneration.GetRange(PRemain, ProjectConstants.EA_BestToKeep);
                // Debug
                CheckPathValidity(NewGeneration, "After Newgeneration keep best");


                // 1. Select based on replace rate
                SelectPopulation();
                // Debug
                CheckPathValidity(NewGeneration, "After SelectPopulation()");

                // 2. Crossover based on crossover rate
                Crossover();
                // Debug
                CheckPathValidity(NewGeneration, "After Crossover()");

                // 3. Add best to keep to end of list and don't mutate those
                for (int i = 0; i < ProjectConstants.EA_BestToKeep; i++)
                {
                    EAPath eap = new EAPath();
                    eap.CDF = NewGeneration[i].CDF;
                    eap.DirPath.AddRange(NewGeneration[i].DirPath);
                    eap.Path.AddRange(NewGeneration[i].Path);
                    NewGeneration.Add(eap);
                }
                // Debug
                CheckPathValidity(NewGeneration, "After add best 3 to end");

                // 4. Mutate based on mutate rate
                Mutate();
                // Debug
                CheckPathValidity(NewGeneration, "After Mutate()");

                // 5. Evaluate
                EvaluateFitness();
                NewGeneration.Sort();
                // Debug
                CheckPathValidity(NewGeneration, "After EvaluateFitness()");

                // 6. Update                    
                CurGeneration.Clear();
                CurGeneration.AddRange(NewGeneration.GetRange(NewGeneration.Count - ProjectConstants.EA_Population, ProjectConstants.EA_Population));
                // Debug
                CheckPathValidity(CurGeneration, "After generating new CurGeneration");

                // 7. Remember best and record improvement
                RememberCDF.Add(CurGeneration[ProjectConstants.EA_Population - 1].CDF);
                Improvement.Add(CurGeneration[ProjectConstants.EA_Population - 1].CDF - CDF);
                CDF = CurGeneration[ProjectConstants.EA_Population - 1].CDF;
                Path = new List<Point>();
                Path.AddRange(CurGeneration[ProjectConstants.EA_Population - 1].Path);
                // Debug
                CheckPathValidity(NewGeneration, "After remember best and record improvement");

                if (Improvement.Count < ProjectConstants.EA_Minimum_Run)
                {
                    epsilon = 1;
                }
                else
                {
                    epsilon = 0;
                    for (int i = 0; i < ProjectConstants.EA_Epsilon_Run; i++)
                    {
                        epsilon += Improvement[Improvement.Count - 1 - i];
                    }
                    epsilon = epsilon / ProjectConstants.EA_Epsilon_Run;
                }
                // Debug
                CheckPathValidity(NewGeneration, "After computing epsilon");

                // 7. Increase counter
                count++;
            }

            // Evolution completed...
            Path = new List<Point>();
            Path.AddRange(CurGeneration[CurGeneration.Count - 1].Path);

            // Report how many iterations
            curRequest.SetLog("Algorithm ran " + count.ToString() + " iterations.\n");

            // Print out improvement log
            curRequest.SetLog("Improvement: \n");
            for (int i = 0; i < Improvement.Count; i++)
            {
                curRequest.SetLog(Improvement[i].ToString() + " ");
            }
            curRequest.SetLog("\n");

            // Cleaning up
            Improvement.Clear();
            Improvement = null;
        }

        // Perform Hiarachical Search and be done if possible
        private bool HiararchicalSearch()
        {
            // If using hiararchical search
            if (curRequest.UseHierarchy)
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
                        && curRequest.T >= mDist.Rows * mDist.Columns)    // Time enough to cover entire map
                {
                    // If there's plenty of time, then just do Complete Coverage
                    curAlg = new AlgCC(curRequest, mDist, mDiff, Efficiency_UB);
                    curAlg.PlanPath();
                }
                else if (curRequest.UseTaskDifficultyMap                            // Use task difficulty map
                        && curRequest.DetectionType == DType.FixAmountInPercentage  // A fixed percentage of original
                        && curRequest.DetectionRate == 1                            // Detection rate is 1
                        && curRequest.T >= mDist.Rows * mDist.Columns
                        / curRequest.DiffRates[curRequest.MaxDifficulty - 1])          // Time enough to cover entire map multiple times to wipe it clean
                {
                    // If there's plenty of time, then just do Complete Coverage
                    curAlg = new AlgCC(curRequest, mDist, mDiff, Efficiency_UB);
                    curAlg.PlanPath();
                }
                // If 1 mode
                if (ModeCount == 1)
                {
                    // Unimodal distribution or path distribution (possibly with splits)
                    // return;
                }

                if (curAlg != null)
                {
                    CDF = curAlg.GetCDF();
                    Path = curAlg.GetPath();
                    return true;
                }
            }

            return false;
        }

        // Function to create initial population of paths
        private List<EAPath> CreatePopulation()
        {
            List<EAPath> AllPaths = new List<EAPath>();
            PathPlanningRequest newRequest = curRequest.Clone();
            AlgPathPlanning myAlg = null;

            // Use other algorithms to generate initial population
            newRequest.AlgToUse = AlgType.CC;
            if(GenerateSeeds(AllPaths, newRequest, myAlg, ProjectConstants.Count_CC))
            {
                return AllPaths;
            }
            newRequest.AlgToUse = AlgType.LHCGWCONV;
            if(GenerateSeeds(AllPaths, newRequest, myAlg, ProjectConstants.Count_LHCGWCONV))
            {
                return AllPaths;
            }
            newRequest.AlgToUse = AlgType.LHCGWPF;
            if(GenerateSeeds(AllPaths, newRequest, myAlg, ProjectConstants.Count_LHCGWPF))
            {
                return AllPaths;
            }
            newRequest.AlgToUse = AlgType.LHCRandom;
            if(GenerateSeeds(AllPaths, newRequest, myAlg, ProjectConstants.Count_LHCRandom))
            {
                return AllPaths;
            }
            newRequest.AlgToUse = AlgType.Random;
            if (GenerateSeeds(AllPaths, newRequest, myAlg, ProjectConstants.EA_Population - AllPaths.Count()))
            {
                return AllPaths;
            }

            return AllPaths;
        }

        // Function to actually generate seeds of various algorithms
        private bool GenerateSeeds(List<EAPath> AllPaths, PathPlanningRequest newRequest, AlgPathPlanning myAlg, int count)
        {
            for (int i = 0; i < count; i++)
            {
                switch (newRequest.AlgToUse)
                {
                    case AlgType.CC:
                        myAlg = new AlgCC(newRequest, mDist, mDiff, Efficiency_UB);
                        break;
                    case AlgType.LHCGWCONV:
                        myAlg = new AlgGlobalWarming(newRequest, ModeCount, mDist, mDiff, Efficiency_UB);
                        break;
                    case AlgType.LHCGWPF:
                        myAlg = new AlgGlobalWarming(newRequest, ModeCount, mDist, mDiff, Efficiency_UB);
                        break;
                    case AlgType.LHCRandom:
                        myAlg = new AlgLHCRandom(newRequest, mDist, mDiff, Efficiency_UB);
                        break;
                    case AlgType.Random:
                        myAlg = new AlgRandom(newRequest, mDist, mDiff, Efficiency_UB);
                        break;
                    default:
                        break;
                }
                myAlg.PlanPath();
                EAPath eap = new EAPath();
                eap.CDF = myAlg.GetCDF();
                eap.Path.AddRange(myAlg.GetPath());
                myAlg = null;

                // Add EAPath to population
                AllPaths.Add(eap);

                // If we already have the best path, then no need to continue
                CDF = eap.CDF;
                Path = eap.Path;
                if (Math.Abs(CDF - Efficiency_UB) < 0.001)
                {
                    return true;
                }
            }
            return false;
        }

        // Select Population based on replacement rate
        private void SelectPopulation()
        {
            // First build the probability table
            BuildProbabilityCDF();

            // Then proportionally select paths (based on replacement rate)
            SelectPaths();
        }

        // Actually select paths
        private void SelectPaths()
        {
            // Select based on replacement rate (97-.3*100)=67
            for (int j = 0; j < (PRemain - Convert.ToInt16(ProjectConstants.EA_ReplacementRate * ProjectConstants.EA_Population)); j++)
            {
                // Pick random number (0,1) (Three digit precision)
                double dblRand = (double)r.Next(0, 1000) / 1000;
                int index = 0;
                if (dblRand > ProbabilityCDF[0])
                {
                    for (int i = 1; i < PRemain; i++)
                    {

                        if (dblRand > ProbabilityCDF[i - 1] && dblRand <= ProbabilityCDF[i])
                        {
                            index = i;
                            break;
                        }
                    }
                }
                // Make sure I've not picked the best ones already kept
                if (index > PRemain)
                {
                    // Do it again
                    j--;
                }
                else
                {
                    NewGeneration.Add(CurGeneration[index]);
                }
            }
        }

        // Build the probability table for Proportional Select
        private void BuildProbabilityCDF()
        {
            double[] Pr = new double[ProjectConstants.EA_Population];
            double SumFitness = 0;

            // Always use proportional select
            for (int i = 0; i < ProjectConstants.EA_Population; i++)
            {
                SumFitness += CurGeneration[i].CDF;
            }
            for (int i = 0; i < ProjectConstants.EA_Population; i++)
            {
                Pr[i] = CurGeneration[i].CDF / SumFitness;
            }
            ProbabilityCDF[0] = Pr[0];
            for (int i = 1; i < ProjectConstants.EA_Population; i++)
            {
                ProbabilityCDF[i] = ProbabilityCDF[i - 1] + Pr[i];
            }
            ProbabilityCDF[ProjectConstants.EA_Population - 1] = 1;
        }

        // Crossover based on replacement rate
        private void Crossover()
        {
            // Generate remaining quota of the population by crossover (Divide by 2 because I am generating 2 children each time)
            for (int k = 0; k < Convert.ToInt16(ProjectConstants.EA_ReplacementRate * ProjectConstants.EA_Population / 2); k++)
            {
                // Find father and good mother
                int split_1_F, split_1_M;
                List<Point> Father, Mother;
                if (!FindFatherMother(out split_1_F, out split_1_M, out Father, out Mother))
                {
                    // Cleaning up
                    Father = null;
                    Mother = null;
                    // Did not find a good Mother
                    k--;
                    continue;
                }

                // Found a good mother that has the node
                // See if father and mother have second common node
                // And perform crossover
                int split_2_F = -1;
                int split_2_M = -1;
                EAPath Son = new EAPath();
                EAPath Daughter = new EAPath();
                bool blnTwoCommonNodes = FindSecondCommonNode(split_1_F, split_1_M, Father, Mother, ref split_2_F, ref split_2_M);
                if (blnTwoCommonNodes)
                {
                    DoublePointCrossover(split_1_F, split_1_M, Father, Mother, split_2_F, split_2_M, Son, Daughter);
                }
                else
                {
                    SinglePointCrossover(split_1_F, split_1_M, Father, Mother, Son, Daughter);
                }

                // Make sure there's no flying backward for non-copter
                if (curRequest.VehicleType != UAVType.Copter)
                {
                    bool blnGoodCrossover = GoodCrossover(split_1_F, split_1_M, split_2_F, split_2_M, Son, Daughter, blnTwoCommonNodes);
                    if (!blnGoodCrossover)
                    {
                        // Cleaning up
                        Father = null;
                        Mother = null;
                        Son = null;
                        Daughter = null;
                        // Try again
                        k--;
                        continue;
                    }
                }

                // Make sure they still fly all the way.
                // Now both children are valid paths, let's truncate or extend so they all have length T+1;
                // After crossover, if Son or Daughter becomes too long, truncate
                EAPath ShorterPath = new EAPath();
                EAPath LongerPath = new EAPath();
                TruncateLongerPath(Son, Daughter, ref ShorterPath, ref LongerPath);
                if (LongerPath.Path.Count == 0)
                {
                    // Both Son and Daughter have right length. Move on!
                    NewGeneration.Add(Son);
                    NewGeneration.Add(Daughter);
                    continue;
                }

                // After crossover, if Son or Daughter becomes too short, 
                // probablistically randomly pick another path and crossover again.
                // This time only keep the longer one and then truncate.
                EAPath OldLongerPath = LongerPath;
                int index1 = -1;
                Father = ShorterPath.Path;
                if (!FindMother(index1, Father, out split_1_F, out split_1_M, out Mother))
                {
                    // Cleaning up
                    Father = null;
                    Mother = null;
                    // Did not find a good Mother
                    k--;
                    continue;
                }
                // Found a good mother that has the node
                Son = new EAPath();
                Daughter = new EAPath();
                SinglePointCrossover(split_1_F, split_1_M, Father, Mother, Son, Daughter);
                bool blnSonLonger = TruncateLongerPath(Son, Daughter, ref ShorterPath, ref LongerPath);
                EAPath NewLongerPath = LongerPath;

                // Make sure there's no flying backward for non-copter
                if (curRequest.VehicleType != UAVType.Copter)
                {
                    // Only worry about LongerPath
                    if (!blnSonLonger)
                    {
                        split_1_F = split_1_M;
                        Son = Daughter;
                    }
                    bool blnGoodCrossover = GoodCrossover(split_1_F, split_1_F, 0, 0, Son, Son, false);
                    if (!blnGoodCrossover)
                    {
                        // Cleaning up
                        Father = null;
                        Mother = null;
                        Son = null;
                        Daughter = null;
                        LongerPath = null;
                        ShorterPath = null;
                        OldLongerPath = null;
                        NewLongerPath = null;
                        // Try again
                        k--;
                        continue;
                    }
                }
                
                // Add new paths to new generation
                NewGeneration.Add(OldLongerPath);
                NewGeneration.Add(NewLongerPath);

                // Cleaning up
                Father = null;
                Mother = null;
                Son = null;
                Daughter = null;
                LongerPath = null;
                ShorterPath = null;
                OldLongerPath = null;
                NewLongerPath = null;
            }
        }

        // Probabilistically select a parent
        private int SelectParent()
        {
            // Pick random number (0,1)
            double dblRand = (double)r.Next(0, 1000) / 1000;
            int index = 0;                                        
            if (dblRand <= ProbabilityCDF[0])
            {
                index = 0;
            }
            else
            {
                for (int i = 1; i < ProjectConstants.EA_Population; i++)
                {
                    if (dblRand > ProbabilityCDF[i - 1] && dblRand <= ProbabilityCDF[i])
                    {
                        index = i;
                        break;
                    }
                }
            }
            return index;
        }

        // Find father rand good mother
        private bool FindFatherMother(out int split_1_F, out int split_1_M, out List<Point> Father, out List<Point> Mother)
        {
            // Proportionately pick a father
            int index1 = SelectParent();
            Father = CurGeneration[index1].Path;
            return FindMother(index1, Father, out split_1_F, out split_1_M, out Mother);
        }

        // Once father is known, find mother
        private bool FindMother(int index1, List<Point> Father, out int split_1_F, out int split_1_M, out List<Point> Mother)
        {
            // Randomly pick a node in Father(1,T-1)
            split_1_F = r.Next(1, Father.Count()-1);
            Point first = Father[split_1_F];

            // Need to find a mother with same node
            int index2 = -1;
            split_1_M = -1;
            // Loop until we find a Mother that also has this node
            // If still unseccessful after 10 times, then pick new Father and new Mother (start this step over)
            int LoopCount = 0;
            do
            {
                LoopCount++;
                // Make sure I am not using the same one to crossover
                do
                {
                    index2 = SelectParent();
                } while (index1 == index2);
                Mother = CurGeneration[index2].Path;
                split_1_M = Mother.IndexOf(first);
            } while (split_1_M == -1 && LoopCount < 10);

            // Did we find a good mother?
            if (split_1_M == -1)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        
        // See if father and mother have second common node
        private bool FindSecondCommonNode(int split_1_F, int split_1_M, List<Point> Father, List<Point> Mother, ref int split_2_F, ref int split_2_M)
        {
            // Look from the end of Father and see if mother also has a second common node (also from end)
            for (int i = Father.Count - 1; i > split_1_F + 1; i--)
            {
                split_2_M = Mother.IndexOf(Father[i], split_1_M);
                if (split_2_M != -1)
                {
                    split_2_F = i;
                    return true;
                }
            }
            return false;
        }

        // Single Point Crossover
        private void SinglePointCrossover(int split_1_F, int split_1_M, List<Point> Father, List<Point> Mother, EAPath Son, EAPath Daughter)
        {
            Son.Path.AddRange(Father.GetRange(0, split_1_F));
            Son.Path.AddRange(Mother.GetRange(split_1_M, Mother.Count() - split_1_M));
            Daughter.Path.AddRange(Mother.GetRange(0, split_1_M));
            Daughter.Path.AddRange(Father.GetRange(split_1_F, Father.Count() - split_1_F));
        }

        // Dobule Point Crossover
        private void DoublePointCrossover(int split_1_F, int split_1_M, List<Point> Father, List<Point> Mother, int split_2_F, int split_2_M, EAPath Son, EAPath Daughter)
        {
            Son.Path.AddRange(Father.GetRange(0, split_1_F));
            Son.Path.AddRange(Mother.GetRange(split_1_M, split_2_M - split_1_M));
            Son.Path.AddRange(Father.GetRange(split_2_F, Father.Count() - split_2_F));
            Daughter.Path.AddRange(Mother.GetRange(0, split_1_M));
            Daughter.Path.AddRange(Father.GetRange(split_1_F, split_2_F - split_1_F));
            Daughter.Path.AddRange(Mother.GetRange(split_2_M, Mother.Count() - split_2_M));
        }

        // Check if crossover points are good
        private bool GoodCrossover(int split_1_F, int split_1_M, int split_2_F, int split_2_M, EAPath Son, EAPath Daughter, bool blnTwoCommonNodes)
        {
            if (blnTwoCommonNodes)
            {
                if (ValidCrossover(split_1_F, Son, 2)               // First crossover point for Son
                    && ValidCrossover(split_1_F, Son, 1)            // First crossover point for Son
                    && ValidCrossover(split_1_M, Daughter, 2)       // First crossover point for Daughter
                    && ValidCrossover(split_1_M, Daughter, 1)       // First crossover point for Daughter
                    && ValidCrossover(split_1_F + split_2_M - split_1_M, Son, 2)        // Second crossover point for Son
                    && ValidCrossover(split_1_F + split_2_M - split_1_M, Son, 1)        // Second crossover point for Son
                    && ValidCrossover(split_1_M + split_2_F - split_1_F, Daughter, 2)   // Second crossover point for Daughter
                    && ValidCrossover(split_1_M + split_2_F - split_1_F, Daughter, 1))  // Second crossover point for Daughter
                {
                    // Both crossover points are good
                    return true;
                }
                else
                {
                    // Some crossover point is bad
                    return false;
                }
            }
            else
            {
                if (ValidCrossover(split_1_F, Son, 2)           // First crossover point for Son
                    && ValidCrossover(split_1_F, Son, 1)        // First crossover point for Son
                    && ValidCrossover(split_1_M, Daughter, 2)   // First crossover point for Daughter
                    && ValidCrossover(split_1_M, Daughter, 1))  // First crossover point for Daughter
                {
                    // First crossover is good
                    return true;
                }
                else
                {
                    // First crossover is bad
                    return false;
                }
            }
        }

        // Check if the crossover is valid for non-copter (no flying backwards)
        private bool ValidCrossover(int split_point, EAPath Son, int i)
        {
            if (split_point != -1 && Son.Path.Count - 1 > split_point && split_point > 1)
            {
                if (!ValidMove(Son.Path[split_point - i], Son.Path[split_point - i + 1], Son.Path[split_point - i + 2]))
                {
                    return false;
                }
            }
            return true;
        }

        // Truncate longer path and identify who is who
        private bool TruncateLongerPath(EAPath Son, EAPath Daughter, ref EAPath ShorterPath, ref EAPath LongerPath)
        {
            if (Son.Path.Count > curRequest.T + 1)
            {
                Son.Path.RemoveRange(curRequest.T + 1, Son.Path.Count - (curRequest.T + 1));
                LongerPath = Son;
                ShorterPath = Daughter;
                return true;
            }
            if (Daughter.Path.Count > curRequest.T + 1)
            {
                Daughter.Path.RemoveRange(curRequest.T + 1, Daughter.Path.Count - (curRequest.T + 1));
                LongerPath = Daughter;
                ShorterPath = Son;
                return false;
            }
            return true;
        }

        // Mutate based on mutation rate
        private void Mutate()
        {
            //TODO Fix the flying back bug here.
            List<int> RemoveIndex = new List<int>();
            int mRate = Convert.ToInt16(ProjectConstants.EA_MutationRate * 100);
            for (int i = 0; i < ProjectConstants.EA_Population; i++)
            {
                EAPath eap = NewGeneration[i];
                // For each path in population, probabilistically decide if it should be mutated
                if (r.Next(0, 100) < mRate)
                {
                    // Mutate
                    bool blnMutationSuccessful = false;
                    while (!blnMutationSuccessful)
                    {
                        // Randomly select a point in the path (and the related neighbors)
                        int mark = r.Next(1, curRequest.T - 2);
                        Point p0 = eap.Path[mark - 1];
                        Point p1 = eap.Path[mark];
                        Point p2 = eap.Path[mark + 1];
                        Point p3 = eap.Path[mark + 2];

                        Point new_p2 = new Point();
                        Point new_p3 = new Point();
                        if (IsLShape(p1, p2, p3))
                        {
                            // If L shape, then use mutation type 1
                            if (p2.X == p1.X && p2.Y == p3.Y)
                            {
                                new_p2.X = p3.X;
                                new_p2.Y = p1.Y;
                            }
                            if (p2.X == p3.X && p2.Y == p1.Y)
                            {
                                new_p2.X = p1.X;
                                new_p2.Y = p3.Y;
                            }
                            if (new_p2.X == p0.X && new_p2.Y == p0.Y)
                            {
                                // This would make it flying backward. Not allowed
                                blnMutationSuccessful = false;
                            }
                            else
                            {
                                // Good mutation, modify path
                                Point np2 = new Point(new_p2.X, new_p2.Y);
                                eap.Path[mark + 1] = np2;
                                // No need to loop
                                blnMutationSuccessful = true;
                            }
                        }
                        else
                        {
                            // If straight line, then use mutation type 2
                            int dir = 0;
                            if (IsLShape(p0, p1, p2))
                            {
                                // If parent is on one side of the straight line, go to the other side
                                int p0_dir = GetDirection(p0, p1);
                                switch (p0_dir)
                                {
                                    case 0:
                                        // p0 is on the north, meaning p1, p2 on same row. Fly south
                                        dir = 2;
                                        new_p2.X = p1.X;
                                        new_p2.Y = p1.Y + 1;
                                        new_p3.X = p2.X;
                                        new_p3.Y = p2.Y + 1;
                                        break;
                                    case 1:
                                        // p0 is on the east, meaning p1, p2 on same column. Flying west
                                        dir = 3;
                                        new_p2.X = p1.X - 1;
                                        new_p2.Y = p1.Y;
                                        new_p3.X = p2.X - 1;
                                        new_p3.Y = p2.Y;
                                        break;
                                    case 2:
                                        // p0 is on teh south, meaning p1, p2 on same row. Fly north
                                        dir = 0;
                                        new_p2.X = p1.X;
                                        new_p2.Y = p1.Y - 1;
                                        new_p3.X = p2.X;
                                        new_p3.Y = p2.Y - 1;
                                        break;
                                    case 3:
                                        // P0 is on the west, meaning p1, p2 on same column. Fly east
                                        dir = 1;
                                        new_p2.X = p1.X + 1;
                                        new_p2.Y = p1.Y;
                                        new_p3.X = p2.X + 1;
                                        new_p3.Y = p2.Y;
                                        break;
                                }
                            }
                            else
                            {
                                // If parent is on same line, choose the side that's not in path
                                if (p0.X == p1.X)
                                {
                                    // p1, p2, p3 on same column, flying west or east
                                    // What if flying west?
                                    dir = 3;
                                    new_p2.X = p1.X - 1;
                                    new_p2.Y = p1.Y;
                                    new_p3.X = p2.X - 1;
                                    new_p3.Y = p2.Y;
                                    if (eap.Path.Contains(new_p2) || eap.Path.Contains(new_p3))
                                    {
                                        // Flying west is bad choice. Try flying east
                                        dir = 1;
                                        new_p2.X = p1.X + 1;
                                        new_p2.Y = p1.Y;
                                        new_p3.X = p2.X + 1;
                                        new_p3.Y = p2.Y;
                                        if (eap.Path.Contains(new_p2) || eap.Path.Contains(new_p3))
                                        {
                                            // This direction is just as bad! Let's randomly pick one
                                            int offset = r.Next(0, 2) * 2;
                                            offset--;
                                            new_p2.X = p1.X + offset;
                                            new_p2.Y = p1.Y;
                                            new_p3.X = p2.X + offset;
                                            new_p3.Y = p2.Y;
                                            dir = 2 - offset;
                                        }
                                    }
                                }
                                else
                                {
                                    // p1, p2, p3 on same row, flying north or south
                                    // What if flying north?
                                    dir = 0;
                                    new_p2.X = p1.X;
                                    new_p2.Y = p1.Y - 1;
                                    new_p3.X = p2.X;
                                    new_p3.Y = p2.Y - 1;
                                    if (eap.Path.Contains(new_p2) || eap.Path.Contains(new_p3))
                                    {
                                        // Flying north is bad choice. Try flying south
                                        dir = 2;
                                        new_p2.X = p1.X;
                                        new_p2.Y = p1.Y + 1;
                                        new_p3.X = p2.X;
                                        new_p3.Y = p2.Y + 1;
                                        if (eap.Path.Contains(new_p2) || eap.Path.Contains(new_p3))
                                        {
                                            // This direction is just as bad! Let's randomly pick one
                                            int offset = r.Next(0, 2) * 2;
                                            offset--;
                                            new_p2.X = p1.X;
                                            new_p2.Y = p1.Y + offset;
                                            new_p3.X = p2.X;
                                            new_p3.Y = p2.Y + offset;
                                            dir = 1 + offset;
                                        }
                                    }
                                }
                            }
                            // Let's  make sure these new points are valid points
                            if (ValidMove(p0, p1, new_p2))
                            {
                                if (ValidMove(p1, new_p2, new_p3))
                                {
                                    blnMutationSuccessful = true;
                                }
                                else
                                {
                                    // If there are invalid points, then mutation failed.
                                    blnMutationSuccessful = false;
                                }
                            }
                            else
                            {
                                // If there are invalid points, then mutation failed.
                                blnMutationSuccessful = false;
                            }

                            if (blnMutationSuccessful)
                            {
                                // Mutation successful. Let's add these points to path
                                eap.Path.Insert(mark + 1, new_p2);
                                eap.Path.Insert(mark + 2, new_p3);
                                // Now the path is two points longer. Truncate at the end
                                eap.Path.RemoveRange(curRequest.T + 1, 2);
                            }
                        }
                    }
                }
                else
                {
                    // Don't mutate
                    if (i < ProjectConstants.EA_BestToKeep)
                    {
                        // Remember which one to remove
                        RemoveIndex.Add(i + ProjectConstants.EA_Population);
                    }
                }
            }

            // Make sure the best paths to keep don't show up twice in new generation
            for (int i = RemoveIndex.Count - 1; i > -1; i--)
            {
                NewGeneration.RemoveAt(RemoveIndex[i]);
            }
        }

        //Function to determine if the three points are in an L shape
        private bool IsLShape(Point p1, Point p2, Point p3)
        {
            if (p1.X == p2.X && p2.X == p3.X)
            {
                return false;
            }
            else if (p1.Y == p2.Y && p2.Y == p3.Y)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        // Evaluate fitness
        private void EvaluateFitness()
        {
            for (int i = 0; i < NewGeneration.Count; i++)
            {
                // For each path in population
                EAPath eap = NewGeneration[i];
                eap.CDF = GetTrueCDF(eap.Path);
            }
        }
        
        // Debugging shouts
        public override void Shout()
        {
            Console.WriteLine("I am AlgEA!");
        }

        // Debug code to make sure all eaps have path
        private void CheckGeneration(List<EAPath> EAPList)
        {
            Console.WriteLine("List has " + EAPList.Count() + " paths.");
            foreach (EAPath eap in EAPList)
            {
                if (eap.Path.Count == 0)
                {
                    System.Windows.Forms.MessageBox.Show("Bummer!");
                    return;
                }
            }
        }

        // Debug code to make sure path length is always T+1;
        private void CheckPathLength()
        {
            foreach (EAPath e in CurGeneration)
            {
                if (e.Path.Count() != curRequest.T + 1)
                {
                    System.Windows.Forms.MessageBox.Show("Bummer!");
                    return;
                }
            }
            foreach (EAPath e in NewGeneration)
            {
                if (e.Path.Count() != curRequest.T + 1)
                {
                    System.Windows.Forms.MessageBox.Show("Bummer!");
                    return;
                }
            }
        }

        // Debug code to do sanity check on path
        private void CheckPathValidity(List<EAPath> EAPList, string s)
        {
            foreach (EAPath eap in EAPList)
            {
                // PathSanityCheck(eap.Path);
            }
        }
        
        #endregion
    }
}
