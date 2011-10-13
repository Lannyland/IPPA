using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rtwmatrix;
using System.Drawing;

namespace IPPA
{
    class AlgLHCGWCONV : AlgLHC
    {
        #region Members

        // Private members
        private int KernalSize = 0;

        #endregion

        #region Constructor, Destructor

        // Constructor
        public AlgLHCGWCONV(PathPlanningRequest _curRequest, RtwMatrix _mDistReachable, 
            RtwMatrix _mDiffReachable, double _Efficiency_UB, int _KernalSize) 
            : base (_curRequest, _mDistReachable, _mDiffReachable, _Efficiency_UB)
        {
            KernalSize = _KernalSize;
        }

        // Destructor
        ~AlgLHCGWCONV()
        {
            // Cleaning up
        }

        #endregion

        #region Other Functions

        // Abstract method different implementation by algorithm
        protected override float[] PrepareTieBreaker(Point me, int cur_T)
        {
            // Really don't need this. Just here for inheritance
            float[] forces = new float[1] {1};
            return forces;
        }

        // Function to find node with higher convolution value as tie-breaker
        protected override float TieBreaker(Point me, Point neighbor, int cur_T, float[] forces)
        {
            float cur_force = Convolve(neighbor, KernalSize);
            return cur_force;
        }
        
        // Function to calculate convolution value
        float Convolve(Point p, int filter_size)
        {
            // Make sure filter_size is an odd number
            if (filter_size % 2 == 0)
            {
                filter_size--;
            }

            // Now loop through points in filter
            int extra = (filter_size - 1) / 2;
            float total = 0;
            for (int y = -extra; y < extra + 1; y++)
            {
                for (int x = -extra; x < extra + 1; x++)
                {
                    int cur_x = p.X + x;
                    int cur_y = p.Y + y;
                    if (cur_x >= 0 && cur_y >= 0 && cur_x < mDist.Columns && cur_y < mDist.Rows)
                    {
                        total += GetPartialDetection(new Point(cur_x, cur_y));
                    }
                }
            }

            // Normalize and return
            return total / (filter_size * filter_size);
        }

        // Debugging shouts
        public override void Shout()
        {
            Console.WriteLine("I am AlgLHCGWCONV!");
        }


        #endregion

    }
}
