using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;



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
    
    public int turn;

    public int energyPerTurn;

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

        //Set starting position
        Player.player.transform.parent = Map.current.monsterContainer;
        Player.player.location = Map.current.entrances[0].toLocation;
        player = Player.player;
        player.Setup();

        //1 Frame pause to set up LOS
        yield return null;
        player.UpdateLOS();
        //LOS.GeneratePlayerLOS(Map.current, player.location, player.visionRadius);

        //Monster setup, before the loop starts
        player.PostSetup(Map.current);

        //Move our camera onto the player for the first frame
        CameraTracking.singleton.JumpToPlayer();


        //Space for any init setup that needs to be done
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


            watch.Restart();
            for (int i = 0; i < Map.current.monsters.Count; i++)
            {
                //Local scope counter for stalling - prevent a monster from taking > 1000 sub-turns
                int stallCount = 0;
                

                //Taken too much time? Quit then! (Done before monster update to handle edge case on last call)
                if (watch.ElapsedMilliseconds > turnMSPerFrame)
                {
                    watch.Stop();
                    yield return null;
                    watch.Restart();
                }

                Monster m = Map.current.monsters[i];

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
                                yield return null;
                                watch.Restart();
                            }
                        }

                        //Stall check!
                        stallCount++;
                        if (stallCount > 10-0)
                        {
                            UnityEngine.Debug.LogError($"Main loop stalled during the turn of {m.friendlyName}, recovering...", m);
                            m.energy = 0;
                            break;
                        }
                        
                    }

                    //Turn is ended!
                    m.EndTurn();
                }
            }
            watch.Stop();
            
            CallTurnEndGlobal();

            turn++;

            //Wait for current frame to finish up
            while (AnimationController.Count > 0)
            {
                yield return null;
            }

            //Clean up anybody who's dead
            for (int i = Map.current.monsters.Count - 1; i >= 0; i--)
            {
                Monster monster = Map.current.monsters[i];
                if (monster.IsDead())
                {
                    Map.current.monsters.RemoveAt(i);
                    Destroy(monster.gameObject);
                }
            }

            if (nextLevel != -1)
            {
                MoveLevel();
            }
        }
    }

    public void CallTurnStartGlobal()
    {
        player.OnTurnStartGlobalCall();
        foreach (Monster m in Map.current.monsters)
        {
            m.OnTurnStartGlobalCall();
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

    //TODO: Determine how monsters get placed if they don't have space to be placed
    public void MoveMonsters(Monster m, Stair stair, Map map)
    {
        m.transform.parent = map.monsterContainer;
        m.SetPositionSnap(stair.GetMatchingLocation());
    }

    private void MoveLevel()
    {
        Stair stair = Map.current.GetTile(player.location) as Stair;
        Map old = Map.current;
        if (stair)
        {
            SystemExitLevel();
            LoadMap(nextLevel);
            MoveMonsters(player, stair, Map.current);
            SystemEnterLevel();
        }
        else
        {
            UnityEngine.Debug.Log("You are unable to change levels.");
        }

        nextLevel = -1;

        LOS.lastCall.Deprint(old);
        LOS.lastCall = null;
        CameraTracking.singleton.JumpToPlayer();
        LOS.GeneratePlayerLOS(Map.current, player.location, player.visionRadius);

    }

    public void CallTurnEndGlobal()
    {
        player.OnTurnEndGlobalCall();
        for (int i = 0; i < Map.current.monsters.Count; i++)
        {
            Monster m = Map.current.monsters[i];
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

    public void AddMonster(Monster m)
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
