using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using rtwmatrix;
using System.Threading;
using System.Threading.Tasks;


namespace IPPA
{
    class AlgTopTwo : AlgPathPlanning
    {
        #region Members

        // Private variables
        private Point Start = new Point();
        private Point End = new Point();
        private Point Centroid1 = new Point();
        private Point Centroid2 = new Point();
        private Point End1 = new Point();
        private Point End2 = new Point();
        private Point MiddlePoint = new Point();
        private int d1 = 0;
        private int d2 = 0;
        private int d3 = 0;
        private int d4 = 0;
        private int t1 = 0;
        private int t2 = 0;
        private int t3 = 0;
        private int t4 = 0;
        private AlgLHCGWCONV Seg1 = null;
        // private AlgGlobalWarming Seg2 = null;
        private AlgLHCGWCONV Seg2 = null;
        // private AlgGlobalWarming Seg3 = null;
        private AlgLHCGWCONV Seg3 = null;
        private AlgLHCGWCONV Seg4 = null;

        private MapModes myModes = null;
        private double curCDF = 0;
        private RtwMatrix mDistAfterSeg1Seg4;
        private List<Point> curPath2;
        private List<Point> curPath3;
        private int StepSize = 0;
        private int CTFTTCoraseLevel;
        private int CTFTTLevelCount;

        private List<double> arrlCurCDFs = new List<double>();

        // Variabes used for threads
        private PathPlanningResponse[] arrResponses = null;
        private List<AlgPathPlanning> lstThreads = new List<AlgPathPlanning>();
        private int bestThreadIndex = 0;

        // Public variables

        #endregion

        #region Constructor, Destructor

        // Constructor
        public AlgTopTwo(PathPlanningRequest _curRequest, int _ModeCount, RtwMatrix _mModes, RtwMatrix _mDistReachable, 
            RtwMatrix _mDiffReachable, double _Efficiency_UB) 
            : base (_curRequest, _mDistReachable, _mDiffReachable, _Efficiency_UB)
        {
            // Start timer
            //DateTime startTime = DateTime.Now;
            myModes = new MapModes(_ModeCount, _mModes, curRequest, _mDistReachable, _mDiffReachable);
            //DateTime stopTime = DateTime.Now;
            //TimeSpan duration = stopTime - startTime;
            //double RunTime = duration.TotalSeconds;
            //System.Windows.Forms.MessageBox.Show("Run time " + RunTime + " seconds!");

            CTFTTCoraseLevel = ProjectConstants.CTFTTCoraseLevel;
            CTFTTLevelCount = ProjectConstants.CTFTTLevelCount;
            mDistAfterSeg1Seg4 = mDist;
        }
        public AlgTopTwo(MapModes _curModes, PathPlanningRequest _curRequest, int _ModeCount, RtwMatrix _mModes, RtwMatrix _mDistReachable,
            RtwMatrix _mDiffReachable, double _Efficiency_UB)
            : base(_curRequest, _mDistReachable, _mDiffReachable, _Efficiency_UB)
        {
            // Start timer
            //DateTime startTime = DateTime.Now;
            myModes = _curModes;
            //DateTime stopTime = DateTime.Now;
            //TimeSpan duration = stopTime - startTime;
            //double RunTime = duration.TotalSeconds;
            //System.Windows.Forms.MessageBox.Show("Run time " + RunTime + " seconds!");

            CTFTTCoraseLevel = ProjectConstants.CTFTTCoraseLevel;
            CTFTTLevelCount = ProjectConstants.CTFTTLevelCount;
            mDistAfterSeg1Seg4 = mDist;
        }

        // Destructor
        ~AlgTopTwo()
        {
            // Cleaning up
            Seg1 = null;
            Seg2 = null;
            Seg3 = null;
            Seg4 = null;
            myModes = null;
        }

        #endregion

        #region Other Functions

        // Method to perform the path planning
        protected override void DoPathPlanning()
        {
            // Sanity check: Don't do this when there is no mode or just 1 mode
            if (myModes.GetModeCount() < 2)
            {
                System.Windows.Forms.MessageBox.Show("Can't use TopTwo algorithm because there are less than 2 modes!");
                return;
            }

            // Figuring our all points
            FigureOutAllPoints();

            // Searth different combinations and find the best one
            PlanPathSegments();

            // Get real CDF
            CDF = GetTrueCDF(Path);
        }

