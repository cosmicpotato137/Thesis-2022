using System;
using System.Collections.Generic;
using UnityEngine;

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

    [Header("Paerser")]
    public TextAsset textFile;

    protected Dictionary<string, Shape> shapeDict;
    protected LinkedList<GameObject> objects;

    protected LinkedList<Matrix4x4> scopeStack;
    protected Matrix4x4 scope;

    protected ShapeGrammarParser parser;

    public void OnEnable()
    {
        shapeDict = new Dictionary<string, Shape>();
        scopeStack = new LinkedList<Matrix4x4>();
        objects = new LinkedList<GameObject>();

        // function definitions for the parser
        parser = new ShapeGrammarParser();
        parser.CompileParser();

        Action<string> p = (name) => PlaceShape(name);
        var a = new SGGenerator<string>("PlaceShape", p);
        parser.AddGenerator(a);

        Action<float, float, float> f = (x, y, z) => Translate(new Vector3(x, y, z));
        var b = new SGGenerator<float, float, float>("T", f);
        parser.AddGenerator(b);

        Action<float, float, float> r = (x, y, z) => Rotate(new Vector3(x, y, z));
        parser.AddGenerator(new SGGenerator<float, float, float>("R", r));

        Action<float, float, float> s = (x, y, z) => Scale(new Vector3(x, y, z));
        parser.AddGenerator(new SGGenerator<float, float, float>("S", s));

        Action<float, float, float> ss = (x, y, z) => SetScale(new Vector3(x, y, z));
        parser.AddGenerator(new SGGenerator<float, float, float>("SS", s));

        Action push = () => SaveTransform();
        parser.AddGenerator(new SGGenerator("Push", push));
        Action pop = () => LoadTransform();
        parser.AddGenerator(new SGGenerator("Pop", pop));
    }

    public void PlaceShape(string str)
    {
        if (!shapeDict.ContainsKey(str))
        {
            Debug.LogError("Shape not found");
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

        SetScope(Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale));

        var res = parser.Parse(textFile);
        if (res.Success)
            Debug.Log("Grammar compiled successfully.");
        else
        {
            Debug.Log("Grammar compilation failure.");
            foreach (var e in res.Errors)
            {
                Debug.LogWarning(e.Description);
            }
        }

        int i = 0;
        int max = 100;
        SGRule r;
        while (i < max && parser.opQueue.Count > 0)
        {
            r = parser.opQueue.First.Value;
            parser.opQueue.RemoveFirst();
            i++;
            r.Call();
        }

        //parser.opQueue.First.Value.Call();
        //parser.opQueue.RemoveFirst();
        //foreach (SGRule rule in parser.opQueue)
        //{
        //    rule.Call();
        //}

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

    public Matrix4x4 GetScope()
    {
        return scope;
    }

    public void SetScope(Matrix4x4 scope)
    {
        this.scope = scope;
    }

    public void Translate(Vector3 translation)
    {
        scope *= Matrix4x4.TRS(translation, Quaternion.identity, Vector3.one);
    }

    public void Rotate(Vector3 eulerRot)
    {
        scope *= Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(eulerRot), Vector3.one);
    }

    public void Rotate(Quaternion quat)
    {
        scope *= Matrix4x4.TRS(Vector3.zero, quat, Vector3.one);
    }

    public void Scale(Vector3 scale)
    {
        scope *= Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
    }

    public void SetScale(Vector3 scale)
    {
        scope = Matrix4x4.TRS(scope.GetPosition(), scope.GetRotation(), scale);
    }

    public void SaveTransform()
    {
        scopeStack.AddLast(scope);
    }

    public void LoadTransform()
    {
        scope = scopeStack.Last.Value;
        scopeStack.RemoveLast();
    }
}
