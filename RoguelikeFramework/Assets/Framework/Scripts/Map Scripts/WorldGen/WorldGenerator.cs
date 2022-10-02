using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum ConnectionType
{
    AllOnSameFloor,
    SpreadEvenly
}

[System.Serializable]
public struct Target
{
    public string branchName;
    public RandomNumber floors;
    public int weight;
    public bool pickIfExists;
}

[System.Serializable]
public struct BranchChoice
{
    public string name;
    public bool enabled;
    public List<Target> entryTargets;
    public List<string> requirements;
    public List<string> antiRequirements;
    public RandomNumber numberOfBranches;
    public List<Branch> branches;
    public ConnectionType connectionType;
    public RandomNumber numberOfConnections;
    public List<Target> exitTargets;
}

[CreateAssetMenu(fileName = "New World", menuName = "Dungeon Generator/World", order = 2)]
public class WorldGenerator : ScriptableObject
{
    public List<BranchChoice> choices;
    World world;
    HashSet<string> chosenLevels;

    public World Generate()
    {
        world = new World();
        chosenLevels = new HashSet<string>();
        chosenLevels.Add("Start");

        foreach (BranchChoice current in choices)
        {
            if (!current.enabled) continue;
            bool valid = true;
            Target chosenEntryTarget = new Target();
            Target chosenExitTarget = new Target();

            { //Check for reqs and anti-reqs
                foreach (string req in current.requirements)
                {
                    valid = valid && chosenLevels.Contains(req);
                }

                foreach (string areq in current.antiRequirements)
                {
                    valid = valid && !chosenLevels.Contains(areq);
                }
            }

            { //Check for entry target - MUST HAPPEN
                List<Target> validTargets = current.entryTargets.Where(x => chosenLevels.Contains(x.branchName)).ToList();

                if (validTargets.Count == 0)
                {
                    Debug.Log("None of the entry targets were valid - stopping generation here.");
                    continue;
                }

                List<Target> priorityTargets = validTargets.Where(x => x.pickIfExists).ToList();

                if (priorityTargets.Count > 0)
                {
                    chosenEntryTarget = priorityTargets[RogueRNG.Linear(0, priorityTargets.Count)];
                }
                else
                {
                    chosenEntryTarget = validTargets[RogueRNG.Linear(0, validTargets.Count)];
                    int maxWeight = validTargets.Sum(x => x.weight);
                    int choice = RogueRNG.Linear(0, maxWeight);
                    foreach (Target t in validTargets)
                    {
                        if (choice < t.weight)
                        {
                            chosenEntryTarget = t;
                            break;
                        }
                        choice -= t.weight;
                    }
                }
            }

            if (current.exitTargets.Count > 0) // If we have exit branch options, we MUST have one of them - this implies that our dungeon is one-way
            {
                List<Target> validTargets = new List<Target>();
                foreach (Target target in current.exitTargets)
                {
                    if (chosenLevels.Contains(target.branchName))
                    {
                        validTargets.Add(target);
                    }
                }

                if (validTargets.Count == 0)
                {
                    Debug.Log("None of the exit targets were valid - stopping generation here.");
                    continue;
                }

                chosenExitTarget = validTargets[RogueRNG.Linear(0, validTargets.Count)];
            }

            int numBranches = current.numberOfBranches.Evaluate();
    
            for (int count = 0; count < numBranches; count++)
            {
                List<Branch> validBranches = new List<Branch>();
                //Generate individual branches
                foreach (Branch branch in current.branches)
                {
                    bool branchValid = true;
                    foreach (string req in branch.requirements)
                    {
                        branchValid = branchValid && chosenLevels.Contains(req);
                    }

                    foreach (string areq in branch.antiRequirements)
                    {
                        branchValid = branchValid && !chosenLevels.Contains(areq);
                    }

                    if (branchValid)
                    {
                        validBranches.Add(branch);
                    }
                }

                if (validBranches.Count == 0)
                {
                    break;
                }

                Branch branchToGen = validBranches[RogueRNG.Linear(0, validBranches.Count)];


                current.branches.Remove(branchToGen);

                //Very important - Instantiate   to remove the connection to the asset
                branchToGen = Instantiate(branchToGen);
                chosenLevels.Add(branchToGen.branchName);

                GenerateBranch(chosenEntryTarget, chosenExitTarget, branchToGen);
            }
        }

        return world;
    }

    public void GenerateBranch(Target entryTarget, Target exitTarget, Branch branchToConnect)
    {
        Branch entryBranch = world.branches.Where(x => x.branchName.Equals(entryTarget.branchName)).FirstOrDefault();

        { //Add initial connection
            LevelConnection connectionToAdd = new LevelConnection($"{entryTarget.branchName}:{entryTarget.floors.Evaluate()}", $"{branchToConnect.branchName}:0", branchToConnect.oneWay);
            connectionToAdd.fromBranch = entryBranch;
            connectionToAdd.toBranch = branchToConnect;
            world.connections.Add(connectionToAdd);
        }

        if (!string.IsNullOrEmpty(exitTarget.branchName))
        {
            //Add Exit connection
            LevelConnection connectionToAdd = new LevelConnection($"{branchToConnect.branchName}:{branchToConnect.numberOfLevels - 1}", $"{exitTarget.branchName}:{exitTarget.floors.Evaluate()}", branchToConnect.oneWay);
            connectionToAdd.fromBranch = branchToConnect;
            connectionToAdd.toBranch = world.branches.Where(x => x.branchName.Equals(exitTarget.branchName)).FirstOrDefault();
            world.connections.Add(connectionToAdd);
        }

        for (int level = 0; level < branchToConnect.numberOfLevels - 1; level++)
        {
            int numRegularConnections = branchToConnect.ConnectionsPerFloor.Evaluate();
            for (int i = 0; i < numRegularConnections; i++)
            {
                //Add standard two way connection
                LevelConnection connectionToAdd = new LevelConnection($"{branchToConnect.branchName}:{level}", $"{branchToConnect.branchName}:{level + 1}", branchToConnect.oneWay);
                connectionToAdd.fromBranch = branchToConnect;
                connectionToAdd.toBranch = branchToConnect;
                world.connections.Add(connectionToAdd);
            }

            int numOneWayConnections = branchToConnect.OneWayConnectionsPerFloor.Evaluate();
            for (int i = 0; i < numOneWayConnections; i++)
            {
                { //First one way
                    LevelConnection firstOneWayConnection = new LevelConnection($"{branchToConnect.branchName}:{level}", $"{branchToConnect.branchName}:{level + 1}", true);
                    firstOneWayConnection.fromBranch = branchToConnect;
                    firstOneWayConnection.toBranch = branchToConnect;
                    world.connections.Add(firstOneWayConnection);
                }

                { //Second one way
                    LevelConnection secondOneWayConnection = new LevelConnection($"{branchToConnect.branchName}:{level + 1}", $"{branchToConnect.branchName}:{level}", true);
                    secondOneWayConnection.fromBranch = branchToConnect;
                    secondOneWayConnection.toBranch = branchToConnect;
                    world.connections.Add(secondOneWayConnection);
                }
            }
        }

        world.branches.Add(branchToConnect);
    }
}