        // Method to figure our all points
        private void FigureOutAllPoints()
        {
            Start = new Point(curRequest.pStart.column, curRequest.pStart.row);
            if (curRequest.UseEndPoint)
            {
                End = new Point(curRequest.pEnd.column, curRequest.pEnd.row);
            }

            // Find best two modes
            List<Point> lstCentroids = myModes.GetModeCentroids();

            // Find closest centroid and make that centriod1            
            //TODO Fix the bug of when End is closer to the centroid closest to Start. Really should permute.
            d1 = MISCLib.ManhattanDistance(Start, lstCentroids[0]);
            d2 = MISCLib.ManhattanDistance(Start, lstCentroids[1]);
            if (d1 < d2)
            {
                Centroid1 = lstCentroids[0];
                Centroid2 = lstCentroids[1];
            }
            else
            {
                Centroid1 = lstCentroids[1];
                Centroid2 = lstCentroids[0];
                d1 = d2;
            }

            // Sanity check: Don't do this if there's not enough time to go from start to centroid 1 and then to centriod 2 (and then to end)
            d2 = MISCLib.ManhattanDistance(Centroid1, Centroid2);
            if (curRequest.UseEndPoint)
            {
                d4 = MISCLib.ManhattanDistance(Centroid2, End);
            }
            if (d1 + d2 + d3 + d4 > curRequest.T)
            {
                System.Windows.Forms.MessageBox.Show("Can't reach both centroids with given flight time!");
                return;
            }

            // Deal with copter or non-copter
            FindEndPoints();
        }

        // Find end points for the two middle path segments
        private void FindEndPoints()
        {
            double x = (Centroid1.X + Centroid2.X) / 2;
            double y = (Centroid1.Y + Centroid2.Y) / 2;
            MiddlePoint = new Point(Convert.ToInt16(Math.Round(x)), Convert.ToInt16(Math.Round(y)));

            if (curRequest.VehicleType == UAVType.Copter)
            {
                End1 = MiddlePoint;
                End2 = MiddlePoint;
            }
            else
            {
                if (Centroid1.X == MiddlePoint.X)
                {
                    // Centroid1, MiddlePoint, and Centrod2 on the same row
                    End1.X = MiddlePoint.X;
                    End2.X = MiddlePoint.X;

                    if (Centroid1.Y > MiddlePoint.Y)
                    {
                        End1.Y = MiddlePoint.Y + 1;
                        End2.Y = MiddlePoint.Y - 1;
                    }
                    else
                    {
                        End1.Y = MiddlePoint.Y - 1;
                        End2.Y = MiddlePoint.Y + 1;
                    }
                }
                else if (Centroid1.Y == MiddlePoint.Y)
                {
                    // Centroid1, MiddlePoint, and Centrod2 on the same column
                    End1.Y = MiddlePoint.Y;
                    End2.Y = MiddlePoint.Y;

                    if (Centroid1.X > MiddlePoint.X)
                    {
                        End1.X = MiddlePoint.X + 1;
                        End2.X = MiddlePoint.X - 1;
                    }
                    else
                    {
                        End1.X = MiddlePoint.X - 1;
                        End2.X = MiddlePoint.X + 1;
                    }
                }
                else
                {
                    // Centroid1, MiddlePoint, and Centrod2 not on the same row or column
                    if (Centroid1.X > MiddlePoint.X)
                    {
                        End1.X = MiddlePoint.X + 1;
                        End1.Y = MiddlePoint.Y;
                        End2.X = MiddlePoint.X - 1;
                        End2.Y = MiddlePoint.Y;
                    }
                    else
                    {
                        End1.X = MiddlePoint.X - 1;
                        End1.Y = MiddlePoint.Y;
                        End2.X = MiddlePoint.X + 1;
                        End2.Y = MiddlePoint.Y;
                    }
                }
            }
        }

