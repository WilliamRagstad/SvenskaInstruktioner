using System;
using System.Collections.Generic;
using System.Text;

namespace SvenskaInstruktioner.Model
{
    class Function
    {
        public Function(string name, List<Token> block)
        {
            Name = name;
            Block = block;
            ID = Guid.NewGuid();
        }

        public Guid ID { get; }
        public string Name { get; }
        public List<Token> Block { get; }

        public override string ToString() {
            return $"<{ID}: Function '{Name}'>";
        }
    }
}
