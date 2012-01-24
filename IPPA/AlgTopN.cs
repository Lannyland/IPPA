using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using rtwmatrix;

namespace IPPA
{
    class AlgTopN : AlgPathPlanning
    {
        #region Members

        // Private variables
        private Point Start;
        private Point End;
        private MapModes myModes = null;
        private int N = 0;  // Actual number of modes we will use
        private List<Point> lstCentroids = null;
        private AlgLHCGWCONV SegFirst = null;
        private AlgLHCGWCONV SegLast = null;
        private Random r = new Random((int)DateTime.Now.Ticks);
        private List<List<Point>> AllLooseEndsPairs = new List<List<Point>>();
        private List<int> FinalPerm = null;
        private List<int> FinalPerm2 = null;


        // Public variables

        #endregion

        #region Constructor, Destructor

        // Constructor
        public AlgTopN(PathPlanningRequest _curRequest, int _ModeCount, RtwMatrix _mModes, RtwMatrix _mDistReachable,
            RtwMatrix _mDiffReachable, double _Efficiency_UB) 
            : base (_curRequest, _mDistReachable, _mDiffReachable, _Efficiency_UB)
        {
            myModes = new MapModes(_ModeCount, _mModes, curRequest, mDist, mDiff);
            Start = new Point(curRequest.pStart.column, curRequest.pStart.row);
            if (curRequest.UseEndPoint)
            {
                End = new Point(curRequest.pEnd.column, curRequest.pEnd.row);
            }
        }

        // Destructor
        ~AlgTopN()
        {
            // Cleaning up
            SegFirst = null;
            SegLast = null;
            lstCentroids.Clear();
            lstCentroids = null;
        }

        #endregion

        #region Other Functions

        protected override void DoPathPlanning()
        {
            // Sanity check: Don't do this when there is no mode or just 1 mode
            if (myModes.GetModeCount() < 2)
            {
                System.Windows.Forms.MessageBox.Show("Can't use TopN algorithm because there are less than 2 modes!");
                return;
            }

            // Determine whether there are TopN modes.
            N = curRequest.TopN;
            if (N > myModes.GetModeCount())
            {
                // If Yes, N is good. If No, use mode count as N.
                N = myModes.GetModeCount();
            }

            // Get only topN centroids.
            lstCentroids = myModes.GetModeCentroids();
            
            // Sanity check: make sure T is enough to cover all TopN centroids
            bool TBigEnough = false;
            while (!TBigEnough && curRequest.TopN > 0)
            {
                // Create permuatation
                List<List<int>> allPerms = Permute(lstCentroids, lstCentroids.Count);
                if (EnoughFlightTime(allPerms))
                {
                    TBigEnough = true;
                }
                else
                {
                    // Reduce TopN by 1
                    curRequest.TopN = curRequest.TopN - 1;
                    N = curRequest.TopN;
                    lstCentroids.RemoveAt(lstCentroids.Count - 1);
                }
            }
            // Can't do TopN algorithm when there's not enough time to hit even the top 1 mode.
            if (curRequest.TopN == 0)
            {
                System.Windows.Forms.MessageBox.Show("Can't use TopN algorithm because not enough flight time to reach any mode.");
                return;
            }

            // Searth different combinations and find the best one
            PlanPathSegments();

            // Get real CDF
            CDF = GetTrueCDF(Path);
        }

        // Create permutation
        private List<List<int>> Permute(List<Point> myList, int n)
        {
            int[] ListIndexes = new int[myList.Count];
            for (int i = 0; i < myList.Count; i++)
            {
                ListIndexes[i] = i;
            }
            List<List<int>> allPerms = new List<List<int>>();
            foreach (IEnumerable<int> permutation in PermuteUtils.Permute<int>(ListIndexes, n))
            {
                List<int> curPerm = new List<int>();
                foreach (int i in permutation)
                {
                    curPerm.Add(i);
                }
                allPerms.Add(curPerm);
            }
            return allPerms;
        }

