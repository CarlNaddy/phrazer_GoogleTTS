using System;
using System.IO;
using System.Collections.Generic;


namespace Phrazer
{
    class VTTConverter
    {

        public static string GetAppdataPath()
        {
            return Environment.CurrentDirectory + Path.DirectorySeparatorChar + "_appdata" + Path.DirectorySeparatorChar;
        }

        public static string GetInputPath()
        {
            return GTTSAppdata.GetAppdataPath() + "_extern" + Path.DirectorySeparatorChar + "inputVTT" + Path.DirectorySeparatorChar;
        }

        public static string GetOutputAbsoluteFilename(string currentFileName)
        {
            string path = GetAppdataPath() + "_extern" + Path.DirectorySeparatorChar + "outputTSV" + Path.DirectorySeparatorChar;
            if(!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
            return path + Path.GetFileNameWithoutExtension(currentFileName) + ".tsv";
        }
        static public void convertToTSV()
        {
            //string[] files = Directory.GetFiles(GetInputPath(), "*.vtt");
            string[] files = Directory.GetFiles(GetInputPath(), "*.vtt.txt");
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

            string text = "";
            string time = "";
            foreach (string row in rows)
            {
                // new row
                if(GetOutputTime(row).Length == 5) {
                    tsvRows.Add("" + "\t" + text + "\t" + "count" + "\t" + "vtt" + "\t" + time);
                    text = "";
                    time = GetOutputTime(row);
                    continue;
                }
                text = text + " " +  row;
            }
            File.WriteAllLines(GetOutputAbsoluteFilename(currentFileName), tsvRows.ToArray());
        }

        public void ProcessVTTRow(string text)
        {


        }


        public static string GetOutputTime(string text)
        {
            text = text.Trim();
            string muster = "03:00.242 --> 03:03.002";
            if(text.Contains(" --> ") && text.Length == muster.Length) {
                return text.Substring(0, 5);
            }
            return "";
        }





        








    }
}
