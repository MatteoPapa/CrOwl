using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Crowl_Alpha.ViewModel.Helpers
{
    public class ProcessHelper
    {
        public static void killProcess(Process process)
        {
            try
            {
                if (process != null && !process.HasExited)
                {
                    // Stop reading output/error streams
                    process.CancelOutputRead();
                    process.CancelErrorRead();

                    process.Kill();

                    // Wait for the process to exit completely
                    process.WaitForExit();


                }
                if (process != null)
                {
                    process.Dispose();
                }

            }

            catch (InvalidOperationException ex)
            {
                Debug.WriteLine("InvalidOperationException: " + ex.Message);  // Process has already exited
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                Debug.WriteLine("Win32Exception: " + ex.Message);  // Handle permission or process-related issues
            }
            catch (Exception ex)
            {
                //Generic Exception
                Debug.WriteLine("Exception occurred: " + (string.IsNullOrWhiteSpace(ex.Message) ? "No message" : ex.Message));
            }

        }
    }
}
