using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonoPipeSharp
{
    //Credits: Darkbyte (Cheat Engine) For the MonoDataCollector dll and source code.
    
    //Ref: https://github.com/cheat-engine/cheat-engine/blob/master/Cheat%20Engine/bin/autorun/monoscript.lua

    class MonoPipe
    {
        /// <summary>
        /// False = x32. True = x64;
        /// </summary>
        public static bool Is64Bit = false;
        public static Encoding defaultEncoding = Encoding.UTF8;

        public const byte MONOCMD_INITMONO = 0;
        public const byte MONOCMD_OBJECT_GETCLASS = 1;
        public const byte MONOCMD_ENUMDOMAINS = 2;
        public const byte MONOCMD_SETCURRENTDOMAIN = 3;
        public const byte MONOCMD_ENUMASSEMBLIES = 4;
        public const byte MONOCMD_GETIMAGEFROMASSEMBLY = 5;
        public const byte MONOCMD_GETIMAGENAME = 6;
        public const byte MONOCMD_ENUMCLASSESINIMAGE = 7;
        public const byte MONOCMD_ENUMFIELDSINCLASS = 8;
        public const byte MONOCMD_ENUMMETHODSINCLASS = 9;
        public const byte MONOCMD_COMPILEMETHOD = 10;
        public const byte MONOCMD_GETMETHODHEADER = 11;
        public const byte MONOCMD_GETMETHODHEADER_CODE = 12;
        public const byte MONOCMD_LOOKUPRVA = 13;
        public const byte MONOCMD_GETJITINFO = 14;
        public const byte MONOCMD_FINDCLASS = 15;
        public const byte MONOCMD_FINDMETHOD = 16;
        public const byte MONOCMD_GETMETHODNAME = 17;
        public const byte MONOCMD_GETMETHODCLASS = 18;
        public const byte MONOCMD_GETCLASSNAME = 19;
        public const byte MONOCMD_GETCLASSNAMESPACE = 20;
        public const byte MONOCMD_FREEMETHOD = 21;
        public const byte MONOCMD_TERMINATE = 22;
        public const byte MONOCMD_DISASSEMBLE = 23;
        public const byte MONOCMD_GETMETHODSIGNATURE = 24;
        public const byte MONOCMD_GETPARENTCLASS = 25;
        public const byte MONOCMD_GETconstFIELDADDRESSFROMCLASS = 26;
        public const byte MONOCMD_GETTYPECLASS = 27;
        public const byte MONOCMD_GETARRAYELEMENTCLASS = 28;
        public const byte MONOCMD_FINDMETHODBYDESC = 29;
        public const byte MONOCMD_INVOKEMETHOD = 30;
        public const byte MONOCMD_LOADASSEMBLY = 31;
        public const byte MONOCMD_GETFULLTYPENAME = 32;
        public const byte MONOCMD_OBJECT_NEW = 33;
        public const byte MONOCMD_OBJECT_INIT = 34;
        public const byte MONOCMD_GETVTABLEFROMCLASS = 35;
        public const byte MONOCMD_GETMETHODPARAMETERS = 36;
        public const byte MONOCMD_ISCLASSGENERIC = 37;
        public const byte MONOCMD_ISIL2CPP = 38;
        public const byte MONOCMD_FILLOPTIONALFUNCTIONLIST = 39;
        public const byte MONOCMD_GET_STATIC_FIELDVALUE = 40;
        //fallback for il2cpp which doesn't expose what's needed;
        public const byte MONOCMD_SET_STATIC_FIELDVALUE = 41;
        public const byte MONOCMD_GETCLASSIMAGE = 42;
        public const byte MONOCMD_FREE = 43;
        public const byte MONOCMD_GETIMAGEFILENAME = 44;
        public const byte MONOCMD_GETCLASSNESTINGTYPE = 45;
        public const byte MONOCMD_LIMITEDCONNECTION = 46;

        public static Process theproc;
        public static NamedPipeClientStream monopipe;
        public static long monoBase;
        public static UInt64 currentDomain;

        public static bool pipe_init(Process p)
        {
            theproc = p;
            int targetProcID = theproc.Id;
            monopipe = new NamedPipeClientStream("cemonodc_pid" + targetProcID.ToString());
            monopipe.Connect();
            if (monopipe.CanWrite)
            {
                WriteByte(MONOCMD_INITMONO);
                monoBase = ReadQword();
                return true;
            }
            return false;
        }

        public static bool pipe_procIl2cpp()
        {
            WriteByte(MONOCMD_ISIL2CPP);
            bool result = (ReadByte() == 1);
            return result;
        }

        public static List<UInt64> pipe_get_domains()
        {
            List<UInt64> temp = new List<UInt64>();
            if (monopipe.CanWrite)
            {
                WriteByte(MONOCMD_ENUMDOMAINS);
                int domainsCount = ReadDword();
                for (int i = 0; i < domainsCount; i++)
                {
                    UInt64 domain = (UInt64)ReadQword();
                    temp.Add(domain);
                }
            }
            return temp;
        }

        public static bool pipe_set_domain(UInt64 domain)
        {
            if (monopipe.CanWrite)
            {
                currentDomain = domain;
                WriteByte(MONOCMD_SETCURRENTDOMAIN);
                WriteQword(domain);
                var result = ReadDword();
                return true;
            }
            return false;
        }





        public static List<UInt64> pipe_domain_get_assemblies()
        {
            List<UInt64> temp = new List<UInt64>();   
            if (monopipe.CanWrite)
            {
                WriteByte(MONOCMD_ENUMASSEMBLIES);
                int assembliesCount = ReadDword();

                for (int i = 0; i < assembliesCount; i++)
                {
                    UInt64 assembly = (UInt64)ReadQword();
                    temp.Add(assembly);
                }
            }
            return temp;
        }

        public static string pipe_image_get_name(UInt64 image)
        {
            if (monopipe.CanWrite)
            {
                WriteByte(MONOCMD_GETIMAGEFILENAME);
                WriteQword(image);
                int length = ReadWord();
                return ReadString(length);
            }
            return "NIL";
        }



        public static Int64 pipe_assembly_get_image(UInt64 assembly)
        {
            if (monopipe.CanWrite)
            {
                WriteByte(MONOCMD_GETIMAGEFROMASSEMBLY);
                WriteQword(assembly);
                return ReadQword();
            }
            return 0;
        }




        //TODO: Add More 'Read' and 'Write' Functions.


        //Reads:

        public static int ReadByte()
        {
            return monopipe.ReadByte();
        }


        public static int ReadWord()
        {
            byte[] dword = new byte[2];
            int bytesRead = monopipe.Read(dword, 0, 2);
            return BitConverter.ToInt16(dword, 0);
        }

        public static Int32 ReadDword()
        {
            byte[] dword = new byte[4];
            int bytesRead = monopipe.Read(dword, 0, 4);
            return BitConverter.ToInt32(dword, 0);
        }

        public static Int64 ReadQword()
        {
            byte[] qword = new byte[8];
            int bytesRead = monopipe.Read(qword, 0, 8);
            return (Int64)BitConverter.ToInt64(qword,0);
        }

        public static string ReadString(int length)
        {
            byte[] chars = new byte[length];
            monopipe.Read(chars,0,length);
            return Encoding.ASCII.GetString(chars);
        }



        //Writes:

        public static void WriteByte(byte value)
        {
            monopipe.WriteByte(value);
        }

        public static void WriteQword(ulong value)
        {
            byte[] qword = BitConverter.GetBytes(value);
            monopipe.Write(qword, 0, qword.Length);
        }

        public static void WriteDword(int value)
        {
            byte[] dword = BitConverter.GetBytes(value);
            monopipe.Write(dword, 0, dword.Length);
        }
    }


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
            return Inject32(p,sleepMS);
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
        private static void InjectDLL(Process targetProcess,string dllName,int sleep)
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

    public class Domain
    {
        public UInt64 pointer;
        public List<Assembly> assemblies = new List<Assembly>();


        public Domain(UInt64 pointer)
        {
            this.pointer = pointer;
        }
    }


    public class Assembly
    {
        public UInt64 pointer;
        public MString name;
        public List<Class> classes = new List<Class>();

        public Assembly(UInt64 pointer)
        {
            this.pointer = pointer;
        }
    }

    public class Class
    {
        public UInt64 pointer;
        public MString name;
        public List<Field> fields = new List<Field>();
        public List<Method> methods = new List<Method>();

        public Class(UInt64 pointer)
        {
            this.pointer = pointer;
        }
    }

    public class Field
    {
        public UInt64 pointer;
        public MString name;
        public bool isStatic;

        public Field(UInt64 pointer)
        {
            this.pointer = pointer;
        }
    }

    public class Method
    {
        public UInt64 pointer;
        public MString name;


        public Method(UInt64 pointer)
        {
            this.pointer = pointer;
        }
    }


    public class MString
    {
        public IntPtr address;
        public int length;

        public MString(IntPtr address,int length)
        {
            this.length = length;
            this.address = address;
        }
    }

}
