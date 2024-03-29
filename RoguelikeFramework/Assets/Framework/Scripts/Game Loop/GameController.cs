﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using System.Linq;



public class GameController : MonoBehaviour
{
    private static GameController Singleton;
    public static GameController singleton
    {
        get
        {
            if (!Singleton)
            {
                GameController g = GameObject.FindObjectOfType<GameController>();
                if (g)
                {
                    Singleton = g;
                }
                else
                {
                    UnityEngine.Debug.LogError("No GameController found!");
                }
            }

            return Singleton;
        }
        set { Singleton = value; }
    }

    [Header("Runtime variables")]
    //Constant variables: change depending on runtime!
    public float turnMSPerFrame = 5;
    public bool waitForAnimations;
    
    public int turn;

    public float energyPerTurn = 100f;

    public Monster player;

    World world;

    [HideInInspector] public int nextLevel = -1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartGame()
    {
        StartCoroutine(BeginGame());
    }

    IEnumerator BeginGame()
    {
        turn = 0;
        int start = LevelLoader.singleton.GetIndexOf(LevelLoader.singleton.startAt);
        if (start < 0) start = 0;
        //Wait for initial level loading to finish
        if (LevelLoader.singleton.JITLoading)
        {
            LoadMap(start);
        }
        else
        {
            if (LevelLoader.singleton.preloadUpTo != start)
            {
                if (start > LevelLoader.singleton.preloadUpTo)
                {
                    UnityEngine.Debug.LogWarning("Waiting to preload for a level that is before where we start? Skipping Preload.");
                }
                else
                {
                    if (start < LevelLoader.singleton.preloadUpTo)
                    {
                        UnityEngine.Debug.Log($"Waiting for level preload load");
                        yield return new WaitUntil(() => LevelLoader.singleton.IsMapLoaded(LevelLoader.singleton.preloadUpTo));
                    }
                }
            }
            LoadMap(start);
        }

        world = LevelLoader.singleton.world;

        //Clean up some potentially old values
        {
            AutoPickupAction.SeenItems.Clear();
            MonsterTable.UsedUniqueIDs?.Clear();
        }

        //Set starting position
        Player.player.value.unity.transform.parent = Map.current.monsterContainer;
        Player.player.value.location = Map.current.entrances[0].toLocation;
        player = Player.player.value;

        //1 Frame pause to set up LOS
        yield return null;
        player.UpdateLOS();
        LOS.WritePlayerGraphics();
        //LOS.GeneratePlayerLOS(Map.current, player.location, player.visionRadius);

        //Monster setup, before the loop starts
        player.PostSetup(Map.current);

        //Move our camera onto the player for the first frame
        CameraTracking.singleton.JumpToPlayer();

        //Notify systems that level 1 has loaded
        SystemEnterLevel();

        StartCoroutine(GameLoop());
    }

    public void LoadMap(int index)
    {
        if (Map.current)
        {
            Map.current.activeGraphics = false;
            Map.current.gameObject.SetActive(false);
        }
        Map.current = LevelLoader.LoadMap(index);
        Map.current.activeGraphics = true;
        Map.current.gameObject.SetActive(true);
    }

