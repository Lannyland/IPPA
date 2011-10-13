using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using rtwmatrix;
using System.Drawing;

namespace IPPA
{
    // Launched from PathPlanningHandler
    // Performs one of the batch path planning tasks
    // Calls the right algorithm based on PathPlanningRequest
    class PathPlanningTask
    {
        #region Members

        // Private members
        private PathPlanningRequest curRequest;
        private int ModeCount = 0;
        private RtwMatrix mDistReachable;
        private RtwMatrix mDiffReachable;
        private double Efficiency_LB = 0;
        private double Efficiency = 0;
        private double RunTime = 0;
        private List<Point> Path;
        AlgPathPlanning curAlg;

        #endregion

        #region Constructor, Destructor

        // Constructor
        public PathPlanningTask(PathPlanningRequest _curRequest, int _ModeCount,
            RtwMatrix _mDistReachable, RtwMatrix _mDiffReachable, double _Efficiency_LB)
        {
            curRequest = _curRequest;
            ModeCount = _ModeCount;
            mDistReachable = _mDistReachable;
            mDiffReachable = _mDiffReachable;
            Efficiency_LB = _Efficiency_LB;
        }

        // Destructor
        ~PathPlanningTask()
        {
            // Cleaning up
            curRequest = null;
            mDistReachable = null;
            mDiffReachable = null;
            curAlg = null;
        }

        #endregion

        #region Other Functions

        // Performing the path planning task
        public void Run()
        {
            // Use the right algorithm
            switch (curRequest.AlgToUse)
            {
                case AlgType.CC:
                    curAlg = new AlgCC(curRequest, mDistReachable, mDiffReachable, Efficiency_LB);
                    curAlg.PlanPath();
                    break;
                case AlgType.CC_E:
                    // TODO handle CC_E
                    break;
                case AlgType.LHCGWCONV:
                    curAlg = new AlgGlobalWarming(curRequest, ModeCount, mDistReachable, mDiffReachable, Efficiency_LB);
                    curAlg.PlanPath();
                    break;
                case AlgType.LHCGWCONV_E:
                    // TODO handle LHCGWCONV_E
                    break;
                case AlgType.LHCGWPF:
                    curAlg = new AlgGlobalWarming(curRequest, ModeCount, mDistReachable, mDiffReachable, Efficiency_LB);
                    curAlg.PlanPath();
                    break;
                case AlgType.LHCGWPF_E:
                    // TODO handle LHCGWPF_E
                    break;
                case AlgType.LHCRandom:
                    curAlg = new AlgLHCRandom(curRequest, mDistReachable, mDiffReachable, Efficiency_LB);
                    curAlg.PlanPath();
                    break;
                case AlgType.LHCRandom_E:
                    // TODO handle LHCGWPF_E
                    break;
                case AlgType.PF:
                    // TODO handle PF
                    break;
                case AlgType.PF_E:
                    // TODO handle PF_E
                    break;
                case AlgType.EA:
                    // TODO handle EA
                    break;
                case AlgType.EA_E:
                    // TODO handle EA_E
                    break;
            }

            // Set things ready for getters
            Efficiency = curAlg.GetEfficiency();
            RunTime = curAlg.GetRunTime();
            Path = curAlg.GetPath();
            curAlg.Shout();

            // Debug code, show actual path
            if (curRequest.DrawPath)
            {
                //// Draw coverage
                //Bitmap CurBMP = new Bitmap(mDistReachable.Columns, mDistReachable.Rows);
                //ImgLib.MatrixToImage(ref mDistReachable, ref CurBMP);
                //frmMap map = new frmMap();
                //map.Text = "Actual UAV path";
                //map.setImage(CurBMP);
                //map.Show();
                //map.resetImage();
                //map.DrawCoverage(Path);
                //map.Refresh();

                //// Draw path
                //Bitmap CurBMP2 = new Bitmap(mDistReachable.Columns, mDistReachable.Rows);
                //ImgLib.MatrixToImage(ref mDistReachable, ref CurBMP2);
                //frmMap map2 = new frmMap();
                //map2.Text = "UAV trajectory simulation";
                //map2.setImage(CurBMP2);
                //map2.Show();
                //map2.resetImage();
                //map2.DrawPath(Path);
                //map2.Refresh();

                // Draw path with map remains
                Bitmap CurBMP3 = new Bitmap(mDistReachable.Columns, mDistReachable.Rows);
                ImgLib.MatrixToImage(ref mDistReachable, ref CurBMP3);
                frmMap map2 = new frmMap();
                map2.Text = "UAV trajectory and coverage";
                map2.setImage(CurBMP3);
                map2.Show();
                map2.resetImage();
                List<float> remains = curAlg.ShowCoverage();
                Color c = Color.FromArgb(255, 0, 0);
                for (int i=0; i<Path.Count; i++)
                {
                    Point p = Path[i];
                    map2.setPointColor(p, c);
                    map2.Refresh();
                    map2.setPointColor(p, remains[i]);
                    map2.Refresh();
                }                

                // Drawing real path
                MISCLib.ShowImage(MISCLib.DrawPath(Path), "Real Path");
            }

            // Log results
            curRequest.SetLog("Best CDF: " + curAlg.GetCDF() + "\n");
            curRequest.SetLog("Best Efficiency: " + curAlg.GetEfficiency() + "\n");
        }

        #region Getters
        public double GetEfficiency()
        {
            return Efficiency;
        }
        public double GetRunTime()
        {
            return RunTime;
        }
        public List<Point> GetPath()
        {
            return Path;
        }
        #endregion

        #endregion




    }
}
