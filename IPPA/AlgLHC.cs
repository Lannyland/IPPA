using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Drawing;
using rtwmatrix;

namespace IPPA
{
    class LHCNode : IComparable
    {
        public Point Loc = new Point();
        public float p;
        public int oldindex;

        public int CompareTo(object obj)
        {
            LHCNode Compare = (LHCNode)obj;
            int result = this.p.CompareTo(Compare.p);
            if (result == 0)
            {
                result = this.p.CompareTo(Compare.p);
            }
            return result;
        }
    }

    class LHC
    {
        #region Members

        // Private variables

        // Public variables

        #endregion

        #region Functions

        #endregion

        #region Constructor, Destructor

        // Constructor
        public LHC(Point start, RtwMatrix map, RtwMatrix reachableregion, int flighttime, float upperbound, int heuType, int param)
        {
        }

        // Destructor
        ~LHC()
        {
        }

        #endregion
    }

    class GlobalWarming
    {
        #region Members

        // Main thread sets this event to stop worker thread:
        ManualResetEvent m_EventStop;
        // Worker thread sets this event when it is stopped:
        ManualResetEvent m_EventStopped;
        // Reference to main form used to make syncronous user interface calls:
        PathPlanningSVM.frmMain m_form;

        // Private variables
        int DIM = PathPlanningSVM.ProjectConstants.DIM;
        Point Start;
        RtwMatrix mMap;
        RtwMatrix mReachableRegion;
        int T;
        float UpperBound;
        int HeuType;

        // Public variables
        public float BestCDF;
        public int NodesExpanded;
        public int PathExplored;
        public int RepeatedVisit;
        public List<Point> BestPoints;

        #endregion

        #region Functions

        public void PlanPath()
        {
            int GWCount = PathPlanningSVM.ProjectConstants.GWCount;
            int ConvCount = PathPlanningSVM.ProjectConstants.ConvCount;
            int PFCount = PathPlanningSVM.ProjectConstants.PFCount;

            // Make copy of map
            RtwMatrix mGW = mMap.Clone();

            // Find max value
            float[] minmax = mGW.MinMaxValue();
            float max = minmax[1];
            float rise = max / (GWCount + 1);

            // Loop many times
            for (int i = 0; i < GWCount; i++)
            {
                // Log("Orean rise " + i.ToString() + "\n");
                if (i > 0)
                {
                    for (int a = 0; a < mGW.Rows; a++)
                    {
                        for (int b = 0; b < mGW.Columns; b++)
                        {
                            mGW[a, b] = mGW[a, b] - rise;
                            if (mGW[a, b] < 0)
                            {
                                mGW[a, b] = 0;
                            }
                        }
                    }
                }

                if (HeuType == 1)
                {
                    // Use convolution as first heuristic
                    // Log("Using convolution as heuristic\n");
                    for (int j = 3; j < DIM; j += (int)(DIM / ConvCount))
                    {
                        // Run LHC1
                        // Log("Using kernel size " + j.ToString() + "x" + j.ToString() + "\n");
                        LHC lhc = new LHC(Start, mGW, mReachableRegion, T, UpperBound, 1, j);
                        lhc.PlanPath();
                        // Log(lhc.NodesExpanded.ToString() + " nodes expanded.\n");
                        // Log("1 full path explored.\n");
                        // Log("Theoretical upper bound is " + UpperBound.ToString() + "\n");
                        // I am recalculating BestCDF because the Global Warming effect lowered the probabilities
                        lhc.BestCDF = GetTrueCDF(lhc.BestPoints);
                        // Log("Cumulative probability is " + lhc.BestCDF.ToString() + "\n");
                        // float Efficiency = lhc.BestCDF / UpperBound * 100;
                        // Log("Efficiency is " + Efficiency.ToString() + "%\n");

                        // Increase node count and path count
                        NodesExpanded += lhc.NodesExpanded;
                        PathExplored++;

                        //Store best so far
                        if (BestCDF < lhc.BestCDF)
                        {
                            BestCDF = lhc.BestCDF;
                            BestPoints.Clear();
                            BestPoints.AddRange(lhc.BestPoints);
                        }

                        // Cleaning up
                        lhc = null;
                    }
                }
                if (HeuType == 2)
                {
                    // Use potential fields as second heuristic
                    // Log("Using potential field as heuristic\n");
                    int Sigma = 0;
                    // Loop 6 times
                    for (int j = 0; j < PFCount; j++)
                    {
                        Sigma += Convert.ToInt16(DIM / 3);
                        // Run LHC2
                        LHC lhc = new LHC(Start, mGW, mReachableRegion, T, UpperBound, HeuType, Sigma);
                        lhc.PlanPath();
                        // Log(lhc.NodesExpanded.ToString() + " nodes expanded.\n");
                        // Log("1 full path explored.\n");
                        // Log("Theoretical upper bound is " + UpperBound.ToString() + "\n");
                        lhc.BestCDF = GetTrueCDF(lhc.BestPoints);
                        // Log("Cumulative probability is " + lhc.BestCDF.ToString() + "\n");
                        // float Efficiency = lhc.BestCDF / UpperBound * 100;
                        // Log("Efficiency is " + Efficiency.ToString() + "%\n");

                        // Increase node count and path count
                        NodesExpanded += lhc.NodesExpanded;
                        PathExplored++;

                        //Store best so far
                        if (BestCDF < lhc.BestCDF)
                        {
                            BestCDF = lhc.BestCDF;
                            BestPoints.Clear();
                            BestPoints.AddRange(lhc.BestPoints);
                            RepeatedVisit = lhc.RepeatedVisit;
                        }

                        // Cleaning up
                        lhc = null;
                    }
                }

                // If we already have the path, then no need to continue
                if (Math.Abs(BestCDF - UpperBound) < 0.001)
                {
                    break;
                }

                ThreadStop();

                // Put a blank line between easy ocean rise
                // Log("\n");
            }

            // Cleaning up                
            mGW = null;

            // Inform main form the thread has finished all work
            ThreadFinish();
        }

