using System;
using System.Collections.Generic;
using UnityEngine;
using cosmicpotato.Scope;
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

    protected ShapeGrammarParser parser;

    public void OnEnable()
    {
        shapeDict = new Dictionary<string, Shape>();
        objects = new LinkedList<GameObject>();
         
        // function definitions for the parser
        parser = new ShapeGrammarParser();
        parser.CompileParser();

        // place shape
        Action<SGProdGen, string> p = (parent, name) => PlaceShape(name, parent.scope);
        parser.AddGenerator(new SGGenerator<string>("PlaceShape", p));

        // translate
        Action<SGProdGen, float, float, float> t = 
            (parent, x, y, z) => parent.scope = parent.scope.Translate(new Vector3(x, y, z));
        parser.AddGenerator(new SGGenerator<float, float, float>("T", t));

        // rotate
        Action<SGProdGen, float, float, float> r = 
            (parent, x, y, z) => parent.scope = parent.scope.Rotate(new Vector3(x, y, z));
        parser.AddGenerator(new SGGenerator<float, float, float>("R", r));

        // scale
        Action<SGProdGen, float, float, float> s = 
            (parent, x, y, z) => parent.scope = parent.scope.Scale(new Vector3(x, y, z));
        parser.AddGenerator(new SGGenerator<float, float, float>("S", s));

        // set scale
        Action<SGProdGen, float, float, float> ss = 
            (parent, x, y, z) => parent.scope = parent.scope.SetScale(new Vector3(x, y, z));
        parser.AddGenerator(new SGGenerator<float, float, float>("SS", s));

        // matrix stack ops
        Action<SGProdGen> push = (parent) => parent.SaveTransform();
        parser.AddGenerator(new SGGenerator("Push", push));
        Action<SGProdGen> pop = (parent) => parent.LoadTransform();
        parser.AddGenerator(new SGGenerator("Pop", pop));

        // subdivide scope
        Action<SGProdGen, int, Axis, SGProdGen[]> subdiv = (parent, divs, axis, rules) =>
        {
            Matrix4x4[] scopes = parent.scope.SubdivideScope(divs, axis);
            for (int i = 0; i < scopes.Length && i < rules.Length; i++)
            {
                rules[i].scope = scopes[i];
                rules[i].parent = parent;
                SGProducer.opQueue.AddLast(rules[i].Copy());
            }
        };
        parser.AddGenerator(new SGGenerator<int, Axis, SGProdGen[]>("Subdiv", subdiv));
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