        // Method to check if there's enough flight time to hit all TopN modes.
        private bool EnoughFlightTime(List<List<int>> allPerms)
        {
            bool TBigEnough = false;
            foreach (List<int> curPerm in allPerms)
            {
                int totalDist = 0;
                // First from start to first centroid
                totalDist += MISCLib.ManhattanDistance(Start, lstCentroids[curPerm[0]]);
                // No need to do the next section if there's only one mode
                if (curPerm.Count > 1)
                {
                    // Connect all centroids
                    for (int i = 0; i < curPerm.Count - 1; i++)
                    {
                        totalDist += MISCLib.ManhattanDistance(lstCentroids[curPerm[i]], lstCentroids[curPerm[i + 1]]);
                    }
                }
                if (curRequest.UseEndPoint)
                {
                    // Last centroid to end
                    totalDist += MISCLib.ManhattanDistance(lstCentroids[curPerm.Count - 1], End);
                }
                // See if there's enough time.
                if (totalDist <= curRequest.T)
                {
                    TBigEnough = true;
                    break;
                }
            }
            return TBigEnough;
        }

        // Method to plan path segments
        protected void PlanPathSegments()
        {
            // Decide which two centroids to connect to start and end.
            //TODO Start to closest centroid and end to closest centroid. If same, give it to the shorter path. For now, don't worry about it.
            List<Point> allCentroids = new List<Point>();
            foreach (Point p in lstCentroids)
            {
                Point newP = new Point(p.X, p.Y);
                allCentroids.Add(newP);
            }

            // Plan first and last segments of the path
            int remainingT = curRequest.T;
            Point newStart = new Point(0, 0);
            Point newEnd = new Point(0, 0);
            RtwMatrix mDistAfterSegFirstSegLast = mDist;
            // Plan first seg
            StraightToClosestCentroid(Start, allCentroids, ref newStart, ref remainingT, ref SegFirst, mDistAfterSegFirstSegLast);
            if(curRequest.UseEndPoint)
            {
                // Plan last seg
                StraightToClosestCentroid(End, allCentroids, ref newEnd, ref remainingT, ref SegLast, mDistAfterSegFirstSegLast);
            }

            // Plan middle segments of the path
            List<List<Point>> MidSegments = new List<List<Point>>();
            PlanMidPathSegments(MidSegments, newStart, newEnd, allCentroids, mDistAfterSegFirstSegLast, remainingT);

            //TODO Join path segments
            JoinPathSegments(MidSegments);
        }

