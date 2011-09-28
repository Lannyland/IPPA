using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IPPA
{
    class PathPlanningServer
    {
        #region Members

        private List<ServerQueueItem> ServerQueue = new List<ServerQueueItem>();

        #endregion

        #region Constructor, Destructor

        // Constructor
        public PathPlanningServer()
        {
        }

        // Destructor
        ~PathPlanningServer()
        {
            // Cleaning up
            ServerQueue.Clear();
            ServerQueue = null;
        }

        #endregion

        #region Other Functions

        // Method to add server queue items
        public void AddRequest(ServerQueueItem item)
        {
            ServerQueue.Add(item);
        }

        #region Getters
        public List<ServerQueueItem> GetServerQueue()
        {
            return ServerQueue;
        }
        #endregion

        #endregion
    }
}
