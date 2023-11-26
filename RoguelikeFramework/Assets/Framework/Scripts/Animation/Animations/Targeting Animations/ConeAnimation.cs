using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Group("Area")]
public class ConeAnimation : TargetingAnimation
{
    public float durationPerTile = .02f;
    Targeting targeting;
    SpriteGrid grid;
    public Sprite[] sprites;
    public bool spreadSpritesEvenly = true;

    Vector2Int center;

    public ConeAnimation() : base()
    {
        
    }

    public override void OnVariablesGenerated(Targeting targeting)
    {
        this.targeting = targeting;
        grid = (new GameObject("ExplosionGrid")).AddComponent<SpriteGrid>();
        grid.Build(radius * 2 + 1, radius * 2 + 1, sprites.Length, Mathf.RoundToInt(sprites[0].rect.width));
        grid.AddSprites(sprites);
        grid.SetCenter(origin);

        center = new Vector2Int(Mathf.RoundToInt(origin.x), Mathf.RoundToInt(origin.y));

        MaxDuration = durationPerTile * (radius + 1);
    }

    public override void OnStart()
    {
        
    }

    public override void OnStep(float delta)
    {
        grid.ClearAll();

        //Calculate the current step
        int step = (int)((currentDuration / MaxDuration) * (radius + 1));
        int spriteNum = (int)((currentDuration / MaxDuration) * (sprites.Length));
        if (step > radius)
        {
            step = radius;
        }

        if (!spreadSpritesEvenly)
        {
            spriteNum = step;
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
                            grid.SetSprite(x, y, Mathf.Min(sprites.Length - 1, spriteNum));
                        }
                    }
                    else
                    {
                        grid.SetSprite(x, y, Mathf.Min(sprites.Length - 1, spriteNum));
                    }
                }
            }
        }

        //Apply sprite updates to grid
        grid.Apply();
    }

    public override void OnEnd()
    {
        GameObject.Destroy(grid.gameObject);
    }
}
