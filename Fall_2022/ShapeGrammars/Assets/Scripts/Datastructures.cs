using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace cosmicpotato.Datastructures
{
    public class Node<T>
    {
        public T Value;
        public Node<T> Prev;
        public Node<T> Next;

        public Node(T value)
        {
            Value = value;
        }
    }

    public class TreeNode<T>
    {
        public T Value;
        public Node<T> Parent;
        public Node<T>[] Children;

        public TreeNode(T value)
        {
            Value = value;
        }
    }

    public class Tree<T>
    {
        public TreeNode<T> Root;

        public Tree(TreeNode<T> root)
        {
            Root = root;
        }
    }
}

