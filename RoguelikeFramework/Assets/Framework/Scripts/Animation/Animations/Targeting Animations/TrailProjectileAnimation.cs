using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Group("Projectiles")]
public class TrailProjectileAnimation : TargetingAnimation
{
    public GameObject projectilePrefab;
    public float speed;
    public float UVSquareSize = 0.1f;
    public float dist = 1;

    GameObject projectileObject;

    public TrailProjectileAnimation() : base()
    {

    }

    public override void OnVariablesGenerated(Targeting targeting)
    {
        MaxDuration = (destination - origin).magnitude / speed;
    }

    public override void OnStart()
    {
        projectileObject = GameObject.Instantiate(projectilePrefab);
        TrailRenderer renderer = projectileObject.GetComponent<TrailRenderer>();
        renderer.time = dist / speed;
        renderer.material.SetFloat("_MainSpeed", speed / dist);
        renderer.material.SetFloat("_NoiseSpeed", speed / dist);

        float width = renderer.widthCurve.Evaluate(0);
        renderer.material.SetVector("_UVGridSize", new Vector4(UVSquareSize * dist / speed, UVSquareSize));

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
