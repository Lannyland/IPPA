using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using rtwmatrix;

namespace IPPA
{
    class AlgTopTwo_E : AlgTopTwo
    {
        #region Members

        // Private variables

        // Public variables

        #endregion

        #region Constructor, Destructor

        // Constructor
        public AlgTopTwo_E(PathPlanningRequest _curRequest, int _ModeCount, RtwMatrix _mModes, RtwMatrix _mDistReachable, 
            RtwMatrix _mDiffReachable, double _Efficiency_UB) 
            : base (_curRequest, _ModeCount, _mModes, _mDistReachable, _mDiffReachable, _Efficiency_UB)
        {
        }

        // Destructor
        ~AlgTopTwo_E()
        {
            // Cleaning up
        }

        #endregion

        #region Other Functions



        // Debugging shouts
        public override void Shout()
        {
            Console.WriteLine("I am AlgTopTwo_E!");
        }

        #endregion
    }
}
