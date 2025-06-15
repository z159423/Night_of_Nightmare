using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

[CreateAssetMenu(fileName = "CharactorData", menuName = "Scriptable Object/CharactorData", order = int.MaxValue)]
public class CharactorData : ScriptableObject
{
    public CharactorType charactorType;

    public string nameKey;
    public string descriptionKey;
    public CharactorPurchaseType purchaseType;
    public int requireGem;
    public string iapKey;
}
