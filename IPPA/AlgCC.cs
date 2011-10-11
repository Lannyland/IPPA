using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rtwmatrix;
using System.Drawing;

namespace IPPA
{
    class AlgCC : AlgPathPlanning
    {
        #region Members
        
        // Private variables
        
        // Public variables
        
        #endregion
        
        #region Functions

        protected override void DoPathPlanning()
        {
            bool blnClean = false;                              // Is there still probability left?
            int CurT = 0;                                       // Time used for current run (lawnmowing pattern)
            List<Point> CurPathSegment = new List<Point>();     // Path planned for current run
            int TLeft = 0;                                      // How much time left after current run
            
            // Plan to do complete coverage multiple times if partial detection is used
            while (!blnClean && TLeft>0)
            {
                // First find bounding box that contains all the non-zero probability nodes
                bool EvenColumns = false;
                int Top, Bottom, Left, Right;
                Top = -1;
                Bottom = -1;
                Left = -1;
                Right = -1;

                RtwMatrix boundingbox = GetBox(ref EvenColumns, ref Top, ref Bottom, ref Left, ref Right);

            }



        }
            
                /*

                // Next check if the starting node is inside or outside of the box.
                int t = -1;
                Point Cur_Point = new Point(Start.X, Start.Y);
                if (boundingbox[Start.Y, Start.X] == 0)
                {
                    // Outside of the box, so plan shortest path to bounding box
                    if (Start.X < Left)
                    {
                        // Move right horizentally
                        while (Cur_Point.X <= Left)
                        {
                            Point p = new Point(Cur_Point.X, Cur_Point.Y);
                            BestPoints.Add(p);
                            t++; 
                            NodesExpanded++;
                            BestCDF += mMap[Cur_Point.Y, Cur_Point.X];
                            mMap[Cur_Point.Y, Cur_Point.X] = 0;
                            Cur_Point.X++;
                        }
                        Cur_Point.X--;
                    }
                    else if (Start.X > Right)
                    {
                        // Move left horizentally
                        while (Cur_Point.X >= Left)
                        {
                            Point p = new Point(Cur_Point.X, Cur_Point.Y);
                            BestPoints.Add(p);
                            t++; 
                            NodesExpanded++;
                            BestCDF += mMap[Cur_Point.Y, Cur_Point.X];
                            mMap[Cur_Point.Y, Cur_Point.X] = 0;
                            Cur_Point.X--;
                        }
                        Cur_Point.X++;
                    }
                    else
                    {
                        // No need to move horizentally
                    }
                    if (Cur_Point.Y < Top)
                    { 
                        // Move down vertically
                        while (Cur_Point.Y <= Top)
                        {
                            Point p = new Point(Cur_Point.X, Cur_Point.Y);
                            BestPoints.Add(p);
                            t++; 
                            NodesExpanded++;
                            BestCDF += mMap[Cur_Point.Y, Cur_Point.X];
                            mMap[Cur_Point.Y, Cur_Point.X] = 0;
                            Cur_Point.Y++;
                        }
                        Cur_Point.Y--;
                    }
                    else if (Cur_Point.Y > Bottom)
                    {
                        // Move up vertically
                        while (Cur_Point.Y >= Bottom)
                        {
                            Point p = new Point(Cur_Point.X, Cur_Point.Y);
                            BestPoints.Add(p);
                            t++; 
                            NodesExpanded++;
                            BestCDF += mMap[Cur_Point.Y, Cur_Point.X];
                            mMap[Cur_Point.Y, Cur_Point.X] = 0;
                            Cur_Point.Y--;
                        }
                        Cur_Point.Y++;
                    }
                    else
                    {
                        // No need to move vertically
                    }
                }
                else
                {
                    // Inside the box. Let's add Current Node first
                    Point p = new Point(Cur_Point.X, Cur_Point.Y);
                    BestPoints.Add(p);
                    NodesExpanded++;
                    BestCDF += mMap[Cur_Point.Y, Cur_Point.X];
                    mMap[Cur_Point.Y, Cur_Point.X] = 0;
                    t++;
                }

                // Remember starting position inside bounding box
                Point boxstart = new Point(Cur_Point.X, Cur_Point.Y);
                int tempt = t;

                // Once inside bounding box fly the pattern until mxn-1 steps (complete coverage)
                if (EvenColumns)
                {
                    // Depending on the current position, decide which direction to go
                    while (((Cur_Point.X != boxstart.X || Cur_Point.Y != boxstart.Y) && t <= T) || t <= tempt)
                    {
                        if (t > tempt)
                        {
                            Point p = new Point(Cur_Point.X, Cur_Point.Y);
                            BestPoints.Add(p);
                            NodesExpanded++;
                            BestCDF += mMap[Cur_Point.Y, Cur_Point.X];
                            mMap[Cur_Point.Y, Cur_Point.X] = 0;
                        }
                        t++;

                        if (Cur_Point.X >= Left && Cur_Point.X < Right && Cur_Point.Y == Top)
                        {
                            // Top left corner. Go right
                            Cur_Point.X++;
                        }
                        else if (Cur_Point.X == Right && Cur_Point.Y >= Top && Cur_Point.Y < Bottom)
                        {
                            // Top right corner. Go down
                            Cur_Point.Y++;
                        }
                        else if (Cur_Point.Y == Bottom && (Cur_Point.X - Left) % 2 == 1)
                        {
                            // Bottom right corners. Go left
                            Cur_Point.X--;
                        }
                        else if (Cur_Point.Y <= Bottom && Cur_Point.Y > Top + 1 && (Cur_Point.X - Left) % 2 == 0)
                        {
                            // Bottom left corners. Go up
                            Cur_Point.Y--;
                        }
                        else if (Cur_Point.Y == Top + 1 && Cur_Point.X > Left && (Cur_Point.X - Left) % 2 == 0)
                        {
                            // Second row right corners. Go left
                            Cur_Point.X--;
                        }
                        else if (Cur_Point.Y >= Top + 1 && Cur_Point.Y < Bottom && (Cur_Point.X - Left) % 2 == 1)
                        {
                            // Second row left corners. Go down
                            Cur_Point.Y++;
                        }
                        else if (Cur_Point.X == Left && Cur_Point.Y == Top + 1)
                        {
                            // Point left of top right corner. Go Right
                            Cur_Point.Y--;
                        }
                    }
                }
                else
                {
                    // Turn the pattern 90 degrees clockwise
                    while (((Cur_Point.X != boxstart.X || Cur_Point.Y != boxstart.Y) && t <= T) || t <= tempt)
                    {
                        if (t > tempt)
                        {
                            Point p = new Point(Cur_Point.X, Cur_Point.Y);
                            BestPoints.Add(p);
                            NodesExpanded++;
                            BestCDF += mMap[Cur_Point.Y, Cur_Point.X];
                            mMap[Cur_Point.Y, Cur_Point.X] = 0;
                        }
                        t++;

                        if (Cur_Point.X == Right && Cur_Point.Y >= Top && Cur_Point.Y < Bottom)
                        {
                            // Top right corner. Go down
                            Cur_Point.Y++;
                        }
                        else if (Cur_Point.X <= Right && Cur_Point.X > Left && Cur_Point.Y == Bottom)
                        {
                            // Bottom right corner. Go left
                            Cur_Point.X--;
                        }
                        else if (Cur_Point.X == Left && (Cur_Point.Y - Top) % 2 == 1)
                        {
                            // Left bottom corners. Go up
                            Cur_Point.Y--;
                        }
                        else if (Cur_Point.X >= Left && Cur_Point.X < Right - 1 && (Cur_Point.Y - Top) % 2 == 0)
                        {
                            // Left top corners. Go right
                            Cur_Point.X++;
                        }
                        else if (Cur_Point.X == Right - 1 && Cur_Point.Y > Top && (Cur_Point.Y - Top) % 2 == 0)
                        {
                            // Second column from right bottom corners. Go up
                            Cur_Point.Y--;
                        }
                        else if (Cur_Point.X <= Right - 1 && Cur_Point.X > Left && (Cur_Point.Y - Top) % 2 == 1)
                        {
                            // Second column from right top corners. Go left
                            Cur_Point.X--;
                        }
                        else if (Cur_Point.X == Right - 1 && Cur_Point.Y == Top)
                        {
                            // Point left of top right corner. Go Right
                            Cur_Point.X++;
                        }
                    }
                }

                // Once everything is covered, let's just fly North, East, South, West until all timesteps are used ups
                // First add the anchor node in path the second time
                if (t < T)
                {
                    BestPoints.Add(boxstart);
                }
                NodesExpanded++;
                // No need to t++
                // Now let's just fly straight lines going north first
                int LastDir = 0;
                while (t <= T)
                {
                    // Loop through all four directions (N, E, S, W)
                    for (int i = LastDir; i < LastDir+4; i++)
                    {
                        // Expand children
                        Point child = MISCLib.get_delta(i%4, Cur_Point);
                        NodesExpanded++;

                        // Check if it's valid children (no repeat first)
                        if (MISCLib.ValidMove(Cur_Point, Cur_Point, child, mReachableRegion, true, mReachableRegion))
                        {
                            BestPoints.Add(child);
                            Cur_Point.X = child.X;
                            Cur_Point.Y = child.Y;
                            LastDir = i%4;
                            break;
                        }
                    }
                    t++;
                }

            }
    }

                */

