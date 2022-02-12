using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MonsterSpawner : MonoBehaviour
{
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

    public List<MonsterPool> pools;

    public void SetMonsterPools(List<DungeonGenerator> generators)
    {
        //Copy pools and instantiate them
        this.pools = generators.Select(x => Instantiate(x.availableMonsters)).ToList();

        //Trim anyone that asks for that, politely.
        for (int i = 0; i < generators.Count; i++)
        {
            pools[i].SetupTables();
        }
    }

    public IEnumerator SpawnForFloor(int floor, Map m, int numMonsters)
    {
        if (floor < 0) yield break;

        List<Vector2Int> starts = new List<Vector2Int>();
        starts.AddRange(m.entrances);
        starts.AddRange(m.exits);

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
                    //Confirm that two monsters won't spawn on each other
                    if (m.GetTile(i, j).currentlyStanding == null)
                    {
                        tickets[i, j] = Mathf.RoundToInt(Mathf.Log(positions[i, j], 2));
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
            Monster monster = pools[floor].SpawnMonster(m.depth);
            while (monster == null)
            {
                yield return null;
                Debug.LogWarning("Floor failed to spawn monster correctly. Retrying...");
                monster = pools[floor].SpawnMonster(m.depth);
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

            m.monsters.Add(monster);
            monster.location = pos;
            monster.transform.parent = m.monsterContainer;
        }
    }
}
