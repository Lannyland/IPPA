using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rtwmatrix;

namespace IPPA
{
    public enum UAVType { FixWing, Copter };
    public enum DType { FixAmount, FixAmountInPercentage, FixPercentage };
    public enum AlgType { CC, CC_E, LHCGWCONV, LHCGWCONV_E, LHCGWPF, LHCGWPF_E, PF, PF_E, EA, EA_E };

    public class PathPlanningRequest
    {
        #region Members

        // Private variables
        private string Log = "";

        // Public variables
        public bool UseDistributionMap = true;
        public bool UseTaskDifficultyMap = false;
        public bool UseCoarseToFineSearch = false;
        public bool UseParallelProcessing = false;
        public UAVType VehicleType = UAVType.FixWing;
        public DType DetectionType = DType.FixPercentage;
        public float DetectionRate = 1;
        public RtwMatrix DistMap;
        public RtwMatrix DiffMap;
        public bool UseEndPoint = false;
        public int T = 150;                                     // In time steps (2 seconds each)
        public DistPoint pStart = new DistPoint(0, 0);
        public DistPoint pEnd = new DistPoint(0, 0);
        public AlgType AlgToUse = AlgType.EA;
        public bool BatchRun = false;
        public int RunTimes = 1;
        
        #endregion

        #region Constructor, Destructor

        // Constructor
        public PathPlanningRequest()
        {
        
        }

        // Destructor
        ~PathPlanningRequest()
        {
            // Cleaning up
            DistMap = null;
            DiffMap = null;
        }

        #endregion

        #region functions

        public bool SanityCheck()
        {
            bool AllVerified = true;
            // If using distribution map, make sure a probability distribution map is provided
            if (UseDistributionMap && DistMap == null)
            {
                Log += "Distribution map not specified.\n";
                AllVerified = false;
            }
            // If using difficulty map, make sure a task-difficulty map is provided
            if (UseTaskDifficultyMap && DiffMap == null)
            {
                Log += "Task-difficulty map not specified.\n";
                AllVerified = false;
            }
            // Make sure algorithm used matches endpoint specification.
            if (UseEndPoint && (int)AlgToUse % 2 == 0)
            {
                // If end point is specified, use one of the _E algorithms
                Log += "Wrong type of algorithm specified. Use a _E algorithm instead.\n";
                AllVerified = false;
            }
            if (!UseEndPoint && (int)AlgToUse % 2 == 1)
            {
                // If end point is specified, use one of the _E algorithms
                Log += "Wrong type of algorithm specified. Don't use _E algorithm.\n";
                AllVerified = false;
            }
            // Make sure percentage is smaller than 1.
            if ((DetectionType == DType.FixAmountInPercentage || DetectionType == DType.FixPercentage) &&
                DetectionRate > 1)
            {
                Log += "Please make sure percentage is smaller than 100%.\n";
                AllVerified = false;
            }
            // Make sure starting point and end point are on the map
            if (pStart.row < 0 || pStart.column < 0 || pEnd.row < 0 || pEnd.column < 0)
            {
                Log += "Please make sure you have specified a valid starting point and end point.\n";
                AllVerified = false;
            }

            return AllVerified;
        }

        // Get Log
        public string GetLog()
        {
            return Log;
        }

        // Set Log
        public void SetLog(string s)
        {
            Log += s+"\n";
        }

        #endregion

    }
}
