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
        protected PathPlanningRequest curRequest;
        protected RtwMatrix mDist;
        protected RtwMatrix mDiff;
        protected double Efficiency_LB = 0;
        protected double CDF;
        protected double RunTime = 0;
        protected double Efficiency = 0;
        protected int NodesExpanded = 0;
        protected int PathExplored = 0;
        protected int RepeatedVisit = 0;
        protected List<Point> Path;
        
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

        public virtual void PlanPath()
        {
        }

        #region Getters
        public double GetCDF()
        {
            return CDF;
        }
        public double GetRunTime()
        {
            return RunTime;
        }
        public double GetEfficiency()
        {
            return Efficiency;
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
