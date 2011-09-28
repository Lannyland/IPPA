using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rtwmatrix;

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
            // base.PlanPath();

        }

        #endregion

    }
}
