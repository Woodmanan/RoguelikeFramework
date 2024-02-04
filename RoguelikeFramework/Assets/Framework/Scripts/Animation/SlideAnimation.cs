using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideAnimation : RogueAnimation
{
    public const float movementDuration = .15f;
    Vector3 startLocation;
    Vector3 endLocation;
    Vector3 midPoint;
    Monster monster;

    public SlideAnimation(Monster monster, Vector2Int oldLocation, Vector2Int newLocation) : base(movementDuration)
    {
        this.monster = monster;
        startLocation = new Vector3(oldLocation.x, oldLocation.y, Monster.monsterZPosition);
        endLocation = new Vector3(newLocation.x, newLocation.y, Monster.monsterZPosition);
    }

    public override void OnStart()
    {
        //Enforce location on creation
        monster.unity.transform.position = startLocation;
    }

    public override void OnStep(float delta)
    {
        float t = currentDuration / MaxDuration;
        monster.unity.transform.position = Vector3.Lerp(startLocation, endLocation, t);
    }

    public override void OnEnd()
    {
        monster.unity.transform.position = endLocation;
    }
}