        // Method to plan path segments and find the best combination
        protected void PlanPathSegments()
        {
            // Take care of Seg1 and Seg4
            if (d1 > 0)
            {
                // Plan path from Start to Centroid1
                t1 = d1;
                PlanPathSeg1();
            }
            if (d4 > 0)
            {
                // Plan path from End to Centroid2
                t4 = d4;
                PlanPathSeg4();
            }
            // Shift time assignment from one centroid to another centroid
            if (!curRequest.UseCoarseToFineSearch && !curRequest.UseParallelProcessing)
            {
                // Just regular search
                // Extensive Search (like GW)
                ExtensiveSearch();
                //Console.WriteLine("Seg2Seg3TotalCDF: " + curCDF);
            }
            if (!curRequest.UseCoarseToFineSearch && curRequest.UseParallelProcessing)
            {
                ParallelSearch();
                //Console.WriteLine("Seg2Seg3TotalCDF: " + bestThreadIndex + "," + curCDF);
            }
            if (curRequest.UseCoarseToFineSearch && !curRequest.UseParallelProcessing)
            {
                CorseToFineSearch();
            }
            if (curRequest.UseCoarseToFineSearch && curRequest.UseParallelProcessing)
            {
                CoarseToFineAndParallelSearch();
            }

            // Join three pieces into one path
            JoinPathSegments();
        }

        // Method to plan path for segment 1
        private void PlanPathSeg1()
        {
            PathPlanningRequest newRequest1 = curRequest.Clone();
            newRequest1.UseEndPoint = true;
            newRequest1.pEnd = new DistPoint(Centroid1.Y, Centroid1.X);
            newRequest1.T = t1;
            newRequest1.AlgToUse = AlgType.LHCGWCONV;
            Seg1 = new AlgLHCGWCONV(newRequest1, mDist, mDiff, Efficiency_UB, 3);
            Seg1.PlanPath();
            mDistAfterSeg1Seg4 = Seg1.GetmCurDist();
        }

        // Method to plan path for segment 4
        private void PlanPathSeg4()
        {
            PathPlanningRequest newRequest4 = curRequest.Clone();
            newRequest4.UseEndPoint = true;
            newRequest4.pStart = new DistPoint(End.Y, End.X);
            newRequest4.pEnd = new DistPoint(Centroid2.Y, Centroid2.X);
            newRequest4.T = t4;
            newRequest4.AlgToUse = AlgType.LHCGWCONV;
            Seg4 = new AlgLHCGWCONV(newRequest4, mDistAfterSeg1Seg4, mDiff, Efficiency_UB, 3);
            Seg4.PlanPath();
            mDistAfterSeg1Seg4 = Seg4.GetmCurDist();
        }

        // Method to perform extensive search in the T space
        private void ExtensiveSearch()
        {
            //TODO For now just assume copter (not checking the if flying backward at joint of Seg1 Seg2 and joint of Seg3 Seg4.
            int t2Min = MISCLib.ManhattanDistance(Centroid1, End1);
            int t3Max = curRequest.T - t1 - t4 - t2Min - 2;
            int t3Min = MISCLib.ManhattanDistance(Centroid2, End2);
            int t2Max = curRequest.T - t1 - t4 - t3Min - 2;
            if (curRequest.VehicleType == UAVType.Copter)
            {
                t3Max += 2;
                t2Max += 2;
            }
            StepSize = Convert.ToInt16(Math.Round(Convert.ToDouble(t2Max - t2Min) / ProjectConstants.SearchResolution));

            // Now let's search
            t2 = t2Min;
            t3 = t3Max;
            int count = 0;
            while (t2 <= t2Max && StepSize>0)
            {
                count++;
                PlanPathSeg2Seg3(count - 1);

                //// Debug: Log
                //if (curRequest.UseEndPoint)
                //{
                //    curRequest.SetLog((Seg1.GetCDF() + Seg2.GetCDF() + Seg3.GetCDF() + Seg4.GetCDF()).ToString() + "\n");
                //}
                //else
                //{
                //    curRequest.SetLog((Seg1.GetCDF() + Seg2.GetCDF() + Seg3.GetCDF()).ToString() + "\n");
                //}
                
                // Next search
                t2 += StepSize;
                t3 -= StepSize;

                // Free memory
                Seg2 = null;
                Seg3 = null;
            }
            
            // Console.WriteLine("Count = " + count);
        }

