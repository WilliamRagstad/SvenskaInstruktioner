using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace SvenskaInstruktioner
{
    class BoundScope
    {
        private Dictionary<string, Variable> _variables = new Dictionary<string, Variable>();
        public readonly BoundScope Parent;

        public BoundScope(BoundScope parent)
        {
            _variables = new Dictionary<string, Variable>();
            Parent = parent;
        }

        public bool TryDeclare(Variable variable)
        {
            if (_variables.ContainsKey(variable.Name))
                return false;

            _variables.Add(variable.Name, variable);
            return true;
        }

        /// <summary>
        /// Re-Assigns a variable in the current scope
        /// </summary>
        /// <param name="variable">The variable to re-assign containt new value</param>
        /// <returns>If the assignment is successfull</returns>
        public bool TryReassign(Variable variable)
        {
            if (_variables.ContainsKey(variable.Name))
            {
                _variables[variable.Name] = variable;
                return true;
            }
            else return false;
        }

        public bool TryLookup(string name, out Variable variable)
        {
            if (_variables.TryGetValue(name, out variable))
                return true;

            if (Parent == null)
                return false;

            return Parent.TryLookup(name, out variable);
        }

        public ImmutableArray<Variable> GetDeclaredVariables() => _variables.Values.ToImmutableArray();
    }
}
