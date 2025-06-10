using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

[CreateAssetMenu(fileName = "StructureData", menuName = "Scriptable Object/StructureData", order = int.MaxValue)]
public class StructureData : ScriptableObject
{
    public StructureType type;

    public BasicStructureType basicType;
    public OreStructureType oreType;
    public GuardStructureType guardType;
    public TrapStructureType trapType;
    public BuffStructureType buffType;

    public Sprite icon;

    public string nameKey;
    public string descriptionKey;

    public int[] upgradeCoin;
    public int[] upgradeEnergy;
    public int purcahseLimit = -1;

    public float[] argment1;
    public float[] argment2;
}