using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

[CreateAssetMenu(fileName = "New Monster Table", menuName = "Distribution/Monster Table", order = 3)]
public class MonsterTable : ScriptableObject
{
    public static HashSet<int> UsedUniqueIDs = new HashSet<int>();
    public List<Monster> monsters;

    public List<Monster> uniques;
    public float chanceForUnique;
    public HashSet<int> containedDepths = new HashSet<int>();

    public void CalculateDepths()
    {
        containedDepths.Clear();
        for (int depth = 0; depth < 40; depth++)
        {
            int numMonsters = monsters.FindAll(x => x.minDepth <= depth && x.maxDepth >= depth).Count;
            if (numMonsters > 0)
            {
                containedDepths.Add(depth);
            }
        }
    }


    public Monster RandomMonsterByDepth(int depth)
    {
        if (Random.Range(0.0f, 99.99f) > chanceForUnique)
        {
            return SpawnUniqueMonster(depth);
        }
        else
        {
            //Spawn regular monster
            return SpawnSimpleMonster(depth);
        }
    }

    public Monster SpawnUniqueMonster(int depth)
    {
        //Spawn Unique monster
        List<Monster> toSpawn = uniques.FindAll(x => x.minDepth <= depth && x.maxDepth >= depth && !UsedUniqueIDs.Contains(x.ID));

        if (toSpawn.Count == 0)
        {
            return SpawnSimpleMonster(depth + 1); //Give them a little bit harder of an encounter
        }

        Monster spawned = toSpawn[Random.Range(0, toSpawn.Count)];
        //Ensure that unique does not get re-added
        UsedUniqueIDs.Add(spawned.ID);
        Debug.Log($"Just spawned Unique {spawned.displayName} with ID {spawned.ID}");
        return spawned.Instantiate();
    }

    public Monster SpawnSimpleMonster(int depth)
    {
        Monster toReturn;
        List<Monster> toSpawn = monsters.FindAll(x => x.minDepth <= depth && x.maxDepth >= depth);
        if (toSpawn.Count == 0)
        {
            Debug.LogError("Table can't spawn any monsters at depth! Returning null for a retry");
            return null;
        }
        else
        {
            toReturn = toSpawn[Random.Range(0, toSpawn.Count)].Instantiate();
        }

        /*
        List<Loadout> usableLoadouts = loadouts.FindAll(x => x.minDepth <= depth && x.maxDepth >= depth);

        //TODO: Sometimes just don't give monsters loadouts, or make them chance based.
        if (usableLoadouts.Count == 0)
        {
            //Use the default loadout, if it exists. Otherwise, they're just unarmed. Sorry, you likely sad goblin.
            toReturn.loadout?.Apply(toReturn);
        }
        else
        {
            //Possible workaround for that TODO: add some null loadouts instead
            usableLoadouts[Random.Range(0, usableLoadouts.Count)]?.Apply(toReturn);
        }*/

        return toReturn;

    }
}
