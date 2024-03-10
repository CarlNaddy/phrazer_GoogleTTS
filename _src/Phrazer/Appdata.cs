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

        public static string GetInputPath()
        {
            return GetAppdataPath() + "input" + Path.DirectorySeparatorChar;
        }

        public static string GetExportPath(string currentFileName, string targetGroup, string folderSuffix, string exportSubfolder)
        {
            string path = GetAppdataPath() 
                + "export" + Path.DirectorySeparatorChar
                + targetGroup + "_"
                + Path.GetFileNameWithoutExtension(currentFileName) 
                + folderSuffix + Path.DirectorySeparatorChar;

            if(exportSubfolder.Length > 0) {
                path = path + exportSubfolder + Path.DirectorySeparatorChar;
            }
            
            if(!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
            return path;
        }


        /**
        * @todo should become mp3 along with jingles 
        */
        public static string GetSoundPath(string sound, string package, string format)
        {
            // 24000 Khz Mono Wav only
            if(package == "") package = "piano_mono";
            return GetAppdataPath() + "_sounds" + Path.DirectorySeparatorChar + package + Path.DirectorySeparatorChar + sound + "." + format;
        }

        public static string GetHistoryPath()
        {
            string path = GetAppdataPath() + "_history" + Path.DirectorySeparatorChar;
            if(!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
            return path;
        }
    }
}
