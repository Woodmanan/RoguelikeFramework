using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Specialized code anim designed to work with taregeters. While not a replacement for 
 * truly customized animations, this should give a strong backbone for adding in simpler
 * and more routine animations.
 * 
 * This class has no constructor and no state - instead, it relies entirely on it's internals 
 * being set by a targeter. That way, it becomes flexible for every type of use case.
 */

[System.Serializable]
public class TargetingAnimation : RogueAnimation
{
	[HideInInspector] public float animationDuration = .15f;
    [HideInInspector] public Vector2 origin;
    [HideInInspector] public Vector2 destination;
    [HideInInspector] public int radius;
    [HideInInspector] public RogueHandle<Monster> owner;
    [HideInInspector] public List<RogueHandle<Monster>> targets;

    private bool visible = false;

    public TargetingAnimation() : base(0.0f)
    {
    }

    public void GenerateFromTargeting(Targeting targeting, int point, RogueHandle<Monster> owner)
    {
        if (targeting != null)
        {
            origin = targeting.origin;
            destination = targeting.points[point];
            radius = targeting.radius;
            this.targets = targeting.affected;

            visible = ((owner[0].playerVisibility & Visibility.VISIBLE) > 0);
            if (!visible)
            {
                for (int y = 0; y < 2 * radius + 1; y++)
                {
                    for (int x = 0; x < 2 * radius + 1; x++)
                    {
                        Vector2Int loc = new Vector2Int(x, y) - radius * Vector2Int.one;
                        Vector2Int worldLoc = loc + targeting.origin;
                        if (Map.current.GetTile(worldLoc).isPlayerVisible)
                        {
                            visible = true;
                            break;
                        }
                    }
                    if (visible)
                    {
                        break;
                    }
                }
            }
        }
        this.owner = owner;
        OnVariablesGenerated(targeting);
    }

    public virtual void OnVariablesGenerated(Targeting targeting)
    {

    }

    public override void OnStart()
    {
    }

    public override void OnStep(float delta)
    {
    }

    public override void OnEnd()
    {
    }

    public TargetingAnimation Instantiate()
    {
        return (TargetingAnimation) this.MemberwiseClone();
    }

    public override bool IsVisible()
    {
        return visible;
    }
}
