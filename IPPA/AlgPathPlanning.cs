using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using rtwmatrix;

namespace IPPA
{
    // Parent class for all path planning algorithms
    public abstract class AlgPathPlanning
    {
        #region Members

        // Private members
        private DateTime startTime;

        protected PathPlanningRequest curRequest;
        protected RtwMatrix mDist;
        protected RtwMatrix mDiff;
        protected RtwMatrix mCurDist;
        protected double Efficiency_UB = 0;
        protected double CDF;
        protected double RunTime = 0;
        protected double Efficiency = 0;
        protected List<Point> Path = new List<Point>();
        protected bool Status = true;
        protected List<float> CDFGraph = new List<float>();

        // Debug multithreaded variables
        public int index = 0;

        #endregion

        #region Constructor, Destructor

        // Constructor
        public AlgPathPlanning(PathPlanningRequest _curRequest,
            RtwMatrix _mDist, RtwMatrix _mDiff, double _Efficiency_UB)
        {
            // Start timer
            startTime = DateTime.Now;

            curRequest = _curRequest;
            mDist = _mDist;
            mDiff = _mDiff;
            // Clone distribution map so we can modify it
            mCurDist = mDist.Clone();
            Efficiency_UB = _Efficiency_UB;
        }

        // Destructor
        ~AlgPathPlanning()
        {
            // Cleaning up
            curRequest = null;
            mDist = null;
            mDiff = null;
            mCurDist = null;
        }

        #endregion

        #region Other Functions

        // Every path planning algorithm needs to find a path
        public void PlanPath()
        {
            //try
            //{
                // Individual implementation of path planning based on algorithm choosen
                DoPathPlanning();

                // Compute run time
                DateTime stopTime = DateTime.Now;
                TimeSpan duration = stopTime - startTime;
                RunTime = duration.TotalSeconds;

                // Compute Efficiency
                Efficiency = CDF / Efficiency_UB;

                #region Debug Code
                //// Debug code, show actual path
                //if (curRequest.DrawPath)
                //{
                //// Draw coverage
                //Bitmap CurBMP = new Bitmap(mDistReachable.Columns, mDistReachable.Rows);
                //ImgLib.MatrixToImage(ref mDistReachable, ref CurBMP);
                //frmMap map = new frmMap();
                //map.Text = "Actual UAV path";
                //map.setImage(CurBMP);
                //map.Show();
                //map.resetImage();
                //map.DrawCoverage(Path);
                //map.Refresh();

                //// Draw path
                //Bitmap CurBMP2 = new Bitmap(mDistReachable.Columns, mDistReachable.Rows);
                //ImgLib.MatrixToImage(ref mDistReachable, ref CurBMP2);
                //frmMap map2 = new frmMap();
                //map2.Text = "UAV trajectory simulation";
                //map2.setImage(CurBMP2);
                //map2.Show();
                //map2.resetImage();
                //map2.DrawPath(Path);
                //map2.Refresh();

                //// Draw path with map remains
                //Bitmap CurBMP3 = new Bitmap(mDist.Columns, mDist.Rows);
                //ImgLib.MatrixToImage(ref mDist, ref CurBMP3);
                //frmMap map3 = new frmMap();
                //map3.Text = "UAV trajectory and coverage";
                //map3.setImage(CurBMP3);
                //map3.Show();
                //map3.resetImage();
                //List<float> remains = ShowCoverage();
                //Color c = Color.FromArgb(255, 0, 0);
                //for (int i = 0; i < Path.Count; i++)
                //{
                //    Point p = Path[i];
                //    map3.setPointColor(p, c);
                //    map3.Refresh();
                //    map3.setPointColor(p, remains[i]);
                //    map3.Refresh();
                //}
                //}

                // PathSanityCheck(Path);
                #endregion
            //}
            //catch (Exception e)
            //{
            //    // Set path planning status to false
            //    Status = false;
            //    // Write to console debug info
            //    Console.WriteLine("{0} Exception caught.", e);
            //}
        }

        // Individual implementation of path planning based on algorithm choosen
        abstract protected void DoPathPlanning();

