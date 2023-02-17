using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Group("Early Branches")]
public class ConveyorSystem : DungeonSystem
{
    ConveyorTile[] tiles;
    int held = 0;

    public override void OnSetup(World world, Branch branch = null, Map map = null)
    {
        tiles = new ConveyorTile[80];
        for (int i = 0; i < map.width; i++)
        {
            for (int j = 0; j < map.height; j++)
            {
                ConveyorTile tile = map.GetTile(i, j) as ConveyorTile;
                if (tile)
                {
                    AddTile(tile);
                }
            }
        }
    }

    public void AddTile(ConveyorTile tile)
    {
        if (held == tiles.Length)
        {
            Array.Resize(ref tiles, tiles.Length * 2);
        }
        tiles[held] = tile;
        held++;
    }

    public override void OnGlobalTurnEnd(int turn)
    {
        //Force all other anims to finish before conveyor anim
        AnimationController.AddAnimation(new BlockAnimation());

        //Step 1 - Clear cached data for movement
        for (int i = 0; i < held; i++)
        {
            tiles[i].ClearMovementCache();
        }

        //Step 2 - tiles pick up what they've got (prevent loop issues)
        for (int i = 0; i < held; i++)
        {
            tiles[i].CheckMovement();
        }

        //Step 3 - tiles pass to whomever they point to.
        for (int i = 0; i < held; i++)
        {
            tiles[i].PassToMovement();
        }
    }
}
