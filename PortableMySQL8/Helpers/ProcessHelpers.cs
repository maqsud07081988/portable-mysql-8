﻿#region License

/*

Copyright 2023 mewtwo0641

Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.

3. Neither the name of the copyright holder nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS “AS IS” AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*/

#endregion License

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;

namespace PortableMySQL8
{
    public static class ProcessHelpers
    {
        public static int RunCommand(string exe, string arguments, bool wait = true)
        {
            bool isHidden = !IsConsoleMode();

            ProcessWindowStyle pws = ProcessWindowStyle.Normal;

            if (isHidden)
                pws = ProcessWindowStyle.Hidden;

            ProcessStartInfo startInfo = new ProcessStartInfo()
            {
                WindowStyle = pws,
                CreateNoWindow = isHidden,
                FileName = exe,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true
            };

            Process process = new Process()
            {
                StartInfo = startInfo
            };

            process.Start();

            process.OutputDataReceived += (sender, args) =>
            {
                if (args.Data != null)
                    Console.WriteLine(args.Data);

                //if (args.Data != null /*&& args.Data.Contains(".")*/)
                //    output = args.Data.ToString();
            };

            process.BeginOutputReadLine();

            if (wait)
            {
                process.WaitForExit();
                Console.WriteLine($"{exe} exited with code: {process.ExitCode}");
                return process.ExitCode;
            }

            return -1;
        }

        /// <summary>
        /// Check if process exists by name
        /// </summary>
        /// <param name="name">Name of process (without .exe)</param>
        /// <returns>true if process is running; false if not</returns>
        public static bool ProcessExists(string name)
        {
            try
            {
                return Process.GetProcessesByName(name).Length > 0;
            }

            catch (Exception)
            {
                return false;
            }
        }

        public static bool IsConsoleMode()
        {
            return Console.OpenStandardInput(1) != Stream.Null;
        }
    }
}