using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Group("Systems")]
public class SystemMachine : Machine
{
    [SerializeReference]
    public List<DungeonSystem> systems;

    public override IEnumerator Activate()
    {
        yield break;
    }

    public override void PostActivation(Map m)
    {
        if (systems.Count == 0)
        {
            Debug.LogError($"Dungeon system machine on floor {m.name} had 0 systems - this seems like a mistake.");
        }

        foreach (DungeonSystem system in systems)
        {
            m.mapSystems.Add(system.Instantiate());
        }
    }
}