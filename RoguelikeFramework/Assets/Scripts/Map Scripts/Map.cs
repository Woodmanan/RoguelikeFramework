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
    private static Map Singleton;
    public static Map singleton
    {
        get
        {
            if (!Singleton)
            {
                Map m = GameObject.FindObjectOfType<Map>();
                if (m)
                {
                    Singleton = m;
                }
                else
                {
                    Debug.LogError("No map found!");
                }
            }

            return Singleton;
        }
        set { Singleton = value; }
    }
    
    //Map space, which controls how movement is allowed
    public static Space space = Space.Chebyshev;

    public int depth;

    public TileList tilesAvailable;


    public CustomTile[,] tiles;
    public bool[,] blocksVision;
    public float[,] moveCosts;

    public int width;
    

    public int height;
    
    // Start is called before the first frame update
    void Start()
    {
        transform.position = Vector3.zero;
        if (singleton != this)
        {
            if (singleton)
            {
                if (singleton.depth > this.depth)
                {
                    Destroy(gameObject);
                }
                else
                {
                    Destroy(Singleton);
                    singleton = this;
                }
            }
            else
            {
                singleton = this;
            }
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PerformTesting()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < width; j++)
            {
                
            }
        }
    }
    

    public void allocateMap(int xSize, int ySize)
    {
        tiles = new CustomTile[xSize, ySize];
        blocksVision = new bool[xSize, ySize];
        moveCosts = new float[xSize, ySize];
        width = xSize;
        height = ySize;
        for (int j = 0; j < height; j++)
        {
            GameObject row = new GameObject {name = $"Row {j}"};
            row.transform.parent = transform;
            for (int i = 0; i < width; i++)
            {
                GameObject g = Instantiate(tilesAvailable.tiles[Random.Range(0,2)], row.transform, true);
                g.name = $"Tile ({i}, {j})";
                CustomTile custom = g.GetComponent<CustomTile>();
                if (!custom)
                {
                    Debug.LogError("Tile did not have tile component.");
                }
                g.transform.position = new Vector3(i, j, 0);
                tiles[i, j] = custom;
                custom.x = i;
                custom.y = j;
            }
        }
        
        //Now that map data is finished, go rebuild it
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                tiles[i,j].RebuildMapData();
            }
        }
    }

    public float MovementCostAt(Vector2Int position)
    {
        return moveCosts[position.x, position.y];
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
}
