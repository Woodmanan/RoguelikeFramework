using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Group("Midgame Branches")]
public class JungleSecretSystem : DungeonSystem
{
    List<int> totems;
    public override void OnSetup(World world, Branch branch = null, Map map = null)
    {
        totems = new List<int>(6);
        for (int i = 0; i < 6; i++)
        {
            totems.Add(-1);
        }
    }

    public override void OnGlobalTurnStart(int turn)
    {

    }

    public override void OnGlobalTurnEnd(int turn)
    {

    }

    public override void OnEnterLevel(Map m)
    {
        if (m.level == 7)
        {
            //Check if we should unlock the City of Gold!
            Debug.Log($"Entering the map! {m.name}");

            int[] path = World.current.BlackboardRead<int[]>("COG Path");
            bool onPath = true;
            for (int i = 0; i < 6; i++)
            {
                if (path[i] != totems[i])
                {
                    onPath = false;
                    break;
                }
            }

            Vector2Int location = World.current.BlackboardRead<Vector2Int>($"{m.name}:0");
            Stair stair = m.GetTile(location) as Stair;
            if (stair == null)
            {
                Debug.LogError("COG tile not in the right spot - something has gone horribly wrong");
            }

            if (onPath)
            {
                Debug.Log("On the path!!!! YAY!");
                stair.Unlock();
            }
            else
            {
                stair.Lock();
            }
        }
    }

    public override void OnExitLevel(Map m)
    {
        if (m.level != 7)
        {
            Debug.Log($"Exiting the map! {m.name} {m.level}");
            TotemType[] mapTotems = World.current.BlackboardRead<TotemType[]>($"{m.name} Totems");
            for (int i = 0; i < 3; i++)
            {
                Vector2Int location = World.current.BlackboardRead<Vector2Int>($"{m.name}:{i}");
                if (location == Player.player.location)
                {
                    totems[m.level - 1] = i;
                    Debug.Log($"Totems {m.level - 1} is now {mapTotems[i]}");
                    return;
                }
            }
        }
    }
}
