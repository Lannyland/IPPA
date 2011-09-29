using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rtwmatrix;
using System.Drawing;

namespace IPPA
{
    class AlgLHCGWCONV : AlgLHC
    {
        #region Members

        // Private members
        private int KernalSize = 0;

        #endregion

        #region Constructor, Destructor

        // Constructor
        public AlgLHCGWCONV(PathPlanningRequest _curRequest, RtwMatrix _mDistReachable, 
            RtwMatrix _mDiffReachable, double _Efficiency_LB, int _KernalSize) 
            : base (_curRequest, _mDistReachable, _mDiffReachable, _Efficiency_LB)
        {
            KernalSize = _KernalSize;
        }

        // Destructor
        ~AlgLHCGWCONV()
        {
            // Cleaning up
        }

        #endregion

        #region Other Functions

        public override void PlanPath()
        {
            // TODO Decide what to abstract out to parent class
            // base.PlanPath();
            
            // Clone distribution map so we can modify it
            RtwMatrix mCurDist = mDist.Clone();

            // First add starting node to path
            Point Start = new Point(curRequest.pStart.column, curRequest.pStart.row);
            Path.Add(Start);
            CDF += mCurDist[Start.Y, Start.X];
            // TODO Deal with partial detection
            mDist[Start.Y, Start.X] = 0;

            for (int i = 0; i < curRequest.T; i++)
            {
                List<LHCNode> neighbors = new List<LHCNode>();
                Point parent;
                Point me;
                Point child;

                // Find parent
                if (BestPoints.Count < 2)
                {
                    // It only has starting point
                    parent = Start;
                    me = Start;
                }
                else
                {
                    parent = BestPoints[BestPoints.Count - 2];
                    me = BestPoints[BestPoints.Count - 1];
                }


                // Loop through all four directions (N, E, S, W)
                for (int j = 0; j < 4; j++)
                {
                    // Expand children
                    child = MISCLib.get_delta(j, me);
                    NodesExpanded++;

                    // Check if it's valid children (no repeat first)
                    if (MISCLib.ValidMove(parent, me, child, mReachableRegion, false, mVisited))
                    {
                        LHCNode n = new LHCNode();
                        n.Loc = child;
                        n.p = mMap[child.Y, child.X];
                        neighbors.Add(n);
                    }
                }

                // If no valid child (meaning have to repeat) then
                if (neighbors.Count < 1)
                {
                    // Loop through all four directions (N, E, S, W)
                    for (int j = 0; j < 4; j++)
                    {
                        // Expand children
                        child = MISCLib.get_delta(j, me);
                        NodesExpanded++;

                        // Check if it's valid children (allow repeat now)
                        if (MISCLib.ValidMove(parent, me, child, mReachableRegion, true, mVisited))
                        {
                            LHCNode n = new LHCNode();
                            n.Loc = child;
                            n.p = mMap[child.Y, child.X];
                            neighbors.Add(n);
                        }
                    }
                    RepeatedVisit++;
                }

                // Decide which way to go.
                Point next;
                int indexOfnext = 0;

                if (neighbors.Count > 1)
                {
                    // More than one valid neighbors
                    neighbors.Sort();
                    neighbors.Reverse();

                    int identicalCount = 1;
                    float p_smallest = neighbors[0].p;
                    neighbors[0].oldindex = 0;
                    // Compare smallest to all other neighbors
                    for (int j = 1; j < neighbors.Count; j++)
                    {
                        float p_cur = neighbors[j].p;
                        neighbors[j].oldindex = j;
                        if (p_smallest == p_cur)
                        {
                            identicalCount++;
                        }
                    }
                    if (identicalCount > 1)
                    {
                        // Tie, use second heuristic
                        if (HeuType == 1)
                        {
                            // Use convolution result as second heuristic
                            indexOfnext = PickNodeWithConvolution(neighbors, identicalCount);
                        }
                        else
                        {
                            // Use potential fields as second heuristic
                            indexOfnext = PickNodeWithPotentialField(me, neighbors, identicalCount, i + 1);
                        }
                    }
                }

                // Add node to path and then collect probability (zero it out)
                next = neighbors[indexOfnext].Loc;
                BestPoints.Add(next);
                BestCDF += mMap[next.Y, next.X];
                mMap[next.Y, next.X] = 0;
                mVisited[next.Y, next.X] = 1;
            }


        }

        #endregion

    }
}
