using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

namespace Crowl_Alpha.ViewModel.Helpers
{
    public class ResourceExtractorTorHelper
    {
        public static event Action TorReady;

        public static Process RunEmbeddedExeWithTorrc(string exe, List<string> configOptions, string torrcContent)
        {
            string resourceName = $"Crowl_Alpha.Resources.{exe}";

            Assembly assembly = Assembly.GetExecutingAssembly();

            // Define the path to extract the .exe to (e.g., Temp folder)
            string tempPath = Path.Combine(Path.GetTempPath(), exe);

            // Check if the file already exists, eventually try to retrieve it
            if (File.Exists(tempPath))
            {
                try
                {
                    // Retrieve the list of processes locking the file
                    var lockingProcesses = RestartManagerHelper.GetProcessesLockingFile(tempPath);

                    if (lockingProcesses.Count > 0)
                    {
                        // For demonstration, return the first locking process
                        // You can modify this logic as needed
                        Debug.WriteLine("Retrieved process ID:" + lockingProcesses[0].Id);
                        TorReady?.Invoke();
                        return lockingProcesses[0];
                    }
                    else
                    {
                        // No locking processes found, safe to delete
                        File.Delete(tempPath);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to delete the temporary file or retrieve locking process: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }

            // Extract tor.exe
            ExtractResource(assembly, resourceName, tempPath);

            // Generate a temporary torrc configuration file
            string torrcPath = Path.Combine(Path.GetTempPath(), "torrc_temp.txt");
            File.WriteAllText(torrcPath, torrcContent);

            // Add the -f option to specify the torrc file
            List<string> torArguments = new List<string>(configOptions);
            torArguments.Add("-f");
            torArguments.Add(torrcPath);

            // Argument String Preparation
            string arguments = string.Join(" ", torArguments);
            Console.WriteLine("Arguments: " + arguments);

            ProcessStartInfo processStartInfo = new ProcessStartInfo
            {
                FileName = tempPath,
                Arguments = arguments,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            // START with ProcessInfo
            Process process = Process.Start(processStartInfo);

            // CAPTURING OUTPUT
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                {
                    Console.WriteLine(e.Data);

                    // Is It Ready ?
                    if (e.Data.Contains("Bootstrapped 100% (done): Done"))
                    {
                        TorReady?.Invoke();
                    }
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                    Console.WriteLine("Error: " + e.Data);
            };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            return process;
        }

        private static void ExtractResource(Assembly assembly, string resourceName, string outputPath)
        {
            using (Stream resourceStream = assembly.GetManifestResourceStream(resourceName))
            {
                if (resourceStream == null)
                    throw new Exception("Resource not found: " + resourceName);

                using (FileStream fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    resourceStream.CopyTo(fileStream);
                }
            }
        }

    }
}
