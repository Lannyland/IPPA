using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPPA
{
    class ComputeEfficiencyUB
    {
        #region Members
        PathPlanningRequest curRequest;
        double Efficiency_UB = 0;
        #endregion

        #region Constructor, Destructor

        // Constructor
        public ComputeEfficiencyUB(PathPlanningRequest _curRequest)
        {
            curRequest = _curRequest;
            Compute();
        }

        // Destructor
        ~ComputeEfficiencyUB()
        {
            // Cleaning up
            curRequest = null;
        }

        #endregion

        #region Other Functions

        // Compute the Efficiency Lower Bound based on PathPlanningRequest scenario
        private void Compute()
        {
            // TODO actually compute the Efficiency_UB
            Efficiency_UB = 100000;
        }

        #region Getters
        public double GetEfficiency_UB()
        {
            return Efficiency_UB;
        }
        #endregion

        #endregion

    }
}
