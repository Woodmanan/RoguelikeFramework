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
    private Map map;
    private List<Sprite> icons;

    private Tilemap tilemap;
    // Start is called before the first frame update
    void Start()
    {
        map = GetComponent<Map>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void UpdateAllTiles()
    {
        for (int i = 0; i < map.width; i++)
        {
            for (int j = 0; j < map.width; j++)
            {
                UpdateTile(i, j);
            }
        }
    }

    void UpdateTile(int x, int y)
    {

        int z =map.getTile(x, y).icon;
    }
}
