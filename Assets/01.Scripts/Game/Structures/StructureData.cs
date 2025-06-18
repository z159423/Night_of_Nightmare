using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

[CreateAssetMenu(fileName = "StructureData", menuName = "Scriptable Object/StructureData", order = int.MaxValue)]
public class StructureData : ScriptableObject
{
    public StructureCategory category;

    public StructureType structureType;

    public Sprite icon;
    public bool baseStructure = false;
    public bool sellable = true;
    public bool onlyOnePurcahse = false;

    public string nameKey;
    public string descriptionKey;

    public int[] upgradeCoin;
    public int[] upgradeEnergy;
    public int purcahseLimit = -1;

    public float[] argment1;
    public float[] argment2;

    public RequireStructure[] requireStructures;

    public Sprite[] sprite1;
    public Sprite[] sprite2;
}

[System.Serializable]
public class RequireStructure
{
    public StructureType type;
    public int level;
}