        // Function to identify a m x n box bounding non-zero nodes where m is even
        RtwMatrix GetBox(ref bool EvenColumns, ref int Top, ref int Bottom, ref int Left, ref int Right)
        {
            RtwMatrix mbox = new RtwMatrix(mDist.Rows, mDist.Columns);
            // First pass find top
            for (int i = 0; i < mDist.Rows; i++)
            {
                for (int j = 0; j < mDist.Columns; j++)
                {
                    if (mDist[i, j] > 0)
                    {
                        Top = i;
                        j = mDist.Columns;
                        i = mDist.Rows;
                    }
                }
            }
            // Second pass find bottom
            for (int i = mDist.Rows - 1; i > -1; i--)
            {
                for (int j = mDist.Columns - 1; j > -1; j--)
                {
                    if (mDist[i, j] > 0)
                    {
                        Bottom = i;
                        j = -1;
                        i = -1;
                    }
                }
            }
            // Third pass find left
            for (int i = 0; i < mDist.Columns; i++)
            {
                for (int j = 0; j < mDist.Rows; j++)
                {
                    if (mDist[j, i] > 0)
                    {
                        Left = i;
                        j = mDist.Rows;
                        i = mDist.Columns;
                    }
                }
            }
            // Fourth pass find right
            for (int i = mDist.Columns - 1; i > -1; i--)
            {
                for (int j = mDist.Rows - 1; j > -1; j--)
                {
                    if (mDist[j, i] > 0)
                    {
                        Right = i;
                        j = -1;
                        i = -1;
                    }
                }
            }

            // Create binary mask matrix
            for (int i = Top; i < Bottom + 1; i++)
            {
                for (int j = Left; j < Right + 1; j++)
                {
                    mbox[i, j] = 1;
                }
            }

            // Next check if it is a m x n box where m is even
            if ((Right + 1 - Left) % 2 == 0)
            {
                // Even number of columns
                EvenColumns = true;
            }
            else if ((Bottom + 1 - Top) % 2 == 0)
            {
                // Odd number of columns but even number or rows
                EvenColumns = false;
            }
            else
            {
                // Odd numbber of columns and odd number of rows
                // Need to patch this
                // Let's first try find the shorter line of nodes to add
                if (Right + 1 - Left > Bottom + 1 - Top)
                {
                    // Row is longer, column is shorter. We should add column
                    // Since DIM is even, we can always add a column
                    if (Left != 0)
                    {
                        Left--;
                        for (int i = Top; i < Bottom + 1; i++)
                        {
                            mbox[i, Left] = 1;
                        }
                    }
                    else
                    {
                        Right++;
                        for (int i = Top; i < Bottom + 1; i++)
                        {
                            mbox[i, Right] = 1;
                        }
                    }
                    EvenColumns = true;
                }
                else
                {
                    // Add a row instead
                    if (Top != 0)
                    {
                        Top--;
                        for (int i = Left; i < Right + 1; i++)
                        {
                            mbox[Top, i] = 1;
                        }
                    }
                    else
                    {
                        Bottom++;
                        for (int i = Left; i < Right + 1; i++)
                        {
                            mbox[Bottom, i] = 1;
                        }
                    }
                    EvenColumns = false;
                }
            }

            return mbox;
        }


        #endregion
        
        #region Constructor, Destructor

        public AlgCC(PathPlanningRequest _curRequest, RtwMatrix _mDistReachable,
            RtwMatrix _mDiffReachable, double _Efficiency_LB, int _Sigma)
            : base(_curRequest, _mDistReachable, _mDiffReachable, _Efficiency_LB)
        {
        }
        
        // Destructor
        ~AlgCC()
        {
        }
        
        #endregion
    }
}
