using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using Google.Cloud.Translation.V2;


namespace Phrazer
{
    class VTTConverter
    {
        // Configuration params
        bool translateRightNow = false;
        bool allowRepeat = false;
        bool transformTextToLowercase = true;

        List<string> tsvRows = new List<string>();
        HashSet<string> dieKontrollliste = new HashSet<string>();
        HashSet<string> dieTimeCodeKontrollliste = new HashSet<string>();

        string[] translatedTextList = new string[10000];
        string[] textBufferList = new string[10000];
        string[] wordCountList = new string[10000];
        int[] rowNumberList = new int[10000];
        string[] timeCodeList = new string[10000];

        public static string RequestTranslationByGoogle (string text) {
            var client = TranslationClient.Create();
            var response = client.TranslateText(text, LanguageCodes.German, LanguageCodes.English);
            return response.TranslatedText;
        }

        public void TranslateText () {
            string text = "";
            foreach (string row in textBufferList)
            {
                text = text + row + "\n";
            }
            string translation = RequestTranslationByGoogle(text);
            translatedTextList = translation.Split('\n');
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

        static public void convertToTSV()
        {
            string[] files = Directory.GetFiles(GetInputPath(), "*.vtt");
            //string[] files = Directory.GetFiles(GetInputPath(), "*.vtt.txt");
            foreach(string file in files) {
                VTTConverter obj = new VTTConverter();
                obj.ProcessVTTFile(file);
            }
        }

        public void ProcessVTTFile(string currentFileName)
        {
            if (!File.Exists(currentFileName))
            {
                Console.WriteLine("File " + currentFileName + " not exists!");
                return;
            }

            string[] rows = File.ReadAllLines(currentFileName);
            
            // Add header
            tsvRows.Add("DE" + "\t" + "EN" + "\t" + "LEN" + "\t" + "ROW" + "\t" + "TIME");

            string textBuffer = "";
            int maxBufferSize = 80;
            int rowNumber = 0;
            string time = "";

            foreach (string row in rows)
            {
                if(row.Trim() == "") continue;
                if(GetOutputTime(row).Length > 0) {
                    time = GetOutputTime(row);
                    continue;
                }
                if(time == "") continue;

                string rowText = SanitizeText(row);
                foreach(string word in rowText.Split(" "))
                {
                    if(textBuffer.Length > maxBufferSize || ShouldCreateNewRow(textBuffer, word))
                    {
                        SaveTextBufferToList(ref textBuffer, ref rowNumber, ref time);
                    }
                    textBuffer = (textBuffer + " " + word).Trim();
                    if(transformTextToLowercase) textBuffer = textBuffer.ToLower();
                }

                if(textBuffer.Length > maxBufferSize) {
                    SaveTextBufferToList(ref textBuffer, ref rowNumber, ref time);
                }
            }
            SaveTextBufferToList(ref textBuffer, ref rowNumber, ref time); // flush last buffer before save

            if(translateRightNow) TranslateText();

            // Prepare and write into file
            for(var i = 0; i < textBufferList.Length; i++) {
                if(textBufferList[i] == null) break;
                tsvRows.Add(translatedTextList[i] + "\t" + textBufferList[i] + "\t" + wordCountList[i] + "\t" + rowNumberList[i] + "\t" + timeCodeList[i]);
            }
            File.WriteAllLines(GetOutputAbsoluteFilename(currentFileName), tsvRows, Encoding.UTF8);
        }

        public string GetTimeCodeSuffix(string currentTime)
        {
            string[] signs = {"a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"};

            for(int i = 0; i < signs.Length; i++) {
                if(!dieTimeCodeKontrollliste.Contains(currentTime + signs[i])) return signs[i];
            }
            return "_ERROR";
        }

        public void SaveTextBufferToList(ref string textBuffer, ref int rowNumber, ref string time)
        {
            if(textBuffer.Length > 0 && time.Length > 0) {
                string kontrollText = textBuffer;
                

                // Add this to remove doubles ignoring special chars
                kontrollText = Regex.Replace(kontrollText, @"[^a-zA-Z0-9,\s]+", "", RegexOptions.Compiled);
                if(!dieKontrollliste.Contains(kontrollText)) {
                    string timeCode = time + GetTimeCodeSuffix(time);

                    textBufferList [rowNumber] = textBuffer;
                    wordCountList  [rowNumber] = WordCount(textBuffer);
                    rowNumberList  [rowNumber] = rowNumber;
                    timeCodeList   [rowNumber] = timeCode;
                    
                    dieTimeCodeKontrollliste.Add(timeCode);
                    rowNumber ++;
                    if(!allowRepeat) dieKontrollliste.Add(kontrollText);
                }
                textBuffer = "";
            }
        }

        public bool AlreadyOnList(string textBuffer)
        {
            if(dieKontrollliste.Contains(textBuffer)) return true;
            return false;
        }

        public bool ShouldCreateNewRow(string textBuffer, string word)
        {
            
            // Experimantal end of phrase recognition feature
            if(
                StartingWordDetected(textBuffer, word)
                || EndOfPhraseDetected(textBuffer, word)
                && !textBuffer.EndsWith("Mr.")
                && !textBuffer.EndsWith("Mrs.")
                && !textBuffer.EndsWith("Dr.")
                && !textBuffer.EndsWith("U.S.")
                && !textBuffer.EndsWith("L.A.")
                && !textBuffer.EndsWith("K.O.")
                && !textBuffer.EndsWith("C.E.O.")
                && textBuffer.Length > 2
            ) return true;

            return false;
        }

        public bool StartingWordDetected(string textBuffer, string word)
        {
            if(
                word.StartsWith("We're")
                || word.StartsWith("The")
                || word.StartsWith("Then")
                || word.StartsWith("To")
                || word.StartsWith("In")
                || word.StartsWith("So")
                || word.StartsWith("Perhaps")
                || word.StartsWith(">>")
            ) return true;

            return false;
        }
        public bool EndOfPhraseDetected(string textBuffer, string word)
        {
            if(
                textBuffer.Length > 25 && textBuffer.EndsWith(",") && !word.EndsWith(".")
                || textBuffer.Length > 25 && textBuffer.EndsWith("-")
                || textBuffer.Length > 5 && textBuffer.EndsWith(".")
                || textBuffer.Length > 5 && textBuffer.EndsWith(":")
                || textBuffer.Length > 5 && textBuffer.EndsWith("?")
                || textBuffer.Length > 5 && textBuffer.EndsWith("!")
                || textBuffer.EndsWith(")")
                || textBuffer.EndsWith("]")
                || textBuffer.EndsWith("--")
                || textBuffer.EndsWith("♪")
                || textBuffer.Contains("NETFLIX")
            ) return true;

            return false;   
        }

        static string SanitizeText(string text)
        {
            text = Regex.Replace(text, @"\s+", " ");
            text = RemoveBracketsText(text, '[', ']');
            text = RemoveBracketsText(text, '(', ')');
            text = text.Trim();
            //text = text.Replace(":", ","); // Wichtiges Trennzeichen
            text = text.Replace("\t", "");
            text = text.Replace("\"", "");
            text = text.Replace("<i>", "");
            text = text.Replace("</i>", "");
            text = text.Replace(" !", "!");
            text = text.Replace(" ?", "?");
            text = text.Replace(" !", "!");
            text = text.Replace(" ?", "?");
            text = text.Replace("’", "'");
            text = text.TrimStart('-');

            // Remove all "NAMES:"  
            if(IsAllUpper(text.Split()[0].Replace(":","")) && text.Contains(":")) {
                text = RemoveBracketsText(text, text[0], ':');
            }

            text = text.Trim();
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

        static string WordCount(string text)
        {
            return ("" + GTTSHelper.GetWordsCount(text)).PadLeft(2, '0');
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
