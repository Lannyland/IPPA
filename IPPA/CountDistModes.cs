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
        private int Count = 0;

        #endregion

        #region Functions

        // Public method to return count
        public int GetCount()
        {
            // First down sample the map
            DownSampleMap();
            // Create mask to mark visited cells (default 0s)
            mVisited = new RtwMatrix(mSmallerMap.Rows, mSmallerMap.Columns);
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
                        LocalHillClimbing(i, j);
                    }
                }
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
        private void LocalHillClimbing(int C_i, int C_j)
        {
            int max_r = mVisited.Rows-1;
            int max_c = mVisited.Columns-1;
            // Recursive function to move up in local hill climbing
            MoveUp(C_i, C_j, max_r, max_c, C_i, C_j);
        }

        // Recursive function to always move up until reaching peak
        private void MoveUp(int C_i, int C_j, int max_r, int max_c, int L_i, int L_j)
        {
            if (C_i == 0 && C_j == 58)
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
                            // First mark it as visited
                            mVisited[i, j] = 1;
                            // If same or better, move to it.
                            if (mSmallerMap[C_i, C_j] <= mSmallerMap[i, j])
                            {
                                // Make sure I am done with this round
                                int cur_i = i;
                                int cur_j = j;
                                i = C_i + 2;
                                j = C_j + 2;
                                blMovedUp = true;
                                // Move up (or flat)
                                MoveUp(cur_i, cur_j, max_r, max_c, C_i, C_j);
                            }
                        }
                    }
                }
            }
            // Didn't find any neighbor same or better
            if (!blMovedUp)
            {
                // I am already the best (hopefully)
                Count++;
                // Make sure I am really the mode by checking all visited neighbors.
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
                            if (j < 0 || j > max_c              // No neighber in that direction
                                || (i == C_i && j == C_j)       // Don't compare against self
                                || (i == L_i && j == L_j)       // Don't check where I came from
                                || mVisited[i,j] == 0)          // Only check visited nodes
                            {
                            }
                            else
                            {
                                if (mSmallerMap[C_i, C_j] <= mSmallerMap[i, j])
                                {
                                    // I am not a mode
                                    i = C_i + 2;
                                    j = C_j + 2;
                                    Count--;
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

        #region Constructor, Destructor

        public CountDistModes(ref RtwMatrix reachableregion)
        {
            mReachableRegion = reachableregion;
        }

        //Destructor
        ~CountDistModes()
        {
            // Cleaning up
            mReachableRegion = null;
        }
        #endregion
    }
}
