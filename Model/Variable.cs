using System;
using System.Collections.Generic;
using System.Text;

namespace SvenskaInstruktioner.Model
{
    enum DataType
    {
        Undefined,
        Number,
        String,
        Boolean
    }
    class Variable
    {
        public Variable(string name, object value, DataType type)
        {
            Name = name;
            Value = value;
            Type = type;
        }

        public string Name { get; }
        public object Value { get; set; }
        public DataType Type { get; set; }

        public override string ToString() {
            if (Type != DataType.Undefined) return $"{Name} = {ValueToString()}";
            else return $"{Name} is undefined";
        }

        public string ValueToString()
        {
            if (Type == DataType.String) return "\"" + Value + '"';
            else if (Type == DataType.Number) return Value.ToString();
            else return $"[Odefinierad]";
        }
    }
}
