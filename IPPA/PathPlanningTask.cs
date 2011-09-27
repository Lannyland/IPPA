using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPPA
{
    class PathPlanningTask
    {
        #region Members

        PathPlanningRequest curRequest;
        #endregion

        #region Constructor, Destructor

        // Constructor
        public PathPlanningTask(PathPlanningRequest _curRequest)
        {
            curRequest = _curRequest;
        }

        // Destructor
        ~PathPlanningTask()
        {
            // Cleaning up
            curRequest = null;
        }

        #endregion

        #region Other Functions

        public void Run()
        {
            int BatchCount = 1;
            float RunTime = 0;
            float AvgRunTime = 0;
            float StdRunTime = 0;
            float Efficiency = 0;
            float AvgEfficiency = 0;
            float StdEfficiency = 0;            

            if (curRequest.BatchRun)
            {
                BatchCount = curRequest.RunTimes;
            }
            for (int i = 0; i < BatchCount; i++)
            {
                // Run them sequencially and don't use multiple threads.

            }
        }

        #endregion




    }
}
