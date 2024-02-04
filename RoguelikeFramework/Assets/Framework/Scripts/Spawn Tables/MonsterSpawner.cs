using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MonsterSpawner : MonoBehaviour
{
    public float dontSpawnNearStairDist;
    private static MonsterSpawner Singleton;
    public static MonsterSpawner singleton
    {
        get
        {
            if (!Singleton)
            {
                MonsterSpawner i = GameObject.FindObjectOfType<MonsterSpawner>();
                if (i)
                {
                    Singleton = i;
                }
                else
                {
                    UnityEngine.Debug.LogError("No MonsterSpawner found!");
                }
            }

            return Singleton;
        }
        set { Singleton = value; }
    }

    World world;

    public void SetMonsterPools(World world)
    {
        this.world = world;
        foreach (Branch branch in world.branches)
        {
            branch.monsterTables = branch.monsterTables.Select(x => Instantiate(x)).ToList();
            foreach (MonsterTable table in branch.monsterTables)
            {
                table.CalculateDepths();
            }
        }
    }

    public IEnumerator SpawnForFloor(int floor, Map m, int numMonsters)
    {
        if (floor < 0) yield break;

        List<Vector2Int> starts = new List<Vector2Int>();

        foreach (LevelConnection connection in m.entrances)
        {
            starts.Add(connection.toLocation);
        }

        /*
        foreach (LevelConnection connection in m.exits)
        {
            starts.Add(connection.fromLocation);
        }*/

        //Create positions map
        float[,] positions = Pathfinding.CreateDijkstraMap(m, starts.ToList());

        yield return null;

        int[,] tickets = new int[positions.GetLength(0), positions.GetLength(1)];

        int ticketSum = 0;
        for (int i = 0; i < positions.GetLength(0); i++)
        {
            for (int j = 0; j < positions.GetLength(1); j++)
            {
                if (positions[i, j] > 0)
                {
                    RogueTile tile = m.GetTile(i, j);
                    //Confirm that two monsters won't spawn on each other
                    if (!tile.IsInteractable() && !m.GetTile(i, j).currentlyStanding)
                    {
                        float dist = Mathf.Max(positions[i, j] - dontSpawnNearStairDist, 1);
                        dist = Mathf.Max(Mathf.Log(dist, 2), 0); //This number can't be negative, or monsters WILL spawn on each other.
                        tickets[i, j] = Mathf.RoundToInt(dist);
                        ticketSum += tickets[i, j];
                    }
                    else
                    {
                        tickets[i, j] = 0;
                    }
                }
            }
            yield return null;
        }

        yield return null;

        for (; m.monsterContainer.transform.childCount < numMonsters;)
        {
            yield return null;

            MonsterSpawnParams toSpawn = GetMonsterFromBranchAndDepth(m.branch, m.depth);
            while (toSpawn == null)
            {
                yield return null;
                Debug.LogWarning("Floor failed to spawn monster correctly. Retrying...");
                toSpawn = GetMonsterFromBranchAndDepth(m.branch, m.depth);
            }

            Vector2Int pos = new Vector2Int(-1, -1);

            int ticket = UnityEngine.Random.Range(0, ticketSum);
            for (int x = 0; x < tickets.GetLength(0); x++)
            {
                for (int y = 0; y < tickets.GetLength(1); y++)
                {
                    if (ticket < tickets[x, y])
                    {
                        pos = new Vector2Int(x, y);
                        //TODO: Remove an area of tickets
                        ticketSum -= tickets[x, y];
                        tickets[x, y] = 0;
                        break;
                    }
                    ticket -= tickets[x, y];
                }
                if (pos.x >= 0)
                {
                    break;
                }
            }

            SpawnMonster(toSpawn, pos, m, postSetup: false);
        }
    }

    public RogueHandle<Monster> SpawnMonster(MonsterSpawnParams spawnParams, Vector2Int location, Map map, bool postSetup = true)
    {
        return SpawnMonster(spawnParams, location, map, RogueHandle<Monster>.Default, postSetup);
    }

    public RogueHandle<Monster> SpawnMonster(MonsterSpawnParams spawnParams, Vector2Int location, Map map, RogueHandle<Monster> creditedTo, bool postSetup = true)
    {
        RogueHandle<Monster> spawnedHandle = spawnParams.SpawnMonster();
        Monster monster = spawnedHandle;

        map.monsters.Add(spawnedHandle);
        monster.location = location;
        monster.unity.transform.parent = map.monsterContainer;
        monster.level = map.depth;
        monster.credit = creditedTo;

        monster.AddEffectInstantiate(world.monsterPassives.ToArray());

        if (!map.ValidLocation(location))
        {
            Debug.Log("Here!");
        }

        monster.currentTile = map.GetTile(location);
        if (postSetup)
        {
            monster.PostSetup(Map.current);
        }
        return spawnedHandle;
    }

    public MonsterSpawnParams GetMonsterFromBranchAndDepth(Branch branch, int depth)
    {
        //Do out-of-depth check
        if (Random.Range(0.0f, 99.99f) < branch.chanceForOutOfDepth)
        {
            depth += branch.depthIncrease.Evaluate();
        }

        //Filter to just tables that can support our query
        List<MonsterTable> options = branch.monsterTables.Where(x => x.containedDepths.Contains(depth)).ToList();
        if (options.Count == 0)
        {
            Debug.LogError($"{branch.branchName} can't support spawning monsters at depth {depth}!");
            return null;
        }

        return options[Random.Range(0, options.Count)].RandomMonsterByDepth(depth);
    }

    public void SpawnMonsterAt(Map map, Vector2Int location, RogueHandle<Monster> creditedTo)
    {
        Branch branch = map.branch;
        MonsterSpawnParams toSpawn = GetMonsterFromBranchAndDepth(branch, map.depth);
        if (toSpawn)
        {
            SpawnMonster(toSpawn, location, map, creditedTo, postSetup: true);
        }
        else
        {
            Debug.LogError("Couldn't spawn anything! Monster to spawn was null.");
        }    
    }
}
