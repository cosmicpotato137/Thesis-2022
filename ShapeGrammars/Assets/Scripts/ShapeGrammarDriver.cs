using System;
using System.Collections.Generic;
using UnityEngine;
using cosmicpotato.Scope;
//using Parabox.CSG;

[ExecuteInEditMode]
public class ShapeGrammarDriver : MonoBehaviour
{
    public List<GameObject> shapes;
    public List<SGGeneratorBase> grammarRules;

    [Header("Parser")]
    public TextAsset textFile;

    protected Dictionary<string, GameObject> shapeDict;
    protected LinkedList<GameObject> objects;

    protected ShapeGrammarParser parser;

    public void OnEnable()
    {
        // initialize dictionary of possible shapes
        shapeDict = new Dictionary<string, GameObject>();
        objects = new LinkedList<GameObject>();

        // function definitions for the parser
        parser = new ShapeGrammarParser();
        parser.CompileParser();

        // place shape
        Action<SGProdGen, string> p = (parent, name) =>
        {
            parent.gameObjects.Add(PlaceShape(name, parent.scope));
        };
        parser.AddGenerator(new SGGenerator<string>("PlaceShape", p));

        // translate
        Action<SGProdGen, float, float, float> t =
            (parent, x, y, z) => parent.scope.Translate(new Vector3(x, y, z));
        parser.AddGenerator(new SGGenerator<float, float, float>("T", t));

        // translate local
        Action<SGProdGen, float, float, float> tl = (parent, x, y, z) =>
        {
            Vector3 v = Vector3.Scale(new Vector3(x, y, z), parent.scope.scale);
            parent.scope.Translate(v);
        };
        parser.AddGenerator(new SGGenerator<float, float, float>("TL", tl));

        // rotate
        Action<SGProdGen, float, float, float> r = (parent, x, y, z) => 
        { 
                parent.scope.Rotate(new Vector3(x, y, z)); 
        };
        parser.AddGenerator(new SGGenerator<float, float, float>("R", r));

        // scale
        Action<SGProdGen, float, float, float> s =
            (parent, x, y, z) => { 
                parent.scope.Scale(new Vector3(x, y, z)); 
            };
        parser.AddGenerator(new SGGenerator<float, float, float>("S", s));

        // set scale
        Action<SGProdGen, float, float, float> ss =
            (parent, x, y, z) => parent.scope.SetScale(new Vector3(x, y, z));
        parser.AddGenerator(new SGGenerator<float, float, float>("SS", s));

        // matrix stack ops
        Action<SGProdGen> push = (parent) => parent.SaveTransform();
        parser.AddGenerator(new SGGenerator("Push", push));
        Action<SGProdGen> pop = (parent) => parent.LoadTransform();
        parser.AddGenerator(new SGGenerator("Pop", pop));

        // subdivide scope
        Action<SGProdGen, int, Axis, SGRule[]> subdiv = (parent, divs, axis, rules) =>
        {
            Scope[] scopes = parent.scope.Subdivide(divs, axis);
            for (int i = 0; i < scopes.Length && i < rules.Length; i++)
            {
                if (rules[i].GetType() != typeof(SGProdGen))
                    throw new ArrayTypeMismatchException($"List value is not of type SGProdGen: {rules[i].GetType()}");
                var prodGen = (SGProdGen)rules[i].Copy();
                prodGen.scope = scopes[i];
                prodGen.parent = parent;
                prodGen.depth = parent.depth + 1;
                prodGen.adoptParentScope = false;
                SGProducer.opQueue.AddLast(prodGen);
            }
        };
        parser.AddGenerator(new SGGenerator<int, Axis, SGRule[]>("Subdiv", subdiv));

        // do something at a given depth
        Action<SGProdGen, int, SGRule[]> dad = (parent, depth, rules) =>
        {
            if (parent.depth >= depth)
            {
                for (int i = 0; i < rules.Length; i++)
                {
                    rules[i].parent = parent;
                    SGProducer.opQueue.AddLast(rules[i].Copy());
                }
            }
        };
        parser.AddGenerator(new SGGenerator<int, SGRule[]>("AtDepth", dad));

        // operate on the component faces of an object
        // todo: test this
        Action<SGProdGen, SGProdGen> getFaces = (parent, rule) =>
        {
            if (parent.gameObjects.Count < 0)
            {
                Debug.Log("Parent GameObject is null");
                return;
            }
            List<GameObject> newList = new List<GameObject>();
            for (int i = 0; i < parent.gameObjects.Count; i++)
            {
                Mesh mesh = parent.gameObjects[i].GetComponent<MeshFilter>().sharedMesh;
                Mesh[] meshes = new Mesh[mesh.triangles.Length / 3];
                for (int j = 0; j < mesh.triangles.Length; j += 6)
                {
                    var idx = mesh.triangles[j];
                    Mesh m = new Mesh();
                    Vector3[] verts = new Vector3[6];
                    verts[0] = mesh.vertices[mesh.triangles[j]];
                    verts[1] = mesh.vertices[mesh.triangles[j + 1]];
                    verts[2] = mesh.vertices[mesh.triangles[j + 2]];
                    verts[3] = mesh.vertices[mesh.triangles[j + 3]];
                    verts[4] = mesh.vertices[mesh.triangles[j + 4]];
                    verts[5] = mesh.vertices[mesh.triangles[j + 5]];
                    m.vertices = verts;
                    m.triangles = new int[] { 0, 1, 2, 3, 4, 5 };
                    m.Optimize();
                    m.OptimizeIndexBuffers();
                    m.RecalculateNormals();
                    m.RecalculateBounds();

                    GameObject g = new GameObject(parent.gameObjects[i].name + "_" + Convert.ToString(j));
                    g.transform.SetParent(parent.gameObjects[i].transform.parent);
                    var f = g.AddComponent<MeshFilter>();
                    f.mesh = m;
                    g.AddComponent<MeshRenderer>();
                    g.GetComponent<Renderer>().material = parent.gameObjects[i].GetComponent<Renderer>().sharedMaterial;

                    SGProdGen r = (SGProdGen)rule.Copy();
                    r.gameObjects.Add(g);
                    r.parent = parent;
                    r.adoptParentScope = false;
                    SGProducer.opQueue.AddLast(r);
                }
                DestroyImmediate(parent.gameObjects[i]);
                parent.gameObjects.RemoveAt(i);
            }
        };
        //parser.AddGenerator(new SGGenerator<SGProdGen>("DecFaces", getFaces));

        parser.Parse(textFile);
    }

