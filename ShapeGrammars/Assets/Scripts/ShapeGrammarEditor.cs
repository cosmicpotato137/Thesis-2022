using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// custom unity editor for ShapeGrammarDriver

[CustomEditor(typeof(ShapeGrammarDriver))]
public class ShapeGrammarDriverEditor : Editor
{
    ShapeGrammarDriver sgd;

    private void OnEnable()
    {
        sgd = target as ShapeGrammarDriver;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        // UI buttons for calling driver methods
        if (GUILayout.Button("Generate Mesh"))
            sgd.GenerateMesh();
        if (GUILayout.Button("Clear Mesh"))
            sgd.ClearMesh();
        if (GUILayout.Button("Parse Grammar"))
            sgd.ParseGrammar();
    }
}