        // Join path segments together into one path
        private void JoinPathSegments(List<List<Point>> MidSegments)
        {
            //TODO Plan LHC to join them.
            //TODO Deal with flying backwards

            // First join
            List<Point> SegFirstPath = SegFirst.GetPath();
            Point p1 = SegFirstPath[SegFirstPath.Count-1];
            Point p2 = MidSegments[0][0];
            if (p1.X == p2.X && p1.Y == p2.Y)
            {
                Path.AddRange(SegFirstPath);
                Path.AddRange(MidSegments[0]);
                Path.RemoveAt(Path.Count - 1);
            }
            else
            {
                // Something is wrong!
                System.Windows.Forms.MessageBox.Show("Seg1 and Seg2 don't connect.");
                return;
            }
            // Find out path index order in MidSegments
            List<int> FinalRealOrder = new List<int>();
            for (int i = 0; i < FinalPerm.Count; i++)
            {
                bool swap = false;
                if (FinalPerm2 != null)
                {
                    for (int j = 0; j < FinalPerm2.Count; j++)
                    {
                        if (FinalPerm[i] == FinalPerm2[j])
                        {
                            swap = true;
                            break;
                        }
                    }
                }
                if (swap)
                {
                    FinalRealOrder.Add(FinalPerm[i] * 2 + 3);
                    FinalRealOrder.Add(FinalPerm[i] * 2 + 2);
                }
                else
                {
                    FinalRealOrder.Add(FinalPerm[i] * 2 + 2);
                    FinalRealOrder.Add(FinalPerm[i] * 2 + 3);                    
                }
            }
            // 0 1-2 3-4 5-6 ...
            FinalRealOrder.Insert(0, 0);
            // Mid Joins
            for (int i = 0; i < FinalRealOrder.Count; i=i+2)
            {
                p1 = MidSegments[FinalRealOrder[i]][MidSegments[FinalRealOrder[i]].Count - 1];
                p2 = MidSegments[FinalRealOrder[i + 1]][MidSegments[FinalRealOrder[i + 1]].Count - 1];
                if ((p1.X == p2.X && p1.Y == p2.Y) ||
                    (p1.X == p2.X && Math.Abs(p1.Y - p2.Y) == 1) ||
                    (Math.Abs(p1.X - p2.X) == 1 && p1.Y == p2.Y))
                {
                    // Two paths are already connected
                    Path.AddRange(MidSegments[FinalRealOrder[i + 1]]);
                }
                else
                {
                    // Need to connect two segments
                    PathPlanningRequest newRequest = curRequest.Clone();
                    newRequest.UseEndPoint = true;
                    newRequest.pStart = new DistPoint(p1.Y, p1.X);
                    newRequest.pEnd = new DistPoint(p2.Y, p2.X);
                    newRequest.T = MISCLib.ManhattanDistance(p1, p2);
                    newRequest.AlgToUse = AlgType.LHCGWCONV;
                    AlgLHCGWCONV curSeg = new AlgLHCGWCONV(newRequest, mCurDist, mDiff, Efficiency_UB, 3);
                    curSeg.PlanPath();
                    mCurDist = curSeg.GetmCurDist();
                    Path.AddRange(curSeg.GetPath());
                    Path.RemoveAt(Path.Count - 1);
                    List<Point> reversePath = MidSegments[FinalRealOrder[i + 1]];
                    reversePath.Reverse();
                    Path.AddRange(reversePath);
                    curSeg = null;
                    Path.AddRange(MidSegments[FinalRealOrder[i + 2]]);
                }
            }
            // Last join
            if (curRequest.UseEndPoint)
            {
                p1 = MidSegments[FinalRealOrder[FinalRealOrder.Count - 1]][MidSegments[FinalRealOrder[FinalRealOrder.Count - 1]].Count - 1];                
                p2 = MidSegments[1][MidSegments[1].Count - 1];
                List<Point> reversePath = MidSegments[1];
                reversePath.Reverse();
                if ((p1.X == p2.X && p1.Y == p2.Y) ||
                    (p1.X == p2.X && Math.Abs(p1.Y - p2.Y) == 1) ||
                    (Math.Abs(p1.X - p2.X) == 1 && p1.Y == p2.Y))
                {
                    // Two paths are already connected
                    Path.AddRange(reversePath);
                }
                else
                {
                    // Need to connect two segments
                    PathPlanningRequest newRequest = curRequest.Clone();
                    newRequest.UseEndPoint = true;
                    newRequest.pStart = new DistPoint(p1.Y, p1.X);
                    newRequest.pEnd = new DistPoint(p2.Y, p2.X);
                    newRequest.T = MISCLib.ManhattanDistance(p1, p2);
                    newRequest.AlgToUse = AlgType.LHCGWCONV;
                    AlgLHCGWCONV curSeg = new AlgLHCGWCONV(newRequest, mCurDist, mDiff, Efficiency_UB, 3);
                    curSeg.PlanPath();
                    mCurDist = curSeg.GetmCurDist();
                    Path.AddRange(curSeg.GetPath());
                    Path.RemoveAt(Path.Count - 1);
                    Path.AddRange(reversePath);
                    curSeg = null;
                }
                Path.RemoveAt(Path.Count - 1);
                                
                List<Point> SegLastPath = SegLast.GetPath();
                SegLastPath.Reverse();
                p1 = MidSegments[1][0];
                p2 = SegLastPath[0];
                if (p1.X == p2.X && p1.Y == p2.Y)
                {
                    Path.AddRange(SegFirstPath);
                }
                else
                {
                    // Something is wrong!
                    System.Windows.Forms.MessageBox.Show("Seg1 and Seg2 don't connect.");
                    return;
                }
            }
        }

