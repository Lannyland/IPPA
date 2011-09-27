using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using rtwmatrix;

namespace IPPA
{
    // Parent class for all path planning algorithms
    public abstract class AlgPathPlanning
    {
        #region Members

        // Private members
        private PathPlanningRequest curRequest;
        private RtwMatrix mDist;
        private RtwMatrix mDiff;
        private double Efficiency_LB = 0;
        private double Efficiency = 0;
        private double CDF;
        private int NodesExpanded;
        private int RepeatedVisit;
        private List<Point> Path;
        
        #endregion

        #region Constructor, Destructor

        // Constructor
        public AlgPathPlanning(PathPlanningRequest _curRequest,
            RtwMatrix _mDist, RtwMatrix _mDiff, double _Efficiency_LB)
        {
            curRequest = _curRequest;
            mDist = _mDist;
            mDiff = _mDiff;
            Efficiency_LB = _Efficiency_LB;
        }

        // Destructor
        ~AlgPathPlanning()
        {
            // Cleaning up
            curRequest = null;
            mDist = null;
            mDiff = null;
        }

        #endregion

        #region Other Functions

        #region Getters
        public double GetEfficiency()
        {
            return Efficiency;
        }
        public double GetCDF()
        {
            return CDF;
        }
        public int GetNodesExpanded()
        {
            return NodesExpanded;
        }
        public int GetRepeatedVisit()
        {
            return RepeatedVisit;
        }
        public List<Point> GetPath()
        {
            return Path;
        }
        #endregion

        #endregion

    }
}
