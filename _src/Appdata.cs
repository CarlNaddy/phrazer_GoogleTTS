using System;
using System.IO;
using System.Text;

namespace Phrazer
{

    class Appdata
    {
        public Appdata(){
        }

        public static string GetTplFileName(string text)
        {
            string[] siblings = text.Split(" ");
            if(siblings.Length == 1) return "word.tpl";
            if(siblings.Length > 1) return "phrase_short.tpl";
            return "none.tpl";
        }

        public static string GetAppdataPath()
        {
            return Environment.CurrentDirectory + Path.DirectorySeparatorChar + "_appdata" + Path.DirectorySeparatorChar;
        }

        public static string GetCsvPath()
        {
            return GetAppdataPath() + "input" + Path.DirectorySeparatorChar;
            // return GetAppdataPath() + "input" + Path.DirectorySeparatorChar + GetCsvFileName();
        }

        public static string GetTplPath(string text)
        {
            return GetAppdataPath() + "_tpl" + Path.DirectorySeparatorChar + "ttsmp3" + Path.DirectorySeparatorChar + GetTplFileName(text);
        }

        public static string GetExportPath(string currentFileName)
        {
            string path = GetAppdataPath() + "export" + Path.DirectorySeparatorChar + GetProjectName(currentFileName) + Path.DirectorySeparatorChar;
            if(!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        public static string GetProjectName(string currentFileName)
        {
            return Path.GetFileNameWithoutExtension(currentFileName);
        }
    }
}
