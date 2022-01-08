using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;

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

    public List<DungeonGenerator> generators;
    int current;
    public float msPerFrame;
    public bool randomSeed;
    public int seed;
    private bool setup = false;

    public static List<Map> maps;

    [Header("Debug tools")]
    [Tooltip("Enables just-in-time loading. Levels are generated on-demand, instead of in the background.")]
    public bool JITLoading;
    [Tooltip("Offsets the generated levels, allowing you to skip to a desired level at the start of the game")]
    public int StartAt;

    // Start is called before the first frame update
    void Start()
    {
        if (!setup) Setup();
        
        if (!JITLoading)
        {
            StartCoroutine(GenerateAllLevels());
        }
    }

    public void Setup()
    {
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

        UnityEngine.Random.InitState(seed);

        maps = new List<Map>(new Map[generators.Count]);

        for (int i = 0; i < generators.Count; i++)
        {
            generators[i].generation = generators[i].GenerateMap(i, UnityEngine.Random.Range(int.MinValue, int.MaxValue), transform);
        }

        #if !UNITY_EDITOR
        if (JITLoading)
        {
            Debug.LogError("JIT Loading was left on! Don't do that!");
            JITLoading = false;
        }
        #endif

        setup = true;
    }

    IEnumerator GenerateAllLevels()
    {
        Stopwatch watch = new Stopwatch();
        Stopwatch total = new Stopwatch();
        watch.Start();
        total.Start();
        current = StartAt < 0 ? -1 : StartAt - 1;
        while (true)
        {
            current++;
            if (current == generators.Count || generators[current].finished || generators[current].JIT)
            {
                current = FirstUnfinishedLevel();
            }
            if (current == -1)
            {
                break;
            }

            while (generators[current].generation.MoveNext())
            {
                if (watch.ElapsedMilliseconds > (msPerFrame / 2))
                {
                    watch.Stop();
                    yield return null;
                    watch.Restart();
                }
            }

            //Take a pause here, to give some other coroutines a buffer to jump in
            yield return null;
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
            if (generators[i] != null && !generators[i].finished && !generators[i].JIT)
            {
                return i;
            }
        }

        return -1;
    }

    public int GetDepthOf(string levelName)
    {
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
}
