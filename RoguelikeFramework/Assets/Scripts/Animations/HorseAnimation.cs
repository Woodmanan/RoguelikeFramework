using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorseAnimation : RogueAnimation
{
    public const float movementDuration = 0.15f;
    Vector3 startLocation;
    Vector3 endLocation;
    RogueTile tile;

    Monster standing;

    public HorseAnimation(RogueTile tile, Vector2Int oldLocation, Vector2Int newLocation, bool isBlocking = false) : base(movementDuration, isBlocking)
    {
        this.tile = tile;
        this.startLocation = new Vector3(oldLocation.x, oldLocation.y, tile.transform.position.z);
        this.endLocation = new Vector3(newLocation.x, newLocation.y, tile.transform.position.z);
        if (tile.currentlyStanding)
        {
            standing = tile.currentlyStanding;
        }
    }

    public void SetPosition(Vector3 position)
    {
        tile.AnimUpdatePosition(position);
    }

    public override void OnStart()
    {
        SetPosition(startLocation);
    }

    public override void OnStep(float delta)
    {
        float t = currentDuration / MaxDuration;
        SetPosition(Vector3.Lerp(startLocation, endLocation, t));
    }

    public override void OnEnd()
    {
        SetPosition(endLocation);
    }
}