        // Compute what amount can be taken off when visiting a node
        protected float GetPartialDetection(Point p)
        {
            return GetPartialDetection(p, mCurDist);
        }
        protected float GetPartialDetection(Point p, RtwMatrix m)
        {
            float original = 0;
            float current = m[p.Y, p.X];
            float amountToDeduct = 0;

            // Always do fixed percent of remaining for DiffRates
            // Based on detection type, compute differently
            switch (curRequest.DetectionType)
            {
                case DType.FixAmount:
                    // Ignore task-difficulty map
                    amountToDeduct = (float)curRequest.DetectionRate;
                    break;
                case DType.FixAmountInPercentage:
                    if (curRequest.UseTaskDifficultyMap)
                    {
                        original = mDist[p.Y, p.X];
                        amountToDeduct = (float)(original * curRequest.DiffRates[Convert.ToInt32(mDiff[p.Y, p.X])] * curRequest.DetectionRate);
                    }
                    else
                    {
                        amountToDeduct = (float)(current * curRequest.DetectionRate);
                    }
                    break;
                case DType.FixPercentage:
                    if (curRequest.UseTaskDifficultyMap)
                    {
                        amountToDeduct = (float)(current * curRequest.DiffRates[Convert.ToInt32(mDiff[p.Y, p.X])] * curRequest.DetectionRate);
                    }
                    else
                    {
                        amountToDeduct = (float)(current * curRequest.DetectionRate);
                    }
                    break;
            }
            if (amountToDeduct > current)
            {
                amountToDeduct = current;
            }
            return amountToDeduct;
        }
        
        // Compute actual detection rate based on task-difficulty map and partial detection rate
        protected float VacuumProbability(Point p)
        {
            return VacuumProbability(p, mCurDist);
        }
        protected float VacuumProbability(Point p, RtwMatrix m)
        {
            float current = m[p.Y, p.X];
            float amountLeft = current - GetPartialDetection(p, m);
            return amountLeft;
        }

        // Expand child according to direction
        protected Point GetDirChild(int dir, Point p)
        {
            Point child = new Point();
            switch (dir)
            {
                case 0:
                    // Going north
                    child.X = p.X;
                    child.Y = p.Y - 1;
                    break;
                case 1:
                    // Going east
                    child.X = p.X + 1;
                    child.Y = p.Y;
                    break;
                case 2:
                    // Going south
                    child.X = p.X;
                    child.Y = p.Y + 1;
                    break;
                case 3:
                    // Going west
                    child.X = p.X - 1;
                    child.Y = p.Y;
                    break;
            }
            return child;
        }

        // Path planning request specific constrains
        protected bool ValidMove(Point Parent, Point Me, Point Child)
        {            
            // No flying out of the map
            if (Child.X < 0 || Child.Y < 0 || 
                Child.X > curRequest.DistMap.Columns - 1 || 
                Child.Y > curRequest.DistMap.Rows - 1)
            {
                // System.Windows.Forms.MessageBox.Show("No flying outside of map!");                    
                return false;
            }

            // For fix-wing, no flying backward or hover
            if (curRequest.VehicleType == UAVType.FixWing && 
                ((Parent.X == Child.X && Parent.Y == Child.Y) ||    // Flying backward
                (Me.X == Child.X && Me.Y == Child.Y)))              // Hover
            {
                // System.Windows.Forms.MessageBox.Show("No flying backward!");
                return false;
            }

            return true;
        }

        // Path planning request specific constrains (with ending piont)
        protected bool ValidMove(Point Parent, Point Me, Point Child, Point End, int T_Left)
        {
            // Regular constraints
            if (!ValidMove(Parent, Me, Child))
            {
                return false;
            }

            // Make sure there are enough steps left to reach End point
            int dist = MISCLib.ManhattanDistance(Child.X, Child.Y, End.X, End.Y);
            // T_Left is how many time steps left from Me, not from Child
            if (dist > T_Left - 1)
            {
                // System.Windows.Forms.MessageBox.Show("Won't make end point!");
                return false;
            }

            // For fix-wing, make sure the third from last step does not reach end (force flying backward)
            if (curRequest.VehicleType == UAVType.FixWing && dist == 0 && T_Left == 3)
            {
                return false;
            }

            // Make sure the direction selected won't force flying backward
            if (curRequest.VehicleType != UAVType.Copter)
            {
                int distM = MISCLib.ManhattanDistance(Me.X, Me.Y, End.X, End.Y);
                if (distM == T_Left - 2)
                {
                    if (End.X == Child.X || End.Y == Child.Y)
                    {
                        if (GetDirection(End, Me) != GetDirection(Child, Me))
                        {
                            if (GetDirection(End, Me) % 2 == GetDirection(Child, Me) % 2)
                            {
                                return false;
                            }
                        }
                    }
                }
            }

            return true;
        }

