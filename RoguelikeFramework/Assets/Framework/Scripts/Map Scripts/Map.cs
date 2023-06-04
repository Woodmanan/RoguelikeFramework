using System.Collections;
using System.Collections.Generic;
using System.Xml;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

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
    public int level;


    public RogueTile[,] tiles;
    public bool[,] blocksVision;

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

    public List<DungeonSystem> mapSystems = new List<DungeonSystem>();

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

        tiles = new RogueTile[xSize, ySize];
        blocksVision = new bool[xSize, ySize];

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
                RogueTile custom = g.GetComponent<RogueTile>();
                if (!custom)
                {
                    Debug.LogError("Tile did not have tile component.");
                }
                g.transform.position = new Vector3(i, j, 0);
                tiles[i, j] = custom;
                custom.SetMap(this, new Vector2Int(i, j));
                custom.Setup();
                yield return null;
            }
            yield return null;
        }

        //Now that map data is finished, go rebuild it
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                tiles[i, j].RebuildMapData();
                yield return null;
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
        return GetTile(position).GetMovementCost();
    }

    public RogueTile GetTile(Vector2Int loc)
    {
        return GetTile(loc.x, loc.y);
    }

    public RogueTile GetTile(int x, int y)
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
        RogueTile tile = GetTile(location);
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

        return MovementCostAt(loc) < 0;
    }

    public Vector2Int GetRandomWalkableTile()
    {
        while (true)
        {
            Vector2Int spot = new Vector2Int(Random.Range(1, width - 1), Random.Range(1, height - 1));
            if (MovementCostAt(spot) > 0)
            {
                return spot;
            }
        }
    }

    public Vector2Int GetRandomWalkableTileAround(Vector2Int point, int radius)
    {
        while (true)
        {
            Vector2Int spot = point + new Vector2Int(Random.Range(-radius, radius), Random.Range(-radius, radius));
            if (ValidLocation(spot) && MovementCostAt(spot) > 0 && GetTile(spot).currentlyStanding == null)
            {
                return spot;
            }
        }
    }

    public Vector2Int GetRandomWalkableTileInSight(Monster viewer, int range = 100)
    {
        foreach (Vector2Int position in viewer.view.GetVisibleTiles(this).Where(x => x.GameDistance(viewer.location) <= range).OrderBy(x => Random.value))
        {
            RogueTile tile = GetTile(position);
            if (tile.movementCost > 0 && tile.currentlyStanding == null)
            {
                return position;
            }
        }

        return new Vector2Int(-1, -1);
    }

    public IEnumerator<Vector2Int> GetWalkableTilesByRange(Vector2Int point, int lower, int higher)
    {
        for (int i = lower; i <= higher; i++)
        {
            foreach (Vector2Int tile in GetTilesInSquareRange(point, i).OrderBy(x => Random.value))
            {
                yield return tile;
            }
        }
    }

    public List<Vector2Int> GetTilesInSquareRange(Vector2Int point, int range)
    {
        List<Vector2Int> points = new List<Vector2Int>();
        for (int i = -range; i <= range; i++)
        {
            points.Add(point + new Vector2Int(i, range));
            points.Add(point + new Vector2Int(i, -range));
        }

        for (int i = -range + 1; i <= range - 1; i++)
        {
            points.Add(point + new Vector2Int(range, i));
            points.Add(point + new Vector2Int(-range, i));
        }

        return points.Where(x => ValidLocation(x) && GetTile(x).currentlyStanding == null && MovementCostAt(x) > 0).ToList();
    }

    public void SwapMonsters(RogueTile first, RogueTile second)
    {
        Monster secondMonster = second.currentlyStanding;
        Monster firstMonster = first.currentlyStanding;

        if (secondMonster)
        {
            secondMonster.currentTile = null;
            
        }

        if (firstMonster)
        {
            firstMonster.currentTile = null;
        }

        second.currentlyStanding = null;
        first.currentlyStanding = null;

        if (secondMonster)
        {
            secondMonster.SetPosition(first.location);
        }

        if (firstMonster)
        {
            firstMonster.SetPosition(second.location);
        }
    }

    public IEnumerable<Vector2Int> GetLocationsAround(Vector2Int point, int radius)
    {
        int minX = Mathf.Max(point.x - radius, 0);
        int maxX = Mathf.Min(point.x + radius, width - 1);
        int minY = Mathf.Max(point.y - radius, 0);
        int maxY = Mathf.Min(point.y + radius, height - 1);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                yield return new Vector2Int(x, y);
            }
        }
    }

    public IEnumerable<Vector2Int> GetOpenLocationsAround(Vector2Int point, int radius)
    {
        return GetLocationsAround(point, radius).Where(x => GetTile(x).IsOpen());
    }

    public IEnumerable<RogueTile> GetNeighboringTiles(RogueTile tile)
    {
        return GetNeighboringTiles(tile.location);
    }

    public IEnumerable<RogueTile> GetNeighboringTiles(Vector2Int tile)
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                //skip middle
                if (i == 0 && j == 0) continue;
                Vector2Int toCheck = tile + new Vector2Int(i, j);

                if (ValidLocation(toCheck))
                {
                    yield return GetTile(toCheck);
                }
            }
        }
    }
}