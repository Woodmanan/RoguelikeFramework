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
    SINGLE_MONSTER,
    MONSTER_FROM_SET,
    DOWN_STAIR,
    UP_STAIR
}

[Serializable]
public class Replacement
{
    public char glyph;
    public ReplacementOption option;
    public GameObject replacement;
    public MonsterSpawnParams monster;
    public ScriptableObject pool;
    public ItemType type;
    public ItemRarity rarity;
    public bool allSame;
}

[CreateAssetMenu(fileName = "New RexRoom", menuName = "Dungeon Generator/New RexRoom", order = 2)]
public class RexRoom : Room
{
    public TextAsset RexFile;
    public SadRex.Image image;

    public OrientOption orientOptions = OrientOption.FLIP_X | OrientOption.FLIP_Y | OrientOption.ROT_90;

    OrientOption appliedOrientation;

    public List<Replacement> conversions;

    public Dictionary<Char, Replacement> conversionDict;
    
    public override void Setup()
    {
        image = RexpaintAssetPipeline.Load(RexFile);
        size = new Vector2Int(image.Width, image.Height);
        conversionDict = new Dictionary<char, Replacement>();
        foreach (Replacement r in conversions)
        {
            conversionDict.Add(r.glyph, r);
        }

        appliedOrientation = orientOptions & (OrientOption) RogueRNG.Linear(int.MinValue, int.MaxValue);
    }

    public override int GetValueAt(int x, int y)
    {
        //Reorient for possible flips
        Vector2Int dataPosition = Reorient(x, y);

        if (dataPosition.x < 0 || dataPosition.x >= image.Layers[0].Width)
        {
            Debug.LogError("X");
        }

        if (dataPosition.y < 0 || dataPosition.y >= image.Layers[0].Height)
        {
            Debug.LogError("Y");
        }

        char c = (char)image.Layers[0][dataPosition.x, dataPosition.y].Character;
        if (conversionDict.ContainsKey(c))
        {
            return 1;
        }

        return c - '0';
    }