    // run the parser on a shape grammar file
    internal void ParseGrammar()
    {
        var res = parser.Parse(textFile);
        if (!res.Success)
        {
            Debug.Log("Grammar compilation failure.");
            foreach (var e in res.Errors)
            {
                Debug.LogWarning(e.Description);
            }
        }
    }

    // generate a mesh based on the parsed shape grammar
    public void GenerateMesh()
    {
        ClearMesh();
        foreach (GameObject s in shapes)
        {
            if (!shapeDict.ContainsKey(s.name))
                shapeDict.Add(s.name, s);
        }

        parser.RunShapeGrammar(9, 1000000);
    }

    // clear all generated meshes
    public void ClearMesh()
    {
        // destroy all objects in the tree and the hierarchy
        while (objects.Count > 0)
        {
            DestroyImmediate(objects.First.Value);
            objects.RemoveFirst();
        }

        // destroy any leftover objects in the hierarchy
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }
    }

    // place a shape at the current scope
    public GameObject PlaceShape(string str, Scope scope)
    {
        if (!shapeDict.ContainsKey(str))
        {
            Debug.LogError($"Shape not found: {str}");
            return null;
        }

        GameObject g = Instantiate(shapeDict[str], transform);
        //Matrix4x4 m = Matrix4x4.TRS(transform.localPosition, transform.localRotation, transform.localScale);
        var s = new Scope(scope);
        s.Translate(g.transform.localPosition);
        s.Rotate(g.transform.localRotation);
        s.Scale(g.transform.localScale);
        g.transform.FromScope(s);

        objects.AddLast(g);
        return g;
    }

}
