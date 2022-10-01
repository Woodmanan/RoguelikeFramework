using System.Collections;
using System.Collections.Generic;
using System.Xml;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Tilemaps;

/* Map class that  is used as a handler and container for the game maps. Holds a the
 * structs and has functionality for getting and setting information about the world
 */


public class Map : MonoBehaviour
{
    public static Map current;

    //Map space, which controls how movement is allowed
    public const MapSpace space = MapSpace.Chebyshev;

    public int depth;
    public int index;


    public CustomTile[,] tiles;
    public bool[,] blocksVision;
    public float[,] moveCosts;

    public int width;

    public int height;

    public bool activeGraphics = false;

    public Branch branch;

    public List<Monster> monsters = new List<Monster>();
    public List<Monster> spawnedMonsters = new List<Monster>();

    public int numStairsUp;
    public int numStairsDown;

    public List<LevelConnection> entrances = new List<LevelConnection>();
    public List<LevelConnection> exits = new List<LevelConnection>();

    public Transform tileContainer;
    public Transform monsterContainer;
    public Transform itemContainer;

    public List<InteractableTile> interactables = new List<InteractableTile>();

    // Start is called before the first frame update
    void Start()
    {
        transform.position = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void LateUpdate()
    {
        if (!activeGraphics) return;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (tiles[i, j].dirty)
                {
                    tiles[i, j].RebuildGraphics();
                }
            }
        }
    }

    public void PerformTesting()
    {

    }

    public void Setup()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                tiles[i, j].Setup();
            }
        }
    }

    public void RefreshGraphics()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                tiles[i, j].RebuildGraphics();
            }
        }
    }

    public void SetAllTiles()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                tiles[i, j].SetInMap(this);
            }
        }
    }


    public IEnumerator BuildFromTemplate(int[,] map, TileList availableTiles)
    {
        int xSize = map.GetLength(0);
        int ySize = map.GetLength(1);

        tiles = new CustomTile[xSize, ySize];
        blocksVision = new bool[xSize, ySize];
        moveCosts = new float[xSize, ySize];
        yield return null;

        tileContainer = new GameObject("Tiles").transform;
        monsterContainer = new GameObject("Monsters").transform;
        itemContainer = new GameObject("Items").transform;

        tileContainer.parent = transform;
        monsterContainer.parent = transform;
        itemContainer.parent = transform;

        width = xSize;
        height = ySize;
        for (int j = 0; j < height; j++)
        {
            GameObject row = new GameObject { name = $"Row {j}" };
            row.transform.parent = tileContainer;
            for (int i = 0; i < width; i++)
            {
                GameObject g = Instantiate(availableTiles.tiles[map[i, j]], row.transform, true);
                g.name = $"Tile ({i}, {j})";
                CustomTile custom = g.GetComponent<CustomTile>();
                if (!custom)
                {
                    Debug.LogError("Tile did not have tile component.");
                }
                g.transform.position = new Vector3(i, j, 0);
                tiles[i, j] = custom;
                custom.SetMap(this, new Vector2Int(i, j));
                custom.Setup();
                if (i % 33 == 32) yield return null;
            }
            yield return null;
        }

        //Now that map data is finished, go rebuild it
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                tiles[i, j].RebuildMapData();
            }
            yield return null;
        }
    }

    public void RebuildAllMapData()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                tiles[i, j].RebuildMapData();
            }
        }
    }

    public float MovementCostAt(Vector2Int position)
    {
        return moveCosts[position.x, position.y];
    }

    public CustomTile GetTile(Vector2Int loc)
    {
        return GetTile(loc.x, loc.y);
    }

    public CustomTile GetTile(int x, int y)
    {
        return tiles[x, y];
    }
    
    public bool ValidLocation(Vector2Int loc)
    {
        return ValidLocation(loc.x, loc.y);
    }

    public bool ValidLocation(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    public bool BlocksSight(int x, int y)
    {
        if (x >= 0 && x < width && y >= 0 && y < height)
        {
            return blocksVision[x, y];
        }
        return true;
    }

    public void SetMapVision(int x, int y, bool val)
    {
        blocksVision[x, y] = val;
    }

    public bool NeedsExploring(Vector2Int location)
    {
        CustomTile tile = GetTile(location);
        if (tile.BlocksMovement()) return false;
        if (tile.isHidden) return true;

        //Check the 8 directions!
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                //Skip middle!
                if (i == 0 && j == 0)
                {
                    continue;
                }

                Vector2Int newLoc = location + new Vector2Int(i, j);

                if (newLoc.x < 0 || newLoc.y < 0 || newLoc.x >= width || newLoc.y >= height)
                {
                    continue;
                }

                if (GetTile(newLoc).isHidden) return true;
            }
        }
        return false;
    }

    public void Reveal(Vector2Int loc)
    {
        if (loc.x < 0 || loc.x >= width || loc.y < 0 || loc.y >= height)
        {
            return;
        }

        tiles[loc.x, loc.y].Reveal();
    }

    public void ClearLOS(Vector2Int loc)
    {
        if (loc.x < 0 || loc.x >= width || loc.y < 0 || loc.y >= height)
        {
            return;
        }

        tiles[loc.x, loc.y].Clear();
    }

    public bool BlocksSight(Vector2Int loc)
    {
        if (loc.x < 0 || loc.x >= width || loc.y < 0 || loc.y >= height)
        {
            return true;
        }

        return tiles[loc.x, loc.y].blocksVision;
    }

    public bool BlocksMovement(Vector2Int loc)
    {
        if (loc.x < 0 || loc.x >= width || loc.y < 0 || loc.y >= height)
        {
            return true;
        }

        return moveCosts[loc.x, loc.y] < 0;
    }

    public Vector2Int GetRandomWalkableTile()
    {
        while (true)
        {
            Vector2Int spot = new Vector2Int(Random.Range(1, width - 1), Random.Range(1, height - 1));
            if (moveCosts[spot.x, spot.y] > 0)
            {
                return spot;
            }
        }
    }
}