        // Search multiple time allocation simutaneously
        private void ParallelSearch()
        {
            ExtensiveSearch();
            SpawnThreads();
        }

        // Method to plan path segments using coarse to fine method
        private void CorseToFineSearch()
        {
            //TODO For now just assume copter (not checking the if flying backward at joint of Seg1 Seg2 and joint of Seg3 Seg4.
            int t2Min = MISCLib.ManhattanDistance(Centroid1, End1);
            int t3Max = curRequest.T - t1 - t4 - t2Min - 2;
            int t3Min = MISCLib.ManhattanDistance(Centroid2, End2);
            int t2Max = curRequest.T - t1 - t4 - t3Min - 2;
            if (curRequest.VehicleType == UAVType.Copter)
            {
                t3Max += 2;
                t2Max += 2;
            }

            // Now let's search
            t2 = t2Min;
            t3 = t3Max;

            // How many to search on each side?
            int sideSearch = CTFTTCoraseLevel - 1;

            // Start from one side (no ocean rise)
            int curMiddle = t3Max;

            // Actual search
            StepSize = (t3Max - t3Min) / CTFTTCoraseLevel;
            double lastBestCDF = 0;
            for (int i = 0; i < CTFTTLevelCount; i++)
            {
                int curLeft = curMiddle + sideSearch * StepSize;
                int curRight = curMiddle - sideSearch * StepSize;
                // Array of CDFs for all searches (left, middle, and right)
                double[] CDFs = new double[CTFTTCoraseLevel * 2 - 1];

                if (i == 0)
                {
                    // Compute middle 
                    PlanPathSeg2Seg3(0);
                    arrlCurCDFs.Sort();
                    CDFs[sideSearch] = arrlCurCDFs[arrlCurCDFs.Count - 1];
                    arrlCurCDFs.Clear();
                    // Also compute one extra (4 StepSizes from middle to the right when t3 = t3min)
                    t2 = t2Max;
                    t3 = t3Min;
                    PlanPathSeg2Seg3(0);
                    arrlCurCDFs.Sort();
                    if (arrlCurCDFs[arrlCurCDFs.Count - 1] > CDFs[sideSearch])
                    {
                        CDFs[sideSearch] = arrlCurCDFs[arrlCurCDFs.Count - 1];
                    }
                    arrlCurCDFs.Clear();
                }
                else
                {
                    // No need to compute middle again. Already done from previous step.
                    CDFs[sideSearch] = lastBestCDF;
                }

                // Now do left side (ocean falls)
                if (curLeft <= t3Max)
                {
                    OneSideSearch(true, curMiddle, StepSize, CDFs, t2Min, t3Max);
                }

                // Now do right side (ocean rises)
                if (curRight > t3Min)
                {
                    OneSideSearch(false, curMiddle, StepSize, CDFs, t2Min, t3Max);
                }

                //// Debug: log
                //for (int k = 0; k < CDFs.Length; k++)
                //{
                //    curRequest.SetLog(CDFs[k].ToString() + ", ");
                //}
                //curRequest.SetLog("\n");

                // No need to do this again in the last level
                if (i < CTFTTLevelCount - 1)
                {
                    // Find best CDF 
                    int indexBest = Array.IndexOf(CDFs, CDFs.Max());
                    lastBestCDF = CDFs[indexBest];
                    // Get the ts for that best one
                    int indexDiff = indexBest - sideSearch;
                    t3 = curMiddle + indexDiff * StepSize;
                    t2 = t2Min + t3Max - t3;

                    curMiddle = t3;
                    StepSize = StepSize / CTFTTCoraseLevel;
                }
            }
        }

        private void OneSideSearch(bool t3Up, int curMiddle, int StepSize, double[] CDFs, int t2Min, int t3Max)
        {
            int sign = -1;
            if (t3Up)
            {
                sign = 1;
            }

            for (int i = 0; i < CTFTTCoraseLevel - 1; i++)
            {
                t3 = curMiddle + sign * StepSize;
                t2 = t2Min + t3Max - t3;
                PlanPathSeg2Seg3(0);
                arrlCurCDFs.Sort();
                CDFs[CTFTTCoraseLevel - 1 + sign + sign * i] = arrlCurCDFs[arrlCurCDFs.Count - 1];
                arrlCurCDFs.Clear();
            }
        }

