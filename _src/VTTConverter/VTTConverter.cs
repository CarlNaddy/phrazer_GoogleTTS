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
        List<string> tsvRows = new List<string>();


        // MOVE TO ROWFILTER
        HashSet<string> dieKontrollliste = new HashSet<string>();
        HashSet<string> dieTimeCodeKontrollliste = new HashSet<string>();
        string lastAddedItem = "";



        string[] translatedTextList = new string[10000];
        string[] textBufferList = new string[10000];
        string[] wordCountList = new string[10000];
        int[] rowNumberList = new int[10000];
        string[] timeCodeList = new string[10000];

        bool harvesterMode = false;
        public VTTConverter(bool hM)
        {
            harvesterMode = hM;
        }

        

        static public void convertToTSV()
        {
            // EMPTY THE ALLINPUTS FILE
            File.WriteAllText(SubsHelper.GetOutputAbsoluteFilename("__ALL_INPUTS", ""), "", Encoding.UTF8);

            // PROCESS VTTs
            string[] vttFiles = Directory.GetFiles(SubsHelper.GetInputPath(), "*.vtt");
            //string[] files = Directory.GetFiles(SubsHelper.GetInputPath(), "*.vtt.txt");
            foreach(string file in vttFiles) {
                VTTConverter obj = new VTTConverter(false);
                obj.ProcessFile(file, "vtt");

                if(SubsConfig.generateHarvesterFiles) {
                    VTTConverter obj2 = new VTTConverter(true);
                    obj2.ProcessFile(file, "vtt");
                }
            }

            // PROCESS TXTs
            string[] txtFiles = Directory.GetFiles(SubsHelper.GetInputPath(), "*.txt");
            foreach(string file in txtFiles) {
                VTTConverter obj = new VTTConverter(false);
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

            SubsNewLineDetector newLineDetector = new SubsNewLineDetector(harvesterMode);

            string[] rows = File.ReadAllLines(currentFileName);
            
            // Add header
            tsvRows.Add("DE" + "\t" + "EN" + "\t" + "LEN" + "\t" + "ROW" + "\t" + "TIME" + "\t" + "COUNT");

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
                    if(textBuffer.Length > SubsNewLineDetector.GetMaxBufferSize() || newLineDetector.ShouldCreateNewRow(textBuffer, word))
                    {
                        SaveTextBufferToList(ref textBuffer, ref rowNumber, ref time);
                    }
                    textBuffer = (textBuffer + " " + word).Trim();
                    if(SubsConfig.transformTextToLowercase) textBuffer = textBuffer.ToLower();
                }

                if(textBuffer.Length > SubsNewLineDetector.GetMaxBufferSize()) {
                    SaveTextBufferToList(ref textBuffer, ref rowNumber, ref time);
                }
            }

            SaveTextBufferToList(ref textBuffer, ref rowNumber, ref time); // flush last buffer before save

            if(SubsConfig.translateWithGoogle) translatedTextList = SubsTextTranslator.TranslateText(textBufferList);

            // Prepare and write into file
            for(var i = 0; i < textBufferList.Length; i++) {
                if(textBufferList[i] == null) break;
                tsvRows.Add(translatedTextList[i] + "\t" + textBufferList[i] + "\t" + wordCountList[i] + "\t" + rowNumberList[i] + "\t" + timeCodeList[i] + "\t" + "1");
            }
            string suffix = (harvesterMode) ? " - HARVESTER" : "";
            File.WriteAllLines(SubsHelper.GetOutputAbsoluteFilename(currentFileName, suffix), tsvRows, Encoding.UTF8);
            
            if(SubsConfig.generateAllInputsStatisticsFile)
                File.AppendAllLines(SubsHelper.GetOutputAbsoluteFilename("__ALL_INPUTS", ""), tsvRows, Encoding.UTF8);
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

            string kontrollText = textBuffer;
            kontrollText = Regex.Replace(kontrollText, @"[^a-zA-Z0-9,\d\s]+", "", RegexOptions.Compiled);

            // Add this to remove doubles ignoring special chars
            if(lastAddedItem == kontrollText || dieKontrollliste.Contains(kontrollText)) {
                textBuffer = "";
                return;
            }
            
            if(SubsRowFilter.SkipRow(textBuffer, harvesterMode)) {
                textBuffer = "";
                return;
            }

            string[] textParts = new string[20];
            
            // So now we have got a real story mode ;)
            if(textBuffer.Length > 50) {
                string partsBuffer = "";
                textParts = textBuffer.Split(",");
                for(int i = 0; i < textParts.Length; i++) {
                    partsBuffer = partsBuffer + textParts[i];
                    if( !textParts[i].EndsWith(".")
                        && !textParts[i].EndsWith("?")
                        && !textParts[i].EndsWith("!")
                    ) {
                        partsBuffer = partsBuffer + ", ";
                        // wenn string zu kurz - einfach weitergehen.
                        if(partsBuffer.Length < 15 || SubsHelper.GetWordsCount(partsBuffer) < 3) continue;
                    }
                    SaveTextPartToList(partsBuffer, ref rowNumber, ref time);
                    partsBuffer = "";
                }
            } else {
                SaveTextPartToList(textBuffer, ref rowNumber, ref time);
            }

            lastAddedItem = kontrollText;
            if(!SubsConfig.allowRepeat) dieKontrollliste.Add(kontrollText);
            textBuffer = "";
        }

        public void SaveTextPartToList(string textPart, ref int rowNumber, ref string time)
        {
            textPart = textPart.Trim();
            
            string timeCode = (time.Length > 0) ? time + GetTimeCodeSuffix(time) : "";

            textBufferList [rowNumber] = textPart;
            wordCountList  [rowNumber] = SubsHelper.GetWordCountString(textPart);
            rowNumberList  [rowNumber] = rowNumber;
            timeCodeList   [rowNumber] = timeCode;
            
            dieTimeCodeKontrollliste.Add(timeCode);
            rowNumber ++;
        }        

    }
}
