using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Player))]
public class PlayerScriptEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        Player myTarget = (Player)target;

        if(GUILayout.Button("Inject"))
        {
            myTarget.Inject();
        }
    }
}
