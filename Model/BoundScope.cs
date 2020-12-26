using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace SvenskaInstruktioner.Model
{
    class BoundScope
    {
        internal Dictionary<string, Variable> _variables = new Dictionary<string, Variable>();
        internal Dictionary<string, Function> _functions = new Dictionary<string, Function>();

        public readonly BoundScope Parent;

        public string Name { get; }

        public BoundScope(BoundScope parent, string name = null)
        {
            _variables = new Dictionary<string, Variable>();
            _functions = new Dictionary<string, Function>();
            Parent = parent;
            Name = name;
        }


        public virtual bool TryInvoke(Token functionToken, List<Token> parameters, Action<Function> onFound, bool debug)
        {
            if (!_functions.ContainsKey(functionToken.Value.ToString().ToUpper()))
            {
                if (Parent.TryInvoke(functionToken, parameters, onFound, debug)) return true;
                return false;
            }
            else
            {
                onFound(_functions[functionToken.Value.ToString().ToUpper()]);
                return true;
            }
        }

        public bool TryDeclare(Variable variable)
        {
            if (_variables.ContainsKey(variable.Name.ToUpper()))
                return false;

            _variables.Add(variable.Name.ToUpper(), variable);
            return true;
        }
        public bool TryDeclare(Function function)
        {
            if (_functions.ContainsKey(function.Name.ToUpper()))
                return false;

            _functions.Add(function.Name.ToUpper(), function);
            return true;
        }

        /// <summary>
        /// Re-Assigns a variable in the current scope
        /// </summary>
        /// <param name="variable">The variable to re-assign containt new value</param>
        /// <returns>If the assignment is successfull</returns>
        public bool TryReassign(Variable variable)
        {
            if (_variables.ContainsKey(variable.Name.ToUpper()))
            {
                _variables[variable.Name.ToUpper()] = variable;
                return true;
            }
            else return false;
        }

        public bool TryLookup(string name, out Variable variable)
        {
            if (_variables.TryGetValue(name.ToUpper(), out variable))
                return true;

            if (Parent == null)
                return false;

            return Parent.TryLookup(name.ToUpper(), out variable);
        }

        public ImmutableArray<Variable> GetDeclaredVariables() => _variables.Values.ToImmutableArray();

        public override string ToString() => '[' + (Name != null ? Name : "BoundScope") + ']';
    }
}
