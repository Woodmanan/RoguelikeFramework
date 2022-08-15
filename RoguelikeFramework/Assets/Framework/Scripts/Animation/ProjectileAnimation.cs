using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ProjectileFlyAnim : RogueAnimation
{
    Vector3 start;
    Vector3 end;
    Sprite[] sprites;
    SpriteRenderer projectile;

    public ProjectileFlyAnim(Vector2 start, Vector2 end, float speed, params Sprite[] sprites) : base((start - end).magnitude / speed, true)
    {
        this.start = start;
        this.end = end;
        this.sprites = sprites;
        if (sprites.Length == 0)
        {
            Debug.LogError("You must give a projectile animation at least one sprite!");
        }
    }

    public override void OnStart()
    {
        projectile = (new GameObject("Projectile")).AddComponent<SpriteRenderer>();
        projectile.sprite = sprites[0];
        projectile.sortingOrder = 2000;
        projectile.transform.position = start;
        projectile.transform.right = end - projectile.transform.position;
    }

    public override void OnStep(float delta)
    {
        float t = Mathf.Min(currentDuration / MaxDuration, .99f);
        projectile.sprite = sprites[(int)(sprites.Length * t)];
        projectile.transform.position = Vector3.Lerp(start, end, t);
    }

    public override void OnEnd()
    {
        MonoBehaviour.Destroy(projectile.gameObject);
    }
}

public class ProjectileBresenhamAnim : RogueAnimation
{
    Vector2Int start;
    Vector2Int end;
    Vector2Int[] line;
    Sprite[] sprites;
    SpriteRenderer projectile;

    public ProjectileBresenhamAnim(Vector2Int start, Vector2Int end, float speed, params Sprite[] sprites) : base(Bresenham.GetPointsOnLine(start.x, start.y, end.x, end.y).Count() / speed, true)
    {
        this.start = start;
        this.end = end;
        this.sprites = sprites;
        line = Bresenham.GetPointsOnLine(start.x, start.y, end.x, end.y).ToArray();

        if (sprites.Length == 0)
        {
            Debug.LogError("You must give a projectile animation at least one sprite!");
        }
    }

    public override void OnStart()
    {
        projectile = (new GameObject("Projectile")).AddComponent<SpriteRenderer>();
        projectile.sprite = sprites[0];
        projectile.sortingOrder = 2000;
        projectile.transform.position = (Vector2)start;
    }

    public override void OnStep(float delta)
    {
        float t = Mathf.Min(currentDuration / MaxDuration, .99f);
        projectile.sprite = sprites[(int)(sprites.Length * t)];
        projectile.transform.position = ((Vector2)line[(int)(line.Length * t)]);
    }

    public override void OnEnd()
    {
        MonoBehaviour.Destroy(projectile.gameObject);
    }
}
