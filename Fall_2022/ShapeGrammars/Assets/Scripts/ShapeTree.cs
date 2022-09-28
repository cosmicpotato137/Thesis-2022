using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShapeTreeNode<T>
{
    //public ShapeTreeNode<T> Parent;
    //public List<ShapeTreeNode<T>> Children;
    T Value;
}

public class ShapeTree<T>
{
    Dictionary<ShapeTreeNode<T>, LinkedList<ShapeTreeNode<T>>> Adj;


}
