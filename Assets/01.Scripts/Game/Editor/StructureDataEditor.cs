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
        SerializedProperty cateogry = so.FindProperty("category");
        EditorGUILayout.PropertyField(cateogry);

        SerializedProperty typeProp = so.FindProperty("structureType");
        EditorGUILayout.PropertyField(typeProp);

        // icon 필드와 미리보기
        SerializedProperty iconProp = so.FindProperty("icon");
        EditorGUILayout.PropertyField(iconProp);
        if (iconProp.objectReferenceValue != null)
        {
            Texture2D tex = AssetPreview.GetAssetPreview(iconProp.objectReferenceValue);
            if (tex != null)
            {
                float aspect = (float)tex.width / tex.height;
                float width = EditorGUIUtility.currentViewWidth * 0.3f;
                float height = width / aspect;
                GUILayout.Label(tex, GUILayout.Width(width), GUILayout.Height(height));
            }
        }

        EditorGUILayout.PropertyField(so.FindProperty("nameKey"));
        EditorGUILayout.PropertyField(so.FindProperty("descriptionKey"));
        EditorGUILayout.PropertyField(so.FindProperty("upgradeCoin"), true);
        EditorGUILayout.PropertyField(so.FindProperty("upgradeEnergy"), true);
        EditorGUILayout.PropertyField(so.FindProperty("purcahseLimit"));
        EditorGUILayout.PropertyField(so.FindProperty("argment1"), true);
        EditorGUILayout.PropertyField(so.FindProperty("argment2"), true);

        so.ApplyModifiedProperties();
    }
}
