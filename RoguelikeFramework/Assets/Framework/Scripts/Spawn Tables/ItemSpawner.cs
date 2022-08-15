using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public class ItemSpawner : MonoBehaviour
{
    private static ItemSpawner Singleton;
    public static ItemSpawner singleton
    {
        get
        {
            if (!Singleton)
            {
                ItemSpawner i = GameObject.FindObjectOfType<ItemSpawner>();
                if (i)
                {
                    Singleton = i;
                }
                else
                {
                    UnityEngine.Debug.LogError("No ItemSpawner found!");
                }
            }

            return Singleton;
        }
        set { Singleton = value; }
    }

    public List<LootPool> pools;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetItemPools(List<DungeonGenerator> generators)
    {
        //Copy pools and instantiate them
        this.pools = generators.Select(x => Instantiate(x.availableItems)).ToList();

        //Trim anyone that asks for that, politely.
        for (int i = 0; i < generators.Count; i++)
        {
            if (pools[i].shouldTrim) pools[i].TrimToDepth(generators[i].depth);
        }
    }

    public IEnumerator SpawnForFloor(int floor, Map m, int numItems)
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
                    tickets[i, j] = Mathf.RoundToInt(Mathf.Log(positions[i, j], 2));
                    ticketSum += tickets[i, j];
                }
            }
            yield return null;
        }


        yield return null;

        for (; m.itemContainer.transform.childCount < numItems;)
        {
            yield return null;
            Item item = pools[floor].GenerateItem();
            while (item == null)
            {
                yield return null;
                Debug.LogWarning("Floor failed to spawn item correctly. Retrying...");
                item = pools[floor].GenerateItem();
            }

            Vector2Int pos = new Vector2Int(-1, -1);
            
            int ticket = UnityEngine.Random.Range(0, ticketSum);
            for (int x = 0; x < tickets.GetLength(0); x++)
            {
                for (int y = 0; y < tickets.GetLength(1); y++)
                {
                    if (ticket < tickets[x,y])
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

            item.transform.parent = m.itemContainer;
            m.GetTile(pos).inventory.Add(item);
        }
    }
}