    IEnumerator GameLoop()
    {
        Stopwatch watch = new Stopwatch();
        Stopwatch monsterWatch = new Stopwatch();
        //Main loop
        while (true)
        {
            CallTurnStartGlobal();

            //Player turn sequence
            player.AddEnergy(energyPerTurn);
            while (player.energy > 0)
            {
                //Set up local turn
                player.StartTurn();
                
                //Run the actual turn itself
                IEnumerator turn = player.LocalTurn();
                while (player.energy > 0 && turn.MoveNext())
                {
                    if (!GameAction.HasSpecialInstruction(turn))
                    {
                        yield return turn.Current;
                    }
                }

                //Turn is ended!
                player.EndTurn();
            }

            if (waitForAnimations)
            {
                watch.Restart();
                monsterWatch.Start();
            }
            int splits = 1;
            for (int i = 0; i < Map.current.monsters.Count; i++)
            {
                //Local scope counter for stalling - prevent a monster from taking > 1000 sub-turns
                int stallCount = 0;


                //Taken too much time? Quit then! (Done before monster update to handle edge case on last call)
                if (watch.ElapsedMilliseconds > turnMSPerFrame)
                {
                    watch.Stop();
                    splits++;
                    yield return null;
                    watch.Restart();
                }

                Monster m = Map.current.monsters[i].value;

                m.AddEnergy(energyPerTurn);
                while (m.energy > 0 && !m.IsDead())
                {
                    //Set up local turn
                    m.StartTurn();

                    //Run the actual turn itself
                    IEnumerator turn = m.LocalTurn();
                    while (m.energy > 0 && turn.MoveNext())
                    {
                        if (!GameAction.HasSpecialInstruction(turn))
                        {
                            watch.Stop();
                            splits++;
                            yield return turn.Current;
                            watch.Restart();
                        }
                        else
                        {
                            //Edge case - take a break during a check if we need to!
                            //Should make things a lot more fluid with complicated monster turns
                            if (watch.ElapsedMilliseconds > turnMSPerFrame)
                            {
                                watch.Stop();
                                splits++;
                                yield return null;
                                watch.Restart();
                            }
                        }

                        //Stall check!
                        stallCount++;
                        if (stallCount > 1000)
                        {
                            UnityEngine.Debug.LogError($"Main loop stalled during the turn of {m.friendlyName}, recovering...", m.unity);
                            m.energy = 0;
                            break;
                        }

                    }

                    //Turn is ended!
                    m.EndTurn();
                }
            }
            
            CallTurnEndGlobal();

            turn++;

            //Wait for current frame to finish up
            if (waitForAnimations)
            {
                while (AnimationController.Count > 0)
                {
                    yield return null;
                }
            }
            else
            {
                yield return null;
            }

            //Clean up anybody who's dead
            for (int i = Map.current.monsters.Count - 1; i >= 0; i--)
            {
                Monster monster = Map.current.monsters[i].value;
                if (monster.IsDead())
                {
                    //Map.current.monsters.RemoveAt(i);
                    monster.unity.gameObject.SetActive(false);
                    //Destroy(monster.gameObject);
                }
            }

            if (nextLevel != -1)
            {
                yield return new WaitUntil(() => !AnimationController.hasAnimations);
                MoveLevel();
            }
        }
    }

    public void CallTurnStartGlobal()
    {
        player.OnTurnStartGlobalCall();
        for (int i = 0; i < Map.current.monsters.Count; i++)
        {
            Monster m = Map.current.monsters[i].value;
            if (!m.IsDead())
            {
                m.OnTurnStartGlobalCall();
            }
        }

        foreach (DungeonSystem system in world.systems)
        {
            system.OnGlobalTurnStart(turn);
        }
        foreach (DungeonSystem system in Map.current.branch.branchSystems)
        {
            system.OnGlobalTurnStart(turn);
        }
        foreach (DungeonSystem system in Map.current.mapSystems)
        {
            system.OnGlobalTurnStart(turn);
        }
    }

    public void MoveToLevel(int newLevel)
    {
        UnityEngine.Debug.Log("Moving to level " + newLevel);
        nextLevel = newLevel;
    }

    void RemoveMonstersFromLevel(List<RogueHandle<Monster>> monsters)
    {
        foreach (RogueHandle<Monster> m in monsters)
        {
            Map.current.monsters.Remove(m);
        }
    }

