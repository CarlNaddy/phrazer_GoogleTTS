using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading;


namespace Phrazer
{
    class PhrazeGenerator
    {
        public string InputFileName { get; set; }
        public int RowNumberTpl { get; set; }
        public string FromLang { get; set; }
        public string ToLang { get; set; }
        public string FromText { get; set; }
        public string ToText { get; set; }
        public string RowNumber { get; set; }
        public string TimeCode { get; set; }
        public string TimeCodeList { get; set; }
        public int MaxTextLength { get; set; }
        public bool SkipWithoutTranslation { get; set; }
        public string FolderSuffix { get; set; }
        public bool FolderSuffixAllowed { get; set; }
        public string CurrentVoice { get; set; }
        public string CurrentSsml { get; set; }
        public GTTSGenerator Generator { get; set; }

        public PhrazeGenerator()
        {
            FromLang = "";
            ToLang = "";
            MaxTextLength = 115;
            RowNumber = "";
            TimeCode = "";
            TimeCodeList = "";
            FolderSuffix = "";
            FolderSuffixAllowed = false;
            SkipWithoutTranslation = true;
        }

        public string GetOutputFilenamePrefix()
        {
            if(TimeCode.Length > 0) return TimeCode + ". ";
            if(RowNumber.Length > 0) return RowNumber + ". ";

            // else standard buildup/wordcount procedure
            return GetFormattedWordCount(ToText) + ". ";
        }

        public string GetOutputFilename()
        {
            string fromText = GTTSHelper.GetSanitizedText(FromText, "filename");
            string toText = GTTSHelper.GetSanitizedText(ToText, "filename");

            // Generate a file from the script and save
            string oFileName = "";
            string oTranslation = "";

            oTranslation = GTTSHelper.Substring(fromText, 0, MaxTextLength - ToText.Length -3);
            if(oTranslation.Length < fromText.Length) oTranslation = oTranslation + "...";
            
            oFileName = AdjustPath(GetOutputFilenamePrefix() 
            + GTTSHelper.Substring(toText, 0, MaxTextLength) 
            + " (" + oTranslation + ")" 
            + ".wav");
            
            return Appdata.GetExportPath(InputFileName, FolderSuffix) + oFileName;
        }

        private string AdjustPath(string Input)
        {
            return System.Text.RegularExpressions.Regex.Replace(Input, @"[\\/:*?""<>|]", string.Empty);
        }

        static public void ProceedAllInputPhrazerFiles()
        {
            string[] files = Directory.GetFiles(Appdata.GetCsvPath(), "*.tsv");
            foreach(string file in files) {
                PhrazeGenerator obj = new PhrazeGenerator();
                obj.ProceedCsvFile(file);
            }
        }
        public void ProceedCsvFile(string currentFileName)
        {
            if (!File.Exists(currentFileName))
            {
                Console.WriteLine("File " + currentFileName + " not exists!");
                return;
            }

            InputFileName = currentFileName;
            string[] rows = File.ReadAllLines(InputFileName);

            FillTheTimeCodeList(rows);
            foreach (string csvRow in rows)
            {
                ProceedRow(csvRow);
            }
        }



        public void ProceedRow(string csvRow)
        {
            string[] csvEntries = csvRow.Split('\t');
            if (csvEntries.Length < 2) return; // Skip not well defined rows
            
            // for the first header row in the input tsv file
            if (FromLang == "" && ToLang == "")
            {
                FromLang = csvEntries[0].Trim();
                ToLang = csvEntries[1].Trim();
                return;
            }

            FromText = csvEntries[0].Trim();
            ToText = csvEntries[1].Trim();
            if(FromText.Length < 1 && ToText.Length < 1) return; // if nothing todo dont start the generator. Just skip!
            
            // Project Metadata / Params to set before creating audio
            RowNumber = (csvEntries.Length > 3 && csvEntries[3].Trim().Length > 0) ? GetFormattedRowNumber(csvEntries[3].Trim()) : RowNumber;
            TimeCode = (csvEntries.Length > 4 && csvEntries[4].Trim().Length > 0) ? GetFormattedTimeCode(csvEntries[4].Trim()) : TimeCode;

            if(FolderSuffixAllowed && RowNumber.Length > 0) FolderSuffix = GTTSHelper.GetFolderSuffix(RowNumber);

            // just add a jingle file with timecode
            if(addJingle(FromText)) return;
            if(addJingle(ToText)) return;

            // Wenn kein Jingle und nicht übersetzt - skip it!
            if(FromText.Length < 1 && SkipWithoutTranslation == true) return;

            ProcessTplFile();
        }


        /* TPL READER STUFF */

