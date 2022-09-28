using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;
using System;

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
    public List<GeneratorRuleBase> grammarRules;

    [Header("Paerser")]
    public TextAsset textFile;

    protected Dictionary<string, Shape> shapeDict;
    protected Matrix4x4 scope;
    protected LinkedList<Matrix4x4> scopeStack;
    protected LinkedList<GameObject> objects;

    protected ShapeGrammarParser parser;

    public void OnEnable()
    {
        shapeDict = new Dictionary<string, Shape>();
        objects = new LinkedList<GameObject>();
        scopeStack = new LinkedList<Matrix4x4>();

        Action<float, float, float> f = (x, y, z) => Translate(new Vector3(x, y, z));


        parser = new ShapeGrammarParser();
        parser.CompileParser();


        Action func = () => { 
            Translate(new Vector3(1, 1, 1)); 
            PlaceShape("Box"); 
        };
        
        var g = new GeneratorRule("PlaceBox", func);
        parser.rules.Add(g.token, g);
    }

    void PlaceShape(string str)
    {
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

    Matrix4x4 GetScope()
    {
        return scope;
    }

    void SetScope(Matrix4x4 scope)
    {
        this.scope = scope;
    }

    void Translate(Vector3 translation)
    {
        scope *= Matrix4x4.TRS(translation, Quaternion.identity, Vector3.one);
    }

    void Rotate(Vector3 eulerRot)
    {
        scope *= Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(eulerRot), Vector3.one);
    }

    void Rotate(Quaternion quat)
    {
        scope *= Matrix4x4.TRS(Vector3.zero, quat, Vector3.one);
    }

    void Scale(Vector3 scale)
    {
        scope *= Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);
    }

    void Save()
    {
        scopeStack.AddLast(scope);
    }

    void Load()
    {
        scope = scopeStack.Last.Value;
        scopeStack.RemoveLast();
    }
}
