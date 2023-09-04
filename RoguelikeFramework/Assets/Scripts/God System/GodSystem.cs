using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GodSystem : DungeonSystem
{
    public GodDefinition[] gods;
    public Monster[] candidates;

    public int numCandidates;

    public override void OnSetup(World world, Branch branch = null, Map map = null)
    {
        Debug.Assert(world != null);

        gods = gods.Select(x => GodDefinition.Instantiate(x)).ToArray();

        candidates = new Monster[numCandidates];
        candidates[0] = Player.player;

        //TODO: Set up the rest of the profiles

        foreach (GodDefinition definition in gods)
        {
            definition.Setup(this);
        }
    }

    public Monster this[int index]
    {
        get { return candidates[index]; }
        set { candidates[index] = value; }
    }

}
