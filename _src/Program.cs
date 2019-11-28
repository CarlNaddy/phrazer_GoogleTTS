using System;
using System.IO;
using System.Text;

namespace Phrazer
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] files = Directory.GetFiles(Appdata.GetCsvPath(), "*.tsv");
            foreach(string file in files) {
                CSVReader obj = new CSVReader();
                obj.ProceedCsvFile(file);
            }
        }
    }
}
