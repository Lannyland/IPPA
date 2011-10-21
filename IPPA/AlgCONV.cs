using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using rtwmatrix;

namespace IPPA
{
    class AlgCONV : AlgLHCGWCONV
    {
        #region Members

        // Private variables

        // Public variables

        #endregion

        #region Constructor, Destructor

        // Constructor
        public AlgCONV(PathPlanningRequest _curRequest, RtwMatrix _mDistReachable, 
            RtwMatrix _mDiffReachable, double _Efficiency_UB, int _KernalSize) 
            : base (_curRequest, _mDistReachable, _mDiffReachable, _Efficiency_UB, _KernalSize)
        {
        }

        // Destructor
        ~AlgCONV()
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

        // Debugging shouts
        public override void Shout()
        {
            Console.WriteLine("I am AlgCONV!");
        }

        #endregion
    }
}
