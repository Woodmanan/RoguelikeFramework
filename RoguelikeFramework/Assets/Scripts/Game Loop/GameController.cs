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

    [Header("Startup variables")]
    [Tooltip("When set, preloads up to this level before letting the player enter the game.")]
    [SerializeField] int preLoadUpTo;

    [Header("Runtime variables")]
    //Constant variables: change depending on runtime!
    public const long MONSTER_UPDATE_MS = 5;
    
    public int turn;

    public int energyPerTurn;

    public Player player;

    [HideInInspector] public int nextLevel = -1;
    
    // Start is called before the first frame update
    void Start()
    {
        turn = 0;
        StartCoroutine(BeginGame());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator BeginGame()
    {
        int start = LevelLoader.singleton.StartAt;
        if (start < 0) start = 0;
        //Wait for initial level loading to finish
        if (LevelLoader.singleton.JITLoading)
        {
            LoadMap(start);
        }
        else
        {
            if (preLoadUpTo != start)
            {
                if (LevelLoader.singleton.generators[preLoadUpTo].JIT)
                {
                    UnityEngine.Debug.LogError("Waiting to preload for a level that is JIT loaded! Skipping that, since it will never happen.");
                }
                else if (start > preLoadUpTo)
                {
                    UnityEngine.Debug.LogWarning("Waiting to preload for a level that is before where we start? Skipping Preload.");
                }
                else
                {
                    if (start < preLoadUpTo)
                    {
                        UnityEngine.Debug.Log($"Waiting for the levels up to {preLoadUpTo} to load");
                        yield return new WaitUntil(() => LevelLoader.singleton.IsMapLoaded(preLoadUpTo));
                    }
                }
            }
            LoadMap(start);
        }

        //Set starting position
        Player.player.transform.parent = Map.current.monsterContainer;
        Player.player.location = Map.current.entrances[0];

        //1 Frame pause to set up LOS
        yield return null;
        player.UpdateLOS();
        //LOS.GeneratePlayerLOS(Map.current, player.location, player.visionRadius);

        //Monster setup, before the loop starts
        player.PostSetup();

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
                    if (turn.Current != GameAction.StateCheck)
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
                //Taken too much time? Quit then! (Done before monster update to handle edge case on last call)
                if (watch.ElapsedMilliseconds > MONSTER_UPDATE_MS)
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
                        if (turn.Current != GameAction.StateCheck)
                        {
                            watch.Stop();
                            yield return turn.Current;
                            watch.Restart();
                        }
                        else
                        {
                            //Edge case - take a break during a check if we need to!
                            //Should make things a lot more fluid with complicated monster turns
                            if (watch.ElapsedMilliseconds > MONSTER_UPDATE_MS)
                            {
                                watch.Stop();
                                yield return null;
                                watch.Restart();
                            }
                        }
                    }

                    //Turn is ended!
                    m.EndTurn();
                }
            }
            watch.Stop();

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
            
            CallTurnEndGlobal();

            turn++;

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
    }

    public void MoveToLevel(int newLevel)
    {
        nextLevel = newLevel;
    }

    //TODO: Determine how monsters get placed if they don't have space to be placed
    public void MoveMonsters(Monster m, Stair stair, Map map)
    {
        if (stair.upStair)
        {
            Vector2Int offset = m.location - stair.location;

            m.SetPosition(map.exits[stair.stairPair] + offset);
        }
        else
        {
            Vector2Int offset = m.location - stair.location;

            m.SetPosition(map.entrances[stair.stairPair] + offset);
        }
        m.transform.parent = map.monsterContainer;
    }

    private void MoveLevel()
    {
        Stair stair = Map.current.GetTile(player.location) as Stair;
        Map old = Map.current;
        if (stair)
        {
            LoadMap(nextLevel);
            MoveMonsters(player, stair, Map.current);
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
        foreach (Monster m in Map.current.monsters)
        {
            m.OnTurnEndGlobalCall();
        }
    }

    public void AddMonster(Monster m)
    {
        Map.current.monsters.Add(m);
    }
}
