using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Drawing;
using rtwmatrix;

namespace IPPA
{
    class LHCNode : IComparable
    {
        public Point Loc = new Point();
        public float p;
        public int oldindex;

        public int CompareTo(object obj)
        {
            LHCNode Compare = (LHCNode)obj;
            int result = this.p.CompareTo(Compare.p);
            if (result == 0)
            {
                result = this.p.CompareTo(Compare.p);
            }
            return result;
        }
    }

    class AlgLHC : AlgPathPlanning
    {
        #region Members

        // Private variables

        // Public variables

        #endregion

        #region Constructor, Destructor

        // Constructor
        public AlgLHC(PathPlanningRequest _curRequest, RtwMatrix _mDistReachable, 
            RtwMatrix _mDiffReachable, double _Efficiency_LB) 
            : base (_curRequest, _mDistReachable, _mDiffReachable, _Efficiency_LB)
        {
        }

        // Destructor
        ~AlgLHC()
        {
            // Cleaning up
        }

        #endregion

        #region Other Functions

        #endregion
    }
}


