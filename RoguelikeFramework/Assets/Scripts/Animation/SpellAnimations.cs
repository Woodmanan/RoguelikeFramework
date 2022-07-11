using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosionAnimation : RogueAnimation
{
    public const float animationDuration = .05f;
    SpriteGrid grid;
    Vector2Int center;
    int radius;
    int maxSprites;
    Targeting targeting;
    

    public ExplosionAnimation(Vector2Int center, int radius, Targeting targeting = null, params Sprite[] sprites) : base(animationDuration * (radius + 1), true)
    {
        this.center = center;
        this.radius = radius;
        maxSprites = sprites.Length - 1;
        this.targeting = targeting;
        grid = (new GameObject("ExplosionGrid")).AddComponent<SpriteGrid>();
        grid.Build(radius * 2 + 1, radius * 2 + 1, sprites.Length, 16);
        grid.AddSprites(sprites);
        grid.SetCenter(center);
    }

    public override void OnStart()
    {
        
    }

    public override void OnStep(float delta)
    {
        grid.ClearAll();

        //Calculate the current step
        int step = (int) ((currentDuration / MaxDuration) * (radius + 1));
        if (step > radius)
        {
            step = radius;
        }

        //Determine which sprites need to be shown
        for (int y = 0; y < 2 * radius + 1; y++)
        {
            for (int x = 0; x < 2 * radius + 1; x++)
            {
                Vector2Int loc = new Vector2Int(x, y) - radius * Vector2Int.one;
                Vector2Int worldLoc = loc + center;

                int rad = Mathf.Max(Mathf.Abs(loc.x), Mathf.Abs(loc.y));
                if (rad == step)
                {
                    if (targeting != null)
                    {
                        if (targeting.ContainsWorldPoint(worldLoc.x, worldLoc.y))
                        {
                            grid.SetSprite(x, y, Mathf.Min(maxSprites, step));
                        }
                    }
                    else
                    {
                        grid.SetSprite(x, y, Mathf.Min(maxSprites, step));
                    }
                }
            }
        }

        //Apply sprite updates to grid
        grid.Apply();
    }

    public override void OnEnd()
    {
        MonoBehaviour.Destroy(grid.gameObject);
    }
}

/*
public class ExplosionAnimation : RogueAnimation
{
    public const float animationDuration = .15f;
    public ExplosionAnimation() : base(animationDuration)
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
}*/