    List<RogueHandle<Monster>> CollectMonstersForTransition(RogueHandle<Monster> m, int friendlyDistance, int enemyDistance)
    {
        List<RogueHandle<Monster>> monsters = new List<RogueHandle<Monster>>();
        monsters.AddRange(m.value.view.visibleFriends.Where(x => x.value.location.GameDistance(m.value.location) <= friendlyDistance));
        monsters.AddRange(m.value.view.visibleEnemies.Where(x => x.value.location.GameDistance(m.value.location) <= enemyDistance));
        return monsters.Where(x => !x.value.tags.HasTag("Monster.CannotLeaveLevel")).ToList();
    }

    //TODO: Determine how monsters get placed if they don't have space to be placed
    public void MoveMonsters(Monster m, Stair stair, List<RogueHandle<Monster>> monstersToMove, Map map)
    {
        m.unity.transform.parent = map.monsterContainer;
        Vector2Int center = stair.GetMatchingLocation();
        m.SetPositionSnap(stair.GetMatchingLocation());

        IEnumerator<Vector2Int> tiles = map.GetWalkableTilesByRange(stair.GetMatchingLocation(), 1, 16);

        foreach (RogueHandle<Monster> monster in monstersToMove.OrderBy(x => Random.value))
        {
            monster.value.unity.transform.parent = map.monsterContainer;
            map.monsters.Add(monster);
            if (tiles.MoveNext())
            {
                monster.value.SetPositionSnap(tiles.Current);
            }
            else
            {
                UnityEngine.Debug.LogWarning("No space around stair - Monster dumped into the ether!");
                monster.value.SetPositionSnap(map.GetRandomWalkableTile());
            }
        }
    }

    private void MoveLevel()
    {
        Stair stair = Map.current.GetTile(player.location) as Stair;
        Map old = Map.current;
        if (stair)
        {
            SystemExitLevel();
            List<RogueHandle<Monster>> toMove = CollectMonstersForTransition(Player.player, 5, 1);
            RemoveMonstersFromLevel(toMove);
            LoadMap(nextLevel);
            MoveMonsters(player, stair, toMove, Map.current);
            SystemEnterLevel();
        }
        else
        {
            UnityEngine.Debug.Log("You are unable to change levels.");
        }

        nextLevel = -1;

        LOS.ClearAllLevelLOS(old);
        CameraTracking.singleton.JumpToPlayer();
        player.UpdateLOS();
        LOS.WritePlayerGraphics(Map.current, player.location, player.visionRadius);
    }

    public void CallTurnEndGlobal()
    {
        player.OnTurnEndGlobalCall();
        for (int i = 0; i < Map.current.monsters.Count; i++)
        {
            Monster m = Map.current.monsters[i].value;
            if (!m.IsDead())
            {
                m.OnTurnEndGlobalCall();
            }
        }


        foreach (DungeonSystem system in world.systems)
        {
            system.OnGlobalTurnEnd(turn);
        }
        foreach (DungeonSystem system in Map.current.branch.branchSystems)
        {
            system.OnGlobalTurnEnd(turn);
        }
        foreach (DungeonSystem system in Map.current.mapSystems)
        {
            system.OnGlobalTurnEnd(turn);
        }
    }

    public void AddMonster(RogueHandle<Monster> m)
    {
        Map.current.monsters.Add(m);
    }

    public void SystemExitLevel()
    {
        foreach (DungeonSystem system in world.systems)
        {
            system.OnExitLevel(Map.current);
        }
        foreach (DungeonSystem system in Map.current.branch.branchSystems)
        {
            system.OnExitLevel(Map.current);
        }
        foreach (DungeonSystem system in Map.current.mapSystems)
        {
            system.OnExitLevel(Map.current);
        }
    }

    public void SystemEnterLevel()
    {
        foreach (DungeonSystem system in world.systems)
        {
            system.OnEnterLevel(Map.current);
        }
        foreach (DungeonSystem system in Map.current.branch.branchSystems)
        {
            system.OnEnterLevel(Map.current);
        }
        foreach (DungeonSystem system in Map.current.mapSystems)
        {
            system.OnEnterLevel(Map.current);
        }
    }
}
