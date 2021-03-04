using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;

namespace Phrazer
{
    class SubsTextSanitizer
    {
        
        public static string SanitizeText(string text)
        {
            text = Regex.Replace(text, @"\s+", " ");
            text = RemoveBracketsText(text, '[', ']');
            text = RemoveBracketsText(text, '(', ')');
            text = text.Trim();
            text = text.Replace("&lt;i>", "");
            text = text.Replace("&lt;/i>", "");
            text = text.Replace("\t", "");
            text = text.Replace("\"", "");
            text = text.Replace("<i>", "");
            text = text.Replace("</i>", "");
            text = text.Replace(" !", "!");
            text = text.Replace(" ?", "?");
            text = text.Replace(" !", "!");
            text = text.Replace(" ?", "?");
            text = text.Replace("I-I", "I");
            text = text.Replace("’", "'");
            text = text.Replace("...", "---");
            text = text.Replace("!", ".");
            text = text.Replace(".", ". ");
            text = text.Replace(">", ". ");
            text = text.TrimStart('-');
            
            // Remove all "NAMES:"  
            if(IsAllUpper(text.Split()[0].Replace(":","")) && text.Contains(":")) {
                text = RemoveBracketsText(text, text[0], ':');
            }

            text = SanitizePrefix(text);
            text = SanitizeMiddle(text);
            text = text.Trim();
            return text;
        }

        static string SanitizePrefix(string text)
        {
            text = text.Trim();
            if(text.Length == 0) return text;

            string[] strings = {
                // DELETE ANYWAY
                "Wow.", "Wow,", "wow,", "Whoa,", "Mmm.", "Nah.", "Hmm.", "Aw.",
                "Blah.", "Blah,", "blah,",
                "Ah.", "Ah,", "ah,", "Ah---", "ah---",
                "Oh.", "Oh,", "oh,", "Oh---", "oh---",
                "Uh.", "Uh,", "uh,", "Uh---", "uh---",
                "Um.", "Um,", "um,", "Um---", "um---",
                "Ooh.", "Ooh,", "ooh,",
                "Yeah.", "Yeah,", "yeah,",
                "Okay.", "Okay,", "okay,",
                "Well.", "Well,", "well,", "Well---", "well---",
                "So.", "So,", "so,", "So---", "so---",
                
                // DELETE MAYBE
                "Yes,", "yes,",
                "No,", "no,",
                "Now,", "now,",
                "And,", "and,",
                "But,", "but,",
                "Trust me,", "trust me,",
                "Listen.", "Listen,",
                "Look.", "Look,",
                "Darling,", "Sweetheart,",

                // DELETE NAMES
                "Alan.", "Alan,",
                "Charlie.", "Charlie,", "Judith,", "Rose,", "Jake,", "Berta,", "Lyndsey,", "Frankie,", "Mom,"
            };

            int affected = 0;
            bool fixCase = false;
            for(int t = 0; t < 10; t++) {
                affected = 0;
                for(int i = 0; i < strings.Length; i++) {
                    if(text.StartsWith(strings[i])) {
                        text = text.Substring(strings[i].Length).Trim();
                        fixCase = true;
                        affected++;
                    }
                }
                if(text.Length == 0) return text;

                // Dont forget to fix case after cutoff
                if(fixCase && affected == 0) {
                    text = text.First().ToString().ToUpper() + text.Substring(1);
                    break;
                }
            }
            if(text == "I don't think so.") {
                bool ok = true;
            }
            return text;
        }

        static string SanitizeMiddle(string text)
        {
            if(text.Length == 0) return text;

            string[] strings = {
                ", ah, ",
                ", oh, ",
                ", uh, ",
                ", um, ",
                ", well,"
            };

            int affected = 0;
            for(int t = 0; t < 10; t++) {
                affected = 0;
                for(int i = 0; i < strings.Length; i++) {
                    if(text.Contains(strings[i])) {
                        text = text.Replace(strings[i], ", ");
                        affected++;
                    }
                }
                if(text.Length == 0) return text;
                if(affected == 0) break;
            }
            return text;
        }





        static bool IsAllUpper(string input)
        {
            for (int i = 0; i < input.Length; i++)
            {
                if (!Char.IsUpper(input[i]))
                    return false;
            }
            return true;
        }



        static string RemoveBracketsText(string text, char startsWith, char endsWith)
        {
            int firstBracket = text.IndexOf(startsWith);
            int lastBracket = text.LastIndexOf(endsWith);

            if(firstBracket == -1) return text;
            if(lastBracket == -1) return text;

            int diff = lastBracket - firstBracket + 1;
            return text.Remove(firstBracket, diff);
        }
        
    }
}
