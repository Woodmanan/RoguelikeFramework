using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Group("Middle Branches")]
public class NightmareSystem : DungeonSystem
{
    public Monster MonsterToSpawn;
    public RandomNumber timeBetweenSummons;
    public int summonWarningAt;
    int timeUntilNext;

    public RandomNumber nightmareLifetime;
    public int lifetimeWarningAt;

    public RandomNumber messageEvery;
    int messageTime;

    bool currentlySummoned = false;

    Monster currentNightmare;

    Vector2Int spawnAround;

    public override void OnSetup(World world, Branch branch = null, Map map = null)
    {
        timeUntilNext = timeBetweenSummons.Evaluate();
        messageTime = 20 + messageEvery.Evaluate();
    }

    public void TickNightmareSummonOnly()
    {
        if (!currentlySummoned)
        {
            TickNightmare();
        }
    }

    public void TickNightmare()
    {
        timeUntilNext--;
        Debug.Log(timeUntilNext);

        if (!currentlySummoned)
        {
            if (timeUntilNext == 0)
            {
                Spawn();
                timeUntilNext = nightmareLifetime.Evaluate();
                Debug.Log("Console: Ah, there you are.");
            }
            else if (timeUntilNext == summonWarningAt)
            {
                Debug.Log("Console: You feel uneasy. Something is coming.");
                spawnAround = Player.player.location;
            }
        }
        else
        {
            if (timeUntilNext == 0)
            {
                Despawn();
                timeUntilNext = timeBetweenSummons.Evaluate();
                Debug.Log("Console: The nightmare fades away... for now.");
            }
            else if (timeUntilNext == lifetimeWarningAt)
            {
                Debug.Log("Console: You feel yourself becoming more lucid.");
            }
        }
    }


    public override void OnGlobalTurnEnd(int turn)
    {
        //Handle creepy messages
        if (!currentlySummoned && timeUntilNext > 15)
        {
            messageTime--;
            if (messageTime == 0)
            {
                PlayMessage();
                messageTime = messageEvery.Evaluate();
            }
        }

        TickNightmare();
    }

    public void PlayMessage()
    {
        Debug.Log("Console: Thump.");
    }

    public void Spawn()
    {
        Vector2Int spawnLoc = GetNearestUnexploredTile();
        if (spawnLoc.x != -1)
        {
            currentNightmare = MonsterSpawner.singleton.SpawnMonsterInstantiate(MonsterToSpawn, spawnLoc, Map.current);
            currentNightmare.GetComponent<MonsterAI>().SetToFollow(Player.player);
            currentlySummoned = true;
        }
    }

    public Vector2Int GetNearestUnexploredTile()
    {
        //Build up the points we need!
        List<Vector2Int> goals = new List<Vector2Int>();
        for (int i = 1; i < Map.current.width - 1; i++)
        {
            for (int j = 1; j < Map.current.height - 1; j++)
            {
                Vector2Int pos = new Vector2Int(i, j);
                RogueTile tile = Map.current.GetTile(pos);
                if (!tile.BlocksMovement() && tile.isHidden && tile.currentlyStanding == null)
                {
                    goals.Add(pos);
                }
            }
        }

        if (goals.Count == 0)
        {
            Debug.Log("Log: There's nothing else to explore!");

            return -1 * Vector2Int.one;
        }

        Path path = Pathfinding.CreateDjikstraPath(Player.player.location, goals);

        if (path.Count() == 0)
        {
            Debug.Log("Log: Can't reach anymore unexplored spaces!");
            return -1 * Vector2Int.one;
        }

        return path.destination;
    }

    public void Despawn()
    {
        //AnimationController.AddAnimation(new BlockAnimation());
        if (currentNightmare != null)
        {
            currentNightmare.DestroyMonster();
        }
        else
        {
            //Slaying the nightmare gives you a much longer breather.
            timeUntilNext = 80 + timeBetweenSummons.Evaluate();
        }
        currentlySummoned = false;
    }
}
