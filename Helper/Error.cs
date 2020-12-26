using SvenskaInstruktioner.Model;
using System;

namespace SvenskaInstruktioner.Helper
{
    static class Error
    {
        public static void General(string message) => Functions.WriteLineColor($"[ERROR] {message}.", ConsoleColor.Red);
        private static string OnTokenRowCol(Token t) => $" on line {t.Line} column {t.Column}";
        public static void WithExpected(string message, string expected) => General($"{message}. Expected: {expected}");

        public static void UnexpectedToken(Token token, string expected) => WithExpected($"Unexpected '{token.Name}'" + OnTokenRowCol(token), expected);
        public static void WrongType(Token token, string expected) => WithExpected($"Invalid type for '{token.Name}'" + OnTokenRowCol(token), expected);
        public static void MissingToken(string whatsMissing, Token token) => General($"Missing {whatsMissing} '{token.Name}'" + OnTokenRowCol(token));
        public static void Undefined(Token token, string typ) => General($"Undefined {typ} '{token.Name}'" + OnTokenRowCol(token));
        public static void FailedTo(Token token, string whatFailed) => General($"Failed to {whatFailed} '{token.Name}'" + OnTokenRowCol(token));
        public static void FailedToDeclare(Token token) => FailedTo(token, "declare");
    }
}
