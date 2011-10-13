using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using rtwmatrix;

namespace IPPA
{
    class AlgEA : AlgPathPlanning
    {
        // TODO Implement EA

        #region Members

        // Private variables
        private int ModeCount = 0;

        // Public variables

        #endregion

        #region Constructor, Destructor

        // Constructor
        public AlgEA(PathPlanningRequest _curRequest, int _ModeCount, 
            RtwMatrix _mDistReachable, RtwMatrix _mDiffReachable, double _Efficiency_UB)
            : base (_curRequest, _mDistReachable, _mDiffReachable, _Efficiency_UB)
        {
            ModeCount = _ModeCount;
        }

        // Destructor
        ~AlgEA()
        {
            // Cleaning up
        }

        #endregion

        #region Other Functions

        // Algorithm specific implementation of the path planning
        protected override void DoPathPlanning()
        {
            // If using hiararchical search
            if (curRequest.UseHiararchy)
            {
                // If there's plenty of time, then just do Complete Coverage
                if (curRequest.T>100)
                {
                    return;
                }

                // If lots of modes, consider using PF as seed
                if (ModeCount > 4)
                {
                    return;
                }

                // If no mode or 1 mode
                if (ModeCount <= 1)
                {
                    // Uniform distribution or unimodal distribution or path distribution (possibly with splits)
                    return;
                }
            }
        }

        // Debugging shouts
        public override void Shout()
        {
            Console.WriteLine("I am AlgEA!");
        }

        #endregion
    }
}
