using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rtwmatrix;

namespace IPPA
{
    class AlgEA_E : AlgPathPlanning
    {
        #region Members

        // Private variables
        private int ModeCount = 0;

        // Public variables

        #endregion

        #region Constructor, Destructor

        // Constructor
        public AlgEA_E(PathPlanningRequest _curRequest, int _ModeCount, 
            RtwMatrix _mDistReachable, RtwMatrix _mDiffReachable, double _Efficiency_UB)
            : base (_curRequest, _mDistReachable, _mDiffReachable, _Efficiency_UB)
        {
            ModeCount = _ModeCount;
        }

        // Destructor
        ~AlgEA_E()
        {
            // Cleaning up
        }

        #endregion

        #region Other Functions

        // Algorithm specific implementation of the path planning
        protected override void DoPathPlanning()
        {
        }

        #endregion
    }
}
