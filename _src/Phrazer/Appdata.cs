using System;
using System.IO;
using System.Text;

namespace Phrazer
{

    class Appdata
    {
        public Appdata(){
        }
        public static string GetAppdataPath()
        {
            return Environment.CurrentDirectory + Path.DirectorySeparatorChar + "_appdata" + Path.DirectorySeparatorChar;
        }

        public static string GetCsvPath()
        {
            return GetAppdataPath() + "input" + Path.DirectorySeparatorChar;
        }

        public static string GetSoundPath(string sound)
        {
            // 24000 Khz Mono Wav only
            string package = "piano_mono";
            return GetAppdataPath() + "_sounds" + Path.DirectorySeparatorChar + package + Path.DirectorySeparatorChar + sound + ".wav";
        }

        public static string GetTplPath(string templateName)
        {
            return GetAppdataPath() + "_tpl" + Path.DirectorySeparatorChar + "GTTS" + Path.DirectorySeparatorChar + templateName;
        }

        public static string GetHistoryPath()
        {
            string path = GetAppdataPath() + "_history" + Path.DirectorySeparatorChar;
            if(!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        public static string GetExportPath(string currentFileName, string folderSuffix)
        {
            string path = GetAppdataPath() + "export" + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(currentFileName) + folderSuffix + Path.DirectorySeparatorChar;
            if(!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
            return path;
        }
    }
}
