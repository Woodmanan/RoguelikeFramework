using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[Serializable]
public struct TypeChance
{
    public ItemType type;
    public int weight;
}

[Serializable]
public struct RarityChance
{
    public ItemRarity rarity;
    public int weight;
}

[Serializable]
public class ItemSpawnInfo
{
    public List<RarityChance> rarities;
    int raritySum;

    public List<TypeChance> types;
    int chanceSum;

    public ItemRarity GetRarity()
    {
        if (raritySum == 0)
        {
            raritySum = rarities.Sum(x => x.weight);
        }
        int choice = RogueRNG.Linear(0, raritySum);
        foreach (RarityChance chance in rarities)
        {
            if (choice < chance.weight)
            {
                return chance.rarity;
            }
            choice -= chance.weight;
        }

        Debug.LogError("Somehow didn't get a rarity. Returning default");
        return ItemRarity.COMMON;
    }

    public ItemType GetItemType()
    {
        if (chanceSum == 0)
        {
            chanceSum = types.Sum(x => x.weight);
        }

        int choice = RogueRNG.Linear(0, chanceSum);
        foreach (TypeChance chance in types)
        {
            if (choice < chance.weight)
            {
                return chance.type;
            }
            choice -= chance.weight;
        }

        Debug.LogError("Somehow didn't get a rarity. Returning a safe default");
        return ItemType.CONSUMABLE;
    }
}

public class LootPool
{
    public Quadtree<Item> tree;
    public LootPool(int maxDepth, int maxRarity)
    {
        tree = new Quadtree<Item>(new Rect(Vector2.zero, new Vector2Int(maxRarity + 1, maxDepth + 1)));
    }

    public void AddItemsFromTable(LootTable table, Transform holder)
    {
        foreach (Item i in table.items)
        {
            Item working = i.Instantiate();
            working.optionalEffects.AddRange(table.elevationOptions); //Tie our table options to the item - now we don't need the table.
            int minRarity = (int)i.rarity;
            int maxRarity = (int)i.elevatesTo + 1;

            int minDepth = i.minDepth;
            int maxDepth = i.maxDepth + 1;
            Rect itemRect = new Rect(minRarity, minDepth, maxRarity - minRarity, maxDepth - minDepth); //Build a spatial rect from our rarity and depth data
            tree.Insert(working, itemRect);
            working.transform.parent = holder;
        }
    }

    public Item GenerateItem(int depth, ItemSpawnInfo info)
    {
        Item toSpawn = null;
        ItemRarity rarity = info.GetRarity();
        ItemType type = info.GetItemType();
        for (int i = 0; i < 20; i++)
        {
            Vector2 searchPoint = new Vector2((int)rarity, depth);

            List<Item> found = tree.GetItemsAt(searchPoint);

            if (found.Count == 0)
            {
                Debug.LogWarning($"Attempt {i}: Could not spawn any item of rarity {rarity} at depth {depth}. {(i < 19 ? " Retrying at lower rarity..." : "")}");
                if (rarity != ItemRarity.COMMON)
                {
                    rarity--;
                }
                else
                {
                    Debug.LogError("Found 0 items of any sort in this pool. Something is FUBAR, returning null.");
                    return null;
                }
            }
            else
            {
                found = found.Where(x => x.type == type).ToList();
                if (found.Count == 0)
                {
                    Debug.LogWarning($"Attempt {i}: Found 0 items of type {type} at depth {depth} and rarity {rarity}. {(i < 19 ? " Retrying with new data..." : "")}");
                    type = info.GetItemType();
                    rarity = info.GetRarity();
                }
                else
                {
                    //********** SUCESS *************
                    toSpawn = found[RogueRNG.Linear(0, found.Count)];
                    break;
                }
            }
        }

        if (toSpawn == null)
        {
            Debug.LogError("Item to spawn was null, cancelling");
            return null;
        }

        toSpawn = toSpawn.Instantiate();

        if (toSpawn.rarity < rarity)
        {
            toSpawn.ElevateRarityTo(rarity);
        }

        return toSpawn;
    }
}
