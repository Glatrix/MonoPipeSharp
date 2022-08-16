<h1>Credit + Contact</h1>
Credit to Dark Byte, and Cheat Engine for the MonoDataCollector dlls.

Wanna Collaborate or have questions? add me on Discord: Glatriix#8936

# MonoPipeSharp
C# Implimentation for Cheat Engine's MonoDataCollector

(You must have Cheat Engine installed for this program to work :P)
This project is a work in progress. Feel free to make a pull request if you have anything to add.
<h3>~Usage</h3>


Exmaple Usage:
```
       static void Main(string[] args)
        {
            Process p = Process.GetProcessesByName("Among Us")[0];
            MonoPipe.Is64Bit = false; //set if the process, is 32 or 64 bit NEEDED
            var injected = Inject.InjectMonoCollector(p, 100); //Inject the dll (which dll, depends on x64 value)
            var initresult = MonoPipe.pipe_init(p); // init the client-side named pipe.
            var domainsList = MonoPipe.pipe_get_domains(); //collect all domains, to be stored inside the domains list.
            var setDomain = MonoPipe.pipe_set_domain(domainsList[0]); //set the current domain on the serverpipe
            var assembliesList = MonoPipe.pipe_domain_get_assemblies(); //grab assemblies from (server)current domain, and put them inside of MonoPipe.domains.assemblies
            var isIl2 = MonoPipe.pipe_isIl2cpp();

            Console.WriteLine(p.ProcessName + (isIl2 ? " ~IL2CPP":" ~MONO"));
            //Console.WriteLine($"Inject: {(injected ? "Success!" : "Failed")}");
            //Console.WriteLine($"Mono Init: {(initresult ? "Success!" : "Failed")}");
            //Console.WriteLine($"Domain Set: {(initresult ? "Success!" : "Failed")}");
            Console.WriteLine($"{domainsList.Count} Domain(s) Found.");
            Console.WriteLine($"{assembliesList.Count} Assemblies Found in default domain [0].\n\n");

            foreach(var asm in assembliesList)
            {
                var image = MonoPipe.pipe_assembly_get_image(asm);
                var image_name = MonoPipe.pipe_image_get_name((ulong)image);
                if (image_name.StartsWith("Assembly-CSharp."))
                {
                    Console.WriteLine("->Assembly-CSharp.dll");
                    var classesList = MonoPipe.pipe_image_enumKlasses((ulong)image);
                    foreach (var klass in classesList.OrderBy((x) => x._classname))
                    {
                        if(klass._classname == "PlayerControl")
                        {
                            Console.WriteLine("-->PlayerControl");
                            var fieldList = MonoPipe.pipe_klass_enumFields((ulong)klass._class);
                            var vtable = MonoPipe.pipe_class_getVTable((ulong)klass._class);
                            foreach(var fld in fieldList.OrderBy((x) => x.isStatic == true))
                            {
                                Console.WriteLine($"---->({fld._typeName}){fld._fieldName}; static: {fld.isStatic.ToString()}");
                                if (fld.isStatic)
                                {
                                    var value = MonoPipe.pipe_get_static_field_value((ulong)vtable,fld._field);
                                    Console.WriteLine($"------->Value = {value.ToString("X")}");
                                }
                            }
                        }
                    }
                }
            }
            Console.WriteLine("\n\n~end of program~"); //for debugging
            Console.ReadKey();
        }
```

![image](https://user-images.githubusercontent.com/73367967/184759737-14fa7b5d-82f1-48b1-873d-96a6f97a31e6.png)

