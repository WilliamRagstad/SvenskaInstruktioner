using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using SvenskaInstruktioner.Model;
using SvenskaInstruktioner.Exception;
using SvenskaInstruktioner.Helper;

namespace SvenskaInstruktioner.Core
{
    enum ExitFlag
    {
        Successfull,
        SyntaxError,
        FatalError
    }
    class Interpreter
    {
        public string EntryFile { get; }

        private char EOF = '\0';

        private static string[] orderOfAritmeticOperations = { "^", "/", "*", "-", "+", "&", "|" };
        private static string[] orderOfLogicalOperations = { "=", "==", "!", "!=", "<", ">", "<=", ">=", "||", "&&" };
        private static string[] orderOfOperations = orderOfAritmeticOperations.Concat(orderOfLogicalOperations).ToArray();
        private static char[] separators = { '(', ')', '{', '}', ',' };
        private bool isOperator(string c) => orderOfOperations.Contains(c);
        private bool isOperator(char c) => isOperator(c.ToString());
        private bool isSeparator(char c) => separators.Contains(c);

        private BoundScope globalScope;

        public Interpreter(string entryfile)
        {
            EntryFile = entryfile;
        }

        public ExitFlag Interpret(bool debug = false)
        {
            List<Token> tokens = Tokenize();
            if (debug) PrintTokens(tokens);

            globalScope = new GlobalScope();

            ExitFlag f;
            try
            {
                f = Execute(tokens, globalScope, debug, true);
            }
            catch(FatalErrorException e)
            {
                Functions.WriteLineColor(e.Message, ConsoleColor.Red);
                Functions.WriteLineColor(e.StackTrace, ConsoleColor.DarkYellow);
                f = ExitFlag.FatalError;
            }
            return f;
        }