        // Plan first or last path segment
        private void StraightToClosestCentroid(Point StartOrEnd, List<Point> allCentroids, ref Point newPoint,
            ref int remainingT, ref AlgLHCGWCONV SegFirstLast, RtwMatrix mDistAfterSegFirstSegLast)
        {
            // Find centroid closest to start
            int closestCentroidIndex = -1;
            int d = curRequest.T;
            for (int i = 0; i < allCentroids.Count; i++)
            {
                int dist = MISCLib.ManhattanDistance(StartOrEnd, allCentroids[i]);
                if (dist < d)
                {
                    closestCentroidIndex = i;
                    d = dist;
                }
            }
            // Time used up is d1
            remainingT -= d;

            // Remember the closest centroid and remove it from list.
            newPoint = allCentroids[closestCentroidIndex];
            allCentroids.RemoveAt(closestCentroidIndex);
            // Plan path from start to this centroid
            PathPlanningRequest newRequest = curRequest.Clone();
            newRequest.UseEndPoint = true;
            newRequest.pStart = new DistPoint(StartOrEnd.Y, StartOrEnd.X);
            newRequest.pEnd = new DistPoint(newPoint.Y, newPoint.X);
            newRequest.T = d;
            newRequest.AlgToUse = AlgType.LHCGWCONV;
            SegFirstLast = new AlgLHCGWCONV(newRequest, mDistAfterSegFirstSegLast, mDiff, Efficiency_UB, 3);
            SegFirstLast.PlanPath();
            mDistAfterSegFirstSegLast = SegFirstLast.GetmCurDist();
        }

