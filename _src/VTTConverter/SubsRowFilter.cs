using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Phrazer
{
    class SubsRowFilter
    {
        
        public static bool SkipRow(string text, bool harvesterMode)
        {
            if(text.Length < 2) return true;
            //string filteredText = Regex.Replace(text, @"[^a-zA-Z0-9,\s]+", "", RegexOptions.Compiled).Trim();
            if(text.Contains("♪")) return true;
            if(text.Contains("&lt;")) return true;
            if(text.Contains("==")) return true;
            if(text.Contains("NETFLIX")) return true;
            if(text.Contains("Subtitles")) return true;

            // CHECK: CUT IT MORE HARD
            if(SubsHelper.GetWordsCount(text) < 2) return true;
            if(text.Contains("...") && text.Length < 8) return true;
            if(text.Contains("--") && text.Length < 6) return true;


            // CUT IT LIKE A BIG HARVESTER
            if(harvesterMode) {
                if(text.Contains("--")) return true;
                if(text.Contains("...")) return true;
                if(SubsHelper.GetWordsCount(text) < 2) return true;
                if(SubsHelper.GetWordsCount(text) > 8) return true;
            }
            
            return false;
        }


    }
}
