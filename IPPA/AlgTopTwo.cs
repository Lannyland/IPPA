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

        // Public variables

        #endregion

        #region Constructor, Destructor

        // Constructor
        public AlgTopTwo(PathPlanningRequest _curRequest, int _ModeCount, RtwMatrix _mModes, RtwMatrix _mDistReachable, 
            RtwMatrix _mDiffReachable, double _Efficiency_UB) 
            : base (_curRequest, _mDistReachable, _mDiffReachable, _Efficiency_UB)
        {
            myModes = new MapModes(_ModeCount, _mModes, 2);
        }

        // Destructor
        ~AlgTopTwo()
        {
            // Cleaning up
            myModes = null;
        }

        #endregion

        #region Other Functions

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

            //TODO Plan shortest path to closest centroid using t1.
            //TODO For now just assume copter
            int t1 = d1;
            int t2 = Convert.ToInt16(Math.Ceiling((Convert.ToDouble(curRequest.T) - t1)/2));
            int t3 = curRequest.T - t1 - t2;

            PathPlanningRequest newRequest1 = curRequest.Clone();
            newRequest1.UseEndPoint = true;
            newRequest1.pEnd = new DistPoint(Centroid1.Y, Centroid1.X);
            newRequest1.T = t1;
            AlgLHCGWCONV Seg1 = new AlgLHCGWCONV(newRequest1, mDist, mDiff, Efficiency_UB, 3);
            Seg1.PlanPath();

            //TODO Plan path from closest centroid to end with Ceiling(T-t1)/2
            PathPlanningRequest newRequest2 = curRequest.Clone();
            newRequest2.UseEndPoint = true;
            newRequest2.pStart = new DistPoint(Centroid1.Y, Centroid1.X);
            newRequest2.pEnd = new DistPoint(End1.Y, End1.X);
            newRequest2.AlgToUse = AlgType.LHCGWCONV_E;
            newRequest2.T = t2;
            AlgGlobalWarming Seg2 = new AlgGlobalWarming(newRequest2, myModes.GetModeCount(), Seg1.GetmCurDist(), mDiff, Efficiency_UB);
            Seg2.SetGWCount(1);
            Seg2.SetConvCount(3);
            Seg2.PlanPath();

            //TODO Plan path from the other centroid to end with Floor(T-t1)/2
            PathPlanningRequest newRequest3 = curRequest.Clone();
            newRequest3.UseEndPoint = true;
            newRequest3.pStart = new DistPoint(Centroid2.Y, Centroid2.X);
            newRequest3.pEnd = new DistPoint(End2.Y, End2.X);
            newRequest3.AlgToUse = AlgType.LHCGWCONV_E;
            newRequest3.T = t3;
            AlgGlobalWarming Seg3 = new AlgGlobalWarming(newRequest3, myModes.GetModeCount(), Seg2.GetmCurDist(), mDiff, Efficiency_UB);
            Seg3.SetGWCount(1);
            Seg3.SetConvCount(3);
            Seg3.PlanPath();
                                    
            //TODO Join three pieces into one path
            Path.AddRange(Seg1.GetPath());
            Path.RemoveAt(Path.Count - 1);
            Path.AddRange(Seg2.GetPath());
            if (curRequest.VehicleType == UAVType.Copter)
            {
                Path.RemoveAt(Path.Count - 1);
            }
            else
            {
                Path.Add(MiddlePoint);
            }
            List<Point> ReversedPath = Seg3.GetPath();
            ReversedPath.Reverse();
            Path.AddRange(ReversedPath);

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

        // Debugging shouts
        public override void Shout()
        {
            Console.WriteLine("I am AlgTopTwo!");
        }

        #endregion
    }
}
