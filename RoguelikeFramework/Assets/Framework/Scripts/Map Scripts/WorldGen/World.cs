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
            for (int level = 0; level < branch.numberOfLevels; level++)
            {
                DungeonGenerator generator = new DungeonGenerator();
                generator.name = $"{branch.branchName}:{level}";
                generator.depth = branch.branchDepth + level; //Depth increases level - fix this later if not intended
                generator.bounds = branch.size;

                generator.branch = branch;
                generator.machines = new List<Machine>();
                generator.machines.AddRange(branch.machines);
                generator.tilesAvailable = branch.tiles;

                generator.numItems = branch.numItemsPerLevel;

                generator.numMonsters = branch.numMonstersPerLevel;

                loader.generators.Add(generator);
            }
        }
    }
}
