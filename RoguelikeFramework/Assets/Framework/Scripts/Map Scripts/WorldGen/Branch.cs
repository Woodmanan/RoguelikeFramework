using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Branch class - a grouping of levels, with their own theme, flavor, and generation styles.
 * 
 * Data only right now
 */

[System.Serializable]
public struct LevelOverride
{
    public int level;
    public Machine machineToAdd;
}

[CreateAssetMenu(fileName = "New Branch", menuName = "Dungeon Generator/Branch", order = 3)]
public class Branch : ScriptableObject
{
    [Header("World Gen Parameters")]
    public List<string> requirements;
    public List<string> antiRequirements;
    public string branchName;
    public int branchDepth;

    [Header("Level Gen Parameters")]
    public int numberOfLevels;
    public RandomNumber ConnectionsPerFloor;
    public RandomNumber OneWayConnectionsPerFloor;
    public bool oneWay = false;
    public Vector2Int size;
    public List<Machine> machines;
    public List<LevelOverride> overrides;

    [Header("Visual Elements")]
    public TileList tiles;
    public Sprite entryTile;
    public Sprite exitTile;

    [Header("Monsters and Items")]
    public LootPool availableItems;
    public RandomNumber numItemsPerLevel;

    public MonsterPool availableMonsters;
    public RandomNumber numMonstersPerLevel;
}
