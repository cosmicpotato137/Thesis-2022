using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

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

        if (GUILayout.Button("Generate Mesh"))
            sgd.GenerateMesh();
        if (GUILayout.Button("Clear Mesh"))
            sgd.ClearMesh();
        if (GUILayout.Button("Recompile Parser"))
            sgd.Init();
    }
}
