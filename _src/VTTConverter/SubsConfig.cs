using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Phrazer
{
    class SubsConfig
    {
        static public bool translateWithGoogle = false;
        static public bool allowRepeat = true;
        static public bool transformTextToLowercase = false;
        static public bool generateHarvesterFiles = false;
        static public bool generateAllInputsStatisticsFile = false;
    }
}
