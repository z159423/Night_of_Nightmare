using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BoostData))]
public class BoostDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SerializedObject so = serializedObject;
        so.Update();

        EditorGUILayout.PropertyField(so.FindProperty("type"));
        EditorGUILayout.PropertyField(so.FindProperty("nameKey"));
        EditorGUILayout.PropertyField(so.FindProperty("descriptionKey"));
        EditorGUILayout.PropertyField(so.FindProperty("price"));
        EditorGUILayout.PropertyField(so.FindProperty("icon"));
        EditorGUILayout.PropertyField(so.FindProperty("argment1"), true);

        so.ApplyModifiedProperties();
    }
}
