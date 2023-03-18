using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Group("Projectiles")]
public class ProjectileAnimation : TargetingAnimation
{
    public Sprite sprite;
    public float speed;

    GameObject projectileObject;

    public ProjectileAnimation() : base()
    {
        
    }

    public override void OnVariablesGenerated(Targeting targeting)
    {
        MaxDuration = (destination - origin).magnitude / speed;
    }

    public override void OnStart()
    {
        projectileObject = new GameObject("Animation Projectile", typeof(SpriteRenderer));
        projectileObject.GetComponent<SpriteRenderer>().sprite = sprite;
        projectileObject.transform.position = origin;
        projectileObject.transform.right = destination - origin;
    }

    public override void OnStep(float delta)
    {
        projectileObject.transform.position = Vector2.Lerp(origin, destination, currentDuration / MaxDuration);
        projectileObject.transform.right = ((Vector3)destination - projectileObject.transform.position);
    }

    public override void OnEnd()
    {
        GameObject.Destroy(projectileObject);
    }
}
