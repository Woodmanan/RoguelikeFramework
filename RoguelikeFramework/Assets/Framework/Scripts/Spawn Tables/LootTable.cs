using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


[CreateAssetMenu(fileName = "New Loot Table", menuName = "Distribution/Loot Table", order = 1)]
public class LootTable : ScriptableObject
{
    public List<Item> items;

    [SerializeReference] public List<Effect> elevationOptions;

    public int Count
    {
        get
        {
            return items.Count;
        }
    }

    //Gets a random item! Assumes trimming has already been completed.
    public Item RandomItemByRarity(ItemRarity rarity, bool takesLower = true)
    {
        List<Item> workingSet;
        workingSet = items.FindAll(x => x.rarity <= rarity && x.elevatesTo >= rarity);

        while (workingSet.Count == 0)
        {
            if (takesLower)
            {
                if (rarity > ItemRarity.COMMON)
                {
                    rarity--;
                    workingSet = items.FindAll(x => x.rarity <= rarity && x.elevatesTo >= rarity);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        Item i = workingSet[UnityEngine.Random.Range(0, workingSet.Count)].Instantiate();
        i.Setup();
        

        i.ElevateRarityTo(rarity, elevationOptions);
        

        return i;
    }
}
