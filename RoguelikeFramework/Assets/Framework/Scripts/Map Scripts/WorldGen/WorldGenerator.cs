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
    public int minFloor;
    public int maxFloor;
}

[System.Serializable]
public struct BranchChoice
{
    public string name;
    public List<Target> targets;
    public List<string> requirements;
    public List<string> antiRequirements;
    public RandomNumber numberOfBranches;
    public List<Branch> branches;
    public ConnectionType connectionType;
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
            Debug.Log($"Starting step {current.name}");
            bool valid = true;
            foreach (string req in current.requirements)
            {
                valid = valid && chosenLevels.Contains(req);
            }

            foreach (string areq in current.antiRequirements)
            {
                valid = valid && !chosenLevels.Contains(areq);
            }

            List<Target> validTargets = new List<Target>();
            foreach (Target target in current.targets)
            {
                if (chosenLevels.Contains(target.branchName))
                {
                    validTargets.Add(target);
                }
            }

            if (validTargets.Count == 0)
            {
                Debug.Log("None of the targets were valid - stopping generation here.");
                continue;
            }

            Target chosenTarget = validTargets[RogueRNG.Linear(0, validTargets.Count)];

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

                //Very important - Instantiate to remove the connection to the asset
                branchToGen = Instantiate(branchToGen);
                chosenLevels.Add(branchToGen.branchName);

                GenerateBranch(chosenTarget.branchName, RogueRNG.Linear(chosenTarget.minFloor, chosenTarget.maxFloor + 1), branchToGen);
            }
        }

        return world;
    }

    public void GenerateBranch(string target, int floor, Branch branch)
    {
        Debug.Log($"Generated connection: {target} ({floor}) -> {branch.branchName} (0)");
        world.branches.Add(branch);
    }
}
