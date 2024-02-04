using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AddressableAssets;

/* 
 * Monster tables! These are very similar to loot tables, but they don't trim down.
 * The reasoning for this is twofold:
 *     1. That might be an unnecessary optimization, after working with the loot tables for a while
 *     2. Monsters have the potential to spawn out of depth - additionally, they have the ability
 *        to do some jank things, like spawning unique monsters only once.
 *        
 *     To try and keep this as simple as possible, we're going to try doing the no-trimming method
 *     first. (Don't try this at home)
 */

[System.Serializable]
public struct WeightedSpawn
{
    public MonsterSpawnParams toSpawn;
    public int weight;
}

[CreateAssetMenu(fileName = "New Monster Table", menuName = "Distribution/Monster Table", order = 3)]
public class MonsterTable : ScriptableObject
{
    public static HashSet<int> UsedUniqueIDs = new HashSet<int>();
    public List<WeightedSpawn> monsters;

    public List<WeightedSpawn> uniques;
    public float chanceForUnique;
    public HashSet<int> containedDepths = new HashSet<int>();

    public void CalculateDepths()
    {
        containedDepths.Clear();
        for (int depth = 0; depth < 40; depth++)
        {
            int numMonsters = monsters.FindAll(x => x.toSpawn.minDepth <= depth && x.toSpawn.maxDepth >= depth).Count;
            if (numMonsters > 0)
            {
                containedDepths.Add(depth);
            }
        }
    }


    public MonsterSpawnParams RandomMonsterByDepth(int depth)
    {
        if (Random.Range(0.0f, 99.99f) < chanceForUnique)
        {
            return SpawnUniqueMonster(depth);
        }
        else
        {
            //Spawn regular monster
            return SpawnSimpleMonster(depth);
        }
    }

    public MonsterSpawnParams SpawnUniqueMonster(int depth)
    {
        //Spawn Unique monster
        List<WeightedSpawn> toSpawn = uniques.FindAll(x => x.toSpawn.minDepth <= depth && x.toSpawn.maxDepth >= depth && !UsedUniqueIDs.Contains(x.toSpawn.ID));

        if (toSpawn.Count == 0)
        {
            return SpawnSimpleMonster(depth + 1); //Give them a little bit harder of an encounter
        }

        int sum = toSpawn.Sum(x => x.weight);
        int choice = RogueRNG.Linear(0, sum);

        MonsterSpawnParams spawnParams = null;

        if (sum != 0)
        {
            foreach (WeightedSpawn option in toSpawn)
            {
                if (choice < option.weight)
                {
                    spawnParams = option.toSpawn;
                    break;
                }
                choice -= option.weight;
            }
        }
        else
        {
            Debug.Log($"All available options for depth {depth} had weight 0. This is bad, as these options will never be chosen. Defaulting to first valid choice.");
            spawnParams = toSpawn[0].toSpawn;
        }

        Debug.Assert(spawnParams != null);

        //Ensure that unique does not get re-added
        UsedUniqueIDs.Add(spawnParams.ID);
        Debug.Log($"Chose to spawn {spawnParams.friendlyName} with ID {spawnParams.ID}");
        return spawnParams;
    }

    public MonsterSpawnParams SpawnSimpleMonster(int depth)
    {
        List<WeightedSpawn> toSpawn = monsters.FindAll(x => x.toSpawn.minDepth <= depth && x.toSpawn.maxDepth >= depth);
        if (toSpawn.Count == 0)
        {
            Debug.LogError("Table can't spawn any monsters at depth! Returning null for a retry");
            return null;
        }

        int sum = toSpawn.Sum(x => x.weight);
        int choice = RogueRNG.Linear(0, sum);
        MonsterSpawnParams spawnParams = null;

        if (sum != 0)
        {
            foreach (WeightedSpawn option in toSpawn)
            {
                if (choice < option.weight)
                {
                    spawnParams = option.toSpawn;
                    break;
                }
                choice -= option.weight;
            }
        }
        else
        {
            Debug.LogError($"All available options for depth {depth} had weight 0. This is bad, as these options will never be chosen. Defaulting to first valid choice.");
            spawnParams = toSpawn[0].toSpawn;
        }

        return spawnParams;
    }
}
