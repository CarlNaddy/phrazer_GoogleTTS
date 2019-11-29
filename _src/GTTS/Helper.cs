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

        public static string GetTextSlow(string text)
        {
            return text.Replace(" ", GetBreakSsmlTag("500ms"));
        }

        public static string GetBreakSsmlTag(string time)
        {
            return " <break time=\"" + time.Trim() + "\"/> ";
        }
        
    }
}