        // Does corase to fine search with parallelization
        private void CoarseToFineAndParallelSearch()
        {
            //TODO For now just assume copter (not checking the if flying backward at joint of Seg1 Seg2 and joint of Seg3 Seg4.
            List<Point> curPath2Copy = null;
            List<Point> curPath3Copy = null;
            int t2Min = MISCLib.ManhattanDistance(Centroid1, End1);
            int t3Max = curRequest.T - t1 - t4 - t2Min - 2;
            int t3Min = MISCLib.ManhattanDistance(Centroid2, End2);
            int t2Max = curRequest.T - t1 - t4 - t3Min - 2;
            if (curRequest.VehicleType == UAVType.Copter)
            {
                t3Max += 2;
                t2Max += 2;
            }

            // Now let's search
            t2 = t2Min;
            t3 = t3Max;

            // How many to search on each side?
            int sideSearch = CTFTTCoraseLevel - 1;

            // Start from one side (no ocean rise)
            int curMiddle = t3Max;

            // Actual search
            StepSize = (t3Max - t3Min) / CTFTTCoraseLevel;
            for (int i = 0; i < CTFTTLevelCount; i++)
            {
                // Clear all threads first to start fresh
                lstThreads.Clear();
                // Clear all responses too
                arrResponses = null;

                // Now do the coarse to fine search
                int curLeft = curMiddle + sideSearch * StepSize;
                int curRight = curMiddle - sideSearch * StepSize;
                // Array of CDFs for all searches (left, middle, and right)
                double[] CDFs = new double[CTFTTCoraseLevel * 2 - 1];

                if (i == 0)
                {
                    // Compute middle 
                    PlanPathSeg2Seg3(sideSearch);

                    // Also compute one extra (4 StepSizes from middle to the right when t3 = t3min)
                    t2 = t2Max;
                    t3 = t3Min;
                    PlanPathSeg2Seg3(-1);
                }
                else
                {
                    // No need to compute middle again. Already done from previous step.
                    CDFs[sideSearch] = curCDF;
                    curPath2Copy = curPath2;
                    curPath3Copy = curPath3;
                }

                // Now do left side (ocean falls)
                if (curLeft <= t3Max)
                {
                    OneSideSearchParallel(true, curMiddle, StepSize, t2Min, t3Max);
                }

                // Now do right side (ocean rises)
                if (curRight > t3Min)
                {
                    OneSideSearchParallel(false, curMiddle, StepSize, t2Min, t3Max);
                }

                // Now do this round of multi-thread path planning
                SpawnThreads();

                //// Debug: log
                //for (int k = 0; k < CDFs.Length; k++)
                //{
                //    curRequest.SetLog(CDFs[k].ToString() + ", ");
                //}
                //curRequest.SetLog("\n");

                // Since we never compute the middle again, what if middle is better than the other 6x3?
                if (CDFs[sideSearch] >= curCDF)
                {
                    curCDF = CDFs[sideSearch];
                    curPath2 = curPath2Copy;
                    curPath3 = curPath3Copy;
                    bestThreadIndex = sideSearch;
                }

                // No need to do this again in the last level
                if (i < CTFTTLevelCount - 1)
                {
                    // Get the ts for that best one
                    int indexDiff = bestThreadIndex - sideSearch;
                    t3 = curMiddle + indexDiff * StepSize;
                    t2 = t2Min + t3Max - t3;
                    curMiddle = t3;
                    StepSize = StepSize / CTFTTCoraseLevel;
                }
            }
        }

        private void OneSideSearchParallel(bool t3Up, int curMiddle, int StepSize, int t2Min, int t3Max)
        {
            int sign = -1;
            if (t3Up)
            {
                sign = 1;
            }

            for (int i = 0; i < CTFTTCoraseLevel - 1; i++)
            {
                t3 = curMiddle + sign * StepSize;
                t2 = t2Min + t3Max - t3;
                PlanPathSeg2Seg3(CTFTTCoraseLevel - 1 + sign + sign * i);
            }
        }