        private List<Token> Tokenize()
        {
            List<Token> tokens = new List<Token>();
            string source = File.ReadAllText(EntryFile) + EOF;
            string ct = string.Empty;
            char nc = '\0';
            bool isString = false;
            bool isComment = false;
            int i;
            int line = 1;
            int column = -1; // column ++ makes it 0

            void addToken(TokenType type, object value = null, Type valueType = null) {
                if (type == TokenType.Comment) { ct = Functions.CleanComment(ct); value = ct; }
                tokens.Add(new Token(ct, type, value, valueType, line, column > 0 ? column : 0 ));
                ct = string.Empty;
            }

            for (i = 0; i < source.Length; i++)
            {
                column++;
                char cc = char.ToLower(source[i]);
                if (i + 1 < source.Length)
                    nc = source[i + 1];
                else
                    nc = EOF;

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

                if ((cc == ' '
                    || cc == '\t'
                    || cc == '\n'
                    || cc == '\r'
                    || isSeparator(cc)
                    || isOperator(cc)
                    || isOperator(cc.ToString() + nc)
                    ) && !isString || cc == EOF)
                {
                    #region Discarded tokens
                    if (ct == "" && cc == '\t') addToken(TokenType.WhiteSpace, "\\t");
                    if (ct == "med" || ct == "än" || ct == "till") { ct = string.Empty; continue; }       // Ex: a multiplicerat med b <=> a multiplicerat b
                    #endregion
                    #region Comments
                    if (isComment) {
                        if (cc == '\n' || cc == '\r' || cc == EOF)
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
                    #endregion
                    #region Named tokens
                    else if (ct == "är")      addToken(TokenType.Is);
                    else if (ct == "om")      addToken(TokenType.If);
                    else if (ct == "så")      addToken(TokenType.Then);
                    else if (ct == "annars")  addToken(TokenType.Else);
                    else if (ct == "sant")    addToken(TokenType.Boolean, true);
                    else if (ct == "falskt")  addToken(TokenType.Boolean, false);
                    else if (ct == "och")     addToken(TokenType.And, "&&");
                    else if (ct == "eller")   addToken(TokenType.Or, "||");
                    else if (ct == "klar" || ct == "klart" || ct == "färdig" || ct == "färdigt" || ct == "slut") addToken(TokenType.Done);
                    else if (ct == "mindre")  addToken(TokenType.Operator, '<');
                    else if (ct == "större")  addToken(TokenType.Operator, '>');
                    else if (ct == "upphöjt") addToken(TokenType.Operator, '^');
                    else if (ct == "lika")    addToken(TokenType.Equal, '=');
                    else if (ct == "plus"      || ct == "adderat"      )  addToken(TokenType.Operator, '+');
                    else if (ct == "minus"     || ct == "subtraherat"  )  addToken(TokenType.Operator, '-');
                    else if (ct == "genom"     || ct == "dividerat"    )  addToken(TokenType.Operator, '/');
                    else if (ct == "gånger"    || ct == "multiplicerat")  addToken(TokenType.Operator, '*');
                    else if (ct == "inte"      || ct == "icke")  addToken(TokenType.Operator, '!');
                    else if (ct == "använd") addToken(TokenType.Action); // använd * från "./mattematik"
                    else if (ct == "konstant") addToken(TokenType.Action);  // konstant pi = 3.1415926535897932384626433832
                    #endregion
                    #region Strings, numbers and literals
                    else if (ct.Trim() != string.Empty)
                    {
                        bool ctIsString = ( ct.StartsWith("\"") && ct.EndsWith("\"") ) || ( ct.StartsWith("'") && ct.EndsWith("'") );
                        // Literal
                        double numericValue;
                        if (!ctIsString && double.TryParse(ct.Replace(".", ","), out numericValue))
                        {
                            addToken(TokenType.Expression, numericValue, typeof(double));
                        }
                        else if (ctIsString)
                        {
                            if (ct.StartsWith("\""))
                                addToken(TokenType.String, ct.TrimStart('"').TrimEnd('"'), typeof(string));
                            else if (ct.StartsWith("'"))
                                addToken(TokenType.String, ct.TrimStart('\'').TrimEnd('\''), typeof(string));
                            else
                                Error.General("Fel formatering för sträng på rad " + line + " kolumn " + column);
                        }
                        else
                        {
                            addToken(TokenType.Literal, ct, null);
                            ct = string.Empty;
                        }
                    }
                    #endregion
                    #region Empty String
                    else if (ct.Trim() == string.Empty) ct = string.Empty;
                    #endregion
                    #region Check literals, operations and other one-char tokens
                    if (isSeparator(cc))
                    {
                        switch(cc)
                        {
                            case '(':
                            case ')':
                                addToken(TokenType.Parenthesis, cc, null);
                                break;
                            case '{':
                            case '}':
                                addToken(TokenType.CurlyBrackets, cc, null);
                                break;
                            case ',':
                                addToken(TokenType.Separator, cc, null);
                                break;
                        }
                    }
                    else if (isOperator(ct + cc))
                    {
                        switch(ct + cc)
                        {
                            case "+":
                            case "/":
                            case "*":
                            case "<":
                            case ">":
                            case "!":
                            case "^": addToken(TokenType.Operator, cc, null); break;

                            case "-":
                                if (char.IsDigit(nc) || nc == '.') // -.2 is valid
                                    ct += cc;
                                else
                                    addToken(TokenType.Operator, cc, null);
                                break;

                            case "|": addToken(TokenType.Or, cc, null); break;
                            case "&": addToken(TokenType.And, cc, null); break;
                            case "=": addToken(TokenType.Equal, cc, null); break;
                        }
                    }
                    else
                    {
                        if (cc == '\n')
                        {
                            addToken(TokenType.NewLine, "\\n", null);
                            column = -1; line++;
                            continue;
                        }
                        else if (cc == EOF && nc == EOF) break;
                    }
                    #endregion
                }
                else
                {
                    if (isString)
                        ct += source[i];    // Add raw character
                    else 
                        ct += cc;           // Add the formatted character
                }
            }

            return tokens;
        }

        private AST Parse(List<Token> tokens, bool debug = false)
        {
            AST ast = new AST();



            return ast;
        }

        private ExitFlag Execute(List<Token> tokens, BoundScope parentScope, bool debug, bool title)
        {
            BoundScope localScope = new BoundScope(parentScope);
            #region Local Functions
            int i = 0;
            Token nextToken(bool ignoreWhitespace = true)
            {
                if (i < tokens.Count)
                {
                    Token t = tokens[i++];
                    if (t.Type == TokenType.Comment
                        || (ignoreWhitespace && t.Type == TokenType.WhiteSpace)) return nextToken(ignoreWhitespace);
                    else return t;
                }
                return null;
            }
            Token peekToken(int j = 0) // j is the offset
            {
                if (i < tokens.Count)
                {
                    Token t = tokens[i+j];
                    if (t.Type == TokenType.Comment) return peekToken(++j);
                    else return t;
                }
                return null;
            }
            List<Token> getExpression()
            {
                List<Token> expressions = new List<Token>();
                Token t = nextToken();
                while(t != null && (t.Type == TokenType.Expression || t.Type == TokenType.Operator || t.Type == TokenType.Boolean || t.Type == TokenType.Literal || t.Type == TokenType.String || t.Type == TokenType.Parenthesis))
                {
                    expressions.Add(t);
                    t = nextToken();
                }
                i--;
                if (expressions.Count == 0) { Error.UnexpectedToken(tokens[i], "Expression"); return null; }
                return expressions;
            }
            List<Token> getStatement()
            {
                List<Token> statement = new List<Token>();
                Token t = nextToken();
                while (t.Type == TokenType.Expression || t.Type == TokenType.Operator || t.Type == TokenType.Boolean || t.Type == TokenType.Literal || t.Type == TokenType.String || t.Type == TokenType.Parenthesis || t.Type == TokenType.Separator || t.Type == TokenType.Is || t.Type == TokenType.And || t.Type == TokenType.Or || t.Type == TokenType.Equal)
                {
                    statement.Add(t);
                    t = nextToken();
                }
                i--;
                if (statement.Count == 0) { Error.UnexpectedToken(tokens[i], "påstående"); return null; }
                return statement;
            }
            Branch getBranch()
            {
                List<Token> block = new List<Token>();
                int scope = 0;
                Token t = nextToken();
                if (t.Type != TokenType.Then) { Error.UnexpectedToken(tokens[i], "start of code block"); return null; }
                t = nextToken(); // Börja med nästa token innanför kodblocket
                while (t != null)
                {
                    if (t.Type == TokenType.Done && scope == 0) break;
                    else if (t.Type == TokenType.Then) scope++;
                    else if (t.Type == TokenType.Done) scope--;

                    if (t.Type != TokenType.WhiteSpace && (t.Type != TokenType.NewLine || (t.Type == TokenType.NewLine && block.Count > 0)))
                        block.Add(t);
                    t = nextToken();
                }
                if (block.Count == 0) { Error.UnexpectedToken(tokens[i], "Non-empty code block"); return null; }
                if (new[] { TokenType.WhiteSpace, TokenType.NewLine }.Contains(block.Last().Type)) block.RemoveAt(block.Count - 1);
                return new Branch(block);
            }
            #endregion

            if (debug)
            {
                Console.WriteLine();
                if (title)
                {
                    Functions.WriteTitle(ConsoleColor.Green, ConsoleColor.DarkGreen, "Execution Debugging");
                    Functions.WriteTableTitles(ConsoleColor.Green, ConsoleColor.DarkGreen, "Row:Col", "Action");
                }
            }

            Token ct = Token.Empty;
            object result = null;
            DataType dataType = DataType.Undefined;
            while (ct != null)
            {
                ct = nextToken();
                if (ct == null) break;
                else if (new[] { TokenType.WhiteSpace, TokenType.NewLine, TokenType.And }.Contains(ct.Type)) continue; // Should and be skipped?
                else if (ct.Type == TokenType.Literal)
                {
                    // functionName param1 param2 =
                    //      ...
                    // # or
                    // variable = ...

                    // variable == variable()

                    Token literal = ct;

                    List<Token> rawParameters = new List<Token>();
                    Token nxt = nextToken();
                    if (nxt != null)
                    {
                        while (!new[] { TokenType.Equal, TokenType.And, TokenType.NewLine }.Contains(nxt.Type))
                        {
                            rawParameters.Add(nxt);
                            nxt = nextToken();
                            if (nxt == null) nxt = new Token("Newline", TokenType.NewLine, null, null, literal.Line, literal.Column);
                        }
                    }
                    switch(nxt.Type)
                    {
                        case TokenType.And:
                        case TokenType.NewLine:
                            // Function call: literal(...parameters)
                            List<Token> statement = getStatement();

                            if (localScope.TryInvoke(literal, rawParameters, (Function f) =>
                            {
                                if (debug) Functions.WriteMessage(literal.Line, literal.Column, "Invokation", "Calling Function " + f);
                                Execute(f.Block, localScope, debug, false);
                            }, debug) && debug) Functions.WriteMessage(literal.Line, literal.Column, "Invokation", "Function was successfully invoked!");
                            else
                            {
                                if (debug) Error.Undefined(literal, $"could not find function ");
                                return ExitFlag.FatalError;
                            }
                            break;
                        case TokenType.Equal:
                            // nxt is an equal sign.
                            if (rawParameters.Count > 0)
                            {
                                // Function declaration

                            }
                            else
                            {
                                // A parameterless function or variable initialization. Variables can be invoked as parameterless functions to return its value, but this is unrecommended and is assumed by the compiler.
                                List<Token> expression = getExpression();
                                dataType = EvaluateExpression(expression, localScope, out result);
                                if (dataType != DataType.Undefined)
                                {
                                    Variable v = new Variable(literal.Name, result, dataType);
                                    if (parentScope.TryDeclare(v))
                                    {
                                        if (debug) Functions.WriteLineVariableMessage(ct.Line, ct.Column, "Variable Initialization", v, ConsoleColor.Cyan, ConsoleColor.Yellow);
                                    }
                                    else
                                    {
                                        if (parentScope.TryReassign(v))
                                        {
                                            if (debug) Functions.WriteLineVariableMessage(ct.Line, ct.Column, "Variable Re-assignment", v, ConsoleColor.Cyan, ConsoleColor.Yellow);
                                        }
                                        else
                                        {
                                            Error.FailedToDeclare(literal);
                                            return ExitFlag.SyntaxError;
                                        }
                                    }
                                }
                                else
                                {
                                    Error.UnexpectedToken(literal, "definable value");
                                    return ExitFlag.SyntaxError;
                                }
                            }
                            break;
                    }

                    
                }
                else if (ct.Type == TokenType.If)
                {
                    List<Token> statement = getStatement(); 
                    Branch branch = getBranch();
                    dataType = EvaluateExpression(statement, localScope, out result);

                    if (dataType != DataType.Undefined)
                    {
                        if ((dataType == DataType.Number && (double)result != 0) ||
                            (dataType == DataType.String) ||
                            (dataType == DataType.Boolean && (bool)result))
                        {
                            if (debug) Functions.WriteMessage(ct.Line, ct.Column, "Branching", $"Entering {branch}. '" + Functions.TokenListToString(statement) + "' was true.", ConsoleColor.Yellow);
                            Execute(branch.Block, localScope, debug, false);
                        }
                        else if (debug) Functions.WriteMessage(ct.Line, ct.Column, "Branching", $"Skipping {branch}. '" + Functions.TokenListToString(statement) + "' was false.", ConsoleColor.Yellow);
                    }
                    else
                    {
                        Error.WithExpected("Invalid statement: " + Functions.TokenListToString(statement), "calculable truth value");
                        return ExitFlag.SyntaxError;
                    }
                }
                else
                {
                    Error.UnexpectedToken(ct, "instruction with context");
                    return ExitFlag.SyntaxError;
                }
            }
            return ExitFlag.Successfull;
        }
        private void PrintTokens(List<Token> tokens)
        {
            Functions.WriteTitle(ConsoleColor.Cyan, ConsoleColor.DarkCyan, "List of Tokens");
            Functions.WriteTableTitles(ConsoleColor.Cyan, ConsoleColor.DarkCyan, "Row:Col", "Value");

            for (int i = 0; i < tokens.Count; i++)
            {
                ConsoleColor colr = ConsoleColor.Yellow;
                if (tokens[i].Type == TokenType.Comment)
                    colr = ConsoleColor.DarkGreen;
                Functions.WriteLineMessage(tokens[i].Line, tokens[i].Column, tokens[i].Type.ToString(), tokens[i].ToString(), colr);
            }
            Console.WriteLine("Tokens: " + tokens.Count);
            Console.WriteLine();
        }
        private DataType EvaluateExpression(List<Token> expression, BoundScope localScope, out object result)
        {
            if (expression == null)
            {
                result = 0;
                return DataType.Undefined;
            }

            if (expression.Count > 2)
            {
                // Strip away wrapping separators
                while (expression.Count(e => e.Type == TokenType.Parenthesis) >= 2 &&
                     expression[0].Type == TokenType.Parenthesis && (char)expression[0].Value == '(' &&
                     expression[expression.Count - 1].Type == TokenType.Parenthesis && (char)expression[expression.Count - 1].Value == ')')
                {
                    // If the expression is wrapped with two parentathes, remove them (it's the same thing). This will otherwise cause a weird bug
                    expression.RemoveAt(0);
                    expression.RemoveAt(expression.Count - 1);
                }

                // Replace any text-code with mathematical symbols
                for (int i = 0; i < expression.Count; i++)
                {
                    switch(expression[i].Type)
                    {
                        case TokenType.Is:
                            if (i + 1 < expression.Count && expression[i + 1].Type == TokenType.Equal)
                            {
                                expression.RemoveAt(i);
                                i--;
                            }
                            break;
                    }
                }
            }
            else if (expression.Count == 2)
            {
                if (expression[0].Type == TokenType.Operator && (char)expression[0].Value == '-' &&
                    expression[1].Type == TokenType.Expression && expression[1].ValueType == typeof(double))
                {
                    result = (double)expression[1].Value * -1;
                    return DataType.Number;
                }
            }
            if (expression.Count == 1)
            {
                Token e = expression[0];
                // Check for literals and therefore variables and functions
                if (e.Type == TokenType.Literal)
                {
                    if (localScope.TryLookup(e.Name, out Variable v))
                    {
                        result = v.Value;
                        return v.Type;
                    }
                    else
                    {
                        Error.Undefined(e, "variable");
                        result = 0;
                        return DataType.Undefined;
                    }
                }
                else if (e.Type == TokenType.String)
                {
                    result = e.Value;
                    return DataType.String;
                }
                else
                {
                    result = e.Value;
                }

                switch (e.Value)
                {
                    case string v: return DataType.String;
                    case int v1:
                    case long v2:
                    case float v3:
                    case double v4: return DataType.Number;
                    case bool v: return DataType.Boolean;
                    default: return DataType.Undefined;
                }
            }

            result = 0;
            List<Token> newExpression;
            Token expressionValue = Token.Empty;

            for (int i = 0; i < orderOfOperations.Length; i++)
            {
                if (expression.Count == 1) break;
                string operation = orderOfOperations[i];
                for (int j = 0; j < expression.Count; j++)
                {
                    Token t = expression[j];
                    if ((t.Type == TokenType.Expression
                        || t.Type == TokenType.Operator
                        || t.Type == TokenType.Equal
                        || t.Type == TokenType.Or
                        || t.Type == TokenType.And) && t.Value.ToString().Equals(operation))
                    {
                        if (!(j > 0 && j < expression.Count))
                        {
                            Error.UnexpectedToken(t, "expressions on both sides of the operator");
                            return DataType.Undefined;
                        }

                        int k = 1;
                        int expressionStart = -1;
                        int expressionEnd = -1;
                        int parenthisesDepth = 0;

                        // Get LHS
                        List<Token> LHS = new List<Token>();
                        Token temp_t = Token.Empty;
                        while (j - k >= 0 && (temp_t.Type == TokenType.Expression || temp_t.Type == TokenType.Operator || temp_t.Type == TokenType.Parenthesis || temp_t.Type == TokenType.Undefined))
                        {
                            temp_t = expression[j - k];
                            if (isOperator(temp_t.Value.ToString()) && parenthisesDepth == 0) { k--; break; }
                            else if (temp_t.Type == TokenType.Parenthesis && (char)temp_t.Value == '(')
                            {
                                parenthisesDepth++;
                                if (parenthisesDepth == 0) { k++; continue; } // Do not add token
                            }
                            else if (temp_t.Type == TokenType.Parenthesis && (char)temp_t.Value == ')')
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
                        while (j + k < expression.Count && (temp_t.Type == TokenType.Expression || temp_t.Type == TokenType.Operator || temp_t.Type == TokenType.Parenthesis || temp_t.Type == TokenType.Literal || temp_t.Type == TokenType.Undefined))
                        {
                            temp_t = expression[j + k];
                            if (isOperator(temp_t.Value.ToString()) && parenthisesDepth == 0) { k--; break; }
                            else if (temp_t.Type == TokenType.Parenthesis && (char)temp_t.Value == '(') parenthisesDepth++;
                            else if (temp_t.Type == TokenType.Parenthesis && (char)temp_t.Value == ')') parenthisesDepth--;
                            RHS.Add(temp_t);
                            k++;
                        }
                        expressionEnd = j + k;

                        object LHS_Value;
                        DataType LHS_TYPE = EvaluateExpression(LHS, localScope, out LHS_Value);

                        object RHS_Value;
                        DataType RHS_TYPE = EvaluateExpression(RHS, localScope, out RHS_Value);

                        bool insureType(DataType value, DataType expected, string side, bool throwError)
                        {
                            if (value != expected)
                            {
                                if (throwError) Error.WrongType(t, $"the {side} side of the operation sign was not evaluated to a {expected}");
                                return false;
                            }
                            return true;
                        }
                        bool insureTypes(DataType left, DataType right) => insureType(LHS_TYPE, left, "left", true) && insureType(RHS_TYPE, right, "right", true);
                        bool testTypes(DataType left, DataType right) => insureType(LHS_TYPE, left, "left", false) && insureType(RHS_TYPE, right, "right", false);

                        switch (operation)
                        {
                            // Aritmetic expressions
                            case "/":
                                if (insureTypes(DataType.Number, DataType.Number)) expressionValue = new Token("EXPR_DIV", TokenType.Expression, (double)LHS_Value / (double)RHS_Value, typeof(double), t.Line, t.Column);
                                else return DataType.Undefined;
                                break;

                            case "*":
                                if (insureTypes(DataType.Number, DataType.Number)) expressionValue = new Token("EXPR_MUL", TokenType.Expression, (double)LHS_Value * (double)RHS_Value, typeof(double), t.Line, t.Column);
                                else return DataType.Undefined;
                                break;

                            case "+":
                                if (testTypes(DataType.Number, DataType.Number)) expressionValue = new Token("EXPR_ADD_NUM", TokenType.Expression, (double)LHS_Value + (double)RHS_Value, typeof(double), t.Line, t.Column);
                                else expressionValue = new Token("EXPR_ADD_STR", TokenType.Expression, LHS_Value.ToString() + RHS_Value.ToString(), typeof(string), t.Line, t.Column);
                                break;

                            case "-":
                                if (insureTypes(DataType.Number, DataType.Number)) expressionValue = new Token("EXPR_SUB", TokenType.Expression, (double)LHS_Value - (double)RHS_Value, typeof(double), t.Line, t.Column);
                                else return DataType.Undefined; 
                                break;

                            case "^":
                                if (insureTypes(DataType.Number, DataType.Number)) expressionValue = new Token("EXPR_POW", TokenType.Expression, Math.Pow((double)LHS_Value, (double)RHS_Value), typeof(double), t.Line, t.Column);
                                else return DataType.Undefined;
                                break;

                            case "&":
                                if (insureTypes(DataType.Number, DataType.Number)) expressionValue = new Token("EXPR_BIT_AND", TokenType.Expression, (long)LHS_Value & (long)RHS_Value, typeof(long), t.Line, t.Column);
                                else return DataType.Undefined;
                                break;

                            // Boolean expressions
                            case "=":
                                expressionValue = new Token("EXPR_EQ", TokenType.Boolean, LHS_Value.Equals(RHS_Value), typeof(bool), t.Line, t.Column);
                                break;

                            case "==":
                                expressionValue = new Token("EXPR_STRICT_EQ", TokenType.Boolean, insureType(RHS_TYPE, LHS_TYPE, "right", false) && LHS_Value.Equals(RHS_Value), typeof(bool), t.Line, t.Column);
                                break;
                            case "&&":
                                if (insureTypes(DataType.Boolean, DataType.Boolean)) expressionValue = new Token("EXPR_LOGIC_AND", TokenType.Expression, (bool)LHS_Value && (bool)RHS_Value, typeof(long), t.Line, t.Column);
                                else return DataType.Undefined;
                                break;
                            case "||":
                                if (insureTypes(DataType.Boolean, DataType.Boolean)) expressionValue = new Token("EXPR_LOGIC_OR", TokenType.Expression, (bool)LHS_Value || (bool)RHS_Value, typeof(long), t.Line, t.Column);
                                else return DataType.Undefined; 
                                break;

                            default:
                                Error.UnexpectedToken(new Token(operation, TokenType.Expression, t.Value, t.ValueType, t.Line, t.Column), "valid operation");
                                return DataType.Undefined;
                        }

                        newExpression = new List<Token>();
                        for (int l = 0; l < expression.Count; l++)
                        {
                            if (l == expressionStart)
                            {
                                newExpression.Add(expressionValue);
                            }
                            else if (l > expressionStart && l <= expressionEnd) continue;
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
            switch(expressionValue.Value)
            {
                case string v: return DataType.String;
                case int v1:
                case long v2:
                case float v3:
                case double v4: return DataType.Number;
                case bool v: return DataType.Boolean;
                default: return DataType.Undefined;
            }
        }

        private Token FindToken(Token[] selection, TokenType? type, string name, object value)
        {
            for (int i = 0; i < selection.Length; i++)
            {
                bool matches_type     = type.HasValue  ? selection[i].Type == type   : true;
                bool matches_name     = !string.IsNullOrEmpty(name)  ? selection[i].Name == name : true;
                bool matches_value    = value != null ? selection[i].Value == value : true;
                
                if (matches_type && matches_name && matches_value) return selection[i];
            }
            return null;
        }
    }
}