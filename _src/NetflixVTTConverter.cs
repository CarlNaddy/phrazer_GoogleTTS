using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;


namespace Phrazer
{
    class NetflixVTTConverter
    {
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
                NetflixVTTConverter obj = new NetflixVTTConverter();
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
                if(row.Trim() == "") continue;
                if(GetOutputTime(row).Length > 0) {
                        time = GetOutputTime(row);
                        continue;
                }
                if(time == "") continue;

                string rowText = SanitizeText(row);

                foreach(string word in rowText.Split(" "))
                {
                    if(ShouldCreateNewRow(textBuffer))
                    {
                        SaveTextBufferToList(tsvRows, ref textBuffer, ref time);
                    }
                    textBuffer = (textBuffer + " " + word).Trim();
                }
            }
            SaveTextBufferToList(tsvRows, ref textBuffer, ref time); // flush last buffer before save
            File.WriteAllLines(GetOutputAbsoluteFilename(currentFileName), tsvRows, Encoding.UTF8);
        }

        public void SaveTextBufferToList(List<string> list, ref string textBuffer, ref string time)
        {
            if(textBuffer.Length > 0 && time.Length > 0) {
                list.Add("" + "\t" + textBuffer + "\t" + WordCount(textBuffer) + "\t" + "vtt" + "\t" + time);
                textBuffer = "";
            }
        }

        public bool ShouldCreateNewRow(string textBuffer)
        {
            if(
                NextRowSignDetected(textBuffer)
                && !textBuffer.EndsWith("Mr.")
                && !textBuffer.EndsWith("Mrs.")
                && textBuffer.Length > 2
            ) return true;

            return false;
        }

        public bool NextRowSignDetected(string textBuffer)
        {
            if(
                textBuffer.EndsWith(".")
                || textBuffer.EndsWith("?")
                || textBuffer.EndsWith("!")
                || textBuffer.EndsWith(")")
                || textBuffer.EndsWith("]")
                || textBuffer.EndsWith("--")
                || textBuffer.Contains("NETFLIX")
            ) return true;

            return false;   
        }

        static string SanitizeText(string text)
        {
            text = Regex.Replace(text, @"\s+", " ");

            text = RemoveBracketsText(text, '[', ']');
            text = RemoveBracketsText(text, '(', ')');

            text = text.Trim();
            //text = text.Replace(":", ","); // Wichtiges Trennzeichen
            text = text.Replace("\t", "");
            text = text.Replace("\"", "");
            text = text.Replace("<i>", "");
            text = text.Replace("</i>", "");
            text = text.Replace(" !", "!");
            text = text.Replace(" ?", "?");
            text = text.Replace(" !", "!");
            text = text.Replace(" ?", "?");
            text = text.Replace("’", "'");
            text = text.TrimStart('-');
            text = text.Trim();
            return text;
        }

        static string WordCount(string text)
        {
            return ("" + GTTSHelper.GetWordsCount(text)).PadLeft(2, '0');
        }

        static string RemoveBracketsText(string text, char startsWith, char endsWith)
        {
            int firstBracket = text.IndexOf(startsWith);
            int lastBracket = text.LastIndexOf(endsWith);

            if(firstBracket == -1) return text;
            if(lastBracket == -1) return text;

            int diff = lastBracket - firstBracket + 1;
            return text.Remove(firstBracket, diff);
        }
    }
}
