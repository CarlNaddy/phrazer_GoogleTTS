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
            //NetflixVTTConverter.convertToTSV(); return;
            
            PhrazeGenerator.ProceedAllInputPhrazerFiles();
        }
    }
}
