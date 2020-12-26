using SvenskaInstruktioner.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace SvenskaInstruktioner.Model
{
    class GlobalScope : BoundScope
    {
        private Dictionary<string, Variable> _builtin_variables = new Dictionary<string, Variable>();
        private Dictionary<string, Delegate> _builtin_functions = new Dictionary<string, Delegate>();

        #region Built-ins

        #region Functions
        private static void SKRIV(string message)
        {
            Console.WriteLine(message);
        }
        #endregion

        #endregion

        public GlobalScope() : base(null, "Global Scope") {
            LoadBuiltIns();
        }

        private void LoadBuiltIns()
        {
            _builtin_functions["SKRIV"] = new Action<string>(SKRIV);
        }
        public override bool TryInvoke(Token functionToken, List<Token> parameters, Action<Function> onFound, bool debug)
        {
            string fkey = functionToken.Value.ToString().ToUpper();
            if (_functions.ContainsKey(fkey))
            { 
                onFound(_functions[fkey]);
                return true;
            }
            else if (_builtin_functions.ContainsKey(fkey))
            {
                if (debug) Functions.WriteMessage(functionToken.Line, functionToken.Column, "Invokation", "Calling built-in function " + functionToken.Value);
                Delegate function = _builtin_functions[fkey];

                // do not call onFound.
                return true;
            }
            return false;
        }
    }
}
