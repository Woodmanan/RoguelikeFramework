using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAnimation : RogueAnimation
{
    public const float movementDuration = .15f;
    Vector3 startLocation;
    Vector3 endLocation;
    Vector3 midPoint;
    Monster monster;

    public MoveAnimation(Monster monster, Vector2Int oldLocation, Vector2Int newLocation) : base(movementDuration)
    {
        this.monster = monster;
        startLocation = new Vector3(oldLocation.x, oldLocation.y, Monster.monsterZPosition);
        endLocation = new Vector3(newLocation.x, newLocation.y, Monster.monsterZPosition);
        midPoint = (startLocation + endLocation) / 2 + Vector3.up;
    }

    public override void OnStart()
    {
        //Enforce location on creation
        monster.transform.position = startLocation;
    }

    public override void OnStep(float delta)
    {
        float t = currentDuration / MaxDuration;
        Vector3 a = Vector3.Lerp(startLocation, midPoint, t);
        Vector3 b = Vector3.Lerp(midPoint, endLocation, t);

        monster.transform.position = Vector3.Lerp(a, b, t);
    }

    public override void OnEnd()
    {
        monster.transform.position = endLocation;
    }
}

public class SnapAnimation : RogueAnimation
{
    public const float duration = 0.025f;

    Monster monster;
    Vector2Int location;

    public SnapAnimation(Monster monster, Vector2Int location) : base(duration)
    {
        this.monster = monster;
        this.location = location;
    }

    public override void OnEnd()
    {
        monster.transform.position = new Vector3(location.x, location.y, Monster.monsterZPosition);
    }
}

public class AttackAnimation : RogueAnimation
{
    public const float attackDuration = .3f;

    Monster monster;
    Vector3 start;
    Vector3 end;

    public AttackAnimation(Monster attacker, Monster defender) : base(attackDuration)
    {
        this.monster = attacker;
        this.start = new Vector3(attacker.location.x, attacker.location.y, Monster.monsterZPosition);
        this.end = new Vector3(defender.location.x, defender.location.y, Monster.monsterZPosition);
    }

    public override void OnStart()
    {
        //Enforce location on creation
        monster.transform.position = start;
    }

    public override void OnStep(float delta)
    {
        float t;
        float half = MaxDuration / 2;
        Vector3 midpoint = Vector3.Lerp(start, end, .5f);

        if (currentDuration < half)
        {
            t = currentDuration / half;
            monster.transform.position = Vector3.Lerp(start, midpoint, t);
        }
        else
        {
            t = (currentDuration - half) / half;
            monster.transform.position = Vector3.Lerp(midpoint, start, t);

        }
    }

    public override void OnEnd()
    {
        monster.transform.position = start;
    }

}

public class DeathAnimation : RogueAnimation
{
    public const float deathDuration = .2f;

    Monster monster;

    public DeathAnimation(Monster monster) : base(deathDuration)
    {
        this.monster = monster;
    }

    public override void OnStart()
    {
        
    }

    public override void OnStep(float delta)
    {
        monster.transform.localScale = Vector3.one * (1f - (currentDuration / MaxDuration));
    }

    public override void OnEnd()
    {
        monster.transform.localScale = Vector3.zero;
    }

}
