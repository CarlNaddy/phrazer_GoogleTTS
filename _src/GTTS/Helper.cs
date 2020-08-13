using System;
using System.IO;
using System.Text;

namespace Phrazer
{
    class GTTSHelper
    {

        /**
        Available voices: https://cloud.google.com/text-to-speech/docs/voices?hl=de
        */
        public static string GetDefaultSpeaker(string lang)
        {
            string speaker = "";
            lang = lang.Trim();
            if(lang == "EN") speaker = "en-GB-Wavenet-C";
            if(lang == "DE") speaker = "de-DE-Wavenet-C";
            if(lang == "RU") speaker = "ru-RU-Wavenet-B";
            return speaker; 
        }

        public static string GetSanitizedText(string text, string forWhat)
        {
            text = text.Replace(":", ","); // Wichtiges Trennzeichen
            text = text.Replace("’", "'");
            text = text.Replace("♪", "");
            text = text.Trim();
            text = text.Trim(',');
            text = text.Trim('.');
            if(forWhat == "filename") {
                text = text.Replace("  ", " ");
                text = text.Replace("(", "-");
                text = text.Replace(")", "-");
                text = text.Replace("?", "..");
            }
            text = System.Text.RegularExpressions.Regex.Replace(text, @"[\\/:*""<>|]", string.Empty); // BEWARE: ? will not be treated here
            return text;
        }

        public static string GetTextSlow(string text)
        {
            return text.Replace(" ", GetBreakSsmlTag("400ms"));
        }

        public static string GetBreakSsmlTag(string time)
        {
            return " <break time=\"" + time.Trim() + "\"/> ";
        }

        public static string GetFormattedTimeCode(string rowTime)
        {
            if(rowTime.Length == 8) return rowTime.Replace(":", ".").Substring(1);
            return rowTime;
        }

        public static int GetFolderNumber(string rowTime, bool justHour)
        {
            if(rowTime.Length != 8) return 0;

            if(justHour) return int.Parse(rowTime.Substring(0, 2));

            int minutesPerPackage = 5;
            // Get time in seconds
            string[] timeArray = rowTime.Split(":");
            int timeInSeconds = int.Parse(timeArray[0]) * 3600 + int.Parse(timeArray[1]) * 60 + int.Parse(timeArray[2]);
            return (int) (Math.Floor((decimal) timeInSeconds/(minutesPerPackage * 62)) + 1);
        }

        public static int GetWordsCount(string text)
        {
            return text.Split(" ").Length;
        }

        public static string Substring(string text, int start, int length)
        {
            if(text.Length >= length) return text.Substring(start, length);
            return text;
        }
        
    }
}
