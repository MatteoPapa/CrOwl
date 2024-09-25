using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

public class RestartManagerHelper
{
    // Constants
    private const int CCH_RM_MAX_APP_NAME = 255;
    private const int CCH_RM_MAX_SVC_NAME = 63;

    // Enums
    private enum RM_APP_TYPE
    {
        RmUnknown = 0,
        RmMainWindow = 1,
        RmOtherWindow = 2,
        RmService = 3,
        RmExplorer = 4,
        RmConsole = 5,
        RmCritical = 1000
    }

    private enum RM_SESSION_INFO_1
    {
        // Placeholder for any session info if needed
    }

    // Structures
    [StructLayout(LayoutKind.Sequential)]
    private struct RM_UNIQUE_PROCESS
    {
        public int dwProcessId;
        public System.Runtime.InteropServices.ComTypes.FILETIME ProcessStartTime;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct RM_PROCESS_INFO
    {
        public RM_UNIQUE_PROCESS Process;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCH_RM_MAX_APP_NAME + 1)]
        public string strAppName;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCH_RM_MAX_SVC_NAME + 1)]
        public string strServiceShortName;

        public RM_APP_TYPE ApplicationType;
        public uint AppStatus;
        public uint TSSessionId;
        [MarshalAs(UnmanagedType.Bool)]
        public bool bRestartable;
    }

    // P/Invoke Signatures
    [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
    private static extern int RmStartSession(out int pSessionHandle, int dwSessionFlags, string strSessionKey);

    [DllImport("rstrtmgr.dll")]
    private static extern int RmEndSession(int pSessionHandle);

    [DllImport("rstrtmgr.dll", CharSet = CharSet.Unicode)]
    private static extern int RmRegisterResources(int pSessionHandle,
                                                 UInt32 nFiles,
                                                 string[] rgsFilenames,
                                                 UInt32 nApplications,
                                                 [In] RM_UNIQUE_PROCESS[] rgApplications,
                                                 UInt32 nServices,
                                                 string[] rgsServiceNames);

    [DllImport("rstrtmgr.dll")]
    private static extern int RmGetList(int dwSessionHandle,
                                       out uint pnProcInfoNeeded,
                                       ref uint pnProcInfo,
                                       [In, Out] RM_PROCESS_INFO[] rgAffectedApps,
                                       ref uint lpdwRebootReasons);

    // Method to get processes locking a file
    public static List<Process> GetProcessesLockingFile(string filePath)
    {
        int sessionHandle;
        string sessionKey = Guid.NewGuid().ToString();
        List<Process> processes = new List<Process>();

        int result = RmStartSession(out sessionHandle, 0, sessionKey);
        if (result != 0)
        {
            throw new Exception("Could not begin Restart Manager session. Unable to determine file locker.");
        }

        try
        {
            string[] resources = new string[] { filePath };

            result = RmRegisterResources(sessionHandle,
                                         (uint)resources.Length,
                                         resources,
                                         0,
                                         null,
                                         0,
                                         null);

            if (result != 0)
            {
                throw new Exception("Could not register resource.");
            }

            uint pnProcInfoNeeded = 0,
                 pnProcInfo = 0,
                 lpdwRebootReasons = 0;

            // First call to RmGetList to get the number of process
            result = RmGetList(sessionHandle, out pnProcInfoNeeded, ref pnProcInfo, null, ref lpdwRebootReasons);

            if (result == 234) // More data available
            {
                RM_PROCESS_INFO[] processInfos = new RM_PROCESS_INFO[pnProcInfoNeeded];
                pnProcInfo = pnProcInfoNeeded;

                // Second call to RmGetList to get the actual processes
                result = RmGetList(sessionHandle, out pnProcInfoNeeded, ref pnProcInfo, processInfos, ref lpdwRebootReasons);

                if (result == 0)
                {
                    for (int i = 0; i < pnProcInfo; i++)
                    {
                        try
                        {
                            Process proc = Process.GetProcessById(processInfos[i].Process.dwProcessId);
                            if (!processes.Contains(proc))
                            {
                                processes.Add(proc);
                            }
                        }
                        catch (ArgumentException)
                        {
                            // Process might have exited between the calls
                        }
                    }
                }
                else
                {
                    throw new Exception("Could not list processes locking resource.");
                }
            }
            else if (result != 0)
            {
                throw new Exception("Could not list processes locking resource. Failed to get size of result.");
            }
        }
        finally
        {
            RmEndSession(sessionHandle);
        }

        return processes;
    }
}
