using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rtwmatrix;

namespace IPPA
{
    class CountDistModes
    {
        #region Members

        // Private variables
        private RtwMatrix mReachableRegion;
        private RtwMatrix mSmallerMap;
        private RtwMatrix mVisited;
        private RtwMatrix mModes;
        private List<int[]> lstCurPath;
        private int Count = 0;

        private RtwMatrix mPaths;
        private int pathCount = 0;

        #endregion

        #region Functions

        // Public method to return count
        public int GetCount()
        {
            // First down sample the map
            DownSampleMap();
            // Create mask to mark visited cells (default 0s)
            mVisited = new RtwMatrix(mSmallerMap.Rows, mSmallerMap.Columns);
            // Create another mask to record mode pixels with connected-component-labeling (default 0s)
            mModes = new RtwMatrix(mSmallerMap.Rows, mSmallerMap.Columns);
            mPaths = new RtwMatrix(mSmallerMap.Rows, mSmallerMap.Columns);
            // Boundaries
            int max_r = mVisited.Rows - 1;
            int max_c = mVisited.Columns - 1;
            // Loop through all unvisited cells and do local hill climbing
            for (int i = 0; i < mSmallerMap.Rows; i++)
            {
                for (int j = 0; j < mSmallerMap.Columns; j++)
                {
                    // Only do this with unvisited cells
                    if (mVisited[i, j] == 0)
                    {
                        // Mark self as visited
                        mVisited[i, j] = 1;
                        // Perform Local Hill Climbing
                        LocalHillClimbing(i, j, max_r, max_c);
                    }
                }
            }

            // Debug
            Console.Write("\n");
            Console.Write("mModes:\n");
            for (int i = 0; i < mModes.Rows; i++)
            {
                for (int j = 0; j < mModes.Columns; j++)
                {
                    Console.Write(mModes[i, j].ToString() + " ");
                }
                Console.Write("\n");
            }

            Console.Write("Paths:\n");
            for (int i = 0; i < mPaths.Rows; i++)
            {
                for (int j = 0; j < mPaths.Columns; j++)
                {
                    Console.Write(mPaths[i, j].ToString() + " ");
                }
                Console.Write("\n");
            }
            return Count;
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
                    Console.Write("\n");
                }
            }
        }

        // LHC Algorithm to identify modes
        private void LocalHillClimbing(int C_i, int C_j, int max_r, int max_c)
        {
            pathCount++;
            if (pathCount == 45)
            {
                Console.Write(" ");
            }
            // Recursive function to move up in local hill climbing
            MoveUp(C_i, C_j, max_r, max_c, C_i, C_j);
        }

        // Recursive function to always move up until reaching peak
        private void MoveUp(int C_i, int C_j, int max_r, int max_c, int L_i, int L_j)
        {
            mPaths[C_i, C_j] = pathCount;
            if (C_i == 3 && C_j == 17)
            {
                Console.Write("Something!");
            }
            // Did I move up to another cell?
            Boolean blMovedUp = false;
            // Visit unvisited neighbors and find max (8-connect)
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
                            // No unvisited neighbor in this direction.
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
                                    int[] point = new int[2];
                                    point[0]=C_i;
                                    point[1]=C_j;
                                    lstCurPath.Add(point);
                                }
                                else 
                                {
                                    // If next point is better, clear the list
                                    lstCurPath.Clear();
                                }
                                // Make sure I am done with this round
                                int cur_i = i;
                                int cur_j = j;
                                i = C_i + 2;
                                j = C_j + 2;
                                blMovedUp = true;
                                // First mark it as visited
                                mVisited[cur_i, cur_j] = 1;
                                // Move up (or flat)
                                MoveUp(cur_i, cur_j, max_r, max_c, C_i, C_j);
                            }
                        }
                    }
                }
            }
            // Didn't find any neighbor same or better, so I am the peak
            if (!blMovedUp)
            {
                // I am already the best (hopefully)
                int[] point = new int[2];
                point[0]=C_i;
                point[1]=C_j;
                lstCurPath.Add(point);
                Count++;
                bool blnRealMode = true;
                // Make sure I am really the mode by checking all visited neighbors.
                float otherMode = -1;
                foreach (int[] p in lstCurPath)
                {
                    if (!blnRealMode)
                    {
                        // As soon as we find a connected mode, stop the loop.
                        break;
                    }
                    for (int i = p[0] - 1; i < p[0] + 2; i++)
                    {
                        if (i < 0 || i > max_r)
                        {
                            // No neighber in that direction
                        }
                        else
                        {
                            for (int j = p[1] - 1; j < p[1] + 2; j++)
                            {
                                if (j < 0 || j > max_c          // No neighber in that direction
                                    || (i == p[0] && j == p[1]) // Don't compare against self
                                    || mModes[i,j]<=0           // Don't check this path because it doesn't have mode label yet.
                                    || mVisited[i, j] == 0)     // Only check visited nodes
                                {
                                }
                                else
                                {
                                    if (mSmallerMap[p[0], p[1]] <= mSmallerMap[i, j])
                                    {
                                        // I am not a real mode
                                        blnRealMode = false;
                                        if (mSmallerMap[p[0], p[1]] == mSmallerMap[i, j]
                                            && mModes[i, j] > 0)
                                        {
                                            // I am part of another mode
                                            otherMode = mModes[i, j];
                                        }
                                        // Make sure I am done.
                                        i = p[0] + 2;
                                        j = p[1]+ 2;
                                        // Correct count
                                        Count--;
                                    }
                                }
                            }
                        }
                    }
                }
                // Connected-Component-Labeling
                if (blnRealMode)
                {
                    // If I am the real mode, then mark all past cells on path with same height the new modeCount value.
                    foreach (int[] p in lstCurPath)
                    {
                        mModes[p[0], p[1]] = Count;
                    }
                }
                else
                {
                    if (otherMode > 0)
                    {
                        foreach (int[] p in lstCurPath)
                        {
                            mModes[p[0], p[1]] = otherMode;
                        }
                    }
                }

                // Clear list
                lstCurPath.Clear();

                /*
                Console.Write("\n");
                Console.Write("mModes:\n");
                for (int i = 0; i < mModes.Rows; i++)
                {
                    for (int j = 0; j < mModes.Columns; j++)
                    {
                        Console.Write(mModes[i, j].ToString() + " ");
                    }
                    Console.Write("\n");
                }
                Console.Write("Paths:\n");
                for (int i = 0; i < mPaths.Rows; i++)
                {
                    for (int j = 0; j < mPaths.Columns; j++)
                    {
                        Console.Write(mPaths[i, j].ToString() + " ");
                    }
                    Console.Write("\n");
                }
                */
            }
        }

        #endregion

        #region Constructor, Destructor

        public CountDistModes(ref RtwMatrix reachableregion)
        {
            mReachableRegion = reachableregion;
            lstCurPath = new List<int[]>();
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
    }
}
