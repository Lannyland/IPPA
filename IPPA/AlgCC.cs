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

        #region Constructor, Destructor

        public AlgCC(PathPlanningRequest _curRequest, RtwMatrix _mDistReachable,
            RtwMatrix _mDiffReachable, double _Efficiency_UB)
            : base(_curRequest, _mDistReachable, _mDiffReachable, _Efficiency_UB)
        {
        }

        // Destructor
        ~AlgCC()
        {
        }

        #endregion
        
        #region Functions

        protected override void DoPathPlanning()
        {
            bool blnClean = false;                              // Is there still probability left?
            int CurT = 0;                                       // Time used for current run (lawnmowing pattern)
            int RealT = 0;                                      // How much time left after current run
            int PatternStepCount = 0;                           // Used to remember last flight pattern inside box
            Point CurStart = new Point(curRequest.pStart.column, curRequest.pStart.row); // Start point in each run
            List<Point> CurPathSegment = new List<Point>();     // Path planned for current run
            CurPathSegment.Add(CurStart);                       // Only do this once. Don't add Start again in future runs.

            // Plan to do complete coverage multiple times if partial detection is used
            // Before distribution map is wiped clean
            while (!blnClean && RealT < curRequest.T)
            {
                // First find bounding box that contains all the non-zero probability nodes
                bool EvenColumns = false;
                int Top, Bottom, Left, Right;
                Top = -1;
                Bottom = -1;
                Left = -1;
                Right = -1;

                RtwMatrix boundingbox = GetBox(ref EvenColumns, ref Top, ref Bottom, ref Left, ref Right);

                // If nothing left on map, exit while loop
                if (Top == -1 && Bottom == -1 && Left == -1 && Right == -1)
                {
                    blnClean = true;
                    break;
                }

                #region Move inside the box if not in
                                               
                // Reset pattern step count
                PatternStepCount = 0;
                // Move inside
                Point Start = CurStart;
                Point CurPoint = new Point(Start.X, Start.Y);
                if (boundingbox[Start.Y, Start.X] == 0)
                {
                    // Outside of the box, so plan shortest path to bounding box
                    Point Parent;
                    if (Path.Count == 0)
                    {
                        Parent = Start;
                    }
                    else if (Path.Count == 1)
                    {
                        Parent = Path[Path.Count - 1];
                    }
                    else
                    {
                        Parent = Path[Path.Count - 2];
                    }
                    if (Start.X < Left)
                    {
                        // Should go right
                        // Make sure it's a valid flight pattern for given UAV
                        Point Child = new Point(CurPoint.X + 1, CurPoint.Y);
                        if(!ValidMove(Parent, CurPoint, Child))
                        {
                            if (CurPoint.Y < Top || CurPoint.Y == 0)
                            {
                                CurPoint.Y++;
                            }
                            else if (CurPoint.Y > Bottom || CurPoint.Y == mDist.Rows - 1)
                            {
                                CurPoint.Y--;
                            }
                            else
                            {
                                // Just go down (choices are up or down)
                                CurPoint.Y++;
                            }
                            AddNodeToPath(CurPathSegment, ref CurT, ref RealT, ref CurPoint);
                        }
                        // Move right horizentally
                        while (CurPoint.X < Left)
                        {
                            CurPoint.X++;
                            AddNodeToPath(CurPathSegment, ref CurT, ref RealT, ref CurPoint);
                        }
                    }
                    else if (Start.X > Right)
                    {
                        // Should go left
                        // Make sure it's a valid flight pattern for given UAV
                        Point Child = new Point(CurPoint.X - 1, CurPoint.Y);
                        if (!ValidMove(Parent, CurPoint, Child))
                        {
                            if (CurPoint.Y < Top || CurPoint.Y == 0)
                            {
                                CurPoint.Y++;
                            }
                            else if (CurPoint.Y > Bottom || CurPoint.Y == mDist.Rows - 1)
                            {
                                CurPoint.Y--;
                            }
                            else
                            {
                                // Just go down (choices are up or down)
                                CurPoint.Y++;
                            }
                            AddNodeToPath(CurPathSegment, ref CurT, ref RealT, ref CurPoint);
                        }
                        // Move left horizentally
                        while (CurPoint.X > Right)
                        {
                            CurPoint.X--;
                            AddNodeToPath(CurPathSegment, ref CurT, ref RealT, ref CurPoint);
                        }
                    }
                    else
                    {
                        // No need to move horizentally
                    }
                    if (CurPoint.Y < Top)
                    {
                        // Should go down
                        // Make sure it's a valid flight pattern for given UAV
                        Point Child = new Point(CurPoint.X, CurPoint.Y + 1);
                        if (!ValidMove(Parent, CurPoint, Child))
                        {
                            if (CurPoint.X < Left || CurPoint.X == 0)
                            {
                                CurPoint.X++;
                            }
                            else if (CurPoint.X > Right || CurPoint.X == mDist.Columns - 1)
                            {
                                CurPoint.X--;
                            }
                            else
                            {
                                // Just go right (choices are left or right)
                                CurPoint.X++;
                            }
                            AddNodeToPath(CurPathSegment, ref CurT, ref RealT, ref CurPoint);
                        }
                        // Move down vertically
                        while (CurPoint.Y < Top)
                        {
                            CurPoint.Y++;
                            AddNodeToPath(CurPathSegment, ref CurT, ref RealT, ref CurPoint);
                        }
                    }
                    else if (CurPoint.Y > Bottom)
                    {
                        // Should go up
                        // Make sure it's a valid flight pattern for given UAV
                        Point Child = new Point(CurPoint.X, CurPoint.Y - 1);
                        if (!ValidMove(Parent, CurPoint, Child))
                        {
                            if (CurPoint.X < Left || CurPoint.X == 0)
                            {
                                CurPoint.X++;
                            }
                            else if (CurPoint.X > Right || CurPoint.X == mDist.Columns - 1)
                            {
                                CurPoint.X--;
                            }
                            else
                            {
                                // Just go right (choices are left or right)
                                CurPoint.X++;
                            }
                            AddNodeToPath(CurPathSegment, ref CurT, ref RealT, ref CurPoint);
                        }                        
                        // Move up vertically
                        while (CurPoint.Y > Bottom)
                        {
                            CurPoint.Y--;
                            AddNodeToPath(CurPathSegment, ref CurT, ref RealT, ref CurPoint);
                        }
                    }
                    else
                    {
                        // No need to move vertically
                    }
                }
                else
                {
                    // Inside the box. Let's add Current Node first
                    // Point already in path segment
                }
                #endregion

                #region Complete Coverage

                // Remember starting position inside bounding box
                Point boxstart = new Point(CurPoint.X, CurPoint.Y);
                // boxstart node counts as part of the pattern, increase counter
                PatternStepCount++;
                // tempt is current timestep, used to identify first step in while loop
                bool AtBoxStart = true;

                // Once inside bounding box fly the pattern until mxn-1 steps (complete coverage)
                if (EvenColumns)
                {
                    // Depending on the current position, decide which direction to go
                    // Do the following as long as there's still time or if I return back to boxstart
                    while (((CurPoint.X != boxstart.X || CurPoint.Y != boxstart.Y) && RealT <= curRequest.T) || AtBoxStart)
                    {
                        // Don't add boxstart, but add future nodes
                        if (!AtBoxStart)
                        {
                            // Add node to path
                            AddNodeToPath(CurPathSegment, ref CurT, ref RealT, ref CurPoint);
                            // Increase time counter
                            PatternStepCount++;
                        }

                        // Move intelligently
                        if (CurPoint.X >= Left && CurPoint.X < Right && CurPoint.Y == Top)
                        {
                            // Top left corner. Go right
                            CurPoint.X++;
                        }
                        else if (CurPoint.X == Right && CurPoint.Y >= Top && CurPoint.Y < Bottom)
                        {
                            // Top right corner. Go down
                            CurPoint.Y++;
                        }
                        else if (CurPoint.Y == Bottom && (CurPoint.X - Left) % 2 == 1)
                        {
                            // Bottom right corners. Go left
                            CurPoint.X--;
                        }
                        else if (CurPoint.Y <= Bottom && CurPoint.Y > Top + 1 && (CurPoint.X - Left) % 2 == 0)
                        {
                            // Bottom left corners. Go up
                            CurPoint.Y--;
                        }
                        else if (CurPoint.Y == Top + 1 && CurPoint.X > Left && (CurPoint.X - Left) % 2 == 0)
                        {
                            // Second row right corners. Go left
                            CurPoint.X--;
                        }
                        else if (CurPoint.Y >= Top + 1 && CurPoint.Y < Bottom && (CurPoint.X - Left) % 2 == 1)
                        {
                            // Second row left corners. Go down
                            CurPoint.Y++;
                        }
                        else if (CurPoint.X == Left && CurPoint.Y == Top + 1)
                        {
                            // Point left of top right corner. Go Right
                            CurPoint.Y--;
                        }

                        AtBoxStart = false;
                    }
                }
                else
                {
                    // Turn the pattern 90 degrees clockwise
                    while (((CurPoint.X != boxstart.X || CurPoint.Y != boxstart.Y) && RealT < curRequest.T) || AtBoxStart)
                    {
                        // Don't add boxstart, but add future nodes
                        if (!AtBoxStart)
                        {
                            // Add node to path
                            AddNodeToPath(CurPathSegment, ref CurT, ref RealT, ref CurPoint);
                            // Increase pattern step counter
                            PatternStepCount++;
                        }

                        if (CurPoint.X == Right && CurPoint.Y >= Top && CurPoint.Y < Bottom)
                        {
                            // Top right corner. Go down
                            CurPoint.Y++;
                        }
                        else if (CurPoint.X <= Right && CurPoint.X > Left && CurPoint.Y == Bottom)
                        {
                            // Bottom right corner. Go left
                            CurPoint.X--;
                        }
                        else if (CurPoint.X == Left && (CurPoint.Y - Top) % 2 == 1)
                        {
                            // Left bottom corners. Go up
                            CurPoint.Y--;
                        }
                        else if (CurPoint.X >= Left && CurPoint.X < Right - 1 && (CurPoint.Y - Top) % 2 == 0)
                        {
                            // Left top corners. Go right
                            CurPoint.X++;
                        }
                        else if (CurPoint.X == Right - 1 && CurPoint.Y > Top && (CurPoint.Y - Top) % 2 == 0)
                        {
                            // Second column from right bottom corners. Go up
                            CurPoint.Y--;
                        }
                        else if (CurPoint.X <= Right - 1 && CurPoint.X > Left && (CurPoint.Y - Top) % 2 == 1)
                        {
                            // Second column from right top corners. Go left
                            CurPoint.X--;
                        }
                        else if (CurPoint.X == Right - 1 && CurPoint.Y == Top)
                        {
                            // Point left of top right corner. Go Right
                            CurPoint.X++;
                        }

                        AtBoxStart = false;
                    }
                }

                #endregion

                // Add current segment of path to total path
                Path.AddRange(CurPathSegment);

                // Clear current segment of path
                CurPathSegment.Clear();

                // Reset timer for current run
                CurT = 0;

                // Remember new start point
                CurStart = new Point(Path[Path.Count-1].X, Path[Path.Count-1].Y);
            }

            // If all time used, we are done.
            if (RealT == curRequest.T)
            {
                return;
            }
            
            // After distribution map is wiped clean and still time left
            int FixedPatternStartAt = Path.Count - PatternStepCount;
            while (RealT < curRequest.T)
            {
                Point p = new Point(Path[FixedPatternStartAt].X, Path[FixedPatternStartAt].Y);
                Path.Add(p);
                RealT++;
                FixedPatternStartAt++;
            }
        }

        // Method to add node to path and partially detect
        private void AddNodeToPath(List<Point> CurPathSegment, ref int CurT, ref int RealT, ref Point CurPoint)
        {
            Point p = new Point(CurPoint.X, CurPoint.Y);
            CurPathSegment.Add(p);
            CurT++;
            RealT++;
            CDF += GetPartialDetection(CurPoint);
            mCurDist[CurPoint.Y, CurPoint.X] = VacuumProbability(CurPoint);
        }

        // Function to identify a mxn box bounding non-zero nodes where m is even
        RtwMatrix GetBox(ref bool EvenColumns, ref int Top, ref int Bottom, ref int Left, ref int Right)
        {
            RtwMatrix mbox = new RtwMatrix(mCurDist.Rows, mCurDist.Columns);

            #region Find four sides
            // First pass find top
            for (int i = 0; i < mCurDist.Rows; i++)
            {
                for (int j = 0; j < mCurDist.Columns; j++)
                {
                    if (mCurDist[i, j] > 0)
                    {
                        Top = i;
                        j = mCurDist.Columns;
                        i = mCurDist.Rows;
                    }

                }
            }

            // Second pass find bottom
            for (int i = mCurDist.Rows - 1; i > -1; i--)
            {
                for (int j = mCurDist.Columns - 1; j > -1; j--)
                {
                    if (mCurDist[i, j] > 0)
                    {
                        Bottom = i;
                        j = -1;
                        i = -1;
                    }
                }
            }
            // Third pass find left
            for (int i = 0; i < mCurDist.Columns; i++)
            {
                for (int j = 0; j < mCurDist.Rows; j++)
                {
                    if (mCurDist[j, i] > 0)
                    {
                        Left = i;
                        j = mCurDist.Rows;
                        i = mCurDist.Columns;
                    }
                }
            }
            // Fourth pass find right
            for (int i = mCurDist.Columns - 1; i > -1; i--)
            {
                for (int j = mCurDist.Rows - 1; j > -1; j--)
                {
                    if (mCurDist[j, i] > 0)
                    {
                        Right = i;
                        j = -1;
                        i = -1;
                    }
                }
            }
            #endregion

            // If distribution map is wiped clean, there's nothing to do here.
            if (Top == -1 || Bottom == -1 || Left == -1 || Right == -1)
            {
                return mbox;
            }

            // Create binary mask matrix
            for (int i = Top; i < Bottom + 1; i++)
            {
                for (int j = Left; j < Right + 1; j++)
                {
                    mbox[i, j] = 1;
                }
            }

            #region Handle odd columns or odd rows

            if (Right - Left == mCurDist.Columns && Bottom - Top == mCurDist.Rows)
            {
                if (mCurDist.Columns % 2 != 0 && mCurDist.Rows % 2 != 0)
                {
                    // Because of dynamic size of distribution map, deal with full map oddxodd right here
                    // Let's first try find the shorter line of nodes to subtract
                    if (Right + 1 - Left > Bottom + 1 - Top)
                    {
                        // Row is longer, column is shorter. We should subtract column
                        Left++;
                        for (int i = Top; i < Bottom + 1; i++)
                        {
                            mbox[i, Left] = 1;
                        }
                        EvenColumns = true;
                    }

                }
            }
            else
            {
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
                    // Odd numbber of columns and odd number of rows (not full map)
                    // Let's first try find the shorter line of nodes to add
                    if (Right + 1 - Left > Bottom + 1 - Top)
                    {
                        // Row is longer, column is shorter. We should add column
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
            }
            #endregion

            return mbox;
        }

        #endregion
    }
}
