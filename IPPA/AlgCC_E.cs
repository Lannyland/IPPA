using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using rtwmatrix;

namespace IPPA
{
    class AlgCC_E : AlgPathPlanning
    {
        // TODO Implement AlgCC_E

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

        // Debugging shouts
        public override void Shout()
        {
            Console.WriteLine("I am AlgCC_E!");
        }

        // Algorithm specific implementation of the path planning
        protected override void DoPathPlanning()
        {
            // TODO AlgCC_E path planning
            throw new NotImplementedException();
        }

        #endregion
    }
}
