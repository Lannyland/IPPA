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
    public class LHCNode : IComparable
    {
        public Point Loc = new Point();
        public float p;
        public int oldindex;

        // Sort LHCNode in descending order with best p on top
        public int CompareTo(object obj)
        {
            LHCNode Compare = (LHCNode)obj;
            int result = this.p.CompareTo(Compare.p);
            //if (result == 0)
            //{
            //    result = this.p.CompareTo(Compare.p);
            //}
            return result;
        }
    }

    public abstract class AlgLHC : AlgPathPlanning
    {
        #region Members

        // Private variables
        private Random r = new Random((int)DateTime.Now.Ticks);

        // Public variables
        public Point BeforeStart = new Point(-1, -1);
        // Debug multithreaded variables
        public int index = 0;
        public int conv = 0;

        #endregion

        #region Constructor, Destructor

        // Constructor
        public AlgLHC(PathPlanningRequest _curRequest, RtwMatrix _mDistReachable, 
            RtwMatrix _mDiffReachable, double _Efficiency_UB) 
            : base (_curRequest, _mDistReachable, _mDiffReachable, _Efficiency_UB)
        {
        }

        // Destructor
        ~AlgLHC()
        {
            // Cleaning up
        }

        #endregion

        #region Other Functions

        protected override void DoPathPlanning()
        {
            // First add starting node to path
            Point Start = new Point(curRequest.pStart.column, curRequest.pStart.row);
            Path.Add(Start);
            CDF += GetPartialDetection(Start);
            mCurDist[Start.Y, Start.X] = VacuumProbability(Start);

            for (int i = 0; i < curRequest.T; i++)
            {
                Point parent;
                Point me;

                // Find parent
                if (Path.Count < 2)
                {
                    if (BeforeStart.X == -1 && BeforeStart.Y == -1)
                    {
                        parent = Start;
                    }
                    else
                    {
                        parent = BeforeStart;
                    }
                    me = Start;
                }
                else
                {
                    parent = Path[Path.Count - 2];
                    me = Path[Path.Count - 1];
                }

                // Find all valid neighbors from current node
                List<LHCNode> ValidNeighbors;
                if (curRequest.UseEndPoint)
                {
                    ValidNeighbors = GetNeighbors(parent, me, new Point(curRequest.pEnd.column, curRequest.pEnd.row), curRequest.T-i);
                }
                else
                {
                    ValidNeighbors = GetNeighbors(parent, me);
                }

                // Decide which way to go.
                Point next;
                int indexOfnext = 0;

                if (ValidNeighbors.Count > 1)
                {
                    FindNodeToGoTo(i, ref me, ref ValidNeighbors, ref indexOfnext);
                }

                // Add node to path and then collect probability (zero it out)
                next = ValidNeighbors[indexOfnext].Loc;
                Path.Add(next);
                CDF += GetPartialDetection(next);
                mCurDist[next.Y, next.X] = VacuumProbability(next);
            }
        }

        // Algorithm specific way of finding where to go next
        protected virtual void FindNodeToGoTo(int i, ref Point me, ref List<LHCNode> ValidNeighbors, ref int indexOfnext)
        {
            // More than one valid neighbors
            ValidNeighbors.Sort();
            ValidNeighbors.Reverse();
            int identicalCount = GetTopIdenticalCount(ref ValidNeighbors);
            if (identicalCount > 1)
            {
                indexOfnext = PickChildNode(me, ValidNeighbors, identicalCount, i + 1);
            }
        }

        // Expand neighboring nodes
        protected virtual List<LHCNode> GetNeighbors(Point parent, Point me)
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

        // Expand neighboring nodes (with end point)
        protected virtual List<LHCNode> GetNeighbors(Point parent, Point me, Point end, int T_Left)
        {
            List<LHCNode> Neighbors = new List<LHCNode>();

            // Add self if UAV can hover
            if (curRequest.VehicleType == UAVType.Copter)
            {
                // Check if it's valid
                if (ValidMove(parent, me, me, end, T_Left))
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
                if (ValidMove(parent, me, child, end, T_Left))
                {
                    LHCNode n = new LHCNode();
                    n.Loc = child;
                    n.p = GetPartialDetection(child);
                    Neighbors.Add(n);
                }
            }
            return Neighbors;
        }
        
        // Function to return the count of identical items at the top (also set oldindex)
        protected int GetTopIdenticalCount(ref List<LHCNode> myList)
        {
            int identicalCount = 1;
            float pMax = myList[0].p;
            myList[0].oldindex = 0;
            // Compare max to all other neighbors
            for (int j = 1; j < myList.Count; j++)
            {
                float p_cur = myList[j].p;
                myList[j].oldindex = j;
                if (pMax == p_cur)
                {
                    identicalCount++;
                }
            }
            return identicalCount;           
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
        protected int PickChildNode(Point me, List<LHCNode> neighbors, int identicalCount, int cur_T)
        {
            int index = 0;
            List<LHCNode> NewList = new List<LHCNode>();

            // Do this for PF but not Conv
            float[] forces = PrepareTieBreaker(me, cur_T);

            // Compute convolution/pf values for these child nodes with identical p values
            for (int i = 0; i < identicalCount; i++)
            {
                LHCNode ln = new LHCNode();
                ln.Loc = neighbors[i].Loc;
                ln.p = TieBreaker(me, ln.Loc, cur_T, forces); ;
                ln.oldindex = neighbors[i].oldindex;
                NewList.Add(ln);
                // Console.Write("(" + ln.Loc.X + "," + ln.Loc.Y + ")conv=" + ln.p + " ");
            }
            // Sort so best convolution/pf value nodes at top
            NewList.Sort();
            NewList.Reverse();

            // Check if more than one neighbor with identical conv/pf value
            // If so, randomly pick one
            int identical = GetTopIdenticalCount(NewList);

            if (identical == 1)
            {
                // Found node with highest conv/pf value
                index = NewList[0].oldindex;
            }
            else
            {
                // Randomly pick a node if have same conv/pf vlaues
                int i = r.Next(0, identical);
                // Console.Write("random i=" + i + " ");
                index = NewList[i].oldindex;
            }

            return index;
        }

        // Abstract method different implementation by algorithm
        abstract protected float[] PrepareTieBreaker(Point me, int cur_T);

        // Abstract method different implementation by algorithm
        abstract protected float TieBreaker(Point me, Point neighbor, int cur_T, float[] forces);
        
        // Debugging shouts
        public override void Shout()
        {
            Console.WriteLine("I am AlgLHC!");
        }
        
        #endregion
    }
}


