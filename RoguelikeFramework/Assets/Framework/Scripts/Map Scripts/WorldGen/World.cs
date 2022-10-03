using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public List<Branch> branches = new List<Branch>();
    public List<LevelConnection> connections = new List<LevelConnection>();

    public void PrepareLevelsForLoad(LevelLoader loader)
    {
        foreach (Branch branch in branches)
        {
            branch.overrides.Sort(OverrideCompare);
            for (int level = 0; level < branch.numberOfLevels; level++)
            {
                DungeonGenerator generator = new DungeonGenerator();
                generator.name = $"{branch.branchName}:{level}";
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
}
