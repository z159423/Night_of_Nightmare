using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TutorialData))]
public class TutorialDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var jubjectTypeProp = serializedObject.FindProperty("jubjectType");
        var upgradeTypeProp = serializedObject.FindProperty("upgradeType");
        var purchaseTypeProp = serializedObject.FindProperty("purchaseType");

        EditorGUILayout.PropertyField(jubjectTypeProp);

        var jubjectType = (TutorialData.JubjectType)jubjectTypeProp.enumValueIndex;

        // LayBed: 아무것도 안 보임
        // PurchaseStructure: purchaseType만 보임
        // UpgradeStructure: upgradeType만 보임
        switch (jubjectType)
        {
            case TutorialData.JubjectType.PurchaseStructure:
                EditorGUILayout.PropertyField(purchaseTypeProp);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("purchaseCount"));
                break;
            case TutorialData.JubjectType.UpgradeStructure:
                EditorGUILayout.PropertyField(upgradeTypeProp);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("upgradeLevel"));
                break;
        }

        EditorGUILayout.PropertyField(serializedObject.FindProperty("gemRewardCount"));

        serializedObject.ApplyModifiedProperties();
    }
}
