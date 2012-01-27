using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using rtwmatrix;

namespace IPPA
{
    class ComputeEfficiencyUB
    {
        #region Members
        PathPlanningRequest curRequest;
        RtwMatrix mDistReachable;
        RtwMatrix mDiffReachable;
        double Efficiency_UB = 0;
        #endregion

        #region Constructor, Destructor

        // Constructor
        public ComputeEfficiencyUB(PathPlanningRequest _curRequest, RtwMatrix _mDistReachable, RtwMatrix _mDiffReachable)
        {
            curRequest = _curRequest;
            mDistReachable = _mDistReachable;
            mDiffReachable = _mDiffReachable;
            Compute();
        }

        // Destructor
        ~ComputeEfficiencyUB()
        {
            // Cleaning up
            curRequest = null;
        }

        #endregion

        #region Other Functions

        // Compute the Efficiency Lower Bound based on PathPlanningRequest scenario
        private void Compute()
        {
            // TODO actually compute the Efficiency_UB

            // For now just teleport again and again and again

            // Clone dist map so we can update it.
            RtwMatrix mCurDist = mDistReachable.Clone();

            // Find closest non-zero node and remember distance
            int dStart = mCurDist.Rows + mCurDist.Columns;
            int dEnd = dStart;
            Point Start = new Point(curRequest.pStart.column, curRequest.pStart.row);
            Point End = new Point(0, 0);
            if (curRequest.UseEndPoint)
            {
                End = new Point(curRequest.pEnd.column, curRequest.pEnd.row);
            }
            for (int i = 0; i < mCurDist.Rows; i++)
            {
                for (int j = 0; j < mCurDist.Columns; j++)
                {
                    if (mCurDist[i, j] > 0)
                    {
                        Point p = new Point(j,i);
                        int distStart = MISCLib.ManhattanDistance(Start, p);
                        if (distStart < dStart)
                        {
                            dStart = distStart;
                        }
                        if (curRequest.UseEndPoint)
                        {
                            int distEnd = MISCLib.ManhattanDistance(End, p);
                            if (distEnd < dEnd)
                            {
                                dEnd = distEnd;
                            }
                        }
                    }
                }
            }
            int d = dStart;
            if (curRequest.UseEndPoint)
            {
                d += dEnd;
            }

            // Find Efficiency_UB
            float maxCDF = 0;
            for (int t = 0; t < curRequest.T + 1 - d; t++)
            {
                float pMax = 0;
                int iMax = 0;
                int jMax = 0;
                // Find best thing
                for (int i = 0; i < mCurDist.Rows; i++)
                {
                    for (int j = 0; j < mCurDist.Columns; j++)
                    {
                        float p = GetPartialDetection(new Point(j, i), mCurDist);
                        if (pMax < p)
                        {
                            pMax = p;
                            iMax = i;
                            jMax = j;
                        }
                    }
                }
                // Found the best, accumulate
                maxCDF += pMax;
                // Update map
                mCurDist[iMax, jMax] -= pMax;
            }

            // Found the best one can do.
            Efficiency_UB = maxCDF;
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
                        original = mDistReachable[p.Y, p.X];
                        amountToDeduct = (float)(original * curRequest.DiffRates[Convert.ToInt32(mDiffReachable[p.Y, p.X])] * curRequest.DetectionRate);
                    }
                    else
                    {
                        amountToDeduct = (float)(current * curRequest.DetectionRate);
                    }
                    break;
                case DType.FixPercentage:
                    if (curRequest.UseTaskDifficultyMap)
                    {
                        amountToDeduct = (float)(current * curRequest.DiffRates[Convert.ToInt32(mDiffReachable[p.Y, p.X])] * curRequest.DetectionRate);
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
        protected float VacuumProbability(Point p, RtwMatrix m)
        {
            float current = m[p.Y, p.X];
            float amountLeft = current - GetPartialDetection(p, m);
            return amountLeft;
        }

        #region Getters
        public double GetEfficiency_UB()
        {
            return Efficiency_UB;
        }
        #endregion

        #endregion

    }
}
