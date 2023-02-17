using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Group("Early Branches")]
public class PowerSystem : DungeonSystem
{
    Quadtree<PowerTowerTile> tiles;

    public override void OnSetup(World world, Branch branch = null, Map map = null)
    {
        tiles = new Quadtree<PowerTowerTile>(new Rect(Vector2.zero, new Vector2(map.width, map.height)));
        if (map == null)
        {
            Debug.LogError("This must be a map system! Can't be anything else.");
            return;
        }

        foreach (InteractableTile tile in map.interactables)
        {
            PowerTowerTile powerTile = tile as PowerTowerTile;
            if (powerTile)
            {
                tiles.Insert(powerTile, new Rect(powerTile.location, Vector2.one));
            }
        }
    }

    public List<PowerTowerTile> GetTilesInRange(Monster m, float radius)
    {
        Vector2 center = Vector2.one / 2 + m.location;
        return tiles.GetItemsIn(center, radius);
    }

    public override void OnGlobalTurnStart(int turn)
    {

    }

    public override void OnGlobalTurnEnd(int turn)
    {

    }

    public override void OnEnterLevel(Map m)
    {

    }

    public override void OnExitLevel(Map m)
    {

    }
}
