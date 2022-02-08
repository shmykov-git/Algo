using System;
using System.Collections.Generic;
using System.Linq;

namespace Model.Trees
{
    public class Tree<TItem>
    {
        public Node Root;

        public Node CreateNode(Node node = null, TItem value = default)
        {
            var newNode = new Node()  { Value = value };

            if (node == null)
            {
                if (Root != null)
                    throw new ArgumentException("Root is not empty");

                Root = newNode;
            }
            else
            {
                node.Children ??= new List<Node>();
                newNode.Parent = node;
                node.Children.Add(newNode);
            }

            return newNode;
        }

        public IEnumerable<(Node node, int level)> LeftVisit()
        {
            IEnumerable<(Node,int)> Visit(Node node, int level)
            {
                yield return (node, level);
                
                if (node.Children != null)
                {
                    foreach (var v in node.Children.SelectMany(c=>Visit(c, level+1)))
                        yield return v;
                }
            }

            return Visit(Root, 0);
        }

        public class Node
        {
            public TItem Value;
            public Node Parent;
            public List<Node> Children;
        }
    }
}
