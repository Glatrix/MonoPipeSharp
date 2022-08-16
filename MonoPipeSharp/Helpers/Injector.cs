using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace MonoPipeSharp
{
    public class Inject
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress,
            uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess,
            IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        // privileges
        const int PROCESS_CREATE_THREAD = 0x0002;
        const int PROCESS_QUERY_INFORMATION = 0x0400;
        const int PROCESS_VM_OPERATION = 0x0008;
        const int PROCESS_VM_WRITE = 0x0020;
        const int PROCESS_VM_READ = 0x0010;

        // used for memory allocation
        const uint MEM_COMMIT = 0x00001000;
        const uint MEM_RESERVE = 0x00002000;
        const uint PAGE_READWRITE = 4;


        private static string checkInstalled(string findByName)
        {
            string displayName;
            string InstallPath;
            string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

            //64 bits computer
            RegistryKey key64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
            RegistryKey key = key64.OpenSubKey(registryKey);

            if (key != null)
            {
                foreach (RegistryKey subkey in key.GetSubKeyNames().Select(keyName => key.OpenSubKey(keyName)))
                {
                    displayName = subkey.GetValue("DisplayName") as string;
                    if (displayName != null && displayName.Contains(findByName))
                    {

                        InstallPath = subkey.GetValue("InstallLocation").ToString();

                        return InstallPath; //or displayName

                    }
                }
                key.Close();
            }

            return null;
        }



        /// <summary>
        /// Inject MonoDataCollector(32 or 64).dll into Process according to set arch.
        /// </summary>
        /// <param name="p">The proc to inject into.</param>
        /// <param name="sleepMS">Milliseconds to sleep before return (void).</param>
        public static bool InjectMonoCollector(Process p, int sleepMS)
        {
            if (MonoPipe.Is64Bit)
            {
                return Inject64(p, sleepMS);
            }
            return Inject32(p, sleepMS);
        }



        /// <summary>
        /// Inject MonoDataCollector.dll into 32-bit process.
        /// </summary>
        /// <param name="p">The proc to inject into.</param>
        /// <param name="sleepMS">Milliseconds to sleep before return (void).</param>
        private static bool Inject32(Process p, int sleepMS)
        {
            string cheatEnginePath = checkInstalled("Cheat Engine");
            //MonoDataCollector32.dll
            string path = Path.Combine(cheatEnginePath, @"autorun\dlls\MonoDataCollector32.dll");

            if (File.Exists(path))
            {
                InjectDLL(p, path, sleepMS);
                return true;
            }
            return false;
        }


        /// <summary>
        /// Inject MonoDataCollector.dll into 64-bit process.
        /// </summary>
        /// <param name="p">The proc to inject into.</param>
        /// <param name="sleepMS">Milliseconds to sleep before return (void).</param>
        private static bool Inject64(Process p, int sleepMS)
        {
            string cheatEnginePath = checkInstalled("Cheat Engine");
            //MonoDataCollector64.dll
            string path = Path.Combine(cheatEnginePath, @"autorun\dlls\MonoDataCollector64.dll");

            if (File.Exists(path))
            {
                InjectDLL(p, path, sleepMS);
                return true;
            }
            return false;
        }


        /// <summary>
        /// Inject any given dll into any given process. (x32 or x64)
        /// </summary>
        /// <param name="targetProcess">the proc to inject into.</param>
        /// <param name="dllName">name of the dll to inject.</param>
        /// <param name="sleep">milliseconds to let the dll load.</param>
        private static void InjectDLL(Process targetProcess, string dllName, int sleep)
        {
            IntPtr procHandle = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ, false, targetProcess.Id);
            IntPtr loadLibraryAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
            IntPtr allocMemAddress = VirtualAllocEx(procHandle, IntPtr.Zero, (uint)((dllName.Length + 1) * Marshal.SizeOf(typeof(char))), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
            UIntPtr bytesWritten;
            WriteProcessMemory(procHandle, allocMemAddress, Encoding.Default.GetBytes(dllName), (uint)((dllName.Length + 1) * Marshal.SizeOf(typeof(char))), out bytesWritten);
            CreateRemoteThread(procHandle, IntPtr.Zero, 0, loadLibraryAddr, allocMemAddress, 0, IntPtr.Zero);
            Thread.Sleep(sleep);
        }
    }
}
