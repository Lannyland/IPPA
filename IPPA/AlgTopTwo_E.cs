using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using rtwmatrix;

namespace IPPA
{
    class AlgTopTwo_E : AlgPathPlanning
    {
        #region Members

        // Private variables

        // Public variables

        #endregion

        #region Constructor, Destructor

        // Constructor
        public AlgTopTwo_E(PathPlanningRequest _curRequest, int _ModeCount, RtwMatrix _mModes, RtwMatrix _mDistReachable, 
            RtwMatrix _mDiffReachable, double _Efficiency_UB) 
            : base (_curRequest, _mDistReachable, _mDiffReachable, _Efficiency_UB)
        {
        }

        // Destructor
        ~AlgTopTwo_E()
        {
            // Cleaning up
        }

        #endregion

        #region Other Functions

        protected override void DoPathPlanning()
        {
            //TODO Count modes and identify mode nodes (Modify existing code)
            //TODO Sanity check: Don't do this when there is no mode of just 1 mode
            //TODO Identify centroid for each Mi
            //TODO Compute GMi and find top two modes
            //TODO Find middle point Round((x1+x2)/2, (y1+y2)/2)
            //TODO Is T even?
                //TODO Yes, T/2 for each side
                //TODO No, Floor(T/2) for one side and Ceiling(T/2) for the other
            //TODO Is the UAV a copter?
                //TODO Yes, just use middle point
                //TODO No, let's find two points next to middle point
            //TODO (Plan path from centroid to middle point) x 2
            //TODO Join two pieces into one path
        }

        // Debugging shouts
        public override void Shout()
        {
            Console.WriteLine("I am AlgTopTwo_E!");
        }

        #endregion
    }
}
