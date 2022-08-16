using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoPipeSharp
{
    class JIT
    {
        //QWORD -?
        public Int64 jitinfo;
        //QWORD -Start address of the method.
        public Int64 code_start;
        //DWORD -Size of the function
        public int code_size;


        public IntPtr Function{get { return (code_start != null) ? (IntPtr)code_start : IntPtr.Zero; }}


        public JIT(Int64 jitinfo, Int64 code_start, int code_size)
        {
            this.jitinfo = jitinfo;
            this.code_start = code_start;
            this.code_size = code_size;
        }
    }
}
