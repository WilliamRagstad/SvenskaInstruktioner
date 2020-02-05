using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace SvenskaInstruktioner
{
    class Interpreter
    {
       private List<Variable> Variables;

        public Interpreter(string file)
        {
            SourceFile = file;
        }

        public global::System.String SourceFile { get; }

        public void Interpret()
        {
            Variables = new List<Variable>();
            List<Token> tokens = Tokenize();
            PrintTokens(tokens);
            Evaluate(tokens);

            Console.ReadKey(true);
        }

        private List<Token> Tokenize()
        {
            List<Token> tokens = new List<Token>();
            string source = File.ReadAllText(SourceFile);
            string ct = string.Empty;
            char nc = '\0';
            bool isString = false;
            bool isComment = false;
            int i;
            int line = 1;
            int column = 1;

            void addToken(TokenType type, object value, Type valueType) { tokens.Add(new Token(ct, type, value, valueType, line, column)); ct = string.Empty; }

            for (i = 0; i < source.Length; i++)
            {
                char cc = char.ToLower(source[i]);
                if (i + 1 < source.Length)
                    nc = source[i + 1];
                else
                    nc = '\0';

                column++;
                if (cc == '\n') { column = 1; line++; }

                if (cc == '"' || cc == '\'')
                {
                    isString = !isString;
                    ct += cc;
                    continue;
                }
                else if (cc == '#')
                {
                    isComment = true;
                    continue;
                }

                if ((cc == ' ' || cc == '\t' || cc == '\n' || cc == '\r' || cc == '(' || cc == ')' || cc == ',') && !isString || i == source.Length - 1)
                {
                    if (i == source.Length - 1) ct += cc;
                    if (ct == "" && cc == '\t') addToken(TokenType.Indent, null, null);
                    if (ct == "med" || ct == "än") { ct = string.Empty; continue; }       // a multiplicerat med b -> a multiplicerat b

                    if (isComment) {
                        if (cc == '\n')
                        {
                            addToken(TokenType.Comment, ct, null);
                            isComment = false;
                        }
                        else
                        {
                            ct += cc;
                            continue;
                        }
                    }
                    else if (ct == "är")      addToken(TokenType.Is, null, null);
                    else if (ct == "om")      addToken(TokenType.If, null, null);
                    else if (ct == "så")      addToken(TokenType.Then, null, null);
                    else if (ct == "annars")  addToken(TokenType.Else, null, null);
                    else if (ct == "sant")    addToken(TokenType.Expression, true, null);
                    else if (ct == "falskt")  addToken(TokenType.Expression, false, null);
                    else if (ct == "sätt")    addToken(TokenType.Assignment, null, null);
                    else if (ct == "lika"      || ct == "==")               addToken(TokenType.Equal, null, null);
                    else if (ct == "till"      ||(ct == "=" && nc != '='))  addToken(TokenType.To, null, null);
                    else if (ct == "och"       || ct == "&")                addToken(TokenType.And, null, null);
                    else if (ct == "eller"     || ct == "|")                addToken(TokenType.Or, null, null);
                    else if (ct == "plus"      || ct == "adderat"         || ct == "+")  addToken(TokenType.Expression, "+", null);
                    else if (ct == "minus"     || ct == "subtraherat"     || ct == "-")  addToken(TokenType.Expression, "-", null);
                    else if (ct == "genom"     || ct == "dividerat"       || ct == "/")  addToken(TokenType.Expression, "/", null);
                    else if (ct == "gånger"    || ct == "multiplicerat"   || ct == "*")  addToken(TokenType.Expression, "*", null);
                    else if (ct == "mindre"    || ct == "<")  addToken(TokenType.Expression, "<", null);
                    else if (ct == "större"    || ct == ">")  addToken(TokenType.Expression, ">", null);
                    else if (ct == "upphöjt"   || ct == "^")  addToken(TokenType.Expression, "^", null);

                    else if (ct == "inkludera")  addToken(TokenType.Action, null, null);
                    else if (ct == "funktion")   addToken(TokenType.Action, null, null);

                    else if (ct.Trim() != string.Empty)
                    {
                        bool ctIsString = ct.StartsWith("\"") && ct.EndsWith("\"");
                        // Literal
                        double numericValue;
                        if (!ctIsString && double.TryParse(ct.Replace(".", ","), out numericValue))
                        {
                            addToken(TokenType.Expression, numericValue, typeof(double));
                        }
                        else if (ctIsString)
                        {
                            addToken(TokenType.Literal, ct.TrimStart('"').TrimEnd('"'), typeof(string));
                        }
                        else
                        {
                            addToken(TokenType.Literal, ct, null);
                            ct = string.Empty;
                        }
                    }
                    else if (ct.Trim() == string.Empty) ct = string.Empty;

                    if (cc == '(' || cc == ')') addToken(TokenType.Separator, cc, null);
                    else if (cc == ':') addToken(TokenType.BlockStart, cc, null);
                    else if (cc == ',') addToken(TokenType.Separator, cc, null);
                }
                else
                {
                    ct += cc;
                }
            }

            return tokens;
        }

        private void Evaluate(List<Token> tokens)
        {
            int i;
            Token nextToken()
            {
                if (i < tokens.Count)
                {
                    Token t = tokens[i];
                    i++;
                    if (t.Type != TokenType.Comment) return t;
                    return nextToken();
                }
                return null;
            }
            List<Token> getExpression()
            {
                List<Token> expressions = new List<Token>();
                Token t = nextToken();
                while(t.Type == TokenType.Expression || t.Type == TokenType.Separator)
                {
                    expressions.Add(t);
                    t = nextToken();
                }
                i--;
                return expressions;
            }

            for (i = 0; i < tokens.Count; i++)
            {
                Token ct = nextToken();
                if (ct.Type == TokenType.Assignment)
                {
                    Token variable = nextToken();
                    if (variable.Type != TokenType.Literal) { Exception_UnexpectedToken(variable, "Variabelnamn"); return; }
                    Token equalSign = nextToken();
                    if (equalSign.Type != TokenType.Equal && equalSign.Type != TokenType.To) { Exception_UnexpectedToken(equalSign, "Lika med, till eller ="); return; }
                    List<Token> expression = getExpression();
                    if (expression.Count == 0) { Exception_UnexpectedToken(equalSign, "Uttryck"); return; }

                    object result;
                    DataType dataType = Evaluate(expression, out result);

                    Variables.Add(new Variable(variable.Name, result, dataType, 0));
                }
                else if (ct.Type == TokenType.And) continue;
                
            }
        }
        private void PrintTokens(List<Token> tokens)
        {
            Console.WriteLine("==== Värdelista (List of Tokens) ====\n");
            Console.WriteLine(" PC    Rad:Kol   Värde\n" +
                              " ¨¨    ¨¨¨¨¨¨¨   ¨¨¨¨¨");

            int PC = 0x4000;
            for (int i = 0; i < tokens.Count; i++)
            {
                PC += 4;
                Console.Write("0x" + PC.ToString("x") + "\t");
                Console.Write(tokens[i].Line + ":" + tokens[i].Column + "\t" + tokens[i].Type);
                if (tokens[i].ValueType != null) { Console.Write("/"); WriteColor(tokens[i].ValueType.ToString().Replace("System.", ""), ConsoleColor.DarkYellow); }
                Console.Write(": ");
                WriteColor(tokens[i].ToString(), ConsoleColor.Yellow);
                Console.Write("\n");
            }
        }

        private DataType Evaluate(List<Token> expression, out object result)
        {
            if (expression.Count == 1)
            {
                result = expression[0].Value;
                if (expression[0].ValueType == typeof(string)) return DataType.String;
                else if (expression[0].ValueType == typeof(double)) return DataType.Number;
                return DataType.Undefined;
            }


            result = 0;
            string[] orderOfOperation = {
                "^", "/", "*", "+", "-"
            };

            List<Token> newExpression;

            Token expressionValue = null;

            for (int i = 0; i < orderOfOperation.Length; i++)
            {
                string operation = orderOfOperation[i];
                for (int j = 0; j < expression.Count; j++)
                {
                    Token t = expression[j];
                    if (t.Type == TokenType.Expression && t.Value == operation)
                    {
                        if (!(j > 0 && j < expression.Count))
                        {
                            Exception_UnexpectedToken(t, "operatorer på båda sidor av operatorn");
                            return DataType.Undefined;
                        }

                        int k = 1;
                        int expressionStart = -1;
                        int expressionEnd = -1;
                        int parenthisesDepth = 0;

                        // Get LHS
                        List<Token> LHS = new List<Token>();
                        Token temp_t = Token.Empty;
                        while (j - k >= 0 && (temp_t.Type == TokenType.Expression || temp_t.Type == TokenType.Separator || temp_t.Type == TokenType.Undefined))
                        {
                            temp_t = expression[j - k];
                            if (TokenIsOperation(temp_t) && parenthisesDepth == 0) { k--; break; }
                            else if (temp_t.Type == TokenType.Separator && (char)temp_t.Value == '(')
                            {
                                parenthisesDepth++;
                                if (parenthisesDepth == 0) { k++; continue; } // Do not add token
                            }
                            else if (temp_t.Type == TokenType.Separator && (char)temp_t.Value == ')')
                            {
                                parenthisesDepth--;
                                if (parenthisesDepth == 0 - 1) { k++; continue; } // Do not add token
                            }
                            LHS.Insert(0, temp_t);
                            k++;
                        }
                        expressionStart = (j - k < 0) ? 0 : j - k;

                        // Get RHS
                        k = 1;
                        parenthisesDepth = 0;
                        List<Token> RHS = new List<Token>();
                        temp_t = Token.Empty;
                        while (j + k < expression.Count && (temp_t.Type == TokenType.Expression || temp_t.Type == TokenType.Separator || temp_t.Type == TokenType.Undefined))
                        {
                            temp_t = expression[j + k];
                            if (TokenIsOperation(temp_t) && parenthisesDepth == 0) { k--; break; }
                            else if (temp_t.Type == TokenType.Separator && (char)temp_t.Value == '(') parenthisesDepth++;
                            else if (temp_t.Type == TokenType.Separator && (char)temp_t.Value == ')') parenthisesDepth--;
                            RHS.Add(temp_t);
                            k++;
                        }
                        expressionEnd = j + k;

                        object LHS_Value;
                        DataType LHS_TYPE = Evaluate(LHS, out LHS_Value);

                        object RHS_Value;
                        DataType RHS_TYPE = Evaluate(RHS, out RHS_Value);

                        switch (operation)
                        {
                            case "/":
                                if (LHS_TYPE != DataType.Number)
                                {
                                    Exception_WrongType(t, "vänster sida av operationstecknet evaluerades inte till ett nummer");
                                    return DataType.Undefined;
                                }
                                if (RHS_TYPE != DataType.Number)
                                {
                                    Exception_WrongType(t, "höger sida av operationstecknet evaluerades inte till ett nummer");
                                    return DataType.Undefined;
                                }
                                expressionValue = new Token("EXPR_DIV", TokenType.Expression, (double)LHS_Value / (double)RHS_Value, typeof(double), t.Line, t.Column);
                                break;

                            case "*":
                                if (LHS_TYPE != DataType.Number)
                                {
                                    Exception_WrongType(t, "vänster sida av operationstecknet evaluerades inte till ett nummer");
                                    return DataType.Undefined;
                                }
                                if (RHS_TYPE != DataType.Number)
                                {
                                    Exception_WrongType(t, "höger sida av operationstecknet evaluerades inte till ett nummer");
                                    return DataType.Undefined;
                                }
                                expressionValue = new Token("EXPR_MUL", TokenType.Expression, (double)LHS_Value * (double)RHS_Value, typeof(double), t.Line, t.Column);
                                break;

                            case "+":
                                if (LHS_TYPE == DataType.Number && RHS_TYPE == DataType.Number)
                                {
                                    expressionValue = new Token("EXPR_ADD_NUM", TokenType.Expression, (double)LHS_Value + (double)RHS_Value, typeof(double), t.Line, t.Column);
                                }
                                else
                                {
                                    expressionValue = new Token("EXPR_ADD_STR", TokenType.Expression, LHS_Value.ToString() + RHS_Value.ToString(), typeof(string), t.Line, t.Column);
                                }
                                break;

                            case "-":
                                if (LHS_TYPE != DataType.Number)
                                {
                                    Exception_WrongType(t, "vänster sida av operationstecknet evaluerades inte till ett nummer");
                                    return DataType.Undefined;
                                }
                                if (RHS_TYPE != DataType.Number)
                                {
                                    Exception_WrongType(t, "höger sida av operationstecknet evaluerades inte till ett nummer");
                                    return DataType.Undefined;
                                }
                                expressionValue = new Token("EXPR_SUB", TokenType.Expression, (double)LHS_Value - (double)RHS_Value, typeof(double), t.Line, t.Column);
                                break;

                            case "^":
                                if (LHS_TYPE != DataType.Number)
                                {
                                    Exception_WrongType(t, "vänster sida av operationstecknet evaluerades inte till ett nummer");
                                    return DataType.Undefined;
                                }
                                if (RHS_TYPE != DataType.Number)
                                {
                                    Exception_WrongType(t, "höger sida av operationstecknet evaluerades inte till ett nummer");
                                    return DataType.Undefined;
                                }
                                expressionValue = new Token("EXPR_POW", TokenType.Expression, Math.Pow((double)LHS_Value, (double)RHS_Value), typeof(double), t.Line, t.Column);
                                break;

                            default:
                                Exception_UnexpectedToken(t, "operationstecken");
                                return DataType.Undefined;
                        }

                        newExpression = new List<Token>();
                        for (int l = 0; l < expression.Count; l++)
                        {
                            if (l == expressionStart)
                            {
                                newExpression.Add(expressionValue);
                            }
                            else if (l > expressionStart && l < expressionEnd) continue;
                            else
                            {
                                newExpression.Add(expression[l]);
                            }
                        }
                        expression = newExpression;
                    }
                }
            }

            // Eval multiplication and division
            result = expressionValue.Value;
            if (expressionValue.ValueType == typeof(string)) return DataType.String;
            else if (expressionValue.ValueType == typeof(double)) return DataType.Number;
            return DataType.Undefined;
        }

        private bool TokenIsOperation(Token t) => t.Type == TokenType.Expression && ( t.Value.ToString() == "*" || t.Value.ToString() == "/" || t.Value.ToString() == "+" || t.Value.ToString() == "-" || t.Value.ToString() == "^");

        private Token FindToken(Token[] selection, TokenType? type, string name, object value)
        {
            for (int i = 0; i < selection.Length; i++)
            {
                bool matches_type     = type.HasValue  ? selection[i].Type == type   : true;
                bool matches_name     = !string.IsNullOrEmpty(name)  ? selection[i].Name == name   : true;
                bool matches_value    = value != null ? selection[i].Value == value : true;
                
                if (matches_type && matches_name && matches_value) return selection[i];
            }
            return null;
        }

        private void WriteColor(string text, ConsoleColor? color = null)
        {
            ConsoleColor fg = Console.ForegroundColor;
            if (color.HasValue) Console.ForegroundColor = color.Value;
            Console.Write(text);
            Console.ForegroundColor = fg;
        }

        private void WriteLineColor(string text, ConsoleColor color) => WriteColor(text + '\n', color);

        private void Exception_UnexpectedToken(Token token, string expected)
        {
            WriteColor($"Undantag! Oväntat värde '{token.Name}' på rad {token.Line} kolumn {token.Column}. Förväntade: {expected}!", ConsoleColor.Red);
        }

        private void Exception_WrongType(Token token, string expected)
        {
            WriteColor($"Undantag! Felaktig typ '{token.Name}' på rad {token.Line} kolumn {token.Column}. Förväntade: {expected}!", ConsoleColor.Red);
        }


    }
}