        // Method to plan path for segment 2 given t2 and t3
        private void PlanPathSeg2Seg3(int index)
        {
            int dim = Math.Max(mDist.Rows, mDist.Columns);
            for (int j = 5; j < dim; j += (int)(dim / ProjectConstants.ConvCount))
            {

                // Plan path from closest centroid to end with current allocated t2
                PathPlanningRequest newRequest2 = curRequest.Clone();
                newRequest2.UseEndPoint = true;
                newRequest2.pStart = new DistPoint(Centroid1.Y, Centroid1.X);
                newRequest2.pEnd = new DistPoint(End1.Y, End1.X);
                newRequest2.AlgToUse = AlgType.LHCGWCONV_E;
                newRequest2.T = t2;
                // Seg2 = new AlgGlobalWarming(newRequest2, myModes.GetModeCount(), mDistAfterSeg1Seg4, mDiff, Efficiency_UB);
                // Seg2.SetGWCount(1);
                // Seg2.SetConvCount(3);
                if (curRequest.UseParallelProcessing)
                {
                    PathPlanningRequest newRequestCopy = newRequest2.DeepClone();
                    RtwMatrix mCopy = mDistAfterSeg1Seg4.Clone();
                    RtwMatrix mDiffCopy = mDiff.Clone();
                    AlgLHCGWCONV myAlg = new AlgLHCGWCONV(newRequestCopy, mCopy, mDiffCopy, Efficiency_UB, j);
                    myAlg.conv = 3;
                    myAlg.index = index * 2;
                    lstThreads.Add(myAlg);
                }
                else
                {
                    Seg2 = new AlgLHCGWCONV(newRequest2, mDistAfterSeg1Seg4, mDiff, Efficiency_UB, j);
                    Seg2.PlanPath();
                }

                // Plan path from the other centroid to end with allocated t3
                PathPlanningRequest newRequest3 = curRequest.Clone();
                newRequest3.UseEndPoint = true;
                newRequest3.pStart = new DistPoint(Centroid2.Y, Centroid2.X);
                newRequest3.pEnd = new DistPoint(End2.Y, End2.X);
                newRequest3.AlgToUse = AlgType.LHCGWCONV_E;
                newRequest3.T = t3;
                // Seg3 = new AlgGlobalWarming(newRequest3, myModes.GetModeCount(), Seg2.GetmCurDist(), mDiff, Efficiency_UB);
                // Seg3.SetGWCount(1);
                // Seg3.SetConvCount(3);
                if (curRequest.UseParallelProcessing)
                {
                    PathPlanningRequest newRequestCopy = newRequest3.DeepClone();
                    RtwMatrix mCopy = mDistAfterSeg1Seg4.Clone();
                    RtwMatrix mDiffCopy = mDiff.Clone();
                    AlgLHCGWCONV myAlg = new AlgLHCGWCONV(newRequestCopy, mCopy, mDiffCopy, Efficiency_UB, j);
                    myAlg.conv = 3;
                    myAlg.index = index * 2 + 1;
                    lstThreads.Add(myAlg);
                }
                else
                {
                    Seg3 = new AlgLHCGWCONV(newRequest3, Seg2.GetmCurDist(), mDiff, Efficiency_UB, j);
                    Seg3.PlanPath();
                }

                // Cleaning up
                newRequest2 = null;
                newRequest3 = null;

                // Remember if true CDF is better
                if (!curRequest.UseParallelProcessing)
                {
                    RememberBestPath();
                }
            }
        }

        // Remember the best combo of Seg2 and Seg3
        private void RememberBestPath()
        {
            //Console.WriteLine((Seg2.GetCDF() + Seg3.GetCDF()));
            arrlCurCDFs.Add(Seg2.GetCDF() + Seg3.GetCDF());

            if (curCDF < (Seg2.GetCDF() + Seg3.GetCDF()))
            {
                curCDF = Seg2.GetCDF() + Seg3.GetCDF();
                curPath2 = Seg2.GetPath();
                curPath3 = Seg3.GetPath();
                //// Debug
                //Console.WriteLine((Seg2.GetCDF() + Seg3.GetCDF()));
                //Console.WriteLine("Best t2 t3 so far: " + t2 + " " + t3);
            }

            //// Debug
            //Console.WriteLine("0," + Seg2.GetCDF() + "," + Seg3.GetCDF() + "," + (Seg2.GetCDF() + Seg3.GetCDF()));
        }

