using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Memory;

namespace MonoPipeSharp
{
    internal class Program
    {
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
    }
}
