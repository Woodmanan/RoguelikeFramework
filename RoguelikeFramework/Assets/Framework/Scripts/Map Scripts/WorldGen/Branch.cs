using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Branch", menuName = "Dungeon Generator/Branch", order = 3)]
public class Branch : ScriptableObject
{
    public List<string> requirements;
    public List<string> antiRequirements;
    public string branchName;
    public int branchDepth;
    public int numberOfLevels;
}
