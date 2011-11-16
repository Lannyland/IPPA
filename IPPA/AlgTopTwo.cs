using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using rtwmatrix;

namespace IPPA
{
    class AlgTopTwo : AlgPathPlanning
    {
        #region Members

        // Private variables
        MapModes myModes = null;
        private double curCDF = 0;
        private List<Point> curPath2;
        private List<Point> curPath3;
        private int StepSize = 0;
        private int CTFTTCoraseLevel;
        private int CTFTTLevelCount;

        // Public variables

        #endregion

        #region Constructor, Destructor

        // Constructor
        public AlgTopTwo(PathPlanningRequest _curRequest, int _ModeCount, RtwMatrix _mModes, RtwMatrix _mDistReachable, 
            RtwMatrix _mDiffReachable, double _Efficiency_UB) 
            : base (_curRequest, _mDistReachable, _mDiffReachable, _Efficiency_UB)
        {
            myModes = new MapModes(_ModeCount, _mModes, 2);
            CTFTTCoraseLevel = ProjectConstants.CTFTTCoraseLevel;
            CTFTTLevelCount = ProjectConstants.CTFTTLevelCount;
        }

        // Destructor
        ~AlgTopTwo()
        {
            // Cleaning up
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
                      
            // Find best two modes
            List<Point> lstCentroids = myModes.GetModeCentroids();

            // Find closest centroid and make that centriod1            
            Point Start = new Point(curRequest.pStart.column, curRequest.pStart.row);
            Point Centroid1, Centroid2;
            Point End1 = new Point();
            Point End2 = new Point();            
            int d1 = MISCLib.ManhattanDistance(Start, lstCentroids[0]);
            int d2 = MISCLib.ManhattanDistance(Start, lstCentroids[1]);
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

            // Sanity check: Don't do this if there's not enough time to go from start to centroid 1 and then to centriod 2
            d2 = MISCLib.ManhattanDistance(Centroid1, Centroid2);
            if (d1 + d2 > curRequest.T)
            {
                System.Windows.Forms.MessageBox.Show("Can't reach both centroids with given flight time!");
                return;
            }
            
            Point MiddlePoint = new Point();

            // Deal with copter or non-copter
            FindEndPoints(ref Centroid1, ref Centroid2, ref End1, ref End2, ref MiddlePoint);
            
            // Searth different combinations and find the best one
            PlanPathSegments(ref Centroid1, ref Centroid2, ref End1, ref End2, d1, ref MiddlePoint);

            // Get real CDF
            CDF = GetTrueCDF(Path);
        }

        // Find end points for the two path segments
        private void FindEndPoints(ref Point Centroid1, ref Point Centroid2, ref Point End1, ref Point End2, ref Point MiddlePoint)
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
        private void PlanPathSegments(ref Point Centroid1, ref Point Centroid2, ref Point End1, ref Point End2, int d1, ref Point MiddlePoint)
        {
            // Plan shortest path to closest centroid using t1.
            //TODO For now just assume copter (not checking the if flying backward at joint of Seg1 and Seg2.
            int t1 = d1;
            int t2Min = MISCLib.ManhattanDistance(Centroid1, End1);
            int t3Max = curRequest.T - t1 - t2Min - 2;
            int t3Min = MISCLib.ManhattanDistance(Centroid2, End2);
            int t2Max = curRequest.T - t1 - t3Min - 2;
            if (curRequest.VehicleType == UAVType.Copter)
            {
                t3Max += 2;
                t2Max += 2;
            }
            StepSize = Convert.ToInt16(Math.Round(Convert.ToDouble(t2Max - t2Min) / ProjectConstants.SearchResolution));

            // First plan path from Start to Centroid1 (shortest path)
            AlgLHCGWCONV Seg1;
            PlanPathSeg1(ref Centroid1, t1, out Seg1);

            // Shift time assignment from one centroid to another centroid
            if (curRequest.UseHiararchy)
            {
                //TODO Hiararchical Search
                //float rise = curMiddle / CTFTTCoraseLevel;

                //for (int i = 0; i < CTFTTLevelCount; i++)
                //{
                //    float curLeft = curMiddle + sideSearch * rise;
                //    float curRight = curMiddle - sideSearch * rise;
                //    // Array of CDFs for all searches (left, middle, and right)
                //    float[] CDFs = new float[CTFTTCoraseLevel * 2 - 1];

                //    // Compute middle
                //    if (PlanPathAtCurrentGW(mGW))
                //    {
                //        // Already found the best path, no need to continue.
                //        return;
                //    }
                //    else
                //    {
                //        arrlCurCDFs.Sort();
                //        CDFs[sideSearch] = arrlCurCDFs[arrlCurCDFs.Count - 1];
                //        arrlCurCDFs.Clear();
                //    }

                //    // Now do left side (ocean falls)
                //    if (curLeft <= globalMax)
                //    {
                //        RtwMatrix mGWLeft = mGW.Clone();
                //        OneSideGWSearch(false, mGWLeft, rise, CDFs);
                //        mGWLeft = null;
                //    }

                //    // Now do right side (ocean rises)
                //    if (curRight > 0)
                //    {
                //        RtwMatrix mGWRight = mGW.Clone();
                //        OneSideGWSearch(true, mGWRight, rise, CDFs);
                //        mGWRight = null;
                //    }

                //    // No need to do this again in the last level
                //    if (i < sideSearch)
                //    {
                //        // Find best CDF 
                //        int indexBest = Array.IndexOf(CDFs, CDFs.Max());
                //        // Get the mGW for that best one
                //        int indexDiff = indexBest - sideSearch;
                //        OceanRises(mGW, indexDiff * rise);

                //        curMiddle = curMiddle - indexDiff * rise;
                //        rise = rise / CTFTTCoraseLevel;
                //    }
                //}
            }
            else
            {
                // Extensive Search (like GW)
                ExtensiveSearch(ref Centroid1, ref Centroid2, ref End1, ref End2, t2Min, t3Max, t2Max, Seg1);
            }

            // Join three pieces into one path
            JoinPathSegments(MiddlePoint, Seg1);
        }

