using System;
using System.Collections.Generic;
using System.Text;

namespace SvenskaInstruktioner
{
    enum DataType
    {
        Undefined,
        Number,
        String
    }
    class Variable
    {
        public Variable(string name, object value, DataType type, int scope)
        {
            Name = name;
            Value = value;
            Scope = scope;
            Type = type;
        }

        public global::System.String Name { get; }
        public global::System.Object Value { get; }
        public global::System.Int32 Scope { get; }
        public DataType Type { get; }
    }
}
