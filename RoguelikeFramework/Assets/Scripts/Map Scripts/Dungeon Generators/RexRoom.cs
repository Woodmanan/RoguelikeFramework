using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Linq;

//Hahaha the name sounds cool
/* 
 * This would probably be better named as a "Prefab Room" type.
 * They're a standard room, but derived from a RexPaint file (which is much
 * easier to edit than a text box), and also can contain monsters and items.
 */

[Serializable]
public struct Replacement
{
    public char glyph;
    public GameObject replacement;
}

[CreateAssetMenu(fileName = "New RexRoom", menuName = "Dungeon Generator/New RexRoom", order = 2)]
public class RexRoom : Room
{
    public TextAsset RexFile;
    public SadRex.Image image;

    public List<Replacement> conversions;
    
    public override void Setup()
    {
        Debug.Log("I was setup!");
        image = Load();
        size = new Vector2Int(image.Width, image.Height);
    }

    public override int GetValueAt(int x, int y)
    {
        char c = (char)image.Layers[0][x, y].Character;
        if (conversions.Any(r => r.glyph == c))
        {
            return 0;
        }

        return c - '0';
    }

    
    public override void PostActivation(Map map)
    {
        //Perform floor overrides first
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                char c = (char)image.Layers[0][i, j].Character;
                if (conversions.Any(x => x.glyph == c))
                {
                    Replacement r = conversions.First(x => x.glyph == c);
                    CustomTile tile = Instantiate(r.replacement).GetComponent<CustomTile>();
                    CustomTile toReplace = map.GetTile(start + new Vector2Int(i, j));
                    tile.gameObject.name = toReplace.gameObject.name;
                    tile.location = toReplace.location;
                    Transform parent = toReplace.transform.parent;
                    map.tiles[start.x + i, start.y + j] = tile;
                    Destroy(toReplace.gameObject);
                    tile.transform.parent = parent;
                    tile.transform.position = new Vector3(tile.location.x, tile.location.y, 0);
                }
            }
        }
    }    
    public SadRex.Image Load()
    {
        MemoryStream stream = new MemoryStream(System.Convert.FromBase64String(RexFile.text));
        return SadRex.Image.Load(stream);
    }
}