        // Method to plan path for segment 1
        private void PlanPathSeg1(ref Point Centroid1, int t1, out AlgLHCGWCONV Seg1)
        {
            PathPlanningRequest newRequest1 = curRequest.Clone();
            newRequest1.UseEndPoint = true;
            newRequest1.pEnd = new DistPoint(Centroid1.Y, Centroid1.X);
            newRequest1.T = t1;
            Seg1 = new AlgLHCGWCONV(newRequest1, mDist, mDiff, Efficiency_UB, 3);
            Seg1.PlanPath();
        }

        // Method to perform extensive search in the T space
        private void ExtensiveSearch(ref Point Centroid1, ref Point Centroid2, ref Point End1, ref Point End2, int t2Min, int t3Max, int t2Max, AlgLHCGWCONV Seg1)
        {
            int t2 = t2Min;
            int t3 = t3Max;
            AlgGlobalWarming Seg2;
            AlgGlobalWarming Seg3;
            while (t2 <= t2Max)
            {
                PlanPathSeg2Seg3(ref Centroid1, ref Centroid2, ref End1, ref End2, Seg1, t2, t3, out Seg2, out Seg3);

                // Debug: Log
                curRequest.SetLog((Seg2.GetCDF() + Seg3.GetCDF()).ToString() + "\n");

                // Remember if true CDF is better
                RememberBestPath(Seg2, Seg3);

                // Next search
                t2 += StepSize;
                t3 -= StepSize;

                // Free memory
                Seg2 = null;
                Seg3 = null;
            }
        }

        // Method to plan path for segment 2 given t2 and t3
        private void PlanPathSeg2Seg3(ref Point Centroid1, ref Point Centroid2, ref Point End1, ref Point End2, AlgLHCGWCONV Seg1, int t2, int t3, out AlgGlobalWarming Seg2, out AlgGlobalWarming Seg3)
        {
            // Plan path from closest centroid to end with current allocated t
            PathPlanningRequest newRequest2 = curRequest.Clone();
            newRequest2.UseEndPoint = true;
            newRequest2.pStart = new DistPoint(Centroid1.Y, Centroid1.X);
            newRequest2.pEnd = new DistPoint(End1.Y, End1.X);
            newRequest2.AlgToUse = AlgType.LHCGWCONV_E;
            newRequest2.T = t2;
            Seg2 = new AlgGlobalWarming(newRequest2, myModes.GetModeCount(), Seg1.GetmCurDist(), mDiff, Efficiency_UB);
            Seg2.SetGWCount(1);
            Seg2.SetConvCount(3);
            Seg2.PlanPath();

            // Plan path from the other centroid to end with allocated t
            PathPlanningRequest newRequest3 = curRequest.Clone();
            newRequest3.UseEndPoint = true;
            newRequest3.pStart = new DistPoint(Centroid2.Y, Centroid2.X);
            newRequest3.pEnd = new DistPoint(End2.Y, End2.X);
            newRequest3.AlgToUse = AlgType.LHCGWCONV_E;
            newRequest3.T = t3;
            Seg3 = new AlgGlobalWarming(newRequest3, myModes.GetModeCount(), Seg2.GetmCurDist(), mDiff, Efficiency_UB);
            Seg3.SetGWCount(1);
            Seg3.SetConvCount(3);
            Seg3.PlanPath();

            // Cleaning up
            newRequest2 = null;
            newRequest3 = null;
        }

        // Compute true CDF based on original dist map (no GW) and remember the best
        private void RememberBestPath(AlgGlobalWarming Seg2, AlgGlobalWarming Seg3)
        {
            if (curCDF < Seg2.GetCDF() + Seg3.GetCDF())
            {
                curCDF = Seg2.GetCDF() + Seg3.GetCDF();
                curPath2 = Seg2.GetPath();
                curPath3 = Seg3.GetPath();
            }
        }

        // Method to join path segments together
        private void JoinPathSegments(Point MiddlePoint, AlgLHCGWCONV Seg1)
        {
            Path.AddRange(Seg1.GetPath());
            Path.RemoveAt(Path.Count - 1);
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
        }

        // Debugging shouts
        public override void Shout()
        {
            Console.WriteLine("I am AlgTopTwo!");
        }

        #endregion
    }
}
