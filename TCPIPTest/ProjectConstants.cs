using System;
using System.Collections.Generic;
using System.Text;

namespace TCPIPTest
{
    class ProjectConstants
    {
        // General parameters
        public static int DefaultDimension = 60;
        public static int MinFlightTime = 10;
        public static int MaxFlightTime = 3600;
        public static int DefaultFlightTime = 300;
        public static string MapsDir = @"C:\Lanny\MAMI\IPPA\Maps";
        // public static string DefaultDistMap = @"C:\Lanny\MAMI\IPPA\Maps\DistMaps\Simple_BimodalFar.csv";
        public static string DefaultDistMap = @"C:\Lanny\MAMI\IPPA\Maps\DistMaps\Smoothed_Small_HikerPaulDist.csv";
        public static string DefaultDiffMap = @"C:\Lanny\MAMI\IPPA\Maps\DiffMaps\Small_HikerPaulDiff.csv";
        
        // Down sample scale in CountDistModes class
        public static int DownSampleScale = 1;

        // Global Warming parameters
        public static int GWCount = 20;             // How many extensive GWs to search for
        public static int ConvCount = 3 ;           // How many convolution kernals to use
        public static int PFCount = 6;              // How many PF discount functions to use
        public static int CTFGWCoraseLevel = 4;     // How many GW to perform at each coarse-to-fine level
        public static int CTFGWLevelCount = 3;      // How many coarse-to-fine levels to search for

        // PF parameters
        public static int PFLoop = 12;              // How many time we should run PF with different sigmas
        public static int PFStep = 6;               // How much to increase sigma in each step

        // EA parameters
        public static int Count_CC = 5;
        public static int Count_LHCGWCONV = 4;
        public static int Count_LHCGWPF = 1;
        public static int Count_LHCRandom = 10;

        public static int EA_Population = 100;
        public static float EA_ReplacementRate = 0.3f;
        public static float EA_MutationRate = 0.50f;
        public static int EA_BestToKeep = 3;
        public static int EA_Crossover_MidSize = 5;
        public static int EA_Minimum_Run = 500;
        public static int EA_Maximum_Run = 3000;
        public static int EA_Epsilon_Run = 200;

        // EA_E parameters
        public static int EA_E_Population = 100;
        public static float EA_E_ReplacementRate = 0.3f;
        public static float EA_E_MutationRate = 0.90f;
        public static int EA_E_BestToKeep = 3;
        public static int EA_E_Crossover_MidSize = 5;
        public static int EA_E_Minimum_Run = 500;
        public static int EA_E_Maximum_Run = 1000;
        public static int EA_E_Epsilon_Run = 200;
        public static int EA_E_Mutate_MidSize = 5;

        // TopTwo parameters
        public static int SearchResolution = 40;    // How many searches
        public static int CTFTTCoraseLevel = 4;     // How many searches to perform at each coarse-to-fine level
        public static int CTFTTLevelCount = 3;      // How many coarse-to-fine levels to search for

        // TopN parameters
        public static int TopNCount = 3;            // Default N
        public static int Kernel_Size = 5;          // What size kenel for convolution       
        public static int Max_N = 5;                // Maximum number of Gaussians to fit 

        // Max wait time (*10)
        public static int MaxWaitTime = 1500;            // 15000 milisecond, so 15 seconds.

    }
}