        public void ProcessTplFile()
        {
            // LOgging for oversized rows
            if (FromText.Length > MaxTextLength || ToText.Length > MaxTextLength)
            {
                File.AppendAllText(Appdata.GetHistoryPath() + "_skipped.csv", FromText + "\t" + ToText + "\n");
                return;
            }
            
            string fileName = GetOutputFilename();
            if (File.Exists(fileName)) {
                Console.WriteLine("! ALREADY EXISTS: " + fileName); return;
            }
            
            Console.WriteLine(">> GENERATE FILE: " + fileName);
            Console.WriteLine("----------- SSML:");

            Generator = new GTTSGenerator();

            string[] rows = File.ReadAllLines(Appdata.GetTplPath(GTTSHelper.GetTemplateName(FromText, ToText, FromLang, ToLang)));
            RowNumberTpl = 0;
            foreach (string row in rows)
            {
                RowNumberTpl++;
                ProcessTplRow(row);
            }
            Generator.ConcatAndSaveWavContents(fileName);
            CreateHistoryRow();
        }



        public void ProcessTplRow(string text)
        {

            // replace TEXTS
            text = text.Replace("__FROMSPEAKER__", GTTSHelper.GetDefaultSpeaker(FromLang));
            text = text.Replace("__FROMTEXT__", GTTSHelper.GetSanitizedText(FromText, "audioengine"));
            text = text.Replace("__TOTEXT__", GTTSHelper.GetSanitizedText(ToText, "audioengine"));
            text = text.Replace("__TOTEXTSLOW__", GTTSHelper.GetTextSlow(GTTSHelper.GetSanitizedText(ToText, "audioengine")));

            // Wartezeit berechnen
            text = text.Replace("__WAITTIMEFROM__", GTTSHelper.GetWaitTime(ToText, true).ToString());
            text = text.Replace("__WAITTIMETO__", GTTSHelper.GetWaitTime(ToText, false).ToString());

            // add some BREAKS
            text = text.Replace(",", GTTSHelper.GetBreakSsmlTag(150));
            text = text.Replace(";", GTTSHelper.GetBreakSsmlTag(200));
            text = text.Replace("... ", GTTSHelper.GetBreakSsmlTag(300) + " ");
            text = text.Replace(". ", GTTSHelper.GetBreakSsmlTag(300) + " ");
            text = text.Replace("! ", GTTSHelper.GetBreakSsmlTag(300) + " ");
            text = text.Replace("? ", GTTSHelper.GetBreakSsmlTag(300) + " ");

            Console.WriteLine(text);

            // Than SPLIT
            string[] csvEntries = text.Split(':');
            if (csvEntries.Length < 2) return;

            CurrentVoice = csvEntries[0].Trim();
            CurrentSsml = csvEntries[1].Trim();

            Generator.SynthesizeSpeechAndAddToBuffer(CurrentVoice, CurrentSsml);
            Thread.Sleep(400);
        }





        public void CreateHistoryRow()
        {
            if(TimeCode.Length > 0) return;
            File.AppendAllText(Appdata.GetHistoryPath() + "_history.csv", FromText + "\t" + ToText + "\n");
        }

        public bool addJingle(string text)
        {

            // Add sound file by name

            if(text.StartsWith("#")) {
                Match match = Regex.Match(text, @"^#([a-zA-Z0-9_.-]+)");
                if(match.Success) {
                    
                    string headingPrefix = match.Value;
                    string soundName = headingPrefix.TrimStart('#').TrimEnd('.');

                    // from henceforce we just create a new jingle file - dont use the Generator
                    string jingleFilename = Appdata.GetSoundPath(soundName, "jingles");
                    if(File.Exists(jingleFilename)) {
                        
                        string oFileName = AdjustPath(GetOutputFilenamePrefix() + soundName + " music playing.wav");
                        string outputFilename = Appdata.GetExportPath(InputFileName, FolderSuffix) + oFileName;
                        Console.WriteLine(">> GENERATE JINGLE: " + outputFilename);

                        System.IO.File.Copy(jingleFilename, outputFilename, true);
                        return true;
                    }
                }
            }
            return false;
        }


        public void FillTheTimeCodeList(string[] rows)
        {
            string timeCode = "";
            foreach (string csvRow in rows)
            {
                string[] csvEntries = csvRow.Split('\t');
                if (csvEntries.Length < 2) return; // Skip not well defined rows
                timeCode = (csvEntries.Length > 4 && csvEntries[4].Trim().Length > 0) ? csvEntries[4].Trim() : "";
                TimeCodeList = TimeCodeList + "_" + timeCode;
            }
        }

        public string GetFormattedWordCount(string text)
        {
            return ("" + GTTSHelper.GetWordsCount(text)).PadLeft(2, '0');
        }

        public string GetFormattedRowNumber(string rowNumber)
        {
            return "i" + rowNumber.PadLeft(3, '0');
        }
        public string GetFormattedTimeCode(string timeCode)
        {
            if(timeCode.Length < 8 && !timeCode.Contains(":")) return timeCode;

            string formattedTime = timeCode;
            formattedTime = timeCode.Replace(":", "");

            if(timeCode.Length > 4 && timeCode.Length < 7) {
                return "l" + formattedTime;
            }

            if(formattedTime.StartsWith("00")) return "l" + formattedTime.Substring(2);
            if(formattedTime.StartsWith("01")) return "ll" + formattedTime.Substring(2);
            if(formattedTime.StartsWith("02")) return "lll" + formattedTime.Substring(2);
            if(formattedTime.StartsWith("03")) return "llll" + formattedTime.Substring(2);
            return timeCode;
        }
    }
}
