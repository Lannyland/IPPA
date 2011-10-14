using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using rtwmatrix;

namespace IPPA
{
    class AlgPFLooper : AlgPathPlanning
    {
        #region Members

        // Private variables

        // Public variables

        #endregion

        #region Constructor, Destructor
        
        // Constructor
        public AlgPFLooper(PathPlanningRequest _curRequest,
            RtwMatrix _mDistReachable, RtwMatrix _mDiffReachable,
            double _Efficiency_UB)
            : base (_curRequest, _mDistReachable, _mDiffReachable, _Efficiency_UB)
        {
        }

        // Destructor
        ~AlgPFLooper()
        {
            // Cleaning up
        }

        #endregion

        #region Other Functions
        
        // Algorithm specific implementation of the path planning
        protected override void DoPathPlanning()
        {
            // Make copy of map
            RtwMatrix mPF = mDist.Clone();
            
            int Sigma = 0;
            
            // Loop many times
            for (int i = 0; i < ProjectConstants.PFLoop; i++)
            {
                Sigma += Math.Max(mCurDist.Rows, mCurDist.Columns) / ProjectConstants.PFStep;
                AlgPF myAlg = new AlgPF(curRequest, mPF, mDiff, Efficiency_UB, Sigma);
                myAlg.PlanPath();

                // Remember if CDF is better
                RememberBestPath(myAlg);

                // Cleaning up                        
                myAlg = null;

            }

            // If we already have the best path, then no need to continue
            if (Math.Abs(Efficiency_UB - CDF) < 0.001)
            {
                return;
            }
        }

        // Remember the best CDF and path so far
        private void RememberBestPath(AlgPF myAlg)
        {
            double curCDF = myAlg.GetCDF();
            if (CDF < curCDF)
            {
                CDF = curCDF;
                Path = myAlg.GetPath();
            }
        }

        // Debugging shouts
        public override void Shout()
        {
            Console.WriteLine("I am AlgPFLooper!");
        }

        #endregion
    }
}
