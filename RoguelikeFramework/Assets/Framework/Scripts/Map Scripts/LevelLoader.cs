using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using UnityEngine.Scripting;

public class LevelLoader : MonoBehaviour
{
    private static LevelLoader Singleton;
    public static LevelLoader singleton
    {
        get
        {
            if (!Singleton)
            {
                LevelLoader l = GameObject.FindObjectOfType<LevelLoader>();
                if (l)
                {
                    Singleton = l;
                }
                else
                {
                    UnityEngine.Debug.LogError("No LevelLoader found!");
                }
            }

            return Singleton;
        }
        set { Singleton = value; }
    }

    public WorldGenerator worldGen;
    [SerializeReference]
    public List<DungeonGenerator> generators;
    [HideInInspector] public int current;
    public float msPerFrame;
    public bool randomSeed;
    public int seed;
    private bool setup = false;

    public static List<Map> maps;

    [Tooltip("When set, preloads up to this level before letting the player enter the game.")]
    public int preloadUpTo;

    [Header("Debug tools")]
    [Tooltip("Enables just-in-time loading. Levels are generated on-demand, instead of in the background.")]
    public bool JITLoading;
    [Tooltip("Disables all loading options - the game wont start until the full map is deemed ready")]
    public bool LoadAllLevelsAtStart;
    [Tooltip("Offsets the generated levels, allowing you to skip to a desired level at the start of the game")]
    public string startAt;

    [HideInInspector] public World world;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void BeginGeneration()
    {
        if (!setup) Setup();

        if (!JITLoading)
        {
            StartCoroutine(GenerateAllLevels());
        }
    }

    public void Setup()
    {
        if (setup) return;
        setup = true;

        #if !UNITY_EDITOR
        startAt = "";
        #else
        JITLoading = true;
        preloadUpTo = 0;
        #endif

        if (singleton != this)
        {
            if (singleton)
            {
                Destroy(this);
                return;
            }
            else
            {
                singleton = this;
            }
        }
        
        if (randomSeed)
        {
            seed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        }

        UnityEngine.Debug.Log($"Creating game with seed {seed}");

        UnityEngine.Random.InitState(seed);

        
        if (generators.Count > 0)
        {
            UnityEngine.Debug.Log("Having generators preset does not work!");
        }
        generators.Clear();

        worldGen = Instantiate(worldGen);
        world = worldGen.Generate();
        world.PrepareLevelsForLoad(this);

        maps = new List<Map>(new Map[generators.Count]);

        for (int i = 0; i < generators.Count; i++)
        {
            generators[i].generation = generators[i].GenerateMap(i, UnityEngine.Random.Range(int.MinValue, int.MaxValue), world, transform);
        }

        //Pull in generator info to the spawning scripts
        MonsterSpawner.singleton.SetMonsterPools(world);
        ItemSpawner.singleton.SetItemPools(world);

        #if !UNITY_EDITOR
        if (JITLoading)
        {
            UnityEngine.Debug.LogError("JIT Loading was left on! Don't do that!");
            JITLoading = false;
        }
        #endif
    }

    IEnumerator GenerateAllLevels()
    {
        Stopwatch watch = new Stopwatch();
        Stopwatch total = new Stopwatch();
        watch.Start();
        total.Start();

        int startIndex = GetIndexOf(startAt);

        current = startIndex < 0 ? 0 : startIndex;
        while (true)
        {
            while (generators[current].finished)
            {
                current++;
                if (current == generators.Count)
                {
                    break;
                }
            }

            if (current == generators.Count)
            {
                current = FirstUnfinishedLevel();
            }
            if (current == -1)
            {
                break;
            }

            while (generators[current].generation.MoveNext())
            {
                if (!LoadAllLevelsAtStart && watch.ElapsedMilliseconds > msPerFrame) 
                {
                    watch.Stop();
                    yield return null;
                    watch.Restart();
                }

            }
        }

        watch.Stop();
        total.Stop();
        float timeSpent = (total.ElapsedMilliseconds) / 1000f;

        UnityEngine.Debug.Log($"Dungeon generation finished! Completed in {timeSpent} seconds (Average time of {timeSpent / generators.Count} seconds per level)");
    }

    public void FastLoadLevel(int index)
    {
        if (generators[index].finished) return;
        while (generators[index].generation.MoveNext()) { }
    }

    public int FirstUnfinishedLevel()
    {
        for (int i = 0; i < generators.Count; i++)
        {
            if (generators[i] != null && !generators[i].finished)
            {
                return i;
            }
        }

        return -1;
    }

    public int GetIndexOf(string levelName)
    {
        int level;
        if (int.TryParse(levelName, out level))
        {
            return level;
        }
        for (int i = 0; i < generators.Count; i++)
        {
            if (generators[i].name.Equals(levelName))
            {
                return i;
            }
        }
        return -1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static Map LoadMap(int index)
    {
        if (!singleton.setup) singleton.Setup();

        if (maps[index] == null)
        {
            singleton.FastLoadLevel(index);
        }

        return maps[index];
    }

    public bool IsMapLoaded(int index)
    {
        if (maps == null) return false;
        return maps[index] != null;
    }

    public int GetLevelIndex(string name)
    {
        for (int i = 0; i < generators.Count; i++)
        {
            if (generators[i].name.Equals(name))
            {
                return i;
            }
        }
        return -1;
    }

    public void ConfirmConnection(LevelConnection c)
    {
        if (c.fromBranch)
        {
            FastLoadLevel(GetLevelIndex(c.from));
        }
        if (c.toBranch)
        {
            FastLoadLevel(GetLevelIndex(c.to));
        }
    }
}
