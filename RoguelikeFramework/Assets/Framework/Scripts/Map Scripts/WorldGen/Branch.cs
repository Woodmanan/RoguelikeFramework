using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Branch class - a grouping of levels, with their own theme, flavor, and generation styles.
 * 
 * Data only right now
 */

public enum MachineOverrideType
{
    Add,
    Delete,
    Resize,
    Replace
}

[System.Serializable]
public struct LevelOverride
{
    public string name;
    public int level;
    public MachineOverrideType type;
    [SerializeReference] public List<Machine> machines;
    public int deleteIndex;
    public Vector2Int resize;
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
    [SerializeReference]
    public List<Machine> machines;
    public List<LevelOverride> overrides;

    [Header("Visual Elements")]
    public TileList tiles;
    public Sprite entryTile;
    public Sprite exitTile;

    [Header("Item Info")]
    public List<LootTable> tables;
    public RandomNumber numItemsPerLevel;
    public bool elevatesItems;
    public ItemSpawnInfo itemSpawnInfo;
    public bool usesCustomSpawnInfo;
    public LootPool lootPool;

    [Header("Monster Info")]
    
    public List<MonsterTable> monsterTables;
    public RandomNumber numMonstersPerLevel;
    [Range(0, 100)]
    public float chanceForOutOfDepth;
    public RandomNumber depthIncrease;
}
