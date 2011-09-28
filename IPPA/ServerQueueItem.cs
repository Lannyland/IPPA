using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPPA
{
    class ServerQueueItem
    {
        #region Members

        private PathPlanningRequest curRequest;
        private string CallerIP = "";

        #endregion

        #region Constructor, Destructor

        // Constructor
        public ServerQueueItem(PathPlanningRequest _curRequest, string _CallerIP)
        {
            curRequest = _curRequest;
        }

        // Destructor
        ~ServerQueueItem()
        {
            // Cleaning up
            curRequest = null;
        }

        #endregion

        #region Other Functions

        #region Getters
        public PathPlanningRequest GetRequest()
        {
            return curRequest;
        }
        #endregion

        #endregion
    }
}
