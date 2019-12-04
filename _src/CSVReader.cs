using System;
using System.IO;


namespace Phrazer
{
    class CSVReader
    {
        public string InputFileName { get; set; }
        public int RowNumberCsv { get; set; }
        public int RowNumberTpl { get; set; }
        public string FromLang { get; set; }
        public string ToLang { get; set; }
        public string FromText { get; set; }
        public string ToText { get; set; }
        public string ProjectType { get; set; }
        public string ProjectName { get; set; }
        public string CurrentVoice { get; set; }
        public string CurrentSsml { get; set; }
        public GTTSGenerator Generator { get; set; }

        public CSVReader()
        {
            ProjectType = "";
            ProjectName = "";
        }

        public string GetOutputFilenamePrefix()
        {
            string wordsCountStr = ("" + GTTSHelper.GetWordsCount(ToText)).PadLeft(2, '0');
            string rowNumberStr = ("" + (RowNumberCsv - 1)).PadLeft(3, '0');

            if (ProjectType == "song")
            {
                return FromLang + "_" + ToLang + "." + ProjectName + "_" + rowNumberStr;
            }

            return FromLang + "_" + ToLang + "." + wordsCountStr;
        }

        public string GetOutputFilename()
        {
            // Generate a file from the script and save
            string OFileName = AdjustPath(GetOutputFilenamePrefix() + "." + FromText + "-" + ToText + "." + "wav");
            return GTTSAppdata.GetExportPath(InputFileName) + OFileName;
        }

        private string AdjustPath(string Input)
        {
            return System.Text.RegularExpressions.Regex.Replace(Input, @"[\\/:*?""<>|]", string.Empty);
        }

        private string FormatProjectName(string text)
        {
            int maxProjectNameLength = 20;
            text = text.Trim().ToLower();
            if (text.Length > maxProjectNameLength) text = text.Substring(0, maxProjectNameLength);
            return text;
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
            string[] csvEntries = csvRow.Trim().Split('\t');

            if (csvEntries.Length < 2) return; // Skip not well defined rows

            // Project Metadata / Params to set before creating audio
            ProjectType = (csvEntries.Length > 2) ? FormatProjectName(csvEntries[2]) : ProjectType;
            ProjectName = (csvEntries.Length > 3) ? FormatProjectName(csvEntries[3]) : ProjectName;

            if (RowNumberCsv == 1)
            {
                FromLang = csvEntries[0].Trim();
                ToLang = csvEntries[1].Trim();
                return;
            }

            FromText = csvEntries[0].Trim();
            ToText = csvEntries[1].Trim();

            ProcessTplFile();
        }


        /* TPL READER STUFF */

        public void ProcessTplFile()
        {
            Generator = new GTTSGenerator();
            string[] rows = File.ReadAllLines(GTTSAppdata.GetTplPath(ToText));
            RowNumberTpl = 0;
            foreach (string row in rows)
            {
                RowNumberTpl++;
                ProcessTplRow(row);
            }

            Generator.ConcatenateAndSaveWavContents(GetOutputFilename());
            
            // Download the file GetOutputFilename()
        }

        public void ProcessTplRow(string text)
        {
            // First REPLACE
            text = text.Replace("__FROMSPEAKER__", GTTSHelper.GetDefaultSpeaker(FromLang));
            text = text.Replace("__FROMTEXT__", FromText);
            text = text.Replace("__TOTEXT__", ToText);
            text = text.Replace("__TOTEXTSLOW__", GTTSHelper.GetTextSlow(ToText));

            text = text.Replace(",", GTTSHelper.GetBreakSsmlTag("400ms"));
            text = text.Replace(".", GTTSHelper.GetBreakSsmlTag("1s"));

            Console.WriteLine("ROW: " + ("" + RowNumberTpl).PadLeft(3, '0') + ": " + text);

            // Than SPLIT
            string[] csvEntries = text.Split(':');
            if (csvEntries.Length < 2) return;

            CurrentVoice = csvEntries[0].Trim();
            CurrentSsml = csvEntries[1].Trim();

            // Please check if exists before going to the G-TTS
            if (File.Exists(GetOutputFilename())) return;

            Generator.SynthesizeSSML(CurrentVoice, CurrentSsml);
        }
    }
}
