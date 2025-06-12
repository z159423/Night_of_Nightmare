using System.Collections.Generic;
using System.Linq;
using static Define;

[System.Serializable]
public class PlayerData
{
    public CharactorType type;

    public int coin;
    public int energy;

    public Room room;
    public List<Structure> structures = new List<Structure>();

    public PlayerData(CharactorType type)
    {
        this.type = type;
    }

    public void BuildStructure(Structure structure)
    {
        if (structures.Contains(structure))
            return;

        structures.Add(structure);
    }

    public void UpgradeStructure(Structure structure)
    {
        if (!structures.Contains(structure))
            return;

        structure.Upgrade();
    }

    public void GetResources()
    {
        coin += 1;
        energy += structures.Where(s => s.type == Define.StructureType.Generator).Count();
    }

    public void UseResource(int coin, int energy)
    {
        this.coin -= coin;
        this.energy -= energy;
    }

    public Structure GetStructure(Define.StructureType type)
    {
        return structures.First(s => s.type == type);
    }

    public void SellStructure(Structure structure)
    {
        if (!structures.Contains(structure))
            return;

        structure.DestroyStructure();
        structures.Remove(structure);
        coin += structure.GetSellValue();
    }
}
