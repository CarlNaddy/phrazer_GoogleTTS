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

        public static string FormatRowTime(string text)
        {
            string rowTime = text.Replace(":", ".").Substring(1);
            if(rowTime.StartsWith("0.")) rowTime = rowTime.Substring(2);
            return rowTime;
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