        // Method to actually generate all mid path segments and store in MidSegments list.
        private void PlanMidPathSegments(List<List<Point>> MidSegments, Point newStart, Point newEnd, List<Point> allCentroids, 
            RtwMatrix mDistAfterSegFirstSegLast, int remainingT)
        {
            //if (curRequest.DrawPath)
            //{
                // Showing path and map remains as we plan
                Bitmap CurBMP = new Bitmap(mDist.Columns, mDist.Rows);
                ImgLib.MatrixToImage(ref mDist, ref CurBMP);
                frmMap map = new frmMap();
                map.Text = "UAV trajectory and coverage";
                map.setImage(CurBMP);
                map.Show();
                map.resetImage();
                // Draw first segment and last segment
                for (int i = 0; i < SegFirst.GetPath().Count; i++)
                {
                    Point p = SegFirst.GetPath()[i];
                    map.setPointColor(p, VacuumProbability(p));
                    map.Refresh();
                }
                for (int i = 0; i < SegLast.GetPath().Count; i++)
                {
                    Point p = SegLast.GetPath()[i];
                    map.setPointColor(p, VacuumProbability(p));
                    map.Refresh();
                }
            //}
            mCurDist = mDistAfterSegFirstSegLast;

            // Initialize all mid segment paths
            for (int i = 0; i < allCentroids.Count * 2 + 2; i++)
            {
                List<Point> curSeg = new List<Point>();
                MidSegments.Add(curSeg);
            }
            
            // First identify all starting nodes and add to paths
            MidSegments[0].Add(newStart);
            MidSegments[1].Add(newEnd);
            // Loop through all centroids (excluding newStart and NewEnd)
            for (int i = 0; i < allCentroids.Count; i++)
            {
                // Add a pair list to allLooseEndsPairs
                List<Point> curPair = new List<Point>();
                // First add centroid
                curPair.Add(allCentroids[i]);
                // Update probability map
                mCurDist[allCentroids[i].Y, allCentroids[i].X] = VacuumProbability(allCentroids[i]);
                // Then add the best valid neighbor
                Point bestNeighbor = FindBestNeighbor(allCentroids[i]);
                // Update probability map
                mCurDist[bestNeighbor.Y, bestNeighbor.X] = VacuumProbability(bestNeighbor);
                // Add pair to list
                curPair.Add(bestNeighbor);
                MidSegments[i * 2 + 2].Add(curPair[0]);
                MidSegments[i * 2 + 3].Add(curPair[1]);
                AllLooseEndsPairs.Add(curPair);
                // Deduct time
                remainingT -= allCentroids.Count * 2;
                // Show path planning process
                //if (curRequest.DrawPath)
                //{
                    map.setPointColor(curPair[0], mCurDist[curPair[0].Y, curPair[0].X]);
                    map.setPointColor(curPair[1], mCurDist[curPair[1].Y, curPair[1].X]);
                    map.Refresh();
                //}
            }
            
            // Loop through remaining flight time.
            int thresholdT = 0;
            for (int t = 0; t < remainingT; t++)
            {
                // Each start node find best neighbor and then choose best one to add to path
                float maxP = 0;
                int bestIndex = 0;
                Point bestPoint = new Point();
                for (int i = 0; i < MidSegments.Count; i++)
                {
                    if (!curRequest.UseEndPoint && i == 1)
                    {
                        // Don't plan
                    }
                    else
                    {
                        Point bestNeighbor = FindBestNeighbor(MidSegments[i][MidSegments[i].Count - 1]);
                        float curP = GetPartialDetection(bestNeighbor);
                        if (curP > maxP)
                        {
                            maxP = curP;
                            bestIndex = i;
                            bestPoint = bestNeighbor;
                        }
                    }
                }
                MidSegments[bestIndex].Add(bestPoint);
                if (bestIndex > 1)
                {
                    if (bestIndex % 2 == 0)
                    {
                        AllLooseEndsPairs[(bestIndex - 2) / 2][0] = bestPoint;
                    }
                    else
                    {
                        AllLooseEndsPairs[(bestIndex - 2) / 2][1] = bestPoint;
                    }
                }
                mCurDist[bestPoint.Y, bestPoint.X] = VacuumProbability(bestPoint);

                // Keep a counter of worst distance among all endpoints of path segments
                thresholdT += 2;
                Console.Write(thresholdT.ToString() + " ");
                if (thresholdT > remainingT - t - 1)
                {
                    // Check if there's enough time to join everything together.                    
                    // Create permuatation
                    List<List<int>> allPerms = Permute(allCentroids, allCentroids.Count);
                    for (int j = 0; j < allPerms.Count; j++)
                    {
                        List<int> curPerm = allPerms[j];
                        int totalDist = ComputeTotalDistances(curPerm, MidSegments, AllLooseEndsPairs);
                        if (totalDist < remainingT - t - 1)
                        {
                            // Stil enough time, no need to check further.
                            thresholdT = 0;
                            FinalPerm = null;
                            FinalPerm2 = null;
                            break;
                        }
                        else
                        {
                            if (totalDist == remainingT - t - 1)
                            {
                                FinalPerm = curPerm;
                            }
                            // Have to permute again to switch end pairs.
                            for (int k = 1; k < allCentroids.Count + 1; k++)
                            {
                                List<List<int>> allPerms2 = Permute(allCentroids, k);
                                for (int l = 0; l < allPerms2.Count; l++)
                                {
                                    List<int> curPerm2 = allPerms2[l];
                                    // Clone AllLooseEndsPairs
                                    List<List<Point>> PairsClone = PairListClone(AllLooseEndsPairs);
                                    // Perform switch(es)
                                    for (int ii = 0; ii < curPerm2.Count; ii++)
                                    {
                                        Point ptemp;
                                        ptemp = PairsClone[curPerm2[ii]][0];
                                        PairsClone[curPerm2[ii]][0] = PairsClone[curPerm2[ii]][1];
                                        PairsClone[curPerm2[ii]][1] = ptemp;
                                    }
                                    // Check if total distance is enough to connect all loose ends.
                                    totalDist = ComputeTotalDistances(curPerm, MidSegments, PairsClone);
                                    if (totalDist < remainingT - t - 1)
                                    {
                                        thresholdT = 0;
                                        l = allPerms2.Count;
                                        k = allCentroids.Count;
                                        j = allPerms.Count;
                                        FinalPerm = null;
                                        FinalPerm2 = null;
                                    }
                                    else if (totalDist == remainingT - t - 1)
                                    {
                                        FinalPerm = curPerm;
                                        FinalPerm2 = curPerm2;
                                    }
                                }
                            }
                        }
                    }
                }
                if (FinalPerm != null)
                {
                    // Just enough time to connect all loose ends in the remembered perm. Remember the perm.                    
                    break;
                }
                //// After this if still not enough time, then something went wrong.
                //if (totalDist < remainingT - t - 1)
                //{
                //    System.Windows.Forms.MessageBox.Show("Something wrong. Don't have enough time left to connect all loose ends");
                //    return;
                //}
                
                // Show path planning process
                //if (curRequest.DrawPath)
                //{
                    map.setPointColor(bestPoint, mCurDist[bestPoint.Y, bestPoint.X]);
                    map.Refresh();
                //}
            }
        }

