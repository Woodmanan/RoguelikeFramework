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
    public static MapSpace space = MapSpace.Chebyshev;

    public int depth;


    public CustomTile[,] tiles;
    public bool[,] blocksVision;
    public float[,] moveCosts;

    public int width;

    public int height;

    public bool activeGraphics = false;

    public List<Monster> monsters = new List<Monster>();

    public List<Vector2Int> entrances = new List<Vector2Int>();
    public List<Vector2Int> exits = new List<Vector2Int>();
    
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
                    tiles[i,j].RebuildGraphics();
                }
            }
        }
    }

    public void PerformTesting()
    {
    
    }
    

    public IEnumerator BuildFromTemplate(int[,] map, TileList availableTiles)
    {
        int xSize = map.GetLength(0);
        int ySize = map.GetLength(1);

        tiles = new CustomTile[xSize, ySize];
        blocksVision = new bool[xSize, ySize];
        moveCosts = new float[xSize, ySize];
        yield return null;

        width = xSize;
        height = ySize;
        for (int j = 0; j < height; j++)
        {
            GameObject row = new GameObject {name = $"Row {j}"};
            row.transform.parent = transform;
            for (int i = 0; i < width; i++)
            {
                GameObject g = Instantiate(availableTiles.tiles[map[i,j]], row.transform, true);
                g.name = $"Tile ({i}, {j})";
                CustomTile custom = g.GetComponent<CustomTile>();
                if (!custom)
                {
                    Debug.LogError("Tile did not have tile component.");
                }
                g.transform.position = new Vector3(i, j, 0);
                tiles[i, j] = custom;
                custom.SetMap(this, i, j);
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
                tiles[i,j].RebuildMapData();
            }
            yield return null;
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
}
