using System;
using System.IO;
using System.Collections.Generic;


namespace Phrazer
{
    class PhrazeGenerator
    {
        public string InputFileName { get; set; }
        public int RowNumberCsv { get; set; }
        public int RowNumberTpl { get; set; }
        public string FromLang { get; set; }
        public string ToLang { get; set; }
        public string FromText { get; set; }
        public string ToText { get; set; }
        public int MaxTextLength { get; set; }
        public string ProjectType { get; set; }
        public string CurrentVoice { get; set; }
        public string CurrentSsml { get; set; }
        public GTTSGenerator Generator { get; set; }

        public PhrazeGenerator()
        {
            MaxTextLength = 140;
            ProjectType = "";
        }

        public List<string> GetAllowedProjectTypes()
        {
            List<string> allowedProjectTypes = new List<string>();

            allowedProjectTypes.Add("buildup");
            allowedProjectTypes.Add("sorted");
            allowedProjectTypes.Add("dialogue");
            allowedProjectTypes.Add("text");
            
            return allowedProjectTypes;
        }

        public string GetOutputFilenamePrefix()
        {
            string wordsCountStr = ("" + GTTSHelper.GetWordsCount(ToText)).PadLeft(2, '0');
            string rowNumberStr = ("" + (RowNumberCsv - 1)).PadLeft(3, '0');

            if (ProjectType == "sorted" || ProjectType == "dialogue")
            {
                return rowNumberStr;
            }

            // buildup is default procedure
            return wordsCountStr;
        }



        public string GetOutputFilename()
        {
            string fromText = GTTSHelper.GetSanitizedText(FromText, "filename");
            string toText = GTTSHelper.GetSanitizedText(ToText, "filename");

            // Generate a file from the script and save
            string OFileName = AdjustPath(GetOutputFilenamePrefix() + "." + GTTSHelper.Substring(toText, 0, MaxTextLength) + " (" + GTTSHelper.Substring(fromText, 0, MaxTextLength - toText.Length) + ")." + "wav");
            Console.WriteLine(">>> Filename: " + OFileName); 
            Console.WriteLine("--> Filename Length  : " + OFileName.Length); 
            return GTTSAppdata.GetExportPath(InputFileName) + OFileName;
        }

        private string AdjustPath(string Input)
        {
            return System.Text.RegularExpressions.Regex.Replace(Input, @"[\\/:*?""<>|]", string.Empty);
        }

        private string FormatProjectType(string text)
        {
            int maxLength = 20;
            text = text.Trim().ToLower();
            if (text.Length > maxLength) text = text.Substring(0, maxLength);
            return text;
        }

        static public void ProceedAllInputPhrazerFiles()
        {
            string[] files = Directory.GetFiles(GTTSAppdata.GetCsvPath(), "*.tsv");
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
            RowNumberCsv = 0;
            foreach (string csvString in rows)
            {
                RowNumberCsv++;
                ProceedRow(csvString);
            }
        }



        public void ProceedRow(string csvRow)
        {
            string[] csvEntries = csvRow.Split('\t');

            if (csvEntries.Length < 2) return; // Skip not well defined rows

            // Project Metadata / Params to set before creating audio
            ProjectType = (csvEntries.Length > 2 && csvEntries[2].Trim().Length > 0) ? FormatProjectType(csvEntries[2]) : ProjectType;

            if (RowNumberCsv == 1)
            {
                FromLang = csvEntries[0].Trim();
                ToLang = csvEntries[1].Trim();
                return;
            }

            FromText = csvEntries[0].Trim();
            ToText = csvEntries[1].Trim();

            // if nothing todo dont start the generator. Just skip!
            if(FromText.Length < 1 && ToText.Length < 1) return;

            ProcessTplFile();
        }


        /* TPL READER STUFF */

        public void ProcessTplFile()
        {
            Generator = new GTTSGenerator();

            string[] rows = File.ReadAllLines(GTTSAppdata.GetTplPath(GetTemplateName()));
            RowNumberTpl = 0;
            foreach (string row in rows)
            {
                RowNumberTpl++;
                ProcessTplRow(row);
            }

            if (File.Exists(GetOutputFilename())) return;

            Generator.ConcatAndSaveWavContents(GetOutputFilename());
        }

        public string addHeadingSoundAndCut(string text, string heading)
        {
            if(text.Contains(heading)) {
                if(heading == "###") Generator.JustAddWavSound("h3");
                if(heading == "##") Generator.JustAddWavSound("h2");
                if(heading == "#") Generator.JustAddWavSound("h1");
                return text.Replace(heading, "");
            }
            return text;
        }

        public int GetWaitTime(string text, bool includingThingingTime)
        {
            double thinkingTime = 0;
            double repeatingTime = text.Split(" ").Length * 0.25 + 2;
            if(includingThingingTime)
                thinkingTime = text.Split(" ").Length * 0.15;
                
            return Convert.ToInt32((repeatingTime + thinkingTime) * 1000);
        }

        public void ProcessTplRow(string text)
        {
            // Please check if exists before going to the G-TTS
            if (ToText.Length > MaxTextLength) return;
            if (File.Exists(GetOutputFilename())) return;

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
            text = text.Replace(".", GTTSHelper.GetBreakSsmlTag("300ms"));

            // add some SOUNDS
            text = addHeadingSoundAndCut(text, "###");
            text = addHeadingSoundAndCut(text, "##");
            text = addHeadingSoundAndCut(text, "#");

            //Console.WriteLine("ROW: " + ("" + RowNumberTpl).PadLeft(3, '0') + ": " + text);

            // Than SPLIT
            string[] csvEntries = text.Split(':');
            if (csvEntries.Length < 2) return;

            CurrentVoice = csvEntries[0].Trim();
            CurrentSsml = csvEntries[1].Trim();

            Generator.SynthesizeSSML(CurrentVoice, CurrentSsml);
        }



        public string GetTemplateName()
        {
            // Dialogue / conversation is a special format.
            if(ProjectType == "dialogue") {
                if(RowNumberCsv % 2 == 0) {
                    return "dialogue_even.tpl";
                } else {
                    return "dialogue_odd.tpl";
                }
            }

            // if no TEXT_FROM, then make just text
            if(FromText.Length == 0) {
                return "text_2.tpl";
            }

            // else just a usual audioflashcard
            return "phrase_3.tpl";
        }
    }
}
