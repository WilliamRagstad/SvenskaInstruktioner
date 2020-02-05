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
        And,
        Or,
        To,
        Is,
        If,
        Then,
        Else,
        BlockStart
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

        public global::System.String Name { get; }
        public TokenType Type { get; }
        public global::System.Object Value { get; }
        public Type ValueType { get; }
        public global::System.Int32 Line { get; }
        public global::System.Int32 Column { get; }

        public override string ToString() => (!string.IsNullOrEmpty(Name) ? Name : "") + (!string.IsNullOrEmpty(Name) && Value != null && Name != Value.ToString() && ValueType != typeof(string) ? " | " : "") + (Value != null && Name != Value.ToString() && ValueType != typeof(string) ? Value : "");

        public static Token Empty => new Token("Empty", TokenType.Undefined, null, null, -1, -1);
    }
}
