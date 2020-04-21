using System;
using System.Collections.Generic;
using System.Text;

namespace SvenskaInstruktioner
{
    enum TokenType
    {
        Undefined,
        Assignment,
        Expression,
        Action,
        Literal,
        Comment,
        Separator,
        Equal,
        Indent,
        LineBreak,
        And,
        Or,
        To,
        Is,
        If,
        Then,
        Else,
        BlockStart,
        BlockEnd
    }

    class Token
    {
        public Token(string name, TokenType type, object value, Type valueType, int line, int col)
        {
            Name = name;
            Type = type;
            Value = value;
            ValueType = valueType;
            Line = line;
            Column = col;
        }

        public string Name { get; }
        public TokenType Type { get; }
        public object Value { get; }
        public Type ValueType { get; }
        public int Line { get; }
        public int Column { get; }

        public override string ToString()
        {
            string result = "";
            if (!string.IsNullOrEmpty(Name))
            {
                result += Name;
                if (Value != null && !Value.Equals(Name) && ValueType != typeof(Double) && ValueType != typeof(string)) // && ValueType != typeof(string)
                {
                    result += " | " + Value;
                }
            }
            else if (Value != null) result += Value;
            return result;
        }
        public static Token Empty => new Token("Empty", TokenType.Undefined, null, null, -1, -1);
    }
}
