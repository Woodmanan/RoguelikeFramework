using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Group("Projectiles")]
public class ProjectileToAllHitAnimation : TargetingAnimation
{
    public GameObject projectile;
    public float speed;

    GameObject[] activeProjectiles;

    public override void OnVariablesGenerated(Targeting targeting)
    {
        foreach (RogueHandle<Monster> target in targets)
        {
            MaxDuration = Mathf.Max((target[0].location - origin).magnitude / speed, MaxDuration);
        }
        
        activeProjectiles = new GameObject[targets.Count];
    }

    public override void OnStart()
    {

        for (int i = 0; i < targets.Count; i++)
        {
            activeProjectiles[i] = GameObject.Instantiate(projectile);
            activeProjectiles[i].transform.position = origin;
            activeProjectiles[i].transform.right = ((Vector2) targets[i][0].unity.transform.position) - origin;
        }
    }

    public override void OnStep(float delta)
    {
        for (int i = 0; i < targets.Count; i++)
        {
            Vector2 destination = targets[i][0].unity.transform.position;

            activeProjectiles[i].transform.position = Vector2.Lerp(origin, destination, currentDuration / MaxDuration);
            activeProjectiles[i].transform.right = (destination - origin);
        }
    }

    public override void OnEnd()
    {
        for (int i = targets.Count - 1; i >= 0; i--)
        {
            GameObject.Destroy(activeProjectiles[i]);
        }
    }
}
