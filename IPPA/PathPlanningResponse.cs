using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rtwmatrix;
using System.Drawing;

namespace IPPA
{
    public class PathPlanningResponse
    {
        #region Members

        // Public variables
        public int index = 0;
        public double CDF;
        public double RunTime = 0;
        public double Efficiency = 0;
        public List<Point> Path = new List<Point>();
        
        #endregion

        #region Constructor, Destructor

        // Constructor
        public PathPlanningResponse()
        {
        }

        public PathPlanningResponse(int _index, double _CDF, double _RunTime, double _Efficiency, List<Point> _Path)
        {
            index = _index;
            CDF = _CDF;
            RunTime = _RunTime;
            Efficiency = _Efficiency;
            Path = _Path;
        }

        // Destructor
        ~PathPlanningResponse()
        {
            // Cleaning up
        }

        #endregion
                
        #region functions

        #endregion

    }
}
