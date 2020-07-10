using System;
using System.IO;
using System.Text;
using System.Collections.Generic;


namespace Phrazer
{
    class VTTConverter
    {
        /* Try this, if you get well scripted subscribtion.
        
        0: Cut on new Timecode and end of sentense only.
        1: Cut on end of sentense and comma, if length greater MaxSentenseLength
        
         */
        
        public static int StrictModeLevel()
        {
            return 1;
        }

        public static int MaxSentenseLength()
        {
            return 60;
        }

        public static string GetOutputTime(string text)
        {
            text = text.Trim();
            string muster = "03:00.242 --> 03:03.002";
            if(text.Contains(" --> ") && text.Length == muster.Length) {
                return text.Substring(0, 5);
            }

            string muster2 = "00:00:20.729 --> 00:00:22.731";
            if(text.Contains(" --> ") && text.Length == muster2.Length) {
                return text.Substring(0, 8);
            }

            return "";
        }

        public static string GetInputPath()
        {
            return GTTSAppdata.GetAppdataPath() + "_extern" + Path.DirectorySeparatorChar + "inputVTT" + Path.DirectorySeparatorChar;
        }

        public static string GetOutputAbsoluteFilename(string currentFileName)
        {
            string path = GTTSAppdata.GetAppdataPath() + "_extern" + Path.DirectorySeparatorChar + "outputTSV" + Path.DirectorySeparatorChar;
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
            List<string> tsvRows = new List<string>();

            // Add header
            tsvRows.Add("DE" + "\t" + "EN" + "\t" + "LENGTH" + "\t" + "TAGS" + "\t" + "TIME");

            string textBuffer = "";
            string time = "";
            foreach (string row in rows)
            {

                if(ShouldCreateNewRow(textBuffer, row))
                {
                    SaveTextBufferToList(tsvRows, ref textBuffer, ref time);
                }


                // new Block determined
                if(GetOutputTime(row).Length > 0) {
                    // Save Buffer
                    if(StrictModeLevel() == 0) SaveTextBufferToList(tsvRows, ref textBuffer, ref time);

                    // Prepare for next block
                    if(StrictModeLevel() == 0) textBuffer = "";
                    
                    time = GetOutputTime(row);
                    continue;
                }

                textBuffer = (textBuffer + " " + SanitizeText(row)).Trim();



                if(textBuffer.Contains("going home.")) {
                    Console.WriteLine("********************BUFFER: " + textBuffer);
                    Console.WriteLine("********************time: " + time);
                }

            }
            SaveTextBufferToList(tsvRows, ref textBuffer, ref time); // flush last buffer before save
            File.WriteAllLines(GetOutputAbsoluteFilename(currentFileName), tsvRows.ToArray(), Encoding.UTF8);
        }

        public void SaveTextBufferToList(List<string> list, ref string textBuffer, ref string time)
        {
            if(textBuffer.Length > 0 && time.Length > 0) {
                list.Add("" + "\t" + textBuffer + "\t" + WordCount(textBuffer) + "\t" + "vtt" + "\t" + time);
                textBuffer = "";
            }
        }

        public bool ShouldCreateNewRow(string textBuffer, string nextValue)
        {

            if(
                textBuffer.StartsWith("- ") && nextValue.StartsWith("- ") // recognize dialogue
                || textBuffer.EndsWith(".")
                || textBuffer.EndsWith("?")
                || textBuffer.EndsWith("!")
                || textBuffer.EndsWith(")")
                || textBuffer.EndsWith(",") && (StrictModeLevel() == 1) 
                && (textBuffer.Length > MaxSentenseLength()/2 && nextValue.Length > MaxSentenseLength()/3)
            ) return true;

            return false;
            
        }

        static string SanitizeText(string text)
        {
            text = text.Trim();
            text = text.Replace(":", ","); // Wichtiges Trennzeichen
            text = text.Replace("\t", "");
            text = text.Replace("\"", "");
            text = text.Replace("<i>", "");
            text = text.Replace("</i>", "");
            text = text.Replace(" !", "!");
            text = text.Replace(" ?", "?");
            text = text.Replace(" !", "!");
            text = text.Replace(" ?", "?");
            text = text.Replace("’", "'");
            return text;
        }

        static string WordCount(string text)
        {
            return ("" + GTTSHelper.GetWordsCount(text)).PadLeft(2, '0');
        }
    }
}
