using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Map))]

/* Initially planned to make this acutally interface with the rendering system, maybe I'll do that on the backpass.
 * For now, this acts as a wrapper for the system that controls a tilemap and tilemap renderer, and uses those to
*/
public class MapRender : MonoBehaviour
{
    [SerializeField] private Map map;
    [SerializeField] private TileList tilelist;
    
    [SerializeField] private Tilemap tilemap;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    

    public void UpdateAllTiles()
    {
        for (int i = 0; i < map.width; i++)
        {
            for (int j = 0; j < map.width; j++)
            {
                UpdateTile(i, j);
            }
        }
    }

    public void UpdateTile(int x, int y)
    {
        //Expensive-ish assertion for while we're in the editor
        #if UNITY_EDITOR
        int iconCount = map.getTile(x, y).icon;
        if (tilelist.tiles.Length <= iconCount)
        {
            Debug.LogError($"Error with tile count at ({x}, {y}): Tile is #{iconCount}, but there are only {tilelist.tiles.Length} tiles.");
        }
        #endif
        
        //Get the sprite the Tilemap[x, y] should be set to.
        TileBase newTile = tilelist.tiles[map.getTile(x, y).icon];

        tilemap.SetTile(new Vector3Int(x, y, 0), newTile);
    }
}