        // Spawn multi threads to do parallel path planning
        private void SpawnThreads()
        {
            // Create task array
            Task[] tasks = new Task[lstThreads.Count / 2];
            // Allocate array space for results
            arrResponses = new PathPlanningResponse[lstThreads.Count];

            for (int i = 0; i < lstThreads.Count; i+=2)
            {
                int cur_i = i;
                tasks[i / 2] = Task.Factory.StartNew(() => StoreResults(cur_i));
                // Thread.Sleep(500);
            }
            Task.WaitAll(tasks);
            // Now that all threads/tasks are done, find the best one
            FindBestPath();
        }

        // Store path planning results to array
        private void StoreResults(int index)
        {
            // Plan path for seg2
            lstThreads[index].PlanPath();
            // Storing results
            arrResponses[index] = new PathPlanningResponse(
                                    lstThreads[index].index,
                                    lstThreads[index].GetCDF(),
                                    lstThreads[index].GetRunTime(),
                                    lstThreads[index].GetEfficiency(),
                                    lstThreads[index].GetPath());
            // Plan path for seg3
            lstThreads[index + 1].SetmDist(lstThreads[index].GetmCurDist());
            lstThreads[index + 1].PlanPath();
            // Storing results            
            arrResponses[index + 1] = new PathPlanningResponse(
                                    lstThreads[index + 1].index,
                                    lstThreads[index + 1].GetCDF(),
                                    lstThreads[index + 1].GetRunTime(),
                                    lstThreads[index + 1].GetEfficiency(),
                                    lstThreads[index + 1].GetPath());
        }

        // Find best path in parallel version
        private void FindBestPath()
        {
            int bestIndex = 0;
            double bestCDF = 0;
            // Find one with best CDF
            for (int i = 0; i < arrResponses.Length / 2; i++)
            {
                if (arrResponses[i] != null)
                {
                    if (bestCDF < (arrResponses[i * 2].CDF + arrResponses[i * 2 + 1].CDF))
                    {
                        bestCDF = arrResponses[i * 2].CDF + arrResponses[i * 2 + 1].CDF;
                        bestIndex = i;
                    }
                }
            }

            //// Debug
            //for (int i = 0; i < arrResponses.Length / 2; i++)
            //{
            //    Console.WriteLine(i + "," + arrResponses[i * 2].CDF + "," + arrResponses[i * 2 + 1].CDF + "," + (arrResponses[i * 2].CDF + arrResponses[i * 2 + 1].CDF));
            //}

            // Set best one as path planning response
            curCDF = arrResponses[bestIndex * 2].CDF + arrResponses[bestIndex * 2 + 1].CDF;
            curPath2 = arrResponses[bestIndex * 2].Path;
            curPath3 = arrResponses[bestIndex * 2 + 1].Path;
            bestThreadIndex = arrResponses[bestIndex * 2].index / 2;
        }

        // Method to join path segments together
        private void JoinPathSegments()
        {
            if (Seg1 != null)
            {
                Path.AddRange(Seg1.GetPath());
                Path.RemoveAt(Path.Count - 1);
            }
            Path.AddRange(curPath2);
            if (curRequest.VehicleType == UAVType.Copter)
            {
                Path.RemoveAt(Path.Count - 1);
            }
            else
            {
                Path.Add(MiddlePoint);
            }
            curPath3.Reverse();
            Path.AddRange(curPath3);
            if (Seg4!=null)
            {
                Path.RemoveAt(Path.Count - 1);
                Seg4.GetPath().Reverse();
                Path.AddRange(Seg4.GetPath());
            }
        }

        // Debugging shouts
        public override void Shout()
        {
            Console.WriteLine("I am AlgTopTwo!");
        }

        #endregion
    }
}
