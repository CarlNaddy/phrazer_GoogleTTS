using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Phrazer
{
    class SubsRowFilter
    {
        
        public static bool SkipRow(string text)
        {
            if(text.Length < 2) return true;
            string filteredText = Regex.Replace(text, @"[^a-zA-Z0-9,\s]+", "", RegexOptions.Compiled).Trim();
            if(text.Contains("♪")) return true;
            if(text.Contains("&lt;")) return true;
            if(text.StartsWith("==")) return true;

            // SORT OUT VALUES
            if(text.StartsWith("Subtitles")) return true;
            if(text.StartsWith("Thank you,")) return true;

            // CUT IT MORE HARD
            if(text.Contains("___") && text.Length < 5) return true;
            if(SubsHelper.GetWordsCount(text) > 15) return true;

            // CUT IT LIKE A BIG HARVESTER
            if(text.Contains("--")) return true;
            if(text.Contains("___")) return true;
            if(SubsHelper.GetWordsCount(text) < 2) return true;
            if(SubsHelper.GetWordsCount(text) > 12) return true;

            if(SubsHelper.GetWordsCount(filteredText) < 2) return true;
            return false;
        }


    }
}