        // Clone AllLooseEndsPairs
        private List<List<Point>> PairListClone(List<List<Point>> AllLooseEndsPairs)
        {
            List<List<Point>> PairsClone = new List<List<Point>>();
            for (int ii = 0; ii < AllLooseEndsPairs.Count; ii++)
            {
                List<Point> curPair = new List<Point>();
                curPair.Add(AllLooseEndsPairs[ii][0]);
                curPair.Add(AllLooseEndsPairs[ii][1]);
                PairsClone.Add(curPair);
            }
            return PairsClone;
        }

        // Method to find best neighbor (best probability * probability of detection) of a given point
        private Point FindBestNeighbor(Point me)
        {
            // Find all valid neighbors from current node
            List<LHCNode> ValidNeighbors;
            ValidNeighbors = GetNeighbors(me, me);

            // Decide which one is the best.
            int indexOfnext = 0;
            if (ValidNeighbors.Count > 1)
            {
                FindNodeToGoTo(me, ref ValidNeighbors, ref indexOfnext);
            }

            // Add node to path and then collect probability (zero it out)
            Point bestNeighbor = ValidNeighbors[indexOfnext].Loc;
            return bestNeighbor;
        }

        // Expand neighboring nodes
        private List<LHCNode> GetNeighbors(Point parent, Point me)
        {
            List<LHCNode> Neighbors = new List<LHCNode>();

            // Add self if UAV can hover
            if (curRequest.VehicleType == UAVType.Copter)
            {
                // Check if it's valid
                if (ValidMove(parent, me, me))
                {
                    LHCNode n = new LHCNode();
                    n.Loc = me;
                    n.p = GetPartialDetection(me);
                    Neighbors.Add(n);
                }
            }

            // Loop through all four directions (N, E, S, W)
            for (int j = 0; j < 4; j++)
            {
                // Expand child
                Point child = GetDirChild(j, me);

                // Check if it's valid
                if (ValidMove(parent, me, child))
                {
                    LHCNode n = new LHCNode();
                    n.Loc = child;
                    n.p = GetPartialDetection(child);
                    Neighbors.Add(n);
                }
            }
            return Neighbors;
        }

        // Algorithm specific way of finding where to go next
        private void FindNodeToGoTo(Point me, ref List<LHCNode> ValidNeighbors, ref int indexOfnext)
        {
            // More than one valid neighbors
            ValidNeighbors.Sort();
            ValidNeighbors.Reverse();
            int identicalCount = GetTopIdenticalCount(ValidNeighbors);
            if (identicalCount > 1)
            {
                indexOfnext = PickChildNode(me, ValidNeighbors, identicalCount);
            }
        }

