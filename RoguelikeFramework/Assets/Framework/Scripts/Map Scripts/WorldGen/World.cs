﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/**************************
 * World
 **************************
 * 
 * Top level class that defines how the dungeon is structured
 * Contains branch information, connection info, and the level structures themselves
 * Currently NOT tied to the level loader - leaving those separate just for better 
 * corrections down the line
 */

public class World
{
    public static World current;

    public List<Branch> branches = new List<Branch>();
    public List<LevelConnection> connections = new List<LevelConnection>();
    public List<DungeonSystem> systems = new List<DungeonSystem>();
    
    [SerializeReference]
    public List<Effect> playerPassives;

    [SerializeReference]
    public List<Effect> monsterPassives;

    Dictionary<string, object> blackboard = new Dictionary<string, object>();


    public void PrepareLevelsForLoad(LevelLoader loader)
    {
        foreach (Branch branch in branches)
        {
            branch.overrides.Sort(OverrideCompare);
            for (int level = 0; level < branch.numberOfLevels; level++)
            {
                DungeonGenerator generator = new DungeonGenerator();
                generator.name = $"{branch.branchName}:{level}";
                generator.level = level + 1;
                generator.depth = branch.branchDepth + level; //Depth increases level - fix this later if not intended
                generator.bounds = branch.size;

                generator.branch = branch;
                generator.machines = new List<Machine>();
                generator.tilesAvailable = branch.tiles;

                generator.machines.AddRange(branch.machines);
                
                foreach (LevelOverride levelOverride in branch.overrides)
                {
                    if (levelOverride.level == level)
                    {
                        switch (levelOverride.type)
                        {
                            case MachineOverrideType.Add:
                                generator.machines.AddRange(levelOverride.machines);
                                break;
                            case MachineOverrideType.Delete:
                                generator.machines.RemoveAt(levelOverride.deleteIndex);
                                break;
                            case MachineOverrideType.Resize:
                                generator.bounds = levelOverride.resize;
                                break;
                            case MachineOverrideType.Replace:
                                generator.bounds = levelOverride.resize;
                                generator.machines.Clear();
                                generator.machines.AddRange(levelOverride.machines);
                                break;
                        }
                    }
                }

                generator.numItems = branch.numItemsPerLevel;

                generator.numMonsters = branch.numMonstersPerLevel;

                loader.generators.Add(generator);

                //branch.branchSystems = branch.branchSystems.Select(x => x.Instantiate()).ToList();
                /*foreach (DungeonSystem system in branch.branchSystems)
                {
                    system.Setup(this, branch);
                }*/
            }
        }
    }

    public int OverrideCompare(LevelOverride one, LevelOverride two)
    {
        int comp = one.type.CompareTo(two.type);
        if (comp == 0)
        {
            comp = two.deleteIndex.CompareTo(one.deleteIndex);
        }
        return comp;
    }

    public void BlackboardWrite<T>(string name, T value)
    {
        blackboard.Add(name, value as object);
    }

    public T BlackboardRead<T>(string name)
    {
        object read;
        if (blackboard.TryGetValue(name, out read))
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (read.GetType() != typeof(T))
            {
                Debug.LogError($"Key of name '{name}' did not have the requested type of {typeof(T).Name}");
            }
            #endif
        }
        else
        {
            Debug.LogError($"Key of name '{name}' did not exist in blackboard!");
        }
        return (T) read;
    }
}
