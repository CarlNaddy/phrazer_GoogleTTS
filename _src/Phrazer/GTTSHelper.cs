using System;
using System.IO;
using System.Text;

namespace Phrazer
{
    class GTTSHelper
    {
        public static int LongPhraseLength = 65;
        public static int SlowTextPauseBetweenWordsMs = 750;

        /**
        Available voices: https://cloud.google.com/text-to-speech/docs/voices?hl=de
        */
        public static string GetDefaultSpeaker(string lang)
        {
            string speaker = "";
            lang = lang.Trim();
            if(lang == "EN") speaker = "en-GB-Wavenet-C";
            if(lang == "DE") speaker = "de-DE-Wavenet-C";
            //if(lang == "RU") speaker = "ru-RU-Wavenet-A"; //woman
            if(lang == "RU") speaker = "ru-RU-Wavenet-B"; // man
            return speaker; 
        }

        public static string GetSanitizedText(string text, string forWhat)
        {
            // First remowe the gender prefixes
            if(text.StartsWith("W:")) text = text.Replace("W:", "");
            if(text.StartsWith("M:")) text = text.Replace("M:", "");

            text = text.Replace(":", "."); // Wichtiges Trennzeichen
            text = text.Replace("/", " ");
            text = text.Replace("’", "'");
            text = text.Replace("♪", "");
            text = text.Replace(">", "");
            text = text.Replace("_", ".");
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
            int currentPauseBetweenWords = 0;
            string[] parts = text.Split(" ");
            for(int i = 0; i < parts.Length; i ++) {
                currentPauseBetweenWords = SlowTextPauseBetweenWordsMs;
                if (parts[i].Length > 9) {
                    currentPauseBetweenWords = SlowTextPauseBetweenWordsMs * 2;
                }
                parts[i] = parts[i] + GetBreakSsmlTag(currentPauseBetweenWords);
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
            double repeatingTime = text.Length * 0.04 + 2;
            
            if(includingThingingTime)
                thinkingTime = text.Length * 0.004 + 1;

            if(text.Length > LongPhraseLength) {
                //if(includingThingingTime) thinkingTime = -1;
                //repeatingTime = 4; // Wartezeiten limitieren bei langen Sätzen

                repeatingTime = text.Length * 0.04 + 1;

                if(includingThingingTime)
                    thinkingTime = text.Length * 0.004;
            }
                
            return Convert.ToInt32((repeatingTime + thinkingTime) * 1000);
        }

        public static string GetTplPath(string langFrom, string langTo)
        {
            string voiceGenerater = "GoogleSSML";
            string targetGroup = langFrom + "_" + langTo;
            return Appdata.GetAppdataPath()
            + "_tpl" + Path.DirectorySeparatorChar 
            + voiceGenerater + Path.DirectorySeparatorChar 
            + targetGroup + Path.DirectorySeparatorChar;
        }
        public static string GetTplName(string FromText, string ToText)
        {
            // gender voice needed (W or M)?
            if(ToText.StartsWith("W:")) {
                return "phrase_3_W.tpl";
            }
            if(ToText.StartsWith("M:")) {
                return "phrase_3_M.tpl";
            }
            // Repeat 2 times on longer phrase
            if(ToText.Length > LongPhraseLength) {
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
