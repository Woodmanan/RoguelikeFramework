using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MoveAnimation : RogueAnimation
{
    public const float movementDuration = .15f;
    Vector3 startLocation;
    Vector3 endLocation;
    Vector3 midPoint;
    Monster monster;

    Vector2Int oldLocation;
    Vector2Int newLocation;

    public MoveAnimation(Monster monster, Vector2Int oldLocation, Vector2Int newLocation) : base(movementDuration)
    {
        this.monster = monster;
        startLocation = new Vector3(oldLocation.x, oldLocation.y, Monster.monsterZPosition);
        endLocation = new Vector3(newLocation.x, newLocation.y, Monster.monsterZPosition);
        midPoint = (startLocation + endLocation) / 2 + Vector3.up;
        this.oldLocation = oldLocation;
        this.newLocation = newLocation;
    }

    public override void OnStart()
    {
        //Enforce location on creation
        monster.unity.transform.position = startLocation;
        monster.ForceGraphicsVisibility(Visibility.VISIBLE);
    }

    public override void OnStep(float delta)
    {
        float t = currentDuration / MaxDuration;
        Vector3 a = Vector3.Lerp(startLocation, midPoint, t);
        Vector3 b = Vector3.Lerp(midPoint, endLocation, t);

        monster.unity.transform.position = Vector3.Lerp(a, b, t);
    }

    public override void OnEnd()
    {
        monster.unity.transform.position = endLocation;
        monster.ForceGraphicsVisibility(Map.current.GetTile(newLocation).graphicsVisibility);
    }

    public override bool IsVisible()
    {
        return Map.current.GetTile(oldLocation).isPlayerVisible || Map.current.GetTile(newLocation).isPlayerVisible;
    }
}

public class SnapAnimation : RogueAnimation
{
    public const float duration = 0.025f;

    Monster monster;
    Vector2Int location;

    public SnapAnimation(RogueHandle<Monster> monster, Vector2Int location) : base(duration)
    {
        this.monster = monster;
        this.location = location;
    }

    public override void OnEnd()
    {
        monster.unity.transform.position = new Vector3(location.x, location.y, Monster.monsterZPosition);
        if (monster == Player.player.value)
        {
            LOS.WritePlayerGraphics(Map.current, location, monster.visionRadius);
        }
    }

    public override bool IsVisible()
    {
        return Map.current.GetTile(location).isPlayerVisible;
    }
}

public class AttackAnimation : RogueAnimation
{
    public const float attackDuration = .15f;

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
        monster.unity.transform.position = start;
    }

    public override void OnStep(float delta)
    {
        float t;
        float half = MaxDuration / 2;
        Vector3 midpoint = Vector3.Lerp(start, end, .5f);

        if (currentDuration < half)
        {
            t = currentDuration / half;
            monster.unity.transform.position = Vector3.Lerp(start, midpoint, t);
        }
        else
        {
            t = (currentDuration - half) / half;
            monster.unity.transform.position = Vector3.Lerp(midpoint, start, t);

        }
    }

    public override void OnEnd()
    {
        monster.unity.transform.position = start;
    }

    public override bool IsVisible()
    {
        return (monster.playerVisibility & Visibility.VISIBLE) > 0;
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
        monster.unity.transform.localScale = Vector3.one * (1f - (currentDuration / MaxDuration));
    }

    public override void OnEnd()
    {
        monster.unity.transform.localScale = Vector3.zero;
    }

    public override bool IsVisible()
    {
        return (monster.playerVisibility & (Visibility.VISIBLE | Visibility.REVEALED)) > 0;
    }
}


/*
 * Learnings from item vision attempts:
 * 
 * The constructor of this anim is the ONLY spot where
 * live data and animation can meet. Tiles cannot reliably report who is 
 * standing on them or what items contain, unless you ask the gameplay 
 * objects the moment this animation is begun.
 */
public class PlayerLOSAnimation : RogueAnimation
{
    public const float duration = 0.001f;
    Vector2Int animLocation;
    RogueHandle<Monster> monster;
    LOSData view;
    Vector2Int[] monsterLocations;

    public PlayerLOSAnimation(RogueHandle<Monster> monster) : base(duration)
    {
        this.monster = monster;
        this.animLocation = monster[0].location;
        view = LOS.LosAt(Map.current, animLocation, monster[0].visionRadius);
        monsterLocations = view.GetVisibleMonsters(RogueHandle<Monster>.Default).Select(x => x[0].location).ToArray();
    }

    public override void OnStart()
    {

    }

    public override void OnStep(float delta)
    {

    }

    public override void OnEnd()
    {
        LOS.WritePlayerGraphics(view, monsterLocations);
        //LOS.WritePlayerGraphics(Map.current, animLocation, monster.visionRadius);
    }
}
