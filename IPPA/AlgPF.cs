using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using rtwmatrix;

namespace IPPA
{
    class AlgPF : AlgLHCGWPF
    {
        #region Members

        // Private variables

        // Public variables

        #endregion

        #region Constructor, Destructor
        
        // Constructor
        public AlgPF(PathPlanningRequest _curRequest, 
            RtwMatrix _mDistReachable, RtwMatrix _mDiffReachable, 
            double _Efficiency_UB, int _Sigma)
            : base (_curRequest, _mDistReachable, _mDiffReachable, _Efficiency_UB, _Sigma)
        {
        }

        // Destructor
        ~AlgPF()
        {
            // Cleaning up
        }

        #endregion

        #region Other Functions

        // Algorithm specific way of finding where to go next
        protected override void FindNodeToGoTo(int i, ref Point me, ref List<LHCNode> ValidNeighbors, ref int indexOfnext)
        {
            // Set old index
            int identicalCount = GetTopIdenticalCount(ref ValidNeighbors);
            // Check all neighbors
            indexOfnext = PickChildNode(me, ValidNeighbors, ValidNeighbors.Count, i + 1);
        }

        // Expand neighboring nodes
        protected override List<LHCNode> GetNeighbors(Point parent, Point me)
        {
            List<LHCNode> Neighbors = new List<LHCNode>();

            // We don't want the UAV to hover in PF because it will get stuck there
            // So don't add self if UAV can hover

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
        
        // Debugging shouts
        public override void Shout()
        {
            Console.WriteLine("I am AlgPF!");
        }

        #endregion
    }
}
