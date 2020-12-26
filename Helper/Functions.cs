using SvenskaInstruktioner.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace SvenskaInstruktioner.Util
{
    static class Functions
    {
        #region Clean-up
        public static string CleanComment(string c) => c.Trim().Replace("\r", "").Replace("\n", "");

        #endregion

        #region Graphics
        public static void WriteTitle(ConsoleColor textColor, ConsoleColor lineColor, string text)
        {
            WriteColor("======== ", lineColor); WriteColor(text, textColor); WriteLineColor(" ========\n", lineColor);
        }
        public static void WriteTableTitles(ConsoleColor textColor, ConsoleColor lineColor, params string[] titles)
        {
            string result = "";
            string underlines = "";

            for(int i = 0; i < titles.Length; i++)
            {
                result += titles[i];
                for (int j = 0; j < titles[i].Length; j++) underlines += '¨';

                if (i < titles.Length - 1)
                {
                    result += " \t";
                    underlines += " \t";
                }
            }

            WriteLineColor(result, textColor);
            WriteLineColor(underlines, lineColor);
        }

        public static void WriteLineVariableMessage(int line, int column, string message, Variable v, ConsoleColor? var_name = null, ConsoleColor? var_value = null)
        {
            WriteMessage(line, column, message, "");
            PrintVariable(v, var_name, var_value);
            Console.WriteLine();
        }

        public static void WriteLineMessage(int line, int column, string type, string message, ConsoleColor? color = null) => WriteMessage(line, column, type, message + '\n', color);
        public static void WriteMessage(int line, int column, string type, string message, ConsoleColor? color = null)
        {
            Console.Write($"{line}:{column}\t\t{type}: ");
            WriteColor(message, color);
        }

        public static void WriteColor(string text, ConsoleColor? color = null)
        {
            ConsoleColor fg = Console.ForegroundColor;
            if (color.HasValue) Console.ForegroundColor = color.Value;
             Console.Write(text);
            Console.ForegroundColor = fg;
        }

        public static void WriteLineColor(string text, ConsoleColor? color = null) => WriteColor(text + '\n', color);
        #endregion

        #region Formatting
        public static void PrintVariable(Variable v, ConsoleColor? name, ConsoleColor? value)
        {
            WriteColor(v.Name.ToUpper() + " = ", name);
            WriteColor(v.ValueToString(), value);
        }

        public static string TokenListToString(List<Token> list)
        {
            string result = "";
            list.ForEach(t => result += t.PreferredString() + " ");
            return result.TrimEnd();
        }

        #endregion
    }
}
