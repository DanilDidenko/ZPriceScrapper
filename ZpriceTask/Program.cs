using System;
using System.Collections.Generic;
using System.IO;

using System.Threading.Tasks;

namespace ZpriceTask
{
    class Program
    {
        static void Main(string[] args)
        {

            var vendor = "POZIS";

            var items = TrumartScraper.getItemsByVendor(vendor);

            foreach (var item in items)
            {
                Console.WriteLine($"{item.Id} - {item.Name} - {item.Price} рублей");
                Console.WriteLine();
            }


            var filePath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)+$@"\{vendor}.txt";
            WriteItems(items, filePath);

            ConsoleKeyInfo keyInfo = Console.ReadKey(true); 
            if (keyInfo.Key == ConsoleKey.Enter || keyInfo.Key == ConsoleKey.Escape) ;


        }

        public static void WriteItems(IEnumerable<Item> items, string path)
        {

            FileStream fcreate = File.Open(path, FileMode.Create);
            fcreate.Close();
            using (TextWriter tw = new StreamWriter(path))
            {
                foreach (Item item in items)
                {
                    tw.WriteLine($"{item.Id} - {item.Name} - {item.Price} рублей");
                    tw.WriteLine("");
                }

            }
        }
    }
}
