using System;
using System.IO;


namespace Phrazer
{
    class CSVReader
    {  
        //public string GeneratorEngine { get; set; }
        
        public string InputFileName { get; set; }
        public string OutputFileExt { get; set; }
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
            OutputFileExt = "wav";
        }

        public string GetOutputFilename()
        {
            // Generate a file from the script and save
            string OutputSortOrderStr = ("0" + RowNumberCsv).PadLeft(3, '0') +"_"+ ("0" + RowNumberTpl).PadLeft(3, '0');
            return Appdata.GetExportPath(InputFileName) + OutputSortOrderStr + "." + FromLang + "-" + ToLang + "." + FromText + "-" + ToText + "." + OutputFileExt;
        }

        public void ProceedRow(string csvRow)
        {
            string[] csvEntries = csvRow.Split('\t');

            if(RowNumberCsv == 1) {
                FromLang = csvEntries[0];
                ToLang = csvEntries[1];
                return;
            }

            FromText = csvEntries[0];
            ToText   = csvEntries[1];

            ProcessTplFile();
        }

        public void ProceedCsvFile(string currentFileName)
        {
            if(!File.Exists(currentFileName)) {
                Console.WriteLine("File " + currentFileName + " not exists!");
                return;
            }

            InputFileName = currentFileName;
            string[] rows = File.ReadAllLines(InputFileName);
            RowNumberCsv = 0;
            foreach(string csvString in rows) {
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
            foreach(string row in rows) {
                RowNumberTpl++;
                ProcessTplRow(row);
            }

            
        }

        public void ProcessTplRow(string text)
        {
            // First REPLACE
            text = text.Replace("__FROMSPEAKER__", GTTSHelper.GetDefaultSpeaker(FromLang));
            text = text.Replace("__FROMTEXT__", FromText);
            text = text.Replace("__TOTEXT__", ToText);
            text = text.Replace("__TOTEXTSLOW__", GTTSHelper.GetTextSlow(ToText));

            //Console.WriteLine("Proceed ROW: " + text);

            // Than SPLIT
            string[] csvEntries = text.Split(':');
            if(csvEntries.Length < 2) return;

            CurrentVoice = csvEntries[0].Trim();
            CurrentSsml   = csvEntries[1].Trim();

            //Generator.SynthesizeSSML(CurrentVoice, CurrentSsml); return;

            Generator.SaveToFile(GetOutputFilename(), Generator.SynthesizeSSML(CurrentVoice, CurrentSsml));

            //Audioproc.WaveToMP3(GetOutputFilename(), GetOutputFilename().Replace(".wav", ".mp3"));
        }
    }
}
