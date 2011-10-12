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
        protected PathPlanningRequest curRequest;
        protected RtwMatrix mDist;
        protected RtwMatrix mDiff;
        protected RtwMatrix mCurDist;
        protected double Efficiency_LB = 0;
        protected double CDF;
        protected double RunTime = 0;
        protected double Efficiency = 0;
        protected List<Point> Path = new List<Point>();
        protected double[] DiffRates;
        
        #endregion

        #region Constructor, Destructor

        // Constructor
        public AlgPathPlanning(PathPlanningRequest _curRequest,
            RtwMatrix _mDist, RtwMatrix _mDiff, double _Efficiency_LB)
        {
            curRequest = _curRequest;
            mDist = _mDist;
            mDiff = _mDiff;
            // Clone distribution map so we can modify it
            mCurDist = mDist.Clone();
            Efficiency_LB = _Efficiency_LB;
            // Set task-difficulty rates
            DiffRates = new double[curRequest.MaxDifficulty + 1];
            double rate = 1.0 / (curRequest.MaxDifficulty + 1);
            for (int i = 0; i < curRequest.MaxDifficulty + 1; i++)
            {
                DiffRates[i] = 1 - i * rate;
            }
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
            // Start timer
            DateTime startTime = DateTime.Now;

            // Individual implementation of path planning based on algorithm choosen
            DoPathPlanning();

            // Compute run time
            DateTime stopTime = DateTime.Now;
            TimeSpan duration = stopTime - startTime;
            RunTime = duration.TotalSeconds;

            // Compute Efficiency
            Efficiency = CDF / Efficiency_LB;

            // Debug code, show map remain (especially for partial detection)
            // Re-enact the flight with real distmap
            if (curRequest.DrawPath)
            {
                RtwMatrix DistMapRemain = mDist.Clone();
                foreach (Point p in Path)
                {
                    DistMapRemain[p.Y, p.X] = VacuumProbability(p, DistMapRemain);
                }
                Bitmap CurBMP = new Bitmap(DistMapRemain.Columns, DistMapRemain.Rows);
                ImgLib.MatrixToImage(ref DistMapRemain, ref CurBMP);
                frmMap map = new frmMap();
                map.setImage(CurBMP);
                map.Show();
                map.resetImage();
                map.Refresh();
            }

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
                        amountToDeduct = (float)(original * DiffRates[Convert.ToInt32(mDiff[p.Y, p.X])] * curRequest.DetectionRate);
                    }
                    else
                    {
                        amountToDeduct = (float)(current * curRequest.DetectionRate);
                    }
                    break;
                case DType.FixPercentage:
                    if (curRequest.UseTaskDifficultyMap)
                    {
                        amountToDeduct = (float)(current * DiffRates[Convert.ToInt32(mDiff[p.Y, p.X])] * curRequest.DetectionRate);
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

        // Debugging shouts
        public virtual void Shout()
        {
            Console.WriteLine("I am AlgPathPlanning!");
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
        #endregion

        #endregion

    }
}
