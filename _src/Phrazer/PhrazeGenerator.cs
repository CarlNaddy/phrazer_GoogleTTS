using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading;
// for calling lame converter
using System.Diagnostics;


namespace Phrazer
{
    class PhrazeGenerator
    {
        public string InputFileName { get; set; }
        public int RowNumberTpl { get; set; }
        public string LangFrom { get; set; }
        public string LangTo { get; set; }
        public int LangFromIndex { get; set; }
        public int LangToIndex { get; set; }
        public string Gender { get; set; }
        public int GenderIndex { get; set; }
        public string TextFrom { get; set; }
        public string TextTo { get; set; }
        public string RowNumber { get; set; }
        public int RowNumberIndex { get; set; }
        public string TimeCode { get; set; }
        public int TimeCodeIndex { get; set; }
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
            // @todo: It might be better to get Lang values from file name
            // UK_DE_xxxxxxxxxxxxxx.tsv
            LangFrom = "UK";
            LangTo = "DE";
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
            return GetFormattedWordCount(TextTo) + ". ";
        }

        public string GetOutputFilename(string exportSubfolder, string extension)
        {
            string fromText = GTTSHelper.GetSanitizedText(TextFrom, "filename");
            string toText = GTTSHelper.GetSanitizedText(TextTo, "filename");

            // Generate a file from the script and save
            string oFileName;
            string oTranslation;

            oTranslation = GTTSHelper.Substring(fromText, 0, MaxTextLength - TextTo.Length -3);
            if(oTranslation.Length < fromText.Length) oTranslation = oTranslation + "...";
            
            oFileName = AdjustPath(GetOutputFilenamePrefix() 
            + GTTSHelper.Substring(toText, 0, MaxTextLength) 
            + " (" + oTranslation + ")" 
            + "." + extension);
            
            return Appdata.GetExportPath(InputFileName, FolderSuffix, exportSubfolder) + oFileName;
        }

        private string AdjustPath(string Input)
        {
            return System.Text.RegularExpressions.Regex.Replace(Input, @"[\\/:*?""<>|]", string.Empty);
        }

        static public void ProceedAllInputPhrazerFiles()
        {
            string[] files = Directory.GetFiles(Appdata.GetInputPath(), "*.tsv");
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
            // for the first header row in the input tsv file
            if (LangFrom == "" || LangTo == "")
            {
                Console.WriteLine("LangFrom or LangTo NOT SET. Abort...");
                return;
            }

            InputFileName = currentFileName;
            string[] rows = File.ReadAllLines(InputFileName);

            FillTheTimeCodeList(rows);
            int index = 0;

            foreach (string csvRow in rows)
            {
                if(index == 0) {
                    string[] csvEntries = csvRow.Split('\t');
                    LangFromIndex = Array.IndexOf(csvEntries, LangFrom);
                    LangToIndex = Array.IndexOf(csvEntries, LangTo);
                    GenderIndex = Array.IndexOf(csvEntries, "GENDER");
                    RowNumberIndex = Array.IndexOf(csvEntries, "ROW");
                    TimeCodeIndex = Array.IndexOf(csvEntries, "TIME");
                    index++;
                    continue;
                }
                if (LangFromIndex == -1 || LangToIndex == -1){
                    Console.WriteLine("Please check your LangFromIndex & LangToIndex settings. Abort...");
                    return;
                }

                ProceedRow(csvRow, index);
                index++;
            }
        }



        public void ProceedRow(string csvRow, int index)
        {
            string[] csvEntries = csvRow.Split('\t');
            if (csvEntries.Length < 2) return; // Skip not well defined rows

            TextFrom = csvEntries[LangFromIndex].Trim();
            TextTo = csvEntries[LangToIndex].Trim();
            Gender = csvEntries[GenderIndex].Trim();
            RowNumber = (csvEntries.Length > 3 && csvEntries[RowNumberIndex].Trim().Length > 0) ? GetFormattedRowNumber(csvEntries[RowNumberIndex].Trim()) : "";
            TimeCode = (csvEntries.Length > 4 && csvEntries[TimeCodeIndex].Trim().Length > 0) ? GetFormattedTimeCode(csvEntries[TimeCodeIndex].Trim()) : "";

            if(FolderSuffixAllowed && RowNumber.Length > 0) FolderSuffix = GTTSHelper.GetFolderSuffix(RowNumber);

            // just add a jingle file with timecode
            if(addJingle(TextFrom)) return;
            if(addJingle(TextTo)) return;

            // Wenn kein Jingle und nicht übersetzt - skip it!
            if(TextFrom.Length < 1 && SkipWithoutTranslation == true) return;

            ProcessTplFile();
        }


        /* TPL READER STUFF */

