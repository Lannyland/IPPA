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

        // Public variables

        #endregion

        #region Constructor, Destructor

        // Constructor
        public AlgTopN(PathPlanningRequest _curRequest, int _ModeCount, RtwMatrix _mModes, RtwMatrix _mDistReachable,
            RtwMatrix _mDiffReachable, double _Efficiency_UB) 
            : base (_curRequest, _mDistReachable, _mDiffReachable, _Efficiency_UB)
        {
        }

        // Destructor
        ~AlgTopN()
        {
            // Cleaning up
        }

        #endregion

        #region Other Functions

        protected override void DoPathPlanning()
        {
            //TODO Count modes and identify mode nodes (Modify existing code)
            //TODO Sanity check: Don't do this when there is no mode of just 1 mode
            //TODO Determine whether there are N modes.
                //TODO If Yes, N is good.
                //TODO If No, use mode count as N.
            //TODO Identify centroid for each Mi (N centroids)
            //TODO Compute GMi and find top N modes
            //TODO Starting from each centroid, do greedy (among modes) path planning
            //TODO Compute minimum distance to cover N current nodes
            //TODO Compute minimum distance from starting point to one of the N current nodes
            //TODO Do it until reaching T.
            //TODO Plan LHC to join them.
            //TODO Deal with flying backwards
        }

        // Debugging shouts
        public override void Shout()
        {
            Console.WriteLine("I am AlgTopN!");
        }

        #endregion
    }
}
