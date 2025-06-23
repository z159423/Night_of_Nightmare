using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BoostData", menuName = "Scriptable Object/BoostData", order = int.MaxValue)]

public class BoostData : ScriptableObject
{
    public Define.BoostType type;

    public string nameKey;
    public string descriptionKey;

    public int price;

    public Sprite icon;

    public float[] argment1;
}
