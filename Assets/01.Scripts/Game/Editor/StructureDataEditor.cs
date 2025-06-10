using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static Define;

[CustomEditor(typeof(StructureData))]
public class StructureDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SerializedObject so = serializedObject;
        so.Update();

        // StructureType 필드 표시
        SerializedProperty typeProp = so.FindProperty("type");
        EditorGUILayout.PropertyField(typeProp);

        // StructureType에 따라 관련 필드만 표시
        StructureType type = (StructureType)typeProp.enumValueIndex;

        switch (type)
        {
            case StructureType.Basic:
                EditorGUILayout.PropertyField(so.FindProperty("basicType"));
                break;
            case StructureType.Ore:
                EditorGUILayout.PropertyField(so.FindProperty("oreType"));
                break;
            case StructureType.Guard:
                EditorGUILayout.PropertyField(so.FindProperty("guardType"));
                break;
            case StructureType.Trap:
                EditorGUILayout.PropertyField(so.FindProperty("trapType"));
                break;
            case StructureType.Buff:
                EditorGUILayout.PropertyField(so.FindProperty("buffType"));
                break;
        }

        EditorGUILayout.PropertyField(so.FindProperty("nameKey"));
        EditorGUILayout.PropertyField(so.FindProperty("descriptionKey"));
        EditorGUILayout.PropertyField(so.FindProperty("upgradeCoin"), true);
        EditorGUILayout.PropertyField(so.FindProperty("upgradeEnergy"), true);
        EditorGUILayout.PropertyField(so.FindProperty("purcahseLimit"));

        so.ApplyModifiedProperties();
    }
}
