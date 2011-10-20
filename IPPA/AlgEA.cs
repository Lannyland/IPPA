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
        int PRemain;
        private double[] Fitness;
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
                    // return;
                }

                // If lots of modes, consider using PF as seed
                if (ModeCount > 4)
                {
                    // return;
                }

                if (curAlg != null)
                {
                    CDF = curAlg.GetCDF();
                    Path = curAlg.GetPath();
                    return;
                }
            }

            #endregion 

            CurGeneration = CreatePopulation();
            CheckGeneration(CurGeneration);
            CurGeneration.Sort();

            // Remaining population size (e.g. 100-3=97)
            PRemain = ProjectConstants.EA_Population - ProjectConstants.EA_BestToKeep;

            // What do we have so far?
            CDF = CurGeneration[CurGeneration.Count - 1].CDF;
            Path.Clear();
            Path.AddRange(CurGeneration[CurGeneration.Count - 1].Path);

            // Iterate until converge or until certain number of iterations
            int count = 1;
            List<double> Improvement = new List<double>();
            Improvement.Add(0);
            List<double> RememberCDF = new List<double>();
            RememberCDF.Add(CurGeneration[CurGeneration.Count - 1].CDF);
            double epsilon = 1;
            while (epsilon > 0 && count < ProjectConstants.EA_Maximum_Run)
            {
                // If we already have the path, then no need to continue
                if (Math.Abs(CDF - Efficiency_UB) < 0.001)
                {
                    break;
                }

                // Create a new generation

                // Add the best three to mid generation 
                // (First three in new generation are the best three from last generation)
                NewGeneration = CurGeneration.GetRange(PRemain, ProjectConstants.EA_BestToKeep);

                // Array to store fitness proportion
                Fitness = new double[ProjectConstants.EA_Population];

                // 1. Select based on replace rate
                SelectPopulation();

                // 2. Crossover based on crossover rate
                Crossover();

                // 3. Add best to keep to end of list and don't mutate those
                for (int i = 0; i < ProjectConstants.EA_BestToKeep; i++)
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
                CurGeneration.AddRange(NewGeneration.GetRange(NewGeneration.Count - ProjectConstants.EA_Population, ProjectConstants.EA_Population));

                // 7. Remember best and record improvement
                RememberCDF.Add(CurGeneration[ProjectConstants.EA_Population - 1].CDF);
                Improvement.Add(CurGeneration[ProjectConstants.EA_Population - 1].CDF - CDF);
                CDF = CurGeneration[ProjectConstants.EA_Population - 1].CDF;
                Path.Clear();
                Path.AddRange(CurGeneration[ProjectConstants.EA_Population - 1].Path);

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

                // 7. Increase counter
                count++;
            }

            // Evolution completed...
            Path.Clear();
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

        // Debug code to make sure all eaps have path
        private void CheckGeneration(List<EAPath> EAPList)
        {
            foreach (EAPath eap in EAPList)
            {
                if (eap.Path.Count == 0)
                {
                    System.Windows.Forms.MessageBox.Show("Bummer!");
                    return;
                }
            }
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
            Fitness[0] = Pr[0];
            for (int i = 1; i < ProjectConstants.EA_Population; i++)
            {
                Fitness[i] = Fitness[i - 1] + Pr[i];
            }
            Fitness[ProjectConstants.EA_Population - 1] = 1;
            
            // Select based on replacement rate
            for (int j = 0; j < (PRemain - Convert.ToInt16(ProjectConstants.EA_ReplacementRate * ProjectConstants.EA_Population)); j++)
            {
                // Pick random number (0,1) (Three digit precision)
                double dblRand = (double)r.Next(0, 1000) / 1000;
                int index = 0;
                if (dblRand >= Fitness[0])
                {
                    for (int i = 1; i < PRemain; i++)
                    {

                        if (dblRand > Fitness[i - 1] && dblRand < Fitness[i])
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

        // Crossover based on replacement rate
        private void Crossover()
        {
            // Generate remaining quota of the population by crossover
            for (int k = 0; k < Convert.ToInt16(ProjectConstants.EA_ReplacementRate * ProjectConstants.EA_Population / 2); k++)
            {
                #region 1. Probabilistically find father and mother
                // Pick random number (0,1)
                double dblRand1 = (double)r.Next(0, 1000) / 1000;
                double dblRand2 = (double)r.Next(0, 1000) / 1000;
                int index1 = 0;
                int index2 = 0;

                if (dblRand1 <= Fitness[0])
                {
                    index1 = 0;
                }
                else
                {
                    for (int i = 1; i < ProjectConstants.EA_Population; i++)
                    {
                        if (dblRand1 > Fitness[i - 1] && dblRand1 <= Fitness[i])
                        {
                            index1 = i;
                            break;
                        }
                    }
                }
                // Make sure I am not using the same one to crossover
                bool blnSame = true;
                int loop_count = 0;
                while (blnSame)
                {
                    loop_count++;
                    if (dblRand2 <= Fitness[0])
                    {
                        index2 = 0;
                    }
                    else
                    {
                        for (int i = 1; i < ProjectConstants.EA_Population; i++)
                        {

                            if (dblRand2 > Fitness[i - 1] && dblRand2 <= Fitness[i])
                            {
                                index2 = i;
                                break;
                            }
                        }
                    }
                    if (index1 != index2)
                    {
                        blnSame = false;
                    }
                    else
                    {
                        dblRand2 = (double)r.Next(0, 1000) / 1000;
                    }
                    if (loop_count > ProjectConstants.EA_Population)
                    {
                        break;
                    }
                }
                #endregion

                #region 2. Path Crossover
                // Crossover paths
                List<Point> Father = CurGeneration[index1].Path;
                List<Point> Mother = CurGeneration[index2].Path;
                EAPath Son = new EAPath();
                EAPath Daughter = new EAPath();

                // Randomly pick a node in Father(1,T-1)
                int split_1_F = r.Next(1, curRequest.T);
                int split_1_M = 0;
                int split_2_F = 0;
                int split_2_M = 0;
                Point first = Father[split_1_F];

                #region Look until we find a Mother that also has this node
                int count = 0;
                List<int> Duplicates = new List<int>();
                Duplicates.Add(index1);
                Duplicates.Add(index2);
                split_1_M = 0;
                while ((!Mother.Contains(first) || split_1_M == 0) && count < 10)
                {
                    // If not, pick another one and try again
                    // Repeat say 10 times, if still unseccessful, then pick new Father and new Mother

                    // Make sure I am not using the same one to crossover
                    blnSame = true;
                    loop_count = 0;
                    while (blnSame)
                    {
                        loop_count++;
                        dblRand2 = (float)r.Next(0, 1000) / 1000;
                        if (dblRand2 < Fitness[0])
                        {
                            index2 = 0;
                        }
                        else
                        {
                            for (int i = 1; i < ProjectConstants.EA_Population; i++)
                            {
                                if (dblRand2 > Fitness[i - 1] && dblRand2 < Fitness[i])
                                {
                                    index2 = i;
                                    break;
                                }
                            }
                        }
                        if (!Duplicates.Contains(index2))
                        {
                            blnSame = false;
                        }
                        if (loop_count > ProjectConstants.EA_Population)
                        {
                            break;
                        }
                    }
                    Mother = null;
                    Mother = CurGeneration[index2].Path;
                    split_1_M = Mother.IndexOf(first);
                    Duplicates.Add(index2);
                    count++;
                }
                if (count == 10)
                {
                    // Did not find a good Mother
                    k--;
                    continue;
                }
                #endregion

                // Found a good mother that has the node
                split_1_M = Mother.IndexOf(first);

                // Look from the end of Father and see if mother also has a second common node (also from end)
                bool blnTwoCommonNodes = false;
                for (int i = Father.Count - 1; i > split_1_F + 1; i--)
                {
                    split_2_M = Mother.IndexOf(Father[i], split_1_M);
                    if (Mother.Contains(Father[i]) && split_2_M != -1)
                    {
                        split_2_F = i;
                        blnTwoCommonNodes = true;
                        break;
                    }
                }

                // Perform crossover
                if (blnTwoCommonNodes)
                {
                    // If yes, perform double point crossover
                    Son.Path.AddRange(Father.GetRange(0, split_1_F));
                    Son.Path.AddRange(Mother.GetRange(split_1_M, split_2_M - split_1_M));
                    Son.Path.AddRange(Father.GetRange(split_2_F, curRequest.T + 1 - split_2_F));
                    Daughter.Path.AddRange(Mother.GetRange(0, split_1_M));
                    Daughter.Path.AddRange(Father.GetRange(split_1_F, split_2_F - split_1_F));
                    Daughter.Path.AddRange(Mother.GetRange(split_2_M, curRequest.T + 1 - split_2_M));
                }
                else
                {
                    // If not, perform single point crossover
                    Son.Path.AddRange(Father.GetRange(0, split_1_F));
                    Son.Path.AddRange(Mother.GetRange(split_1_M, curRequest.T + 1 - split_1_M));
                    Daughter.Path.AddRange(Mother.GetRange(0, split_1_M));
                    Daughter.Path.AddRange(Father.GetRange(split_1_F, curRequest.T + 1 - split_1_F));
                }

                #region Make sure there's no flying backward for non-copter
                bool blnCrossoverFailed = false;
                if(curRequest.VehicleType!=UAVType.Copter)
                {
                    // For the first crossover point 
                    // For Son
                    if (split_1_F > 1)
                    {
                        if (!ValidMove(Son.Path[split_1_F - 2], Son.Path[split_1_F - 1], Son.Path[split_1_F]))
                        {
                            blnCrossoverFailed = true;
                        }
                    }
                    if (Son.Path.Count - 1 > split_1_F)
                    {
                        if (!ValidMove(Son.Path[split_1_F - 1], Son.Path[split_1_F], Son.Path[split_1_F + 1]))
                        {
                            blnCrossoverFailed = true;
                        }
                    }
                    // For Daughter
                    if (split_1_M > 1)
                    {
                        if (!ValidMove(Daughter.Path[split_1_M - 2], Daughter.Path[split_1_M - 1], Daughter.Path[split_1_M]))
                        {
                            blnCrossoverFailed = true;
                        }
                    }
                    if (Daughter.Path.Count - 1 > split_1_M)
                    {
                        if (!ValidMove(Daughter.Path[split_1_M - 1], Daughter.Path[split_1_M], Daughter.Path[split_1_M + 1]))
                        {
                            blnCrossoverFailed = true;
                        }
                    }
                    if (blnTwoCommonNodes && !blnCrossoverFailed)
                    {
                        // For the second crossover point
                        // For Son
                        int intTemp = split_1_F + split_2_M - split_1_M;
                        if (intTemp > 1)
                        {
                            if (!ValidMove(Son.Path[intTemp - 2], Son.Path[intTemp - 1], Son.Path[intTemp]))
                            {
                                blnCrossoverFailed = true;
                            }
                        }
                        if (Son.Path.Count - 1 > intTemp)
                        {
                            if (!ValidMove(Son.Path[intTemp - 1], Son.Path[intTemp], Son.Path[intTemp + 1]))
                            {
                                blnCrossoverFailed = true;
                            }
                        }
                        // For Daughter
                        intTemp = split_1_M + split_2_F - split_1_F;
                        if (intTemp > 1)
                        {
                            if (!ValidMove(Daughter.Path[intTemp - 2], Daughter.Path[intTemp - 1], Daughter.Path[intTemp]))
                            {
                                blnCrossoverFailed = true;
                            }
                        }
                        if (Daughter.Path.Count - 1 > intTemp)
                        {
                            if (!ValidMove(Daughter.Path[intTemp - 1], Daughter.Path[intTemp], Daughter.Path[intTemp + 1]))
                            {
                                blnCrossoverFailed = true;
                            }
                        }
                    }
                }
                #endregion

                #region Make sure they still fly all the way.
                if (!blnCrossoverFailed)
                {
                    // Now both children are valid paths
                    // Let's truncate or extend so they all have length T+1;

                    // After crossover, if Son or Daughter becomes too long, truncate
                    EAPath ShorterPath = new EAPath();
                    EAPath LongerPath = new EAPath();

                    if (Son.Path.Count - 1 >= curRequest.T + 1)
                    {
                        Son.Path.RemoveRange(curRequest.T + 1, Son.Path.Count - (curRequest.T + 1));
                        LongerPath = Son;
                        ShorterPath = Daughter;
                    }
                    if (Daughter.Path.Count - 1 >= curRequest.T + 1)
                    {
                        Daughter.Path.RemoveRange(curRequest.T + 1, Daughter.Path.Count - (curRequest.T + 1));
                        LongerPath = Daughter;
                        ShorterPath = Son;
                    }
                    if (LongerPath.Path.Count == 0)
                    {
                        LongerPath = Son;
                        ShorterPath = Daughter;
                    }

                    // After crossover, if Son or Daughter becomes too short, 
                    // probablistically randomly pick another path and crossover again.
                    // This time only keep the longer one and then truncate.
                    dblRand1 = (float)r.Next(0, 1000) / 1000;
                    index1 = 0;
                    if (dblRand1 < Fitness[0])
                    {
                        index1 = 0;
                    }
                    else
                    {
                        for (int i = 1; i < ProjectConstants.EA_Population; i++)
                        {
                            if (dblRand1 > Fitness[i - 1] && dblRand1 < Fitness[i])
                            {
                                index1 = i;
                                break;
                            }
                        }
                    }
                    List<Point> BetterHalf = CurGeneration[index1].Path;
                    split_1_F = r.Next(1, ShorterPath.Path.Count - 1);
                    first = BetterHalf[split_1_F];
                    split_1_M = 0;

                    #region Look until we find a BetterHalf that also has this node
                    count = 0;
                    Duplicates.Clear();
                    Duplicates.Add(index1);
                    Duplicates.Add(index2);
                    int indexOfFirst = 0;
                    while ((!BetterHalf.Contains(first) || indexOfFirst == 0) && count < 10)
                    {
                        // If not, pick another one and try again
                        // Repeat say 10 times, if still unseccessful, then pick new Father and new Mother

                        // Make sure I am not using the same one to crossover
                        blnSame = true;
                        loop_count = 0;
                        while (blnSame)
                        {
                            loop_count++;
                            dblRand2 = (float)r.Next(0, 1000) / 1000;
                            if (dblRand2 < Fitness[0])
                            {
                                index2 = 0;
                            }
                            else
                            {
                                for (int i = 1; i < ProjectConstants.EA_Population; i++)
                                {
                                    if (dblRand2 > Fitness[i - 1] && dblRand2 < Fitness[i])
                                    {
                                        index2 = i;
                                        break;
                                    }
                                }
                            }
                            if (!Duplicates.Contains(index2))
                            {
                                blnSame = false;
                            }
                            if (loop_count > ProjectConstants.EA_Population)
                            {
                                break;
                            }
                        }
                        BetterHalf = null;
                        BetterHalf = CurGeneration[index2].Path;
                        indexOfFirst = BetterHalf.IndexOf(first);
                        Duplicates.Add(index2);
                        count++;

                    }
                    if (count == 10)
                    {
                        // Did not find a good Mother
                        k--;
                        continue;
                    }
                    #endregion

                    EAPath Baby1 = new EAPath();
                    EAPath Baby2 = new EAPath();
                    Baby1.Path.AddRange(ShorterPath.Path.GetRange(0, split_1_F));
                    Baby1.Path.AddRange(BetterHalf.GetRange(split_1_M, curRequest.T + 1 - split_1_M));
                    Baby2.Path.AddRange(BetterHalf.GetRange(0, split_1_M));
                    Baby2.Path.AddRange(ShorterPath.Path.GetRange(split_1_F, ShorterPath.Path.Count - split_1_F));
                    if (Baby1.Path.Count - 1 >= curRequest.T + 1)
                    {
                        Baby1.Path.RemoveRange(curRequest.T + 1, Baby1.Path.Count - (curRequest.T + 1));
                        ShorterPath = null;
                        ShorterPath = Baby1;
                    }
                    else if (Baby2.Path.Count - 1 >= curRequest.T + 1)
                    {
                        Baby2.Path.RemoveRange(curRequest.T + 1, Baby2.Path.Count - (curRequest.T + 1));
                        ShorterPath = null;
                        ShorterPath = Baby2;
                    }
                    else
                    {
                        blnCrossoverFailed = true;
                    }

                    #region Make sure there's no flying backward
                    if(curRequest.VehicleType!=UAVType.Copter)
                    {
                        if (split_1_F > 1)
                        {
                            if (!ValidMove(ShorterPath.Path[split_1_F - 2], ShorterPath.Path[split_1_F - 1], ShorterPath.Path[split_1_F]))
                            {
                                blnCrossoverFailed = true;
                            }
                        }
                        if (ShorterPath.Path.Count - 1 > split_1_F)
                        {
                            if (!ValidMove(ShorterPath.Path[split_1_F - 1], ShorterPath.Path[split_1_F], ShorterPath.Path[split_1_F + 1]))
                            {
                                blnCrossoverFailed = true;
                            }
                        }
                    }
                    #endregion

                    if (!blnCrossoverFailed)
                    {
                        // Let's add crossover results to the new generation
                        NewGeneration.Add(LongerPath);
                        NewGeneration.Add(ShorterPath);
                    }
                    LongerPath = null;
                    ShorterPath = null;
                    Baby1 = null;
                    Baby2 = null;
                }
                #endregion

                if (blnCrossoverFailed)
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
                #endregion
            }
        }

        // Mutate based on mutation rate
        private void Mutate()
        {
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

        #endregion
    }
}
