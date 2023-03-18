using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[Group("End Branches")]
public class HorseSystem : DungeonSystem
{
    public RandomNumber numHorsesPerBurst;
    public RandomNumber numBursts;
    public RandomNumber timeBetweenBursts;
    public RogueTile tilePrefab;
    public RogueTile airPrefab;

    List<RogueTile> currentHorses;
    RogueTile goal;

    List<RogueTile> goalSpots;
    List<RogueTile> rightGoals;
    List<RogueTile> leftGoals;
    int currentDuration;
    int bursts;

    public override void OnSetup(World world, Branch branch = null, Map map = null)
    {
        goalSpots = new List<RogueTile>();
        rightGoals = new List<RogueTile>();
        leftGoals = new List<RogueTile>();
        currentHorses = new List<RogueTile>();
        if (map == null)
        {
            Debug.LogError("This must be a map system! Can't be anything else.");
            return;
        }

        GenerateGoalSpots(map);
        bursts = numBursts.Evaluate();
        StartTimer();
    }

    public void GenerateGoalSpots(Map map)
    {
        if (goalSpots == null)
        {
            goalSpots = new List<RogueTile>();
        }

        goalSpots.Clear();

        for (int i = 1; i < map.width - 1; i++)
        {
            for (int j = 1; j < map.height - 1; j++)
            {
                RogueTile current = map.GetTile(i, j);
                if (!current.BlocksMovement())
                {
                    bool nextToOpen = false;
                    RogueTile up = map.GetTile(i, j + 1);
                    RogueTile down = map.GetTile(i, j - 1);
                    RogueTile right = map.GetTile(i + 1, j);
                    RogueTile left = map.GetTile(i - 1, j);

                    bool rightOpen = (right.BlocksMovement() && !right.blocksVision && right.color.a == 0);
                    bool leftOpen = (left.BlocksMovement() && !left.blocksVision && left.color.a == 0);

                    nextToOpen |= (up.BlocksMovement() && !up.blocksVision && up.color.a == 0);
                    nextToOpen |= (down.BlocksMovement() && !down.blocksVision && down.color.a == 0);
                    nextToOpen |= rightOpen;
                    nextToOpen |= leftOpen;

                    if (nextToOpen)
                    {
                        goalSpots.Add(current);

                        if (rightOpen)
                        {
                            rightGoals.Add(current);
                        }

                        if (leftOpen)
                        {
                            leftGoals.Add(current);
                        }
                    }
                }
            }
        }
    }

    public void StartTimer()
    {
        currentDuration = timeBetweenBursts.Evaluate();
    }

