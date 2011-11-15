using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rtwmatrix;

namespace IPPA
{
    public class DistPoint
    {
        public int row;
        public int column;

        #region Constructor, Destructor

        // Constructor
        public DistPoint(int _row, int _column)
        {
            row = _row;
            column = _column;
        }

        public DistPoint()
        {
        }

        // Destructor
        ~DistPoint()
        {
        }

        #endregion
    }

    class CountDistModes
    {
        #region Members

        // Private variables
        private RtwMatrix mReachableRegion;
        private RtwMatrix mSmallerMap;
        private RtwMatrix mVisited;
        private RtwMatrix mModes;
        private List<DistPoint> lstCurPath;
        private int Count = 0;
        private int max_r;
        private int max_c;

        private RtwMatrix mPaths;
        private int pathCount = 0;

        #endregion

        #region Constructor, Destructor

        // Constructor
        public CountDistModes(RtwMatrix reachableregion)
        {
            mReachableRegion = reachableregion;
            lstCurPath = new List<DistPoint>();
        }

        //Destructor
        ~CountDistModes()
        {
            // Cleaning up
            mSmallerMap = null;
            mVisited = null;
            mModes = null;
            lstCurPath.Clear();
            lstCurPath = null;
        }
        #endregion

        #region Functions

        // Public method to return count
        public int GetCount()
        {
            // First down sample the map
            DownSampleMap();
            // If uniform distribution, then just return 0 mode
            if (CheckUniformDist())
            {
                return 0;
            }
            // Create mask to mark visited cells (default 0s)
            mVisited = new RtwMatrix(mSmallerMap.Rows, mSmallerMap.Columns);
            // Create another mask to record mode pixels with connected-component-labeling (default 0s)
            mModes = new RtwMatrix(mSmallerMap.Rows, mSmallerMap.Columns);
            mPaths = new RtwMatrix(mSmallerMap.Rows, mSmallerMap.Columns);
            // Boundaries
            max_r = mVisited.Rows - 1;
            max_c = mVisited.Columns - 1;
            
            // Loop through all unvisited cells and do local hill climbing
            for (int i = 0; i < mSmallerMap.Rows; i++)
            {
                for (int j = 0; j < mSmallerMap.Columns; j++)
                {
                    // Only do this with unvisited cells
                    if (mVisited[i, j] == 0)
                    {
                        // Perform Local Hill Climbing
                        LocalHillClimbing(i, j);
                    }
                }
            }

            //// Debug
            //Console.Write("\n");
            //Console.Write("mModes:\n");
            //for (int i = 0; i < mModes.Rows; i++)
            //{
            //    for (int j = 0; j < mModes.Columns; j++)
            //    {
            //        Console.Write(mModes[i, j].ToString() + " ");
            //    }
            //    Console.Write("\n");
            //}

            //Console.Write("Paths:\n");
            //for (int i = 0; i < mPaths.Rows; i++)
            //{
            //    for (int j = 0; j < mPaths.Columns; j++)
            //    {
            //        Console.Write(mPaths[i, j].ToString() + " ");
            //    }
            //    Console.Write("\n");
            //}
            return Count;
        }

        // Publoic method to return modes nodes
        public RtwMatrix GetModes()
        {
            return mModes;
        }

        // Checking whether the distribution is a uniform distribution.
        private bool CheckUniformDist()
        {
            bool blnUniform = true;
            float value = mSmallerMap[0, 0];
            // Loop through all cells and compare
            for (int i = 0; i < mSmallerMap.Rows; i++)
            {
                for (int j = 0; j < mSmallerMap.Columns; j++)
                {
                    if (value != mSmallerMap[i,j])
                    {
                        blnUniform = false;
                        i = mSmallerMap.Rows;
                        j = mSmallerMap.Columns;
                    }
                }
            }
            return blnUniform;
        }

        // Down sample a given matrix using scale specified in project constants page.
        private void DownSampleMap()
        {
            int s = ProjectConstants.DownSampleScale;
            if (s == 1)
            {
                mSmallerMap = mReachableRegion.Clone();
            }
            else
            {
                mSmallerMap = new RtwMatrix(mReachableRegion.Rows / s, mReachableRegion.Columns / s);
                int iBig = 0;
                int jBig = 0;
                for (int i = 0; i < mSmallerMap.Rows; i++)
                {
                    iBig = i * s;
                    for (int j = 0; j < mSmallerMap.Columns; j++)
                    {
                        jBig = j * s;
                        mSmallerMap[i, j] = mReachableRegion[iBig, jBig];
                        // Console.Write(mSmallerMap[i, j] + " ");
                    }
                    // Console.Write("\n");
                }
            }
        }

