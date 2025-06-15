using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using static Define;

[CustomEditor(typeof(CharactorData))]
public class CharactorDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SerializedObject so = serializedObject;
        so.Update();

        // CharactorType 필드 표시
        SerializedProperty typeProp = so.FindProperty("charactorType");
        EditorGUILayout.PropertyField(typeProp);

        // nameKey, descriptionKey 필드 표시
        EditorGUILayout.PropertyField(so.FindProperty("nameKey"));
        EditorGUILayout.PropertyField(so.FindProperty("descriptionKey"));

        // purchaseType 필드 표시
        SerializedProperty purchaseTypeProp = so.FindProperty("purchaseType");
        EditorGUILayout.PropertyField(purchaseTypeProp);

        CharactorPurchaseType purchaseType = (CharactorPurchaseType)purchaseTypeProp.enumValueIndex;

        // purchaseType에 따라 필드 표시
        if (purchaseType == CharactorPurchaseType.Gem)
        {
            EditorGUILayout.PropertyField(so.FindProperty("requireGem"));
        }
        else if (purchaseType == CharactorPurchaseType.Iap)
        {
            EditorGUILayout.PropertyField(so.FindProperty("iapKey"));
        }

        so.ApplyModifiedProperties();
    }
}
