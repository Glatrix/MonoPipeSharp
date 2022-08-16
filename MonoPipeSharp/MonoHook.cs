using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonoPipeSharp
{
    //Credits: Darkbyte (Cheat Engine) For the MonoDataCollector dll and source code.
    //Ref: https://github.com/cheat-engine/cheat-engine/blob/master/Cheat%20Engine/bin/autorun/monoscript.lua

    public enum FIELDATTRIBUTES
    {
        FIELD_ACCESS_MASK = 0x0007,
        COMPILER_CONTROLLED = 0x0000,
        PRIVATE = 0x0001,
        FAM_AND_ASSEM = 0x0002,
        ASSEMBLY = 0x0003,
        FAMILY = 0x0004,
        FAM_OR_ASSEM = 0x0005,
        PUBLIC = 0x0006,
        STATIC = 0x0010,
        INIT_ONLY = 0x0020,
        LITERAL = 0x0040,
        NOT_SERIALIZED = 0x0080,
        SPECIAL_NAME = 0x0200,
        PINVOKE_IMPL = 0x2000,
        RESERVED_MASK = 0x9500,
        RT_SPECIAL_NAME = 0x0400,
        HAS_FIELD_MARSHAL = 0x1000,
        HAS_DEFAULT = 0x8000,
        HAS_FIELD_RVA = 0x0100
    }

    class MonoPipe
    {
        /// <summary>
        /// False = x32. True = x64;
        /// </summary>
        public static bool Is64Bit = false;
        public static Encoding defaultEncoding = Encoding.ASCII;
        
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
        public const byte MONOCMD_GET_STATIC_FIELDVALUE = 40; //fallback for il2cpp which doesn't expose what's needed;
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

        public static bool pipe_isIl2cpp()
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


        public static string pipe_image_get_filename(UInt64 image)
        {
            if (monopipe.CanWrite)
            {
                WriteByte(MONOCMD_GETIMAGEFILENAME);
                WriteQword(image);
                int length = ReadWord();
                return ReadString(length);
            }
            return "??";
        }

        public static string pipe_image_get_name(UInt64 image)
        {
            if (monopipe.CanWrite)
            {
                WriteByte(MONOCMD_GETIMAGENAME);
                WriteQword(image);
                int length = ReadWord();
                return ReadString(length);
            }
            return "??";
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

        public static List<RawClass> pipe_image_enumKlasses(UInt64 image)
        {
            List<RawClass> temp = new List<RawClass>();
            if (monopipe.CanWrite)
            {
                WriteByte(MONOCMD_ENUMCLASSESINIMAGE);
                WriteQword(image);
                int klassCount = ReadDword();
                
                for (int i = 0; i < klassCount; i++)
                {
                    RawClass rawClass = new RawClass();
                    rawClass._class = (UInt64)ReadQword();
                    var nameLength = ReadWord();
                    rawClass._classname = ReadString(nameLength);
                    var namesLength = ReadWord();
                    rawClass._namespace = ReadString(namesLength);
                    temp.Add(rawClass);
                }
            }
            return temp;
        }

        public static string pipe_klass_get_name(UInt64 klass)
        {
            if (monopipe.CanWrite)
            { 
                WriteByte(MONOCMD_GETCLASSNAME);
                WriteQword(klass);
                int nameLength = ReadWord();
                return ReadString(nameLength);
            }
            return "??";
        }

        public static List<RawField> pipe_klass_enumFields(UInt64 klass)
        {
            List<RawField> temp = new List<RawField>();
            if (monopipe.CanWrite)
            {
                WriteByte(MONOCMD_ENUMFIELDSINCLASS);
                WriteQword(klass);
                //int fieldCount = ReadDword();

                for (int i = 0; i < 999; i++)
                {
                    RawField rawField = new RawField();
                    rawField._field = (UInt64)ReadQword();

                    if(rawField._field == null || rawField._field == 0)
                    {
                        break;
                    }


                    rawField._type = (UInt64)ReadQword();
                    rawField._monotype = ReadDword();

                    rawField._parent = (UInt64)ReadQword();
                    rawField._offset = ReadDword();
                    rawField._flags = ReadDword();

                    rawField.isStatic = (((FIELDATTRIBUTES)rawField._flags & FIELDATTRIBUTES.STATIC) == FIELDATTRIBUTES.STATIC);

                    int nameLength = ReadWord();
                    rawField._fieldName = ReadString(nameLength);

                    int typeNameLength = ReadWord();
                    rawField._typeName = ReadString(typeNameLength);

                    temp.Add(rawField);
                }
            }
            return temp;
        }


        public static Int64 pipe_class_getVTable(UInt64 klass)
        {
            if (monopipe.CanWrite)
            {
                WriteByte(MONOCMD_GETVTABLEFROMCLASS);
                WriteQword(currentDomain);
                WriteQword(klass);
                return ReadQword();
            }
            return 0;
        }

        public static Int64 pipe_class_getVTable(UInt64 domain, UInt64 klass)
        {
            if (monopipe.CanWrite)
            {
                WriteByte(MONOCMD_GETVTABLEFROMCLASS);
                WriteQword(domain);
                WriteQword(klass);
                return ReadQword();
            }
            return 0;
        }


        public static Int64 pipe_get_static_field_address_from_class(UInt64 klass)
        {
            if (monopipe.CanWrite)
            {
                WriteByte(MONOCMD_GETconstFIELDADDRESSFROMCLASS);
                WriteQword(currentDomain);
                WriteQword(klass);
                return ReadQword();
            }
            return 0;
        }


        public static Int64 pipe_get_static_field_value(UInt64 vtable, UInt64 field)
        {
            if (monopipe.CanWrite)
            {
                WriteByte(MONOCMD_GET_STATIC_FIELDVALUE);
                WriteQword(vtable);
                WriteQword(field);
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
            if(length > 0)
            {
                byte[] chars = new byte[length];
                monopipe.Read(chars, 0, length);
                return defaultEncoding.GetString(chars);
            }
            return "";
        }

        //Writes:

        public static void WriteByte(byte value)
        {
            if (!monopipe.CanWrite) { return; }
            monopipe.WriteByte(value);
        }

        public static void WriteQword(ulong value)
        {
            if (!monopipe.CanWrite) { return; }
            byte[] qword = BitConverter.GetBytes(value);
            monopipe.Write(qword, 0, qword.Length);
        }

        public static void WriteDword(int value)
        {
            if (!monopipe.CanWrite) { return; }
            byte[] dword = BitConverter.GetBytes(value);
            monopipe.Write(dword, 0, dword.Length);
        }
    }
}
