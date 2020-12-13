using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

/* Map class that  is used as a handler and container for the game maps. Holds a the
 * structs and has functionality for getting and setting information about the world
 */

public struct TileInfo
{
    [CanBeNull] public string name;
    public float movementCost;
    public float visionCost;
    public int icon;
    [CanBeNull] public GameObject currentlyStanding;
    [CanBeNull] public List<GameObject> containedItmes;
}

public class Map : MonoBehaviour
{

    public TileInfo[,] tiles;

    public int width;
    

    public int height;
    
    // Start is called before the first frame update
    void Start()
    {
       SetUp30x30(); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    //Setup function, for testing
    void SetUp30x30()
    {
       allocateMap(30, 30);
       for (int i = 0; i < width; i++)
       {
           for (int j = 0; j < height; j++)
           {
               tiles[i, j].icon = 0;
           }
       }
       GetComponent<MapRender>().UpdateAllTiles();
    }

    public void allocateMap(int xSize, int ySize)
    {
        tiles = new TileInfo[xSize, ySize];
        width = xSize;
        height = ySize;
    }

    public TileInfo getTile(int x, int y)
    {
        return tiles[x, y];
    }
}