        // Function to return the count of identical items at the top
        protected int GetTopIdenticalCount(List<LHCNode> myList)
        {
            int identicalCount = 1;
            float pMax = myList[0].p;
            // Compare max to all other neighbors
            for (int j = 1; j < myList.Count; j++)
            {
                float p_cur = myList[j].p;
                if (pMax == p_cur)
                {
                    identicalCount++;
                }
            }
            return identicalCount;
        }

        // Function to pick which child node to follow.
        protected int PickChildNode(Point me, List<LHCNode> neighbors, int identicalCount)
        {
            int index = 0;
            List<LHCNode> NewList = new List<LHCNode>();

            // Compute convolution values for these child nodes with identical p values
            for (int i = 0; i < identicalCount; i++)
            {
                LHCNode ln = new LHCNode();
                ln.Loc = neighbors[i].Loc;
                ln.p = TieBreaker(me, ln.Loc); ;
                ln.oldindex = neighbors[i].oldindex;
                NewList.Add(ln);
                // Console.Write("(" + ln.Loc.X + "," + ln.Loc.Y + ")conv=" + ln.p + " ");
            }
            // Sort so best convolution value nodes at top
            NewList.Sort();
            NewList.Reverse();

            // Check if more than one neighbor with identical conv value
            // If so, randomly pick one
            int identical = GetTopIdenticalCount(NewList);

            if (identical == 1)
            {
                // Found node with highest conv value
                index = NewList[0].oldindex;
            }
            else
            {
                // Randomly pick a node if have same conv vlaues
                int i = r.Next(0, identical);
                // Console.Write("random i=" + i + " ");
                index = NewList[i].oldindex;
            }

            return index;
        }

        // Function to find node with higher convolution value as tie-breaker
        private float TieBreaker(Point me, Point neighbor)
        {
            float cur_force = Convolve(neighbor, ProjectConstants.Kernel_Size);
            return cur_force;
        }

        // Function to calculate convolution value
        float Convolve(Point p, int filter_size)
        {
            // Make sure filter_size is an odd number
            if (filter_size % 2 == 0)
            {
                filter_size--;
            }

            // Now loop through points in filter
            int extra = (filter_size - 1) / 2;
            float total = 0;
            for (int y = -extra; y < extra + 1; y++)
            {
                for (int x = -extra; x < extra + 1; x++)
                {
                    int cur_x = p.X + x;
                    int cur_y = p.Y + y;
                    if (cur_x >= 0 && cur_y >= 0 && cur_x < mDist.Columns && cur_y < mDist.Rows)
                    {
                        total += GetPartialDetection(new Point(cur_x, cur_y));
                    }
                }
            }

            // Normalize and return
            return total / (filter_size * filter_size);
        }

        // Method to compute total distances of all loose ends of path segments
        private int ComputeTotalDistances(List<int> curPerm, List<List<Point>> MidSegments, List<List<Point>> LooseEndsPairs)
        {
            int dist = 0;
            // Distance from newStart loose end to first loose end in perm
            dist += MISCLib.ManhattanDistance(MidSegments[0][MidSegments[0].Count - 1], LooseEndsPairs[0][0]);
            for (int i = 0; i < curPerm.Count - 1; i++)
            {
                dist += MISCLib.ManhattanDistance(LooseEndsPairs[i][1], LooseEndsPairs[i + 1][0]);
            }
            if (curRequest.UseEndPoint)
            {
                dist += MISCLib.ManhattanDistance(LooseEndsPairs[LooseEndsPairs.Count - 1][1], MidSegments[1][MidSegments[1].Count - 1]);
            }
            return dist;
        }



        // Debugging shouts
        public override void Shout()
        {
            Console.WriteLine("I am AlgTopN!");
        }

        #endregion
    }
}
