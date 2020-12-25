using System;
using System.Collections.Generic;
using System.Text;

namespace SvenskaInstruktioner.Model
{
    enum TokenType
    {
        Undefined,
        Expression,
        Boolean,
        String,
        Action,
        Literal,
        Comment,
        Parenthesis,
        CurlyBrackets,
        Separator,
        Equal,

        WhiteSpace,

        And,
        Or,
        Is,
        If,
        Then,
        Else,
        Done,
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
        public string PreferredString()
        {
            switch(Type)
            {
                case TokenType.Boolean: return Name;
                case TokenType.Equal: return Value.ToString();
                default: return ToString();
            }
        }
        public Type ValueType { get; }
        public int Line { get; }
        public int Column { get; }

        public override string ToString()
        {
            string result = "";
            if (!string.IsNullOrEmpty(Name))
            {
                result += Name;
                if (Value != null && !Value.Equals(Name) && ValueType != typeof(Double) && ValueType != typeof(string))
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
