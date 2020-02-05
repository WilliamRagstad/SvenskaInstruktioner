using System;
using System.Collections.Generic;
using System.Text;

namespace SvenskaInstruktioner
{
    enum NodeType
    {
        Root,
        Number,
        String,
        Operation
    }
    class Node
    {
        public Node(NodeType type)
        {
            Name = type.ToString();
            Type = type;
            Children = new List<Node>();
        }
        public Node(string name, NodeType type)
        {
            Name = name;
            Type = type;
            Children = new List<Node>();
        }
        public Node(string name, NodeType type, List<Node> children)
        {
            Name = name;
            Type = type;
            Children = children;
        }

        public global::System.String Name { get; }
        public NodeType Type { get; }
        public List<Node> Children { get; }
    }
}