        // Return the direction of the current node from previous node in path
        protected int GetDirection(Point cur_node, Point previous)
        {
            // 4 directions. 0 for north and 3 for west
            if (cur_node.Y > previous.Y)
            {
                return 2; // South
            }
            if (cur_node.Y < previous.Y)
            {
                return 0; // North
            }
            if (cur_node.X > previous.X)
            {
                return 1; // East
            }
            if (cur_node.X < previous.X)
            {
                return 3; // West
            }
            return -1; // In case cur_node is previous node
        }

        // Function to calculate true cummulative probability using original map
        protected float GetTrueCDF(List<Point> curPath)
        {
            float curCDF = 0;

            RtwMatrix mCDF = mDist.Clone();
            for (int i = 0; i < curRequest.T + 1; i++)
            {
                curCDF += GetPartialDetection(curPath[i], mCDF);
                mCDF[curPath[i].Y, curPath[i].X] = VacuumProbability(curPath[i], mCDF);
            }
            // Cleaning up
            mCDF = null;

            return curCDF;
        }

        // Showing UAV trajectory and coverage (including partial detection)
        public List<float> ShowCoverage()
        {
            List<float> remains = new List<float>();
            RtwMatrix mCoverage = mDist.Clone();
            foreach (Point p in Path)
            {
                float remain = VacuumProbability(p, mCoverage);
                mCoverage[p.Y, p.X] = remain;
                remains.Add(remain);
            }
            return remains;
        }

        // Return path planning status
        public bool GetPathPlanningStatus()
        {
            return Status;
        }

        // Debugging shouts
        public virtual void Shout()
        {
            Console.WriteLine("I am AlgPathPlanning!");
        }

        // Debugging check
        protected void PathSanityCheck(List<Point> curPath)
        {
            // Debug code: Sanity check to make sure no flying backwards
            if (curRequest.VehicleType != UAVType.Copter)
            {
                for (int i = 1; i < curPath.Count() - 1; i++)
                {
                    if (!ValidMove(curPath[i - 1], curPath[i], curPath[i + 1]))
                    {
                        System.Windows.Forms.MessageBox.Show("Bummer!");
                        return;
                    }
                }
            }

            // Debug code: Sanity check to make sure path is all connected
            for (int i = 1; i < curPath.Count(); i++)
            {
                Point p1 = curPath[i - 1];
                Point p2 = curPath[i];
                if ((p1.X == p2.X && p1.Y == p2.Y) ||
                    (p1.X == p2.X && Math.Abs(p1.Y - p2.Y) == 1) ||
                    (Math.Abs(p1.X - p2.X) == 1 && p1.Y == p2.Y))
                {
                    // Good. Connected
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("Path broke!");
                    Console.Write("\n");
                    Console.WriteLine("i=" + i + " p1=(" + p1.X + "," + p1.Y + ") p2=(" + p2.X + "," + p2.Y);
                    return;
                }
            }
        }

        // Print to console CDF progress for graphing purposes
        protected void PrintCDFGraph()
        {
            PrintCDFGraph(Path, mDist);
        }

        // Print to console CDF progress for graphing purposes
        protected void PrintCDFGraph(List<Point> curPath, RtwMatrix curDist)
        {
            float curCDF = 0;

            RtwMatrix mCDF = curDist.Clone();
            Console.Write("curCDF=");
            for (int i = 0; i < curRequest.T + 1; i++)
            {
                curCDF += GetPartialDetection(curPath[i], mCDF);
                mCDF[curPath[i].Y, curPath[i].X] = VacuumProbability(curPath[i], mCDF);
                // Write out CDF as time progresses for chart
                Console.Write(curCDF + " ");
            }
            Console.Write("\n");
            // Cleaning up
            mCDF = null;
        }

        #region Getters
        public double GetCDF()
        {
            return CDF;
        }
        public double GetRunTime()
        {
            return RunTime;
        }
        public double GetEfficiency()
        {
            return Efficiency;
        }
        public List<Point> GetPath()
        {
            return Path;
        }
        public RtwMatrix GetmCurDist()
        {
            return mCurDist;
        }
        public PathPlanningRequest GetCurRequest()
        {
            return curRequest;
        }
        #endregion

        #region Setters
        public void SetmDist(RtwMatrix m)
        {
            mDist = m;
            mCurDist = mDist.Clone();
        }
        #endregion

        #endregion

    }
}
