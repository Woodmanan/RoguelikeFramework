using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableTile : CustomTile
{
    public Query useQuery;
    public bool allowOutOfCombat = true;
    public bool allowInCombat = false;


    public virtual float Inspect(Monster toInspect)
    {
        return 1.0f;
    }

    public virtual GameAction GetAction()
    {
        Debug.LogError("Iteractable tile does not override GetAction - Therefore, monsters who use it will just wait instead.", this);
        return new WaitAction();
    }

    //Leaving costs in here, rather than in the action. This way, it can abort if necessary.
    //This is essentially just a gameaction funciton where the action is the tile itself.
    public virtual IEnumerator Interact(Monster caller)
    {
        Debug.LogError("Interactable tile doesn't support interaction? Should just be regular tile, then.", this);
        caller.energy -= 100;
        yield break;
    }

    //Called at the end of map construction, once this tile is guarunteed to be in the map!
    public override void SetInMap(Map m)
    {
        m.interactables.Add(this);
    }

    public bool FilterByCombat(bool isInCombat)
    {
        if (isInCombat) return allowInCombat;
        else return allowOutOfCombat;
    }
}
