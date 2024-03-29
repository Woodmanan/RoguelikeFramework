﻿using System.Collections;
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

    public ItemSpawnInfo spawnInfo;

    [Tooltip("The max depth we expect an item to be spawned at. Used for the quadtree bounds.")]
    public int maxDepth;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetItemPools(World world)
    {
        foreach (Branch branch in world.branches)
        {
            branch.lootPool = new LootPool(maxDepth, (int)ItemRarity.UNIQUE);
            foreach (LootTable table in branch.tables)
            {
                branch.lootPool.AddItemsFromTable(table, transform);
            }
        }
    }

    public Item GetItemFromBranchAndDepth(Branch branch, int depth)
    {
        if (branch.usesCustomSpawnInfo)
        {
            return branch.lootPool.GenerateItem(depth, branch.itemSpawnInfo);
        }

        //Use default set on the loot spawner
        return branch.lootPool.GenerateItem(depth, spawnInfo);
    }

    public Item GetItemByID(int id)
    {
        foreach (Branch branch in World.current.branches)
        {
            foreach (Item item in branch.lootPool.tree.GetItemsIn(branch.lootPool.tree.rect))
            {
                if (item.ID == id)
                {
                    return item.Instantiate();
                }
            }
        }

        return null;
    }

    public void SpawnItem(Item item, Vector2Int location, Map map, ItemRarity rarity = ItemRarity.COMMON)
    {
        if (item == null)
        {
            Debug.LogError("Can't spawn null item!");
            return;
        }
        item.Setup();
        if (item.rarity < rarity)
        {
            item.ElevateRarityTo(rarity);
        }
        item.transform.parent = map.itemContainer;
        item.gameObject.SetActive(true);
        map.GetTile(location).inventory.Add(item);
    }

    public void SpawnItemInstantiate(Item item, Vector2Int location, Map map, ItemRarity rarity = ItemRarity.COMMON)
    {
        SpawnItem(item?.Instantiate(), location, map);
    }

    public IEnumerator SpawnForFloor(int floor, Map m, int numItems)
    {
        if (floor < 0) yield break;

        List<Vector2Int> starts = new List<Vector2Int>();

        foreach (LevelConnection connection in m.entrances)
        {
            starts.Add(connection.toLocation);
        }

        foreach (LevelConnection connection in m.exits)
        {
            starts.Add(connection.fromLocation);
        }

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

            Item item = GetItemFromBranchAndDepth(m.branch, m.depth);
            int tries = 0;
            while (item == null)
            {
                tries++;
                if (tries == 100)
                {
                    numItems--;
                    break;
                }
                yield return null;
                item = GetItemFromBranchAndDepth(m.branch, m.depth);
            }

            if (tries == 100)
            {
                Debug.LogWarning($"Failed to spawn item for floor {m.name}");
                continue;
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
            item.gameObject.SetActive(true);
            m.GetTile(pos).inventory.Add(item);
        }
    }
}
