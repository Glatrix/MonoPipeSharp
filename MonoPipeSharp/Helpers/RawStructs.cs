using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonoPipeSharp
{
    //"Raw" structs added as fix for lua 
    public struct RawClass
    {
        public UInt64 _class;
        public string _classname;
        public string _namespace;

        public RawClass(UInt64 _class, string _classname, string _namespace)
        {
            this._class = _class;
            this._classname = _classname;
            this._namespace = _namespace;
        }
    }

    public struct RawField
    {
        public UInt64 _field;
        public UInt64 _type;
        public int _monotype;
        public UInt64 _parent;
        public int _offset;
        public int _flags;
        public bool isStatic;
        public string _fieldName;
        public string _typeName;
    }
}
