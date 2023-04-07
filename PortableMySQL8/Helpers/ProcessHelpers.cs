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