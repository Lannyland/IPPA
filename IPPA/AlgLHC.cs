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

        #endregion

        #region Constructor, Destructor

        // Constructor
        public AlgLHC(PathPlanningRequest _curRequest, RtwMatrix _mDistReachable, 
            RtwMatrix _mDiffReachable, double _Efficiency_LB) 
            : base (_curRequest, _mDistReachable, _mDiffReachable, _Efficiency_LB)
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
            //int sameCount = 0;

            for (int i = 0; i < curRequest.T; i++)
            {
                Point parent;
                Point me;

                // Find parent
                if (Path.Count < 2)
                {
                    // It only has starting point
                    parent = Start;
                    me = Start;
                }
                else
                {
                    parent = Path[Path.Count - 2];
                    me = Path[Path.Count - 1];
                }

                // Console.Write("Now at (" + me.X +"," + me.Y + ") ");

                // Find all valid neighbors from current node
                List<LHCNode> ValidNeighbors = GetNeighbors(parent, me);

                // Console.Write(ValidNeighbors.Count + " valid neighbors ");
                // foreach (LHCNode lhc in ValidNeighbors)
                // {
                    // Console.Write("(" + lhc.Loc.X + "," + lhc.Loc.Y + ")" + "p=" + lhc.p + " ");
                // }

                // Decide which way to go.
                Point next;
                int indexOfnext = 0;

                if (ValidNeighbors.Count > 1)
                {
                    // More than one valid neighbors
                    ValidNeighbors.Sort();
                    ValidNeighbors.Reverse();
                    int identicalCount = GetTopIdenticalCount(ref ValidNeighbors);
                    if (identicalCount > 1)
                    {
                        //Console.Write("T=" + i + " count=" + ValidNeighbors.Count + " ");
                        //foreach (LHCNode lhc in ValidNeighbors)
                        //{
                        //    Console.Write(lhc.p + " ");
                        //}
                        indexOfnext = PickChildNode(me, ValidNeighbors, identicalCount, i+1);
                        //sameCount++;
                    }
                }

                // Add node to path and then collect probability (zero it out)
                next = ValidNeighbors[indexOfnext].Loc;
                Path.Add(next);
                CDF += GetPartialDetection(next);
                mCurDist[next.Y, next.X] = VacuumProbability(next);

                // Console.Write("moving to (" + next.X + "," + next.Y + ").\n");
            }
            //Console.Write("Had to make conv choice " + sameCount + " times.\n");
        }

        // Expand neighboring nodes
        protected List<LHCNode> GetNeighbors(Point parent, Point me)
        {
            List<LHCNode> Neighbors = new List<LHCNode>();

            // Add self if UAV can hover
            if (curRequest.VehicleType == UAVType.Copter)
            {
                LHCNode n = new LHCNode();
                n.Loc = me;
                n.p = GetPartialDetection(me);
                Neighbors.Add(n);
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


