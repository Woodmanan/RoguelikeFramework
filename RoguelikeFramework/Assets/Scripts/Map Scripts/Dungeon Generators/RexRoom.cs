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

public enum ReplacementOption
{
    TILE,
    SINGLE_ITEM,
    ITEM_FROM_SET,
    SINGLE_MONSTER
}

[Serializable]
public class Replacement
{
    public char glyph;
    public ReplacementOption option;
    public GameObject replacement;
    public LootPool pool;
    public ItemType type;
    public ItemRarity rarity;
    public bool allSame;
}

[CreateAssetMenu(fileName = "New RexRoom", menuName = "Dungeon Generator/New RexRoom", order = 2)]
public class RexRoom : Room
{
    public TextAsset RexFile;
    public SadRex.Image image;

    public List<Replacement> conversions;
    
    public override void Setup()
    {
        image = RexpaintAssetPipeline.Load(RexFile);
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

    
    public override IEnumerator PostActivation(Map map)
    {
        List<Replacement> tiles = conversions.Where(x => x.option == ReplacementOption.TILE).ToList();
        //Perform floor overrides first
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                char c = (char)image.Layers[0][i, j].Character;
                if (tiles.Any(x => x.glyph == c))
                {
                    Replacement r = tiles.First(x => x.glyph == c);
                    CustomTile tile = Instantiate(r.replacement).GetComponent<CustomTile>();
                    CustomTile toReplace = map.GetTile(start + new Vector2Int(i, j));
                    tile.gameObject.name = toReplace.gameObject.name;
                    tile.location = toReplace.location;
                    Transform parent = toReplace.transform.parent;
                    map.tiles[start.x + i, start.y + j] = tile;
                    Destroy(toReplace.gameObject);
                    tile.transform.parent = parent;
                    tile.transform.position = new Vector3(tile.location.x, tile.location.y, 0);
                    tile.SetMap(map, tile.location);
                    yield return null;
                }
            }
            yield return null;
        }

        yield return null;

        //Item overrides
        if (image.LayerCount > 1)
        {
            List<Replacement> items = conversions.Where(x => x.option == ReplacementOption.SINGLE_ITEM
                                                          || x.option == ReplacementOption.ITEM_FROM_SET).ToList();
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i].option == ReplacementOption.ITEM_FROM_SET && items[i].allSame)
                {
                    items[i].replacement = null;
                }
            }

            for (int i = 0; i < size.x; i++)
            {
                for (int j = 0; j < size.y; j++)
                {
                    if (!image.Layers[1][i,j].IsTransparent())
                    {
                        char c = (char)image.Layers[1][i, j].Character;
                        Replacement r = items.First(x => x.glyph == c);
                        if (r.option == ReplacementOption.SINGLE_ITEM)
                        {
                            //Instantiate item
                            Item item = r.replacement.GetComponent<Item>().Instantiate();
                            Vector2Int pos = start + new Vector2Int(i, j);

                            //Place it in the world
                            item.transform.parent = map.itemContainer;
                            map.GetTile(pos).inventory.Add(item);
                            yield return null;
                        }
                        else
                        {
                            //Create and trim loot pool
                            LootPool pool = Instantiate(r.pool);
                            if (pool.shouldTrim) pool.TrimToDepth(map.depth);
                            yield return null;

                            //Generate item from trimmed pool, based on specs.
                            Item item = pool.GenerateItem(r.type, r.rarity);

                            //Place item into world.
                            Vector2Int pos = start + new Vector2Int(i, j);
                            item.transform.parent = map.itemContainer;
                            map.GetTile(pos).inventory.Add(item);
                            yield return null;

                            if (r.allSame)
                            {
                                r.replacement = item.gameObject;
                                r.option = ReplacementOption.SINGLE_ITEM;
                            }
                        }
                    }
                }
                yield return null;
            }

        }
    }
}
