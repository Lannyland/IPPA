using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rtwmatrix;

namespace IPPA
{
    // Launched from PathPlanningHandler
    // Performs one of the batch path planning tasks
    // Calls the right algorithm based on PathPlanningRequest
    class PathPlanningTask
    {
        #region Members

        // Private members
        private PathPlanningRequest curRequest;
        private int ModeCount = 0;
        private RtwMatrix mDistReachable;
        private RtwMatrix mDiffReachable;
        private double Efficiency_LB = 0;
        private double Efficiency = 0;
        private double RunTime = 0;

        #endregion

        #region Constructor, Destructor

        // Constructor
        public PathPlanningTask(PathPlanningRequest _curRequest, int _ModeCount,
            RtwMatrix _mDistReachable, RtwMatrix _mDiffReachable, double _Efficiency_LB)
        {
            curRequest = _curRequest;
            ModeCount = _ModeCount;
            mDistReachable = _mDistReachable;
            mDiffReachable = _mDiffReachable;
            Efficiency_LB = _Efficiency_LB;
        }

        // Destructor
        ~PathPlanningTask()
        {
            // Cleaning up
            curRequest = null;
            mDistReachable = null;
            mDiffReachable = null;
        }

        #endregion

        #region Other Functions

        // Performing the path planning task
        public void Run()
        {
            switch (curRequest.AlgToUse)
            {
                case AlgType.CC:
                    // TODO handle CC
                    break;
                case AlgType.CC_E:
                    // TODO handle CC_E
                    break;
                case AlgType.EA:
                    // TODO handle EA
                    break;
                case AlgType.EA_E:
                    // TODO handle EA_E
                    break;
                case AlgType.LHCGWCONV:
                    // First do Global Warming
                    AlgGlobalWarming myGW = new AlgGlobalWarming(curRequest, ModeCount, mDistReachable, mDiffReachable, Efficiency_LB);
                    break;
                case AlgType.LHCGWCONV_E:
                    // TODO handle LHCGWCONV_E
                    break;
                case AlgType.LHCGWPF:
                    // TODO handle LHCGWPF
                    break;
                case AlgType.LHCGWPF_E:
                    // TODO handle LHCGWPF_E
                    break;
                case AlgType.PF:
                    // TODO handle PF
                    break;
                case AlgType.PF_E:
                    // TODO handle PF_E
                    break;
            }
        }


        #region Getters
        public double GetEfficiency()
        {
            return Efficiency;
        }
        public double GetRunTime()
        {
            return RunTime;
        }
        #endregion

        #endregion




    }
}
