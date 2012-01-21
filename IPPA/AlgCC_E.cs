using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using rtwmatrix;

namespace IPPA
{
    class AlgCC_E : AlgCC
    {
        #region Members

        // Private variables

        // Public variables

        #endregion

        #region Constructor, Destructor

        // Constructor
        public AlgCC_E(PathPlanningRequest _curRequest, 
            RtwMatrix _mDistReachable, RtwMatrix _mDiffReachable, double _Efficiency_UB)
            : base (_curRequest, _mDistReachable, _mDiffReachable, _Efficiency_UB)
        {
        }

        // Destructor
        ~AlgCC_E()
        {
            // Cleaning up
        }

        #endregion

        #region Other Functions

        // Algorithm specific implementation of the path planning
        protected override void DoPathPlanning()
        {
            bool blnClean = false;                              // Is there still probability left?
            int CurT = 0;                                       // Time used for current run (lawnmowing pattern)
            int RealT = 0;                                      // How much time left after current run
            int PatternStepCount = 0;                           // Used to remember last flight pattern inside box
            Point CurStart = new Point(curRequest.pStart.column, curRequest.pStart.row);    // Start point in each run
            Point End = new Point(curRequest.pEnd.column, curRequest.pEnd.row);             // End point for entire request
            List<Point> CurPathSegment = new List<Point>();     // Path planned for current run
            CurPathSegment.Add(CurStart);                       // Only do this once. Don't add Start again in future runs.

            int dist;
            Point CurPoint = new Point(CurStart.X, CurStart.Y);

            // Plan to do complete coverage multiple times if partial detection is used
            // Before distribution map is wiped clean
            while (!blnClean && RealT < curRequest.T)
            {
                // If no more time left, go straight to end point                            
                dist = MISCLib.ManhattanDistance(CurStart.X, CurStart.Y, End.X, End.Y);
                if (dist + 2 >= curRequest.T - RealT)
                {
                    goto BreakoutCC;
                }

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
                CurPoint = new Point(Start.X, Start.Y);
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
                        if (!ValidMove(Parent, CurPoint, Child, End, curRequest.T - RealT))
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

                            // If no more time left, go straight to end point                            
                            dist = MISCLib.ManhattanDistance(CurPoint.X, CurPoint.Y, End.X, End.Y);
                            if (dist + 2 >= curRequest.T - RealT)
                            {
                                goto BreakoutCC;
                            }
                        }
                        // Move right horizentally
                        while (CurPoint.X < Left)
                        {
                            CurPoint.X++;
                            AddNodeToPath(CurPathSegment, ref CurT, ref RealT, ref CurPoint);

                            // If no more time left, go straight to end point                            
                            dist = MISCLib.ManhattanDistance(CurPoint.X, CurPoint.Y, End.X, End.Y);
                            if (dist + 2 >= curRequest.T - RealT)
                            {
                                goto BreakoutCC;
                            }
                        }
                    }
                    else if (Start.X > Right)
                    {
                        // Should go left
                        // Make sure it's a valid flight pattern for given UAV
                        Point Child = new Point(CurPoint.X - 1, CurPoint.Y);
                        if (!ValidMove(Parent, CurPoint, Child, End, curRequest.T - RealT))
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

                            // If no more time left, go straight to end point                            
                            dist = MISCLib.ManhattanDistance(CurPoint.X, CurPoint.Y, End.X, End.Y);
                            if (dist + 2 >= curRequest.T - RealT)
                            {
                                goto BreakoutCC;
                            }
                        }
                        // Move left horizentally
                        while (CurPoint.X > Right)
                        {
                            CurPoint.X--;
                            AddNodeToPath(CurPathSegment, ref CurT, ref RealT, ref CurPoint);

                            // If no more time left, go straight to end point                            
                            dist = MISCLib.ManhattanDistance(CurPoint.X, CurPoint.Y, End.X, End.Y);
                            if (dist + 2 >= curRequest.T - RealT)
                            {
                                goto BreakoutCC;
                            }
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
                        if (!ValidMove(Parent, CurPoint, Child, End, curRequest.T - RealT))
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

                            // If no more time left, go straight to end point                            
                            dist = MISCLib.ManhattanDistance(CurPoint.X, CurPoint.Y, End.X, End.Y);
                            if (dist + 2 >= curRequest.T - RealT)
                            {
                                goto BreakoutCC;
                            }
                        }
                        // Move down vertically
                        while (CurPoint.Y < Top)
                        {
                            CurPoint.Y++;
                            AddNodeToPath(CurPathSegment, ref CurT, ref RealT, ref CurPoint);

                            // If no more time left, go straight to end point                            
                            dist = MISCLib.ManhattanDistance(CurPoint.X, CurPoint.Y, End.X, End.Y);
                            if (dist + 2 >= curRequest.T - RealT)
                            {
                                goto BreakoutCC;
                            }
                        }
                    }
                    else if (CurPoint.Y > Bottom)
                    {
                        // Should go up
                        // Make sure it's a valid flight pattern for given UAV
                        Point Child = new Point(CurPoint.X, CurPoint.Y - 1);
                        if (!ValidMove(Parent, CurPoint, Child, End, curRequest.T - RealT))
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

                            // If no more time left, go straight to end point                            
                            dist = MISCLib.ManhattanDistance(CurPoint.X, CurPoint.Y, End.X, End.Y);
                            if (dist + 2 >= curRequest.T - RealT)
                            {
                                goto BreakoutCC;
                            }
                        }
                        // Move up vertically
                        while (CurPoint.Y > Bottom)
                        {
                            CurPoint.Y--;
                            AddNodeToPath(CurPathSegment, ref CurT, ref RealT, ref CurPoint);

                            // If no more time left, go straight to end point                            
                            dist = MISCLib.ManhattanDistance(CurPoint.X, CurPoint.Y, End.X, End.Y);
                            if (dist + 2 >= curRequest.T - RealT)
                            {
                                goto BreakoutCC;
                            }
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
                            // If no more time left, go straight to end point                            
                            dist = MISCLib.ManhattanDistance(CurPoint.X, CurPoint.Y, End.X, End.Y);
                            if (dist + 2 >= curRequest.T - RealT)
                            {
                                goto BreakoutCC;
                            }
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
                            // If no more time left, go straight to end point                            
                            dist = MISCLib.ManhattanDistance(CurPoint.X, CurPoint.Y, End.X, End.Y);
                            if (dist + 2 >= curRequest.T - RealT)
                            {
                                goto BreakoutCC;
                            }
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
                CurStart = new Point(Path[Path.Count - 1].X, Path[Path.Count - 1].Y);
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
                CurPoint = new Point(Path[FixedPatternStartAt].X, Path[FixedPatternStartAt].Y);
                Path.Add(CurPoint);
                RealT++;
                FixedPatternStartAt++;
                // If no more time left, go straight to end point                            
                dist = MISCLib.ManhattanDistance(CurPoint.X, CurPoint.Y, End.X, End.Y);
                if (dist + 2 >= curRequest.T - RealT)
                {
                    goto BreakoutCC;
                }
            }

            return;

        // Just enough time to go straight to end point now
        BreakoutCC:
            // Add current segment of path to total path
            if (CurPathSegment.Count > 0)
            {
                Path.AddRange(CurPathSegment);
                // Clear current segment of path
                CurPathSegment.Clear();
                CurPathSegment = null;
            }
            
            PathPlanningRequest newRequest = curRequest.Clone();
            newRequest.pStart.column = CurPoint.X;
            newRequest.pStart.row = CurPoint.Y;
            newRequest.T = curRequest.T - RealT;
            CDF -= lastCDF;
            mCurDist[CurPoint.Y, CurPoint.X] += (float)lastCDF;
            AlgLHCGWCONV myAlg = new AlgLHCGWCONV(newRequest, mCurDist, mDiff, Efficiency_UB, 3);
            if (Path.Count > 1)
            {
                myAlg.BeforeStart = Path[Path.Count - 2];
            }
            myAlg.PlanPath();

            // Record all related paths stuff
            CDF += myAlg.GetCDF();
            Path.AddRange(myAlg.GetPath());
            myAlg = null;

            return;
        }

        // Debugging shouts
        public override void Shout()
        {
            Console.WriteLine("I am AlgCC_E!");
        }

        #endregion
    }
}