        public void ProcessTplFile()
        {
            // LOgging for oversized rows
            if (TextFrom.Length > MaxTextLength || TextTo.Length > MaxTextLength)
            {
                File.AppendAllText(Appdata.GetHistoryPath() + "_skipped.csv", TextFrom + "\t" + TextTo + "\n");
                return;
            }
            
            string wavFileName = GetOutputFilename("__wav__", "wav");
            if (File.Exists(wavFileName)) {
                Console.WriteLine("! ALREADY EXISTS: " + wavFileName); return;
            }
            
            Console.WriteLine(">> GENERATE FILE: " + wavFileName);
            Console.WriteLine("----------- SSML:");

            Generator = new GTTSGenerator();

            string[] rows = File.ReadAllLines(GTTSHelper.GetTplPath(LangFrom, LangTo) + GTTSHelper.GetTplName(TextFrom, TextTo, Gender));
            RowNumberTpl = 0;
            foreach (string row in rows)
            {
                RowNumberTpl++;
                ProcessTplRow(row);
            }
            Generator.ConcatAndSaveWavContents(wavFileName);

            // Convert to mp3
            if (!File.Exists("lame.exe"))
            {
                Console.WriteLine("Please add _lame.exe to this folder in order to generate MP3 files!");
                Console.WriteLine("Current folder: " + Environment.CurrentDirectory);
            } else {
                string mp3FileName = GetOutputFilename("", "mp3");
                Process.Start(@"lame.exe", @"-V2 " + "\"" + wavFileName + "\"" + " " + "\"" + mp3FileName + "\"");
            }

            CreateHistoryRow();
        }



        public void ProcessTplRow(string text)
        {

            // replace TEXTS
            //text = text.Replace("__FROMSPEAKER__", GTTSHelper.GetDefaultSpeaker(LangFrom));
            text = text.Replace("__FROMTEXT__", GTTSHelper.GetSanitizedText(TextFrom, "audioengine"));
            text = text.Replace("__TOTEXT__", GTTSHelper.GetSanitizedText(TextTo, "audioengine"));
            text = text.Replace("__TOTEXTSLOW__", GTTSHelper.GetTextSlow(GTTSHelper.GetSanitizedText(TextTo, "audioengine")));

            // Wartezeit berechnen
            text = text.Replace("__WAITTIMEFROM__", GTTSHelper.GetWaitTime(TextTo, true).ToString());
            text = text.Replace("__WAITTIMETO__", GTTSHelper.GetWaitTime(TextTo, false).ToString());

            // add some BREAKS
            text = text.Replace(",", GTTSHelper.GetBreakSsmlTag(200));
            text = text.Replace(";", GTTSHelper.GetBreakSsmlTag(200));
            text = text.Replace("... ", GTTSHelper.GetBreakSsmlTag(400) + " ");
            text = text.Replace(". ", GTTSHelper.GetBreakSsmlTag(400) + " ");
            text = text.Replace("! ", GTTSHelper.GetBreakSsmlTag(400) + " ");
            text = text.Replace("? ", GTTSHelper.GetBreakSsmlTag(400) + " ");

            Console.WriteLine(text);

            // Than SPLIT
            string[] csvValues = text.Split(':');
            if (csvValues.Length < 2) return;

            CurrentVoice = csvValues[0].Trim();
            CurrentSsml = csvValues[1].Trim();

            Generator.SynthesizeSpeechAndAddToBuffer(CurrentVoice, CurrentSsml);
            Thread.Sleep(400);
        }





        public void CreateHistoryRow()
        {
            int len = TextFrom.Length + TextTo.Length;
            string tpl = LangFrom + "_" + LangTo + "\\" + GTTSHelper.GetTplName(TextFrom, TextTo, Gender);

            string file = Appdata.GetHistoryPath() + "_history.csv";
            if(!File.Exists(file)){
                File.AppendAllText(file, 
                    "Month" + "\t" 
                    + "Date" + "\t" 
                    + "TextFrom" + "\t" 
                    + "TextTo" + "\t" 
                    + "Len"  + "\t"  
                    + "Tpl" + "\n"
                );
            }

            File.AppendAllText(file, 
                DateTime.Now.ToString("yyyy-MM") + "\t" 
                    + DateTime.Now.ToString("dd-MM-yyyy") + "\t" 
                    + TextFrom + "\t" 
                    + TextTo + "\t" 
                    + len  + "\t"  
                    + tpl + "\n"
            );
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
                    string jingleFilename = Appdata.GetSoundPath(soundName, "jingles", "mp3");
                    if(File.Exists(jingleFilename)) {
                        
                        string oFileName = AdjustPath(GetOutputFilenamePrefix() + soundName + " playing.mp3");
                        string outputFilename = Appdata.GetExportPath(InputFileName, FolderSuffix, "") + oFileName;
                        Console.WriteLine(">> GENERATE JINGLE: " + outputFilename);

                        File.Copy(jingleFilename, outputFilename, true);
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
                timeCode = (TimeCodeIndex > 0 && csvEntries[TimeCodeIndex].Trim().Length > 0) ? csvEntries[TimeCodeIndex].Trim() : "";
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
