using System;
using System.Collections.Generic;
using UnityEngine;
using Parabox.CSG;

public enum Primitive
{
    Box = 0,
    Sphere
}

[ExecuteInEditMode]
//[RequireComponent(typeof(ShapeGrammarParser))]
public class ShapeGrammarDriver : MonoBehaviour
{
    public List<Shape> shapes;
    public List<SGGeneratorBase> grammarRules;

    [Header("Parser")]
    public TextAsset textFile;

    protected Dictionary<string, Shape> shapeDict;
    protected LinkedList<GameObject> objects;

    protected LinkedList<Matrix4x4> scopeStack;
    //protected Matrix4x4 scope;

    protected ShapeGrammarParser parser;

    public void OnEnable()
    {
        shapeDict = new Dictionary<string, Shape>();
        scopeStack = new LinkedList<Matrix4x4>();
        objects = new LinkedList<GameObject>();
         
        // function definitions for the parser
        parser = new ShapeGrammarParser();
        parser.CompileParser();

        Action<SGProducer, string> p = (parent, name) =>
        {
            PlaceShape(name, parent.scope);
        };
        var a = new SGGenerator<string>("PlaceShape", p);
        parser.AddGenerator(a);

        Action<SGProducer, float, float, float> t = 
            (parent, x, y, z) => parent.scope = parent.scope.Translate(new Vector3(x, y, z));
        parser.AddGenerator(new SGGenerator<float, float, float>("T", t));

        Action<SGProducer, float, float, float> r = 
            (parent, x, y, z) => parent.scope = parent.scope.Rotate(new Vector3(x, y, z));
        parser.AddGenerator(new SGGenerator<float, float, float>("R", r));

        Action<SGProducer, float, float, float> s = 
            (parent, x, y, z) => parent.scope = parent.scope.Scale(new Vector3(x, y, z));
        parser.AddGenerator(new SGGenerator<float, float, float>("S", s));

        Action<SGProducer, float, float, float> ss = 
            (parent, x, y, z) => parent.scope = parent.scope.SetScale(new Vector3(x, y, z));
        parser.AddGenerator(new SGGenerator<float, float, float>("SS", s));

        Action<SGProducer> push = (parent) => parent.SaveTransform();
        parser.AddGenerator(new SGGenerator("Push", push));
        Action<SGProducer> pop = (parent) => parent.LoadTransform();
        parser.AddGenerator(new SGGenerator("Pop", pop));
    }

    public void PlaceShape(string str, Matrix4x4 scope)
    {
        if (!shapeDict.ContainsKey(str))
        {
            Debug.LogError($"Shape not found: {str}");
            return;
        }    

        Shape shape = shapeDict[str];
        GameObject g = new GameObject(shape.name);
        g.transform.FromMatrix(scope);
        g.transform.SetParent(transform);
        g.AddComponent<MeshFilter>().mesh = shape.mesh;
        g.AddComponent<MeshRenderer>();
        g.GetComponent<Renderer>().material = shape.material;

        objects.AddLast(g);
    }

    public void GenerateMesh()
    {
        ClearMesh();
        foreach (Shape s in shapes)
        {
            if (!shapeDict.ContainsKey(s.name))
                shapeDict.Add(s.name, s);
        }


        var res = parser.Parse(textFile);
        if (!res.Success)
        {
            Debug.Log("Grammar compilation failure.");
            foreach (var e in res.Errors)
            {
                Debug.LogWarning(e.Description);
            }
        }

        parser.RunShapeGrammar(9, 1000000);

        // test 1
        //Save();
        //Translate(new Vector3(1, 2, 0));
        //PlaceShape("Box");
        //Load();
        //PlaceShape("Box");
    }

    public void ClearMesh()
    {
        while (objects.Count > 0)
        {
            DestroyImmediate(objects.First.Value);
            objects.RemoveFirst();
        }
    }

}