    public void SpawnWave()
    {
        if (bursts > 0)
        {
            bursts--;
            if (goalSpots.Count == 0)
            {
                bursts = 0;
                return;
            }

            bool right = (RogueRNG.Linear(0, leftGoals.Count + rightGoals.Count) >= leftGoals.Count);

            if (right)
            {
                int index = RogueRNG.Linear(0, rightGoals.Count);
                goal = rightGoals[index];
                rightGoals.RemoveAt(index);
            } 
            else
            {
                int index = RogueRNG.Linear(0, leftGoals.Count);
                goal = leftGoals[index];
                leftGoals.RemoveAt(index);
            }

            goal.color = Color.yellow;

            List<RogueTile> openSpots = new List<RogueTile>();

            if (!right)
            {
                for (int x = 0; x < 4; x++)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        RogueTile current = map.GetTile(x, y);
                        if (!current.blocksVision && current.BlocksMovement())
                        {
                            openSpots.Add(current);
                        }
                    }
                }
            }
            else
            {
                for (int x = map.width - 4; x < map.width; x++)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        RogueTile current = map.GetTile(x, y);
                        if (!current.blocksVision && current.BlocksMovement())
                        {
                            openSpots.Add(current);
                        }
                    }
                }
            }

            int numHorses = numHorsesPerBurst.Evaluate();
            numHorses = Mathf.Min(numHorses, openSpots.Count);

            for (int i = 0; i < numHorses; i++)
            {
                int horseIndex = RogueRNG.Linear(0, openSpots.Count);
                RogueTile tile = openSpots[horseIndex];
                openSpots.RemoveAt(horseIndex);

                ReplaceTile(tilePrefab, tile);

                //currentHorses.Add(map.GetTile(tile.location));

                MonsterSpawner.singleton.SpawnMonsterAt(map, tile.location);
            }
        }
    }

    public void ReplaceTile(RogueTile toSpawn, RogueTile current)
    {
        RogueTile replacement = GameObject.Instantiate(toSpawn).GetComponent<RogueTile>();
        replacement.transform.parent = current.transform.parent;
        replacement.transform.position = current.transform.position;
        map.tiles[current.location.x, current.location.y] = replacement;
        replacement.SetMap(map, current.location);
        replacement.Setup();
        currentHorses.Add(replacement);
    }

    public void UpdateHorses()
    {
        bool movedAny = false;
        foreach (RogueTile horse in currentHorses)
        {
            if (Pathfinding.FindPath(horse.location, goal.location).Count() == 0)
            {
                if (!movedAny)
                {
                    AnimationController.PushNewAnimGroup();
                    movedAny = true;
                }

                if (horse.location.y != goal.location.y)
                {
                    MoveHorse(horse, Vector2Int.up);
                }
                else
                {
                    if (horse.location.x < goal.location.x)
                    {
                        MoveHorse(horse, Vector2Int.right);
                    }
                    else
                    {
                        MoveHorse(horse, Vector2Int.left);
                    }
                }
            }
        }

        if (!movedAny)
        {
            StartTimer();
        }

        /*}
        else
        {
            for (int i = currentHorses.Count - 1; i >= 0; i--)
            {
                RogueTile horse = currentHorses[i];
                if (horse.location.x < goal.location.x)
                {
                    Vector2Int newSpot = horse.location + Vector2Int.left;
                    if (newSpot.x < 0 || newSpot.x >= map.width)
                    {
                        ReplaceTile(airPrefab, horse);
                        currentHorses.RemoveAt(i);
                    }
                    else
                    {
                        MoveHorse(horse, Vector2Int.left);
                    }
                }
                else
                {
                    Vector2Int newSpot = horse.location + Vector2Int.right;
                    if (newSpot.x < 0 || newSpot.x >= map.width)
                    {
                        ReplaceTile(airPrefab, horse);
                        currentHorses.RemoveAt(i);
                    }
                    else
                    {
                        MoveHorse(horse, Vector2Int.right);
                    }
                }
            }
        }*/
    }

    public override void OnGlobalTurnStart(int turn)
    {

    }

    public override void OnGlobalTurnEnd(int turn)
    {
        if (currentDuration == 0)
        {
            Debug.Log($"We have {currentHorses.Count} horses being driven.");
            UpdateHorses();
        }
        else
        {
            currentDuration--;
            if (currentDuration == 0)
            {
                SpawnWave();
            }
        }
    }

    public override void OnEnterLevel(Map m)
    {

    }

    public override void OnExitLevel(Map m)
    {

    }

    public void MoveHorse(RogueTile horse, Vector2Int offset)
    {
        RogueTile target = map.GetTile(horse.location + offset);
        if (!target.BlocksMovement()) return;
        Vector2Int hold = target.location;
        map.tiles[hold.x, hold.y] = horse;
        map.tiles[horse.location.x, horse.location.y] = target;
        target.location = horse.location;
        horse.location = hold;

        Vector3 posHold = target.transform.position;
        target.transform.position = horse.transform.position;

        if (horse.currentlyStanding)
        {
            horse.currentlyStanding.location = horse.location;
        }

        if (target.isVisible || !horse.isHidden)
        {
            AnimationController.AddAnimationForObject(new HorseAnimation(horse, target.location, horse.location), horse);
        }
        else
        {
            horse.transform.position = posHold;
            if (horse.currentlyStanding)
            {
                //horse.currentlyStanding.location = horse.location;
                horse.currentlyStanding.SetPositionSnap(horse.location);
            }
        }
    }
}
