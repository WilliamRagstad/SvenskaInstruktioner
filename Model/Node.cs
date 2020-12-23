using System;
using System.Collections.Generic;
using System.Text;

namespace SvenskaInstruktioner.Model
{
    enum NodeType
    {
        Number,
        String,
        Operation,
        Root
    }
    class Node
    {
        public Node(NodeType type) : this(type.ToString(), type) { }
        public Node(string name, NodeType type) : this(name, type, new List<Node>()) { }
        public Node(string name, NodeType type, List<Node> children)
        {
            Name = name;
            Type = type;
            Children = children;
        }

        public string Name { get; }
        public NodeType Type { get; }
        public List<Node> Children { get; }
    }
}
