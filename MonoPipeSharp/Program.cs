using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoPipeSharp
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Process p = Process.GetProcessesByName("Among Us")[0];
            MonoPipe.Is64Bit = false; //set if the process, is 32 or 64 bit NEEDED
            Inject.InjectMonoCollector(p, 100); //Inject the dll (which dll depends on x64 value)
            MonoPipe.pipe_init(p); // init the client-side named pipe.
            MonoPipe.pipe_get_domains(); //collect all domains, to be stored inside the domains list.
            MonoPipe.pipe_set_domain(MonoPipe.domains[0]); //set the current domain on the serverpipe
            MonoPipe.pipe_domain_get_assemblies(); //grab assemblies from (server)current domain, and put them inside of MonoPipe.domains.assemblies

            Console.WriteLine($"{MonoPipe.domains.Count} Domain(s) Found.");
            Console.WriteLine($"{MonoPipe.currentDomain.assemblies.Count} Assemblies Found in default domain [0].");


            Console.WriteLine("~end of program~"); //for debugging
            Console.ReadKey();
        }
    }
}
