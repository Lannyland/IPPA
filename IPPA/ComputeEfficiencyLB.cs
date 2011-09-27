using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPPA
{
    class ComputeEfficiencyLB
    {
        #region Members
        PathPlanningRequest curRequest;
        double Efficiency_LB = 0;
        #endregion

        #region Constructor, Destructor

        // Constructor
        public ComputeEfficiencyLB(PathPlanningRequest _curRequest)
        {
            curRequest = _curRequest;
            Compute();
        }

        // Destructor
        ~ComputeEfficiencyLB()
        {
            // Cleaning up
            curRequest = null;
        }

        #endregion

        #region Other Functions

        // Compute the Efficiency Lower Bound based on PathPlanningRequest scenario
        private void Compute()
        {
            // TODO actually compute the Efficiency_LB
            Efficiency_LB = 50000;
        }

        #region Getters
        public double GetEfficiency_LB()
        {
            return Efficiency_LB;
        }
        #endregion

        #endregion

    }
}
