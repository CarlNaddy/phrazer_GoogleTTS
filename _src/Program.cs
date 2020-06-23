using System;
using System.IO;
using System.Text;

namespace Phrazer
{
    class Program
    {
        static void Main(string[] args)
        {
            VTTConverter.convertToTSV(); return;


            PhrazeGenerator.ProceedAllInputPhrazerFiles();
        }
    }
}
