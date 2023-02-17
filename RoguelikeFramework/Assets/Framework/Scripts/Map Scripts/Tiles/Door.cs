using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : InteractableTile
{
    bool open = false;
    public Sprite openSprite;

    public override IEnumerator Interact(Monster caller)
    {
        caller.energy -= 100;
        Open();
        caller.UpdateLOS();
        yield break;
    }

    public void Open()
    {
        open = true;
        blocksVision = false;
        blocksProjectiles = false;
        RebuildMapData();
        GetComponent<SpriteRenderer>().sprite = openSprite;
    }

    public override bool IsInteractable()
    {
        return !open;
    }

    public override float GetMovementCost()
    {
        float extra = currentlyStanding ? 5 : 1;
        if (open)
        {
            return movementCost * extra;
        }
        else
        {
            return movementCost + 1;
        }
    }
}
