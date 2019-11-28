using System;
using System.IO;
using System.Text;

namespace Phrazer
{
    class Ttsmp3
    {
        public Ttsmp3(){
        }

        public static string GetDefaultSpeaker(string lang)
        {
            string speaker = "";
            if(lang == "EN") speaker = "Matthew";
            if(lang == "DE") speaker = "Hans";
            if(lang == "RU") speaker = "Maxim";
            return speaker; 
        }

        public static string GetTextSlow(string text)
        {
            return text.Replace(" ", ", ");
        }
        
    }
}
