using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Phrazer
{
    class SubsHelper
    {
        public static string GetInputPath()
        {
            return Appdata.GetAppdataPath() + "_extern" + Path.DirectorySeparatorChar + "inputVTT" + Path.DirectorySeparatorChar;
        }

        public static string GetOutputAbsoluteFilename(string currentFileName)
        {
            string path = Appdata.GetAppdataPath() + "_extern" + Path.DirectorySeparatorChar + "outputTSV" + Path.DirectorySeparatorChar;
            if(!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
            return path + Path.GetFileNameWithoutExtension(currentFileName) + ".tsv";
        }



        public static string GetOutputTime(string text)
        {
            text = text.Trim();
            string muster = "03:00.242 --> 03:03.002";
            if(text.Contains(" --> ") && text.Length == muster.Length) {
                return "00:" + text.Substring(0, 5);
            }

            string muster1_1 = "1:03:00.242 --> 1:03:03.002";
            if(text.Contains(" --> ") && text.Length == muster1_1.Length) {
                return "0" + text.Substring(0, 7);
            }

            string muster1_2 = "59:58.512 --> 1:00:00.472";
            if(text.Contains(" --> ") && text.Length == muster1_2.Length) {
                return "00:" + text.Substring(0, 5);
            }

            string muster2 = "00:00:20.729 --> 00:00:22.731";
            if(text.Contains(" --> ") && text.Length == muster2.Length) {
                return text.Substring(0, 8);
            }

            return "";
        }


        public static bool SkipRow(string text)
        {
            string filteredText = Regex.Replace(text, @"[^a-zA-Z0-9,\s]+", "", RegexOptions.Compiled).Trim();
            if(text.Contains("♪")) return true;
            if(text.StartsWith("&lt;")) return true;
            if(text.StartsWith("==")) return true;


            if(GetWordsCount(filteredText) < 2) return true;
            return false;
        }



        
        public static string GetWordCountString(string text)
        {
            return ("" + GetWordsCount(text)).PadLeft(2, '0');
        }

        public static int GetWordsCount(string text)
        {
            return text.Split(" ").Length;
        }
    }
}