    public override IEnumerator PreStairActivation(Map m, DungeonGenerator generator)
    {
        //Perform Stair overrides
        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                char toReplace = (char)image.Layers[0][i, j].Character;
                Replacement r;
                if (conversionDict.TryGetValue((char)image.Layers[0][i, j].Character, out r))
                {
                    switch (r.option)
                    {
                        case ReplacementOption.DOWN_STAIR:
                            generator.desiredOutStairs.Add(start + new Vector2Int(i, j));
                            break;
                        case ReplacementOption.UP_STAIR:
                            generator.desiredInStairs.Add(start + new Vector2Int(i, j));
                            break;
                        case ReplacementOption.TILE:
                            break;
                        default:
                            Debug.LogError($"Tile layer cannot handle replacement of {r.glyph} with type {r.option} at this step.");
                            break;
                    }

                }
                yield return null;
            }
            yield return null;
        }
    }


    public override IEnumerator PostActivation(Map map, DungeonGenerator generator)
    {
        Vector2Int adjustedSize = GetSize();

        //Perform floor overrides first
        for (int i = 0; i < adjustedSize.x; i++)
        {
            for (int j = 0; j < adjustedSize.y; j++)
            {
                Vector2Int localPosition = new Vector2Int(i, j);
                Vector2Int worldPosition = localPosition + start;
                Vector2Int dataPosition = Reorient(localPosition);

                Replacement r;
                if (conversionDict.TryGetValue((char)image.Layers[0][dataPosition.x, dataPosition.y].Character, out r))
                {
                    switch (r.option)
                    { 
                        case ReplacementOption.TILE:
                            RogueTile tile = Instantiate(r.replacement).GetComponent<RogueTile>();
                            RogueTile toReplace = map.GetTile(worldPosition);
                            tile.gameObject.name = toReplace.gameObject.name;
                            tile.location = toReplace.location;
                            Transform parent = toReplace.transform.parent;
                            map.tiles[worldPosition.x, worldPosition.y] = tile;
                            Destroy(toReplace.gameObject);
                            tile.transform.parent = parent;
                            tile.transform.position = new Vector3(tile.location.x, tile.location.y, 0);
                            tile.SetMap(map, tile.location);
                            yield return null;
                            break;
                        case ReplacementOption.DOWN_STAIR:
                            //generator.desiredOutStairs.Add(start + new Vector2Int(i, j));
                            break;
                        case ReplacementOption.UP_STAIR:
                            //generator.desiredInStairs.Add(start + new Vector2Int(i, j));
                            break;
                        default:
                            Debug.LogError($"Tile layer cannot handle replacement of {r.glyph} with type {r.option}");
                            break;
                    }
                    
                }
                yield return null;
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

            for (int i = 0; i < adjustedSize.x; i++)
            {
                for (int j = 0; j < adjustedSize.y; j++)
                {
                    Vector2Int localPosition = new Vector2Int(i, j);
                    Vector2Int worldPosition = localPosition + start;
                    Vector2Int dataPosition = Reorient(localPosition);

                    if (!image.Layers[1][dataPosition.x, dataPosition.y].IsTransparent())
                    {
                        Replacement r;
                        if (conversionDict.TryGetValue((char)image.Layers[1][dataPosition.x, dataPosition.y].Character, out r))
                        {

                            if (r.option == ReplacementOption.SINGLE_ITEM)
                            {
                                //Instantiate item
                                if (r.replacement == null)
                                {
                                    continue;
                                }
                                Item item = r.replacement.GetComponent<Item>().Instantiate();

                                //Place it in the world
                                item.transform.parent = map.itemContainer;
                                if (map.GetTile(worldPosition).BlocksMovement())
                                {
                                    Debug.LogError("RexRoom tried to place an object in a wall. Wut?");
                                    yield break;
                                }

                                map.GetTile(worldPosition).inventory.Add(item);
                                yield return null;
                            }
                            else
                            {
                                //Create and trim loot pool
                                if (r.pool == null)
                                {
                                    continue;
                                }
                                LootTable pool = Instantiate<LootTable>((LootTable)r.pool);
                                yield return null;

                                //Generate item from trimmed pool, based on specs.
                                Item item = pool.RandomItemByRarity(r.rarity, true);

                                if (item == null)
                                {
                                    Debug.LogWarning($"Pool couldn't spawn item of rarity {r.rarity} or lower into room");
                                }

                                //Place item into world.
                                item.transform.parent = map.itemContainer;
                                map.GetTile(worldPosition).inventory.Add(item);
                                yield return null;

                                if (r.allSame)
                                {
                                    r.replacement = item.gameObject;
                                    r.option = ReplacementOption.SINGLE_ITEM;
                                }
                            }
                        }
                    }
                }
                yield return null;
            }
        }

        //Item overrides
        if (image.LayerCount > 2)
        {
            List<Replacement> monsters = conversions.Where(x => x.option == ReplacementOption.SINGLE_MONSTER
                                                          || x.option == ReplacementOption.MONSTER_FROM_SET).ToList();
            for (int i = 0; i < monsters.Count; i++)
            {
                if (monsters[i].option == ReplacementOption.MONSTER_FROM_SET && monsters[i].allSame)
                {
                    monsters[i].replacement = null;
                }
            }

            for (int i = 0; i < adjustedSize.x; i++)
            {
                for (int j = 0; j < adjustedSize.y; j++)
                {
                    Vector2Int localPosition = new Vector2Int(i, j);
                    Vector2Int worldPosition = localPosition + start;
                    Vector2Int dataPosition = Reorient(localPosition);

                    if (!image.Layers[2][dataPosition.x, dataPosition.y].IsTransparent())
                    {
                        Replacement r;
                        if (conversionDict.TryGetValue((char)image.Layers[2][dataPosition.x, dataPosition.y].Character, out r))
                        {
                            if (r == null)
                            {
                                continue;
                            }
                            if (r.option == ReplacementOption.SINGLE_MONSTER)
                            {
                                //Instantiate monster
                                if (r.replacement == null)
                                {
                                    continue;
                                }

                                RogueHandle<Monster> monster = r.monster.SpawnMonster();
                                Monster vMonster = monster;

                                //Place it in the world
                                vMonster.unity.transform.parent = map.monsterContainer;
                                map.monsters.Add(monster);
                                vMonster.location = worldPosition;
                                map.GetTile(worldPosition).SetMonster(monster);
                                vMonster.currentTile = map.GetTile(worldPosition);
                                vMonster.unity.transform.parent = map.monsterContainer;
                                vMonster.level = map.depth;
                                vMonster.Setup();
                                yield return null;
                            }
                            else
                            {
                                //Create and trim loot pool
                                if (r.pool == null)
                                {
                                    continue;
                                }
                                MonsterTable pool = Instantiate<MonsterTable>((MonsterTable)r.pool);
                                yield return null;

                                //Generate monster from table, based on depth
                                RogueHandle<Monster> monster = MonsterSpawner.singleton.SpawnMonster(pool.RandomMonsterByDepth(map.depth), worldPosition, map, false);

                                yield return null;
                            }
                        }
                    }
                }
                yield return null;
            }
        }
    }

    public Vector2Int OrientedPosition(int x, int y)
    {
        if ((appliedOrientation & OrientOption.FLIP_X) > 0)
        {
            x = (size.x - 1) - x;
        }

        if ((appliedOrientation & OrientOption.FLIP_Y) > 0)
        {
            y = (size.y - 1) - y;
        }

        //Inputs will be asking in switched form - convert back to regular transform
        if ((appliedOrientation & OrientOption.ROT_90) > 0)
        {
            int hold = x;
            x = (size.y - 1) - y;
            y = hold;
        }

        return new Vector2Int(x, y);
    }

    public Vector2Int Reorient(Vector2Int localPosition)
    {
        return Reorient(localPosition.x, localPosition.y);
    }

    public Vector2Int Reorient(int x, int y)
    {
        Vector2Int rotSize = GetSize();
        //Inputs will be asking in switched form - convert back to regular transform
        if ((appliedOrientation & OrientOption.ROT_90) > 0)
        {
            int hold = x;
            x = (rotSize.y - 1) - y;
            y = hold;
        }

        if ((appliedOrientation & OrientOption.FLIP_X) > 0)
        {
            x = (size.x - 1) - x;
        }

        if ((appliedOrientation & OrientOption.FLIP_Y) > 0)
        {
            y = (size.y - 1) - y;
        }

        return new Vector2Int(x, y);
    }


    public override Vector2Int GetSize()
    {
        if ((appliedOrientation & OrientOption.ROT_90) > 0)
        {
            return new Vector2Int(size.y, size.x);
        }
        return size;
    }
}