        // Function to handle thread stop call from main form
        void ThreadStop()
        {
            // Make synchronous call to main form.
            // frmMain.Log function runs in main thread.
            // To make asynchronous call use BeginInvoke
            // m_form.Invoke(m_form.m_DelegateLog, new Object[] {  });

            // check if thread is cancelled
            if (m_EventStop != null)
            {
                if (m_EventStop.WaitOne(0, true))
                {
                    // clean-up operations may be placed here
                    // ...

                    // inform main thread that this thread stopped
                    m_EventStopped.Set();
                    // Log("Search is cancelled!\n");
                    return;
                }
            }
        }

        // Function to inform main form thread has finished
        void ThreadFinish()
        {
            // Make asynchronous call to main form
            // to inform it that thread finished
            // Log("Search completed normally!\n");
            if (m_form != null)
            {
                m_form.Invoke(m_form.m_DelegateThreadFinished, null);
            }
        }

        // Function to send log message to main form
        void Log(string msg)
        {
            if (m_form != null)
            {
                m_form.Invoke(m_form.m_DelegateLog, new Object[] { msg });
            }
        }

        // Function to calculate true cummulative probability using original map
        float GetTrueCDF(List<Point> BestPoints)
        {
            float CDF = 0;

            // Duplicate original map
            RtwMatrix mCDF = mMap.Clone();

            // Fly through the map
            for (int i = 0; i < T + 1; i++)
            {
                CDF += mCDF[BestPoints[i].Y, BestPoints[i].X];
                mCDF[BestPoints[i].Y, BestPoints[i].X] = 0;
            }

            // Cleaning up
            mCDF = null;

            return CDF;
        }

        #endregion

        #region Constructor, Destructor

        public GlobalWarming(ManualResetEvent eventStop, ManualResetEvent eventStopped, PathPlanningSVM.frmMain form,
            Point start, RtwMatrix map, RtwMatrix reachableregion, int flighttime, float upperbound, int heuType)
        {
            // Thread related
            m_EventStop = eventStop;
            m_EventStopped = eventStopped;
            m_form = form;

            // Search related
            Start = start;
            mMap = map.Clone();
            mReachableRegion = reachableregion;
            T = flighttime;
            UpperBound = upperbound;
            HeuType = heuType;

            // Instantiate and Initialize
            BestCDF = 0;
            NodesExpanded = 0;
            PathExplored = 0;
            RepeatedVisit = 0;
            BestPoints = new List<Point>();
        }

        // Destructor
        ~GlobalWarming()
        {
            mMap = null;
            mReachableRegion = null;
            m_form = null;
            BestPoints.Clear();
            BestPoints = null;
        }

        #endregion
    }


