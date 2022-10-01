using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

[Serializable]
public struct LootChance
{
    public ItemType type;
    public int weight;
}

public class LootPool
{
    public bool shouldTrim;
    public bool elevatesItems;

    public List<LootChance> chances;
    int chanceSum;

    public List<int> rarities;
    int raritySum;

    public List<LootTable> tables;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void TrimToDepth(int depth)
    {
        tables = tables.Select(x => x.TrimToDepth(depth, elevatesItems)).ToList();
        chanceSum = chances.Sum(x => x.weight);
        raritySum = rarities.Sum();
    }

    public Item GenerateItem(ItemType type, ItemRarity rarity)
    {
        List<LootTable> options = tables.Where(x => (x.type & type) > 0).ToList();
        if (options.Count == 0)
        {
            UnityEngine.Debug.Log("We have no items of that type!");
            return null;
        }

        LootTable choice = options[UnityEngine.Random.Range(0, options.Count)];

        Item i = choice.RandomItemByRarity(rarity, elevatesItems);

        return i;
    }

    public Item GenerateItem()
    {
        int val = UnityEngine.Random.Range(0, chanceSum);

        //Choose type
        ItemType chosenType = ItemType.CONSUMABLE;
        foreach (LootChance chance in chances)
        {
            if (val < chance.weight)
            {
                chosenType = chance.type;
                break;
            }
            val -= chance.weight;
        }

        val = UnityEngine.Random.Range(0, raritySum);
        ItemRarity chosenRarity = ItemRarity.COMMON;
        for (int i = 0; i < rarities.Count; i++)
        {
            if (val < rarities[i])
            {
                chosenRarity = (ItemRarity)i;
                break;
            }
            val -= rarities[i];
        }

        return GenerateItem(chosenType, chosenRarity);
    }
}
