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
        public string CurrentVoice { get; set; }
        public string CurrentSsml { get; set; }
        public GTTSGenerator Generator { get; set; }

        public CSVReader()
        {
        }

        public string GetOutputFilename()
        {
            // Generate a file from the script and save
            string OutputSortOrderStr = ("" + (RowNumberCsv - 1)).PadLeft(3, '0');
            string OFileName = AdjustPath(OutputSortOrderStr + "." + FromLang + "-" + ToLang + "." + FromText + "-" + ToText + "." + "wav");
            return GTTSAppdata.GetExportPath(InputFileName) + OFileName;
        }

        private string AdjustPath(string Input)
        {
            return System.Text.RegularExpressions.Regex.Replace(Input, @"[\\/:*?""<>|]", string.Empty);
        }

        public void ProceedRow(string csvRow)
        {
            string[] csvEntries = csvRow.Trim().Split('\t');

            // File.WriteAllText(GetOutputFilename() + ".txt", csvRow.Trim());
            // return;

            if (csvEntries.Length < 2) return; // Skip not well defined row /placeholder

            if (RowNumberCsv == 1)
            {
                FromLang = csvEntries[0];
                ToLang = csvEntries[1];
                return;
            }

            FromText = csvEntries[0];
            ToText = csvEntries[1];

            ProcessTplFile();
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
        }

        public void ProcessTplRow(string text)
        {
            // First REPLACE
            text = text.Replace("__FROMSPEAKER__", GTTSHelper.GetDefaultSpeaker(FromLang));
            text = text.Replace("__FROMTEXT__", FromText);
            text = text.Replace("__TOTEXT__", ToText);
            text = text.Replace("__TOTEXTSLOW__", GTTSHelper.GetTextSlow(ToText));

            text = text.Replace(",", GTTSHelper.GetBreakSsmlTag("250ms"));
            text = text.Replace(".", GTTSHelper.GetBreakSsmlTag("750ms"));

            Console.WriteLine("ROW: " + ("" + RowNumberTpl).PadLeft(3, '0') + ": " + text);

            // Than SPLIT
            string[] csvEntries = text.Split(':');
            if (csvEntries.Length < 2) return;

            CurrentVoice = csvEntries[0].Trim();
            CurrentSsml = csvEntries[1].Trim();

            // Please call before going to the G-TTS
            if (File.Exists(GetOutputFilename())) return;

            Generator.SynthesizeSSML(CurrentVoice, CurrentSsml);
        }
    }
}
