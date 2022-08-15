Credit to Dark Byte, and Cheat Engine for the MonoDataCollector dlls.

Wanna Collaborate or have questions? add me on Discord: Glatriix#8936


(You must have Cheat Engine installed for this program to work :P)

This project is a work in progress. Feel free to make a pull request if you have anything to add.

# MonoPipeSharp
C# Implimentation for Cheat Engine's MonoDataCollector

Exmaple Usage:
```
        static void Main(string[] args)
        {
            Process p = Process.GetProcessesByName("Among Us")[0];
            Mem.Init();
            Mem.m.OpenProcess(p.Id);
            MonoPipe.Is64Bit = false; //set if the process, is 32 or 64 bit NEEDED
            var injected = Inject.InjectMonoCollector(p, 100); //Inject the dll (which dll, depends on x64 value)
            var initresult = MonoPipe.pipe_init(p); // init the client-side named pipe.
            var domainsList = MonoPipe.pipe_get_domains(); //collect all domains, to be stored inside the domains list.
            var setDomain = MonoPipe.pipe_set_domain(domainsList[0]); //set the current domain on the serverpipe
            var assembliesList = MonoPipe.pipe_domain_get_assemblies(); //grab assemblies from (server)current domain, and put them inside of MonoPipe.domains.assemblies
            Console.WriteLine($"{domainsList.Count} Domain(s) Found.");
            Console.WriteLine($"{assembliesList.Count} Assemblies Found in default domain [0].");
            foreach(var asm in assembliesList)
            {
                var image = MonoPipe.pipe_assembly_get_image(asm);
                var a = MonoPipe.pipe_image_get_name((ulong)image);

                if(a.Contains("dll"))
                {
                    Console.WriteLine(a);
                }
            }
            Console.WriteLine("~end of program~"); //for debugging
            Console.ReadKey();
        }
```
![image](https://user-images.githubusercontent.com/73367967/184643142-5f09df4f-0731-495c-af62-6d9acd6d8c42.png)

