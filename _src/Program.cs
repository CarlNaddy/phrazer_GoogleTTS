using System;
using System.IO;
using System.Text;

namespace Phrazer
{
    class Program
    {
        static void Main(string[] args)
        {
            // Uncomment if need to convert VTT files
            
            //VTTConverter.createDownloadSubtitlesHTML(); return;

            //VTTConverter.convertToTSV(); return;

            //VTTCollector.convertToTSV(); return;
            
            PhrazeGenerator.ProceedAllInputPhrazerFiles();

        }
    }
}
