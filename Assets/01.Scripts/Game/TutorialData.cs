using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TutorialData", menuName = "Scriptable Object/TutorialData", order = int.MaxValue)]
public class TutorialData : ScriptableObject
{
    public enum JubjectType
    {
        LayBed,
        PurchaseStructure,
        UpgradeStructure
    }

    public JubjectType jubjectType;
    public Define.StructureType upgradeType;
    public Define.StructureType purchaseType;
    public int purchaseCount = 0;
    public int upgradeLevel = 0;
}
