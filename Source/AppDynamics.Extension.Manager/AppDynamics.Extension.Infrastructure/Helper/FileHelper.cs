using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace AppDynamics.Infrastructure.Helper
{
    public class FileHelper
    {
        public static string GetAbsolutePath(string relativePath)
        {
            string absPath = AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar 
                + relativePath;
            return absPath;
        }

        internal static bool FileExists(string fileName)
        {
            return File.Exists(fileName);
        }

        internal static string GetOutputofScript(string _scriptPath, out string error)
        {
            string output = "";

            var processInfo = new ProcessStartInfo("cmd.exe", "/c \"" + _scriptPath + "\"");

            processInfo.CreateNoWindow = true;

            processInfo.UseShellExecute = false;

            processInfo.RedirectStandardError = true;

            processInfo.RedirectStandardOutput = true;

            var process = Process.Start(processInfo);

            process.Start();

            process.WaitForExit();

            output = process.StandardOutput.ReadToEnd();

            error = process.StandardError.ReadToEnd();

            if (!process.HasExited)
                process.Kill();

            return output;
        }


        internal static string GetDirNamefromPath(string extName)
        {
            string[] names = extName.Split(Path.DirectorySeparatorChar);

            if (names.Length > 1)
                return names[names.Length - 1];
            else
                return extName;

        }
    }
}