        // LHC Algorithm to identify modes
        private void LocalHillClimbing(int C_i, int C_j)
        {
            // A new path now
            pathCount++;
            
            #region FindMode

            // Climb
            bool blnMode = false;
            while (!blnMode)
            {
                // Mark self as visited
                mVisited[C_i, C_j] = 1;
                // Label cell with current path count
                mPaths[C_i, C_j] = pathCount;
                // Loop through all unvisited neighbors to see if there's someone same or better
                bool blnMoved = false;
                for (int i = C_i - 1; i < C_i + 2; i++)
                {
                    if (i < 0 || i > max_r)
                    {
                        // No neighber in that direction
                    }
                    else
                    {
                        for (int j = C_j - 1; j < C_j + 2; j++)
                        {
                            if (j < 0 || j > max_c               // No neighber in that direction
                                || (i == C_i && j == C_j)        // Don't compare against self
                                || mVisited[i, j] == 1)          // Don't worry about visited cell
                            {
                                // No valid neighbor in this direction.
                            }
                            else
                            {
                                // There is an unvisited neightbor
                                // If same or better, move to it.
                                if (mSmallerMap[C_i, C_j] <= mSmallerMap[i, j])
                                {
                                    if (mSmallerMap[C_i, C_j] == mSmallerMap[i, j])
                                    {
                                        // If same, then remember it
                                        DistPoint p = new DistPoint();
                                        p.row = C_i;
                                        p.column = C_j;
                                        lstCurPath.Add(p);
                                    }
                                    else
                                    {
                                        // If next point is better, clear the list
                                        lstCurPath.Clear();
                                    }
                                    // Move to this neighbor
                                    blnMoved = true;
                                    C_i = i;
                                    C_j = j;
                                    // Make sure I am done with this round
                                    i = C_i + 2;
                                    j = C_j + 2;
                                }
                            }
                        }

                    }
                }
                if (!blnMoved)
                {
                    // No better neighbor, so I am the mode.
                    blnMode = true;
                }
            }

            #endregion
            
            #region ValidateMode

            // Mark self as visited
            mVisited[C_i, C_j] = 1;
            // Label cell with current path count
            mPaths[C_i, C_j] = pathCount;
            // Add self to path (mode section)
            DistPoint C_p = new DistPoint();
            C_p.row = C_i;
            C_p.column = C_j;
            lstCurPath.Add(C_p);

            //// Debug
            //Console.Write("\n");
            //Console.Write("mModes:\n");
            //for (int i = 0; i < mModes.Rows; i++)
            //{
            //    for (int j = 0; j < mModes.Columns; j++)
            //    {
            //        Console.Write(mModes[i, j].ToString() + " ");
            //    }
            //    Console.Write("\n");
            //}
            //Console.Write("Paths:\n");
            //for (int i = 0; i < mPaths.Rows; i++)
            //{
            //    for (int j = 0; j < mPaths.Columns; j++)
            //    {
            //        Console.Write(mPaths[i, j].ToString() + " ");
            //    }
            //    Console.Write("\n");
            //}

            // Now let's see if this is really a mode
            bool blnRealMode = true;

            // Make sure I am really the mode by checking all visited neighbors along the path (mode section)
            float otherMode = -1;
            foreach (DistPoint p in lstCurPath)
            {
                if (!blnRealMode)
                {
                    // As soon as we find a connected mode, stop the loop.
                    break;
                }
                for (int i = p.row - 1; i < p.row + 2; i++)
                {
                    if (i < 0 || i > max_r)
                    {
                        // No neighber in that direction
                    }
                    else
                    {
                        for (int j = p.column - 1; j < p.column + 2; j++)
                        {
                            DistPoint pp = new DistPoint();
                            pp.row = i;
                            pp.column = j;
                            if (j < 0 || j > max_c                  // No neighber in that direction
                                || (i == p.row && j == p.column)    // Don't compare against self
                                || PointInPath(pp)                   // Don't check this path because it doesn't have mode label yet.
                                || mVisited[i, j] == 0)             // Only check visited nodes
                            {
                                // No valid neighbor
                            }
                            else
                            {
                                if (mSmallerMap[p.row, p.column] <= mSmallerMap[i, j])
                                {
                                    // I am not a real mode
                                    blnRealMode = false;
                                    if (mSmallerMap[p.row, p.column] == mSmallerMap[i, j]
                                        && mModes[i, j] >= 0)
                                    {
                                        // I am part of another mode
                                        otherMode = mModes[i, j];
                                    }
                                    // Make sure I am done.
                                    i = p.row + 2;
                                    j = p.column + 2;
                                }
                            }
                        }
                    }
                }
            }

            if (blnRealMode)
            {
                Count++;
            }

            #endregion
            
            // Connected-Component-Labeling
            if (blnRealMode)
            {
                // If I am the real mode, then mark all past cells on path with same height the new modeCount value.
                foreach (DistPoint p in lstCurPath)
                {
                    mModes[p.row, p.column] = Count;
                }
            }
            else
            {
                if (otherMode > 0)
                {
                    foreach (DistPoint p in lstCurPath)
                    {
                        mModes[p.row, p.column] = otherMode;
                    }
                }
            }

            // Clear list
            lstCurPath.Clear();
        }

        // See if a point is in the current path mode section
        private bool PointInPath(DistPoint C_p)
        {
            bool blnInPath = false;
            foreach (DistPoint p in lstCurPath)
            {
                if (p.row == C_p.row && p.column == C_p.column)
                {
                    blnInPath = true;
                    break;
                }
            }
            return blnInPath;
        }

        #endregion
    }
}
