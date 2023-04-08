using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MySQLLauncher
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args == null)
            {
                Console.WriteLine("Can not start with null arguments!\r\nPress the any key to exit.");
                Console.ReadKey();
                Environment.Exit(-1);
                return;
            }

            if (args.Length < 2)
            {
                Console.WriteLine("Wrong number of arguments given! (Expected: 2)\r\nPress the any key to exit.");
                Console.ReadKey();
                Environment.Exit(-2);
                return;
            }

            //0 should always be the exe name
            string exe = args[0];

            //1 and above should always be the parameters
            string prams = String.Empty;

            //Build the parameter string
            for (int i = 1; i < args.Length; i++)
            {
                prams += args[i] + " ";
            }

            prams = prams.Trim();

            Console.WriteLine(exe);
            Console.WriteLine(prams);

            ProcessStartInfo proc = new ProcessStartInfo()
            {
                FileName = exe,
                WindowStyle = ProcessWindowStyle.Hidden,
                Arguments = prams,
                CreateNoWindow = true,
                UseShellExecute = true
            };

            Process.Start(proc);
            Environment.Exit(0);
        }
    }
}
