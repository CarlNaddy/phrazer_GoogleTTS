using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;


namespace Phrazer
{
    class VTTConverter
    {
        // Configuration params

        bool phrazeMode = true;
        bool translateWithGoogle = false;
        bool allowRepeat = true;
        bool transformTextToLowercase = false;
        bool capitalizeFirstSign = false;

        List<string> tsvRows = new List<string>();
        HashSet<string> dieKontrollliste = new HashSet<string>();
        HashSet<string> dieTimeCodeKontrollliste = new HashSet<string>();

        string[] translatedTextList = new string[10000];
        string[] textBufferList = new string[10000];
        string[] wordCountList = new string[10000];
        int[] rowNumberList = new int[10000];
        string[] timeCodeList = new string[10000];

        

        

        static public void convertToTSV()
        {
            // EMPTY THE ALLINPUTS FILE
            File.WriteAllText(SubsHelper.GetOutputAbsoluteFilename("__ALL_INPUTS"), "", Encoding.UTF8);

            // PROCESS VTTs
            string[] vttFiles = Directory.GetFiles(SubsHelper.GetInputPath(), "*.vtt");
            //string[] files = Directory.GetFiles(SubsHelper.GetInputPath(), "*.vtt.txt");
            foreach(string file in vttFiles) {
                VTTConverter obj = new VTTConverter();
                obj.ProcessFile(file, "vtt");
            }

            // PROCESS TXTs
            string[] txtFiles = Directory.GetFiles(SubsHelper.GetInputPath(), "*.txt");
            foreach(string file in txtFiles) {
                VTTConverter obj = new VTTConverter();
                obj.ProcessFile(file, "txt");
            }
        }





        public void ProcessFile(string currentFileName, string inputFormat)
        {
            if (!File.Exists(currentFileName))
            {
                Console.WriteLine("File " + currentFileName + " not exists!");
                return;
            }

            SubsNewLineDetector newLineDetector = new SubsNewLineDetector(phrazeMode);

            string[] rows = File.ReadAllLines(currentFileName);
            
            // Add header
            tsvRows.Add("DE" + "\t" + "EN" + "\t" + "LEN" + "\t" + "ROW" + "\t" + "TIME");

            string textBuffer = "";
            int rowNumber = 0;
            string time = "";



            foreach (string row in rows)
            {
                if(inputFormat == "vtt") {
                    if(row.Trim() == "") continue;
                    if(SubsHelper.GetOutputTime(row).Length > 0) {
                        time = SubsHelper.GetOutputTime(row);
                        continue;
                    }
                    if(time == "") continue;
                }

                
                string rowText = SubsTextSanitizer.SanitizeText(row);
                if(rowText == "") continue;

                foreach(string word in rowText.Split(" "))
                {
                    if(textBuffer.Length > newLineDetector.GetMaxBufferSize() || newLineDetector.ShouldCreateNewRow(textBuffer, word))
                    {
                        SaveTextBufferToList(ref textBuffer, ref rowNumber, ref time);
                    }
                    textBuffer = (textBuffer + " " + word).Trim();

                    if(textBuffer.Length > 1 && (phrazeMode || capitalizeFirstSign)) {
                        textBuffer = textBuffer.First().ToString().ToUpper() + textBuffer.Substring(1);
                    }

                    if(transformTextToLowercase) textBuffer = textBuffer.ToLower();
                }

                if(textBuffer.Length > newLineDetector.GetMaxBufferSize()) {
                    SaveTextBufferToList(ref textBuffer, ref rowNumber, ref time);
                }
            }

            SaveTextBufferToList(ref textBuffer, ref rowNumber, ref time); // flush last buffer before save

            if(translateWithGoogle) translatedTextList = SubsTextTranslator.TranslateText(textBufferList);

            // Prepare and write into file
            for(var i = 0; i < textBufferList.Length; i++) {
                if(textBufferList[i] == null) break;
                tsvRows.Add(translatedTextList[i] + "\t" + textBufferList[i] + "\t" + wordCountList[i] + "\t" + rowNumberList[i] + "\t" + timeCodeList[i]);
            }
            File.WriteAllLines(SubsHelper.GetOutputAbsoluteFilename(currentFileName), tsvRows, Encoding.UTF8);
            File.AppendAllLines(SubsHelper.GetOutputAbsoluteFilename("__ALL_INPUTS"), tsvRows, Encoding.UTF8);
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
            if(textBuffer.Length < 2) return;

            textBuffer = SubsTextSanitizer.SanitizeText(textBuffer);

            if(!SubsHelper.SkipRow(textBuffer)) {
                // Add this to remove doubles ignoring special chars
                string kontrollText = textBuffer;
                kontrollText = Regex.Replace(kontrollText, @"[^a-zA-Z0-9,\d\s]+", "", RegexOptions.Compiled);
                if(!dieKontrollliste.Contains(kontrollText)) {
                    string timeCode = (time.Length > 0) ? time + GetTimeCodeSuffix(time) : "";

                    textBufferList [rowNumber] = textBuffer;
                    wordCountList  [rowNumber] = SubsHelper.GetWordCountString(textBuffer);
                    rowNumberList  [rowNumber] = rowNumber;
                    timeCodeList   [rowNumber] = timeCode;
                    
                    dieTimeCodeKontrollliste.Add(timeCode);
                    rowNumber ++;
                    if(!allowRepeat) dieKontrollliste.Add(kontrollText);
                }
            }

            textBuffer = "";
        }



        public bool AlreadyOnList(string textBuffer)
        {
            if(dieKontrollliste.Contains(textBuffer)) return true;
            return false;
        }

        
        


    }
}
