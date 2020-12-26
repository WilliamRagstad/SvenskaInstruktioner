using System;
using System.Collections.Generic;
using System.Text;

namespace SvenskaInstruktioner.Model
{
    class Branch
    {
        public Branch(List<Token> block)
        {
            Block = block;
            ID = Guid.NewGuid();
        }

        public List<Token> Block { get; }
        public Guid ID { get; }

        public override string ToString()
        {
            return $"<{ID}: Branch>";
        }
    }
}
