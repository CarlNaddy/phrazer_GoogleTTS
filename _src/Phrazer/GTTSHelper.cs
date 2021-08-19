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
            text = text.Replace(":", "."); // Wichtiges Trennzeichen
            text = text.Replace("/", " ");
            text = text.Replace("’", "'");
            text = text.Replace("♪", "");
            text = text.Replace(">", "");
            text = text.Replace("... ", "...");
            text = text.Replace(" ...", "...");
            text = text.Trim();
            
            if(forWhat == "filename") {
                text = text.Trim(',');
                text = text.Trim('.');
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
            string[] parts = text.Split(" ");
            for(int i = 0; i < parts.Length; i ++) {
                parts[i] = parts[i] + GetBreakSsmlTag(1000);
            }
            return string.Join("", parts);
        }

        public static string GetBreakSsmlTag(int milliseconds)
        {
            return " <break time=\"" + milliseconds + "ms\"/> ";
        }


        public static int GetWaitTime(string text, bool includingThingingTime)
        {
            double thinkingTime = 0;

            // TEXT LENGTH BASED
            double repeatingTime = text.Length * 0.05 + 2;
            
            if(includingThingingTime)
                thinkingTime = text.Length * 0.005 + 1;

            if(text.Length > 75) {
                if(includingThingingTime) thinkingTime = -1;
                repeatingTime = 4; // Wartezeiten limitieren bei langen Sätzen
            }
                
            return Convert.ToInt32((repeatingTime + thinkingTime) * 1000);
        }

        public static string GetTemplateName(string FromText, string ToText)
        {
            // Repeat 2 times on longer phrase
            if(ToText.Length > 75) {
                return "phrase_2.tpl";
            }

            // if no TEXT_FROM, then make just text
            if(FromText.Length == 0) {
                return "text_2.tpl";
            }

            // else just a usual audioflashcard
            return "phrase_3.tpl";
        }






        public static string GetFolderSuffix(string rowNumber)
        {
            int rn = int.Parse(rowNumber);
            if(rn == 0) return "";
            int folderNumber = (int) (Math.Ceiling(((decimal)rn / 500)));
            if(folderNumber > 1) return "_" + folderNumber;
            return "";
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
