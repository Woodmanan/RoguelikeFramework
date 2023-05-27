using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerTowerTile : InteractableTile
{
    public bool powered = true;
    public Color poweredSprite;
    public Color unpoweredSprite;

    public override IEnumerator Interact(Monster caller)
    {
        caller.energy -= 100;
        if (caller == Player.player || (caller.CompareTag("Keeper") && !powered))
        {
            powered = !powered;

            if (isVisible)
            {
                if (powered)
                {
                    Debug.Log("Console: The tower starts up!");
                }
                else
                {
                    Debug.Log("Console: The tower powers down with a whir.");
                }
            }

            this.color = powered ? poweredSprite : unpoweredSprite;
            GetComponent<SpriteRenderer>().color = color;
        }

        yield break;
    }

    public override GameAction GetAction()
    {
        Vector2Int newLoc = location + new Vector2Int(Random.Range(-3, 3), Random.Range(-3, 3));
        if (!powered)
        {
            newLoc = location;
        }

        while (!Map.current.ValidLocation(newLoc))
        {
            newLoc = location + new Vector2Int(Random.Range(-3, 3), Random.Range(-3, 3));
        }
        ActionPlan plan = new ActionPlan();
        plan.AddAction(new PathfindAction(newLoc));
        return plan;
    }
}
