#region License

/*

Copyright 2023 mewtwo0641
(See ADDITIONAL_COPYRIGHTS.txt for full list of copyright holders)

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS “AS IS” AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

#endregion License

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
                Console.WriteLine("Error: Arguments were null\r\nPress the any key to exit.");
                Console.ReadKey();
                Environment.Exit(-1);
                return;
            }

            if (args.Length < 2)
            {
                Console.WriteLine("Wrong number of arguments given! (Expected: 2)\r\n\r\nThis application is not intended to be ran manually.\r\n\r\nPress the any key to exit.");
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
