using System.Diagnostics;
using System.IO;
using System.Text;

namespace Kickstart.Utility
{
    public class CommandProcessor
    {
        public static string ExecuteCommand(string command, string outputRootPath, bool writeToBatFirst = false)
        {
            command = $"cd {outputRootPath}" + "&" + command;
            if (writeToBatFirst)
            {
                File.WriteAllText(Path.Combine(outputRootPath, "gen.bat"), command);
                command = $"{outputRootPath}\\gen.bat";

            }

            ProcessStartInfo ProcessInfo;
            Process Process;

            ProcessInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            ProcessInfo.RedirectStandardOutput = true;
            ProcessInfo.RedirectStandardInput = false;
            ProcessInfo.RedirectStandardError = true;
            ProcessInfo.CreateNoWindow = true;
            ProcessInfo.UseShellExecute = false;
            ProcessInfo.WindowStyle = ProcessWindowStyle.Hidden;


            Process = Process.Start(ProcessInfo);
            var output = new StringBuilder();
            while (!Process.StandardOutput.EndOfStream)
            {
                string line = Process.StandardOutput.ReadLine();
                output.AppendLine(line);
            }
            var error = Process.StandardError.ReadToEnd();
            output.AppendLine(error);
            Process.WaitForExit();


            return output.ToString();
        }

        public static void ExecuteCommandWindow(string command, string outputRootPath, bool writeToBatFirst = false)
        {
            command = $"cd {outputRootPath}" + "&" + command;
            if (writeToBatFirst)
            {
                File.WriteAllText(Path.Combine(outputRootPath, "gen.bat"), command);
                command = $"{outputRootPath}\\gen.bat";

            }

            ProcessStartInfo ProcessInfo;
            Process process;
            ProcessInfo = new ProcessStartInfo("cmd.exe", "/c " + command);
            process = Process.Start(ProcessInfo);
            process.WaitForExit();
        }
    }
}