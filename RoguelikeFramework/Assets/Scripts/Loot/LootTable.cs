using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


[CreateAssetMenu(fileName = "New Loot Table", menuName = "Loot/Loot Table", order = 1)]
public class LootTable : ScriptableObject
{
    public ItemType type;

    public List<Item> items;

    [HideInInspector] public ItemRarity minRarity;
    [HideInInspector] public ItemRarity maxRarity;

    public StatusEffectList elevationOptions = new StatusEffectList();

    public int Count
    {
        get
        {
            return items.Count;
        }
    }

    public LootTable TrimToDepth(int depth, bool willElevate)
    {
        LootTable table = Instantiate(this);
        table.items = items.FindAll(x => x.minDepth <= depth && x.maxDepth >= depth);
        if (willElevate)
        {
            table.minRarity = table.items.Min(x => x.rarity);
            table.maxRarity = table.items.Max(x => x.elevatesTo);
        }
        else
        {
            table.minRarity = table.items.Min(x => x.rarity);
            table.maxRarity = table.items.Max(x => x.rarity);
        }

        Debug.Log($"For depth {depth}, table {table.name} has min {table.minRarity} and max {table.maxRarity}");
        return table;
    }

    //Gets a random item! Assumes trimming has already been completed.
    public Item RandomItemByRarity(ItemRarity rarity, bool takesLower)
    {
        if (rarity < minRarity)
        {
            Debug.LogWarning($"Table {name} has no items of rarity {rarity}. Bumping it up to {minRarity}!");
            rarity = minRarity;
        }
        if (rarity > maxRarity)
        {
            Debug.LogWarning($"Table {name} has no items of rarity {rarity}. Cutting it to {maxRarity} :(");
            rarity = maxRarity;
        }
        List<Item> workingSet;
        if (takesLower)
        {
            workingSet = items.FindAll(x => x.rarity <= rarity && x.elevatesTo >= rarity);
        }
        else
        {
            workingSet = items.FindAll(x => x.rarity == rarity);
        }

        Item i =  workingSet[UnityEngine.Random.Range(0, workingSet.Count)].Instantiate();

        if (i.rarity < rarity)
        {
            i.ElevateRarityTo(rarity, elevationOptions);
        }

        return i;
    }
}
