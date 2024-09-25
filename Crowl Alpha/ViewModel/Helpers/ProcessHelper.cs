using System;
using System.Diagnostics;

namespace Crowl_Alpha.ViewModel.Helpers
{
    public class ProcessHelper
    {
        public static bool KillProcess(Process process)
        {
            if (process == null)
            {
                Debug.WriteLine("KillProcess: Provided process is null.");
                return true; // Nothing to kill
            }

            try
            {
                if (!process.HasExited)
                {
                    // Conditionally cancel asynchronous reads
                    if (process.StartInfo.RedirectStandardOutput)
                    {
                        try
                        {
                            process.CancelOutputRead();
                            Debug.WriteLine("KillProcess: Cancelled asynchronous output read.");
                        }
                        catch (InvalidOperationException ex)
                        {
                            Debug.WriteLine("KillProcess: Unable to cancel output read - " + ex.Message);
                        }
                    }

                    if (process.StartInfo.RedirectStandardError)
                    {
                        try
                        {
                            process.CancelErrorRead();
                            Debug.WriteLine("KillProcess: Cancelled asynchronous error read.");
                        }
                        catch (InvalidOperationException ex)
                        {
                            Debug.WriteLine("KillProcess: Unable to cancel error read - " + ex.Message);
                        }
                    }

                    // Attempt to kill the process
                    process.Kill();

                    // Wait for the process to exit completely
                    bool exited = process.WaitForExit(5000); // Wait up to 5 seconds
                    if (!exited)
                    {
                        Debug.WriteLine("KillProcess: Process did not exit within the timeout period.");
                        return false;
                    }
                }

                // Dispose of the process resources
                process.Dispose();
                return true;
            }
            catch (InvalidOperationException ex)
            {
                // Process has already exited
                Debug.WriteLine("KillProcess - InvalidOperationException: " + ex.Message);
                return true;
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                // Handle permission or process-related issues
                Debug.WriteLine("KillProcess - Win32Exception: " + ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                // Handle any other exceptions
                Debug.WriteLine("KillProcess - Exception occurred: " + (string.IsNullOrWhiteSpace(ex.Message) ? "No message" : ex.Message));
                return false;
            }
        }
    }
}
