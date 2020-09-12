using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;


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
        public int RowNumber { get; set; }
        public string TimeCode { get; set; }
        public string TimeCodeList { get; set; }
        public int MaxTextLength { get; set; }
        public bool SkipWithoutTranslation { get; set; }
        public string FilenamePrefixFormat { get; set; }
        public string FolderSuffix { get; set; }
        public bool FolderSuffixAllowed { get; set; }
        public string CurrentVoice { get; set; }
        public string CurrentSsml { get; set; }
        public GTTSGenerator Generator { get; set; }

        public PhrazeGenerator()
        {
            FromLang = "";
            ToLang = "";
            MaxTextLength = 120;
            RowNumber = 0;
            TimeCode = "";
            TimeCodeList = "";
            FilenamePrefixFormat = "timecode";
            FolderSuffix = "";
            FolderSuffixAllowed = false;
            SkipWithoutTranslation = true;
        }

        public string GetOutputFilenamePrefix()
        {
            string rowNumberStr = ("" + RowNumber).PadLeft(4, '0');
            string wordCount = ("" + GTTSHelper.GetWordsCount(ToText)).PadLeft(2, '0');

            if(FilenamePrefixFormat == "timecode" && TimeCode.Length > 0) return TimeCode + ".";
            if(FilenamePrefixFormat == "sorted" && RowNumber > 0) return rowNumberStr + wordCount + ".";
            
            return wordCount + "."; // buildup is default procedure
        }

        public string GetOutputFilename()
        {
            string fromText = GTTSHelper.GetSanitizedText(FromText, "filename");
            string toText = GTTSHelper.GetSanitizedText(ToText, "filename");

            // Generate a file from the script and save
            string oFileName = "";

            // Special format for Marcel
            if(FromLang == "DE" && ToLang == "DE") {
                oFileName = AdjustPath(fromText + " ..... ..... ..... .... .... .... ..... ..... ..... .... " + toText + ".wav");
                return Appdata.GetExportPath(InputFileName, FolderSuffix) + oFileName;
            }
            
            oFileName = AdjustPath(GetOutputFilenamePrefix() 
            + GTTSHelper.Substring(toText, 0, MaxTextLength) 
            + " (" + GTTSHelper.Substring(fromText, 0, MaxTextLength - ToText.Length) + ")" 
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
            
            if (FromLang == "" && ToLang == "")
            {
                FromLang = csvEntries[0].Trim();
                ToLang = csvEntries[1].Trim();
                return;
            }

            FromText = csvEntries[0].Trim();
            ToText = csvEntries[1].Trim();
            if(FromText.Length < 1 && ToText.Length < 1) return; // if nothing todo dont start the generator. Just skip!
            if(FromText.Length < 1 && SkipWithoutTranslation == true) return; 

            // Project Metadata / Params to set before creating audio
            RowNumber = (csvEntries.Length > 3 && csvEntries[3].Trim().Length > 0) ? int.Parse(csvEntries[3].Trim()) : RowNumber;
            TimeCode = (csvEntries.Length > 4 && csvEntries[4].Trim().Length > 0) ? GetFormattedTimeCode(csvEntries[4].Trim()) : TimeCode;

            if(FolderSuffixAllowed && RowNumber > 0) FolderSuffix = GTTSHelper.GetFolderSuffix(RowNumber);

            ProcessTplFile();
        }


        /* TPL READER STUFF */

        public void ProcessTplFile()
        {
            Generator = new GTTSGenerator();

            string[] rows = File.ReadAllLines(Appdata.GetTplPath(GetTemplateName()));
            RowNumberTpl = 0;
            foreach (string row in rows)
            {
                RowNumberTpl++;
                ProcessTplRow(row);
            }

            string fileName = GetOutputFilename();
            if (File.Exists(fileName)) {
                Console.WriteLine("! ALREADY EXISTS: " + fileName); return;
            }
            Console.WriteLine(">> GENERATE FILE: " + fileName);
            Generator.ConcatAndSaveWavContents(fileName);
        }

        public string addHeadingSoundAndCut(string FromText)
        {
            // Just for Compat with legacy stuff
            if(FromText.StartsWith("### ")) { Generator.JustAddWavSound("h3"); FromText = FromText.Replace("### ", ""); }
            if(FromText.StartsWith("## ")) { Generator.JustAddWavSound("h2"); FromText = FromText.Replace("## ", ""); }
            if(FromText.StartsWith("# ")) { Generator.JustAddWavSound("h1"); FromText = FromText.Replace("# ", ""); }


            // Add sound file by name
            if(FromText.StartsWith("#")) {
                Match match = Regex.Match(FromText, @"^#([a-zA-Z0-9]+)\.");
                if(match.Success) {
                    string headingPrefix = match.Value;
                    string soundName = headingPrefix.TrimStart('#').TrimEnd('.');

                    Generator.JustAddWavSound(soundName); FromText = FromText.Replace(headingPrefix, "").Trim();
                }
            }

            
            return FromText;
        }

        public int GetWaitTime(string text, bool includingThingingTime)
        {
            double thinkingTime = 0;
            double repeatingTime = text.Split(" ").Length * 0.2 + 2;
            if(includingThingingTime)
                thinkingTime = text.Split(" ").Length * 0.15;
                
            return Convert.ToInt32((repeatingTime + thinkingTime) * 1000);
        }

        public void ProcessTplRow(string text)
        {
            // Please check if exists before going to the G-TTS
            if (ToText.Length > MaxTextLength) return;
            if (File.Exists(GetOutputFilename())) return;

            // add some SOUNDS
            FromText = addHeadingSoundAndCut(FromText);

            // replace TEXTS
            text = text.Replace("__FROMSPEAKER__", GTTSHelper.GetDefaultSpeaker(FromLang));
            text = text.Replace("__FROMTEXT__", GTTSHelper.GetSanitizedText(FromText, "audioengine"));
            text = text.Replace("__TOTEXT__", GTTSHelper.GetSanitizedText(ToText, "audioengine"));
            text = text.Replace("__TOTEXTSLOW__", GTTSHelper.GetTextSlow(GTTSHelper.GetSanitizedText(ToText, "audioengine")));

            // Wartezeit berechnen (spaeter extrahieren)
            text = text.Replace("__WAITTIMEFROM__", GetWaitTime(ToText, true).ToString());
            text = text.Replace("__WAITTIMETO__", GetWaitTime(ToText, false).ToString());

            // add some BREAKS
            text = text.Replace(",", GTTSHelper.GetBreakSsmlTag("150ms"));
            text = text.Replace(";", GTTSHelper.GetBreakSsmlTag("200ms"));
            text = text.Replace("...", GTTSHelper.GetBreakSsmlTag("250ms"));
            text = text.Replace(".", GTTSHelper.GetBreakSsmlTag("300ms"));

            // Than SPLIT
            string[] csvEntries = text.Split(':');
            if (csvEntries.Length < 2) return;

            CurrentVoice = csvEntries[0].Trim();
            CurrentSsml = csvEntries[1].Trim();

            Generator.SynthesizeSSML(CurrentVoice, CurrentSsml);
        }



        public string GetTemplateName()
        {
            if(FromLang == "DE" && ToLang == "DE") {
                return "de_de.tpl";
            }

            // if no TEXT_FROM, then make just text
            if(FromText.Length == 0) {
                return "text_2.tpl";
            }

            // else just a usual audioflashcard
            return "phrase_3.tpl";
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

        public string GetFormattedTimeCode(string timeCode)
        {
            if(timeCode.Length < 8 && !timeCode.Contains(":")) return timeCode;

            string formattedTime = timeCode;
            formattedTime = timeCode.Replace(":", "");

            if(timeCode.EndsWith("a")) {
                if(!TimeCodeList.Contains("_" + timeCode.Replace("a", "") + "b")) formattedTime = formattedTime.Replace("a", "");
            }

            if(formattedTime.StartsWith("00")) return formattedTime.Substring(2);
            if(formattedTime.StartsWith("01")) return "I" + formattedTime.Substring(2);
            if(formattedTime.StartsWith("02")) return "II" + formattedTime.Substring(2);
            if(formattedTime.StartsWith("03")) return "III" + formattedTime.Substring(2);
            return timeCode;
        }
    }
}
