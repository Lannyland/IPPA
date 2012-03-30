using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rtwmatrix;

namespace IPPA
{
    class AlgSearchReverse : AlgPathPlanning
    {
        #region Members

        // Private variables
        private int ModeCount = 0;
        private RtwMatrix mMode;

        // Public variables

        #endregion

        #region Constructor, Destructor

        // Constructor
        public AlgSearchReverse(PathPlanningRequest _curRequest, int _ModeCount, RtwMatrix _mMode, 
            RtwMatrix _mDistReachable, RtwMatrix _mDiffReachable, double _Efficiency_UB)
            : base (_curRequest, _mDistReachable, _mDiffReachable, _Efficiency_UB)
        {
            ModeCount = _ModeCount;
            mMode = _mMode;
        }

        // Destructor
        ~AlgSearchReverse()
        {
            // Cleaning up
        }

        #endregion

        #region Other Functions

        // Algorithm specific implementation of the path planning
        protected override void DoPathPlanning()
        {
            AlgPathPlanning curAlg;
            AlgPathPlanning curAlgReversed;
            PathPlanningRequest curRequestReversed = curRequest.Clone();
            curRequestReversed.pStart = curRequest.pEnd;
            curRequestReversed.pEnd = curRequest.pStart;

            // Use the right algorithm
            switch (curRequest.AlgToUse)
            {
                case AlgType.CC_E:
                    curAlg = new AlgCC_E(curRequest, mDist, mDiff, Efficiency_UB);
                    curAlgReversed = new AlgCC_E(curRequestReversed, mDist, mDiff, Efficiency_UB);
                    break;
                case AlgType.LHCGWCONV_E:
                    curAlg = new AlgGlobalWarming(curRequest, ModeCount, mDist, mDiff, Efficiency_UB);
                    curAlgReversed = new AlgGlobalWarming(curRequestReversed, ModeCount, mDist, mDiff, Efficiency_UB);
                    break;
                case AlgType.LHCGWPF_E:
                    curAlg = new AlgGlobalWarming(curRequest, ModeCount, mDist, mDiff, Efficiency_UB);
                    curAlgReversed = new AlgGlobalWarming(curRequestReversed, ModeCount, mDist, mDiff, Efficiency_UB);
                    break;
                case AlgType.LHCRandom_E:
                    curAlg = new AlgLHCRandom(curRequest, mDist, mDiff, Efficiency_UB);
                    curAlgReversed = new AlgLHCRandom(curRequestReversed, mDist, mDiff, Efficiency_UB);
                    break;
                case AlgType.Random_E:
                    curAlg = new AlgRandom(curRequest, mDist, mDiff, Efficiency_UB);
                    curAlgReversed = new AlgRandom(curRequestReversed, mDist, mDiff, Efficiency_UB);
                    break;
                case AlgType.CONV_E:
                    curAlg = new AlgGlobalWarming(curRequest, ModeCount, mDist, mDiff, Efficiency_UB);
                    curAlgReversed = new AlgGlobalWarming(curRequestReversed, ModeCount, mDist, mDiff, Efficiency_UB);
                    break;
                case AlgType.PF_E:
                    curAlg = new AlgPFLooper(curRequest, mDist, mDiff, Efficiency_UB);
                    curAlgReversed = new AlgPFLooper(curRequestReversed, mDist, mDiff, Efficiency_UB);
                    break;
                case AlgType.EA_E:
                    curAlg = new AlgEA_E(curRequest, ModeCount, mDist, mDiff, Efficiency_UB);
                    curAlgReversed = new AlgEA_E(curRequestReversed, ModeCount, mDist, mDiff, Efficiency_UB);
                    break;
                default:
                    curAlg = null;
                    curAlgReversed = null;
                    break;
            }

            curAlg.PlanPath();
            curAlgReversed.PlanPath();

            if (curAlg.GetCDF() >= curAlgReversed.GetCDF())
            {
                CDF = curAlg.GetCDF();
                Path = curAlg.GetPath();
            }
            else
            {
                CDF = curAlgReversed.GetCDF();
                Path = curAlgReversed.GetPath();
                Path.Reverse();
            }
        }

        #endregion
    }
}
