using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rtwmatrix;
using System.Drawing;

namespace IPPA
{
    class AlgLHCGWPF : AlgLHC
    {
        #region Members

        // Private members
        int Sigma = 0;

        #endregion

        #region Constructor, Destructor

        // Constructor
        public AlgLHCGWPF(PathPlanningRequest _curRequest, RtwMatrix _mDistReachable, 
            RtwMatrix _mDiffReachable, double _Efficiency_UB, int _Sigma) 
            : base (_curRequest, _mDistReachable, _mDiffReachable, _Efficiency_UB)
        {
            Sigma = _Sigma;
        }

        // Destructor
        ~AlgLHCGWPF()
        {
            // Cleaning up
        }

        #endregion

        #region Other Functions

        // Method different implementation by algorithm
        protected override float[] PrepareTieBreaker(Point me, int cur_T)
        {
            return CalculateForces(me, cur_T);
        }

        // Function to find node with higher convolution value as tie-breaker
        protected override float TieBreaker(Point me, Point neighbor, int cur_T, float[] forces)
        {
            List<int> directions = GetEndDirection(me, neighbor);
            float cur_force = forces[directions[0]];
            return cur_force;
        }

        // Return the potential field forces in four directions
        protected float[] CalculateForces(Point cur_node, int cur_T)
        {
            // Variables to store forces in different directions
            float[] forces = new float[4] { 0, 0, 0, 0 };

            // Loop through each pixel to calculate the force (North and East are positive forces)
            for (int i = 0; i < mCurDist.Rows; i++)
            {
                for (int j = 0; j < mCurDist.Columns; j++)
                {
                    // Check if the pixel is within reachable area for the remaining steps
                    int d = MISCLib.ManhattanDistance(cur_node.X, cur_node.Y, j, i);
                    if (d > 0 && d < (curRequest.T - cur_T))
                    {
                        // Identify where is the pixel compared to current node
                        List<int> directions = GetEndDirection(cur_node, new Point(j, i));
                        float Gamma = (float)(Math.Exp(-1 * d * d / (2 * Sigma * Sigma)));
                        float force = GetPartialDetection(new Point(j, i)) * Gamma;
                        
                        // Accumulate attractive forces
                        foreach (int dir in directions)
                        {
                            forces[dir] += force;
                        }
                    }
                    else
                    {
                        // d==0 means self. Safely ignore. No effect.
                        // d<(T-t) means impossible to reach. Safely ignore. No effect.
                    }
                }
            }
            
            //// Debug: print out forces
            //curRequest.SetLog(forces[0] + ", " + forces[1] + ", " + forces[2] + ", " + forces[3] + "\n");
            
            return forces;
        }

        // Return the direction of the end node
        private List<int> GetEndDirection(Point start, Point end)
        {
            // 8 directions. 0 for north and 7 for norstwest
            List<int> neighbors = new List<int>();
            if (start.Y > end.Y)
            {
                // All North related directions
                neighbors.Add(0);
                if (start.X > end.X)
                {
                    // Northwest
                    neighbors.Add(3);
                }
                else if (start.X < end.X)
                {
                    // Northeast
                    neighbors.Add(1);
                }
            }
            else if (start.Y == end.Y)
            {
                // East or west
                if (start.X > end.X)
                {
                    // West
                    neighbors.Add(3);
                }
                else if (start.X < end.X)
                {
                    // East
                    neighbors.Add(1);
                }
            }
            else
            {
                // All South related directions
                neighbors.Add(2);
                if (start.X > end.X)
                {
                    // Southwest
                    neighbors.Add(3);
                }
                else if (start.X < end.X)
                {
                    // Southeast
                    neighbors.Add(1);
                }
            }
            return neighbors;
        }
        
        // Debugging shouts
        public override void Shout()
        {
            Console.WriteLine("I am AlgLHCGWCONV!");
        }
        
        #endregion

    }
}
