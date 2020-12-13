using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

/* Map class that  is used as a handler and container for the game maps. Holds a the
 * structs and has functionality for getting and setting information about the world
 */

public struct Tile
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

    public Tile[,] tiles;

    public int width;

    public int height;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void allocateMap(int xSize, int ySize)
    {
        tiles = new Tile[xSize, ySize];
    }

    public Tile getTile(int x, int y)
    {
        return tiles[x, y];
    }
}
