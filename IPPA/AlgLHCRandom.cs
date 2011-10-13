using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rtwmatrix;
using System.Drawing;

namespace IPPA
{
    class AlgLHCRandom : AlgLHC
    {
        #region Members

        // Private members

        #endregion

        #region Constructor, Destructor

        // Constructor
        public AlgLHCRandom(PathPlanningRequest _curRequest, RtwMatrix _mDistReachable, 
            RtwMatrix _mDiffReachable, double _Efficiency_UB) 
            : base (_curRequest, _mDistReachable, _mDiffReachable, _Efficiency_UB)
        {
        }

        // Destructor
        ~AlgLHCRandom()
        {
            // Cleaning up
        }

        #endregion

        #region Other Functions

        // Abstract method different implementation by algorithm
        protected override float[] PrepareTieBreaker(Point me, int cur_T)
        {
            // Really don't need this. Just here for inheritance
            float[] forces = new float[1] {1};
            return forces;
        }

        // Just make the force the same so a random selection is needed
        protected override float TieBreaker(Point me, Point neighbor, int cur_T, float[] forces)
        {
            float cur_force = 0;
            return cur_force;
        }

        // Debugging shouts
        public override void Shout()
        {
            Console.WriteLine("I am AlgLHCRandom!");
        }


        #endregion

    }
}
