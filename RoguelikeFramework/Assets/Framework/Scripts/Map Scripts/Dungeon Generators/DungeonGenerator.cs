using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.Scripting;

[Serializable]
public class DungeonGenerator
{
    public string name;
    public int depth;
    [HideInInspector] public int seed;
    public Vector2Int bounds;
    public Branch branch;
    [HideInInspector] public List<Machine> machines;
    [HideInInspector] public List<Room> rooms;
    public TileList tilesAvailable;

    //Generation parameters
    public int numberOfAttempts = 100;
    public int attemptsPerMachine = 100;

    public LootPool availableItems;
    public RandomNumber numItems;

    public RandomNumber numMonsters;

    public int[,] map;

    public IEnumerator generation = null;
    public bool finished = false;
    

    public IEnumerator GenerateMap(int index, int seed, World world, Transform parent)
    {
        this.seed = seed;
        UnityEngine.Random.State state;
        UnityEngine.Random.State oldState = UnityEngine.Random.state;
        UnityEngine.Random.InitState(seed);

        rooms = new List<Room>();

        GameObject mapInstance = new GameObject();
        Map gameMap = mapInstance.AddComponent<Map>();

        gameMap.depth = depth;
        gameMap.index = index;
        gameMap.branch = branch;

        mapInstance.name = name;
        mapInstance.transform.parent = parent;
        mapInstance.SetActive(false);

        //Cull null instances
        machines = machines.FindAll(x => x != null);

        //Instance them all
        machines = machines.Select(x => x.Instantiate()).ToList();

        //Shuffle remaining instances, then sort (Equal priority machines are shuffled relative, still)
        var randomized = machines.OrderBy(item => UnityEngine.Random.Range(int.MinValue, int.MaxValue));
        machines = randomized.OrderBy(item => item.priority).ToList();

        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        for (int i = 0; i < machines.Count - 1; i++)
        {
            if (machines[i].priority == machines[i + 1].priority)
            {
                Debug.LogError($"Generator {name} has two machines with the same priority ({machines[i].priority})! This can cause unexpected generation errors");
            }
        }
        #endif
        //Lower than here!

        foreach (Machine m in machines)
        {
            m.Connect(this);
        }
        

        if (FindPacking())
        {

            state = UnityEngine.Random.state;
            UnityEngine.Random.state = oldState;
            yield return null;
            oldState = UnityEngine.Random.state;
            UnityEngine.Random.state = state;
            map = new int[bounds.x, bounds.y];

            foreach (Machine m in machines)
            {
                IEnumerator machine = m.Activate();
                while (machine.MoveNext())
                {
                    state = UnityEngine.Random.state;
                    UnityEngine.Random.state = oldState;
                    yield return machine.Current;
                    oldState = UnityEngine.Random.state;
                    UnityEngine.Random.state = state;
                }
            }

            //Add stairs as final step
            StairPlacer stairs = new StairPlacer();
            stairs.Activate(world, this, index);

            gameMap.name = name;

            IEnumerator build = gameMap.BuildFromTemplate(map, tilesAvailable);

            while (build.MoveNext())
            {
                state = UnityEngine.Random.state;
                UnityEngine.Random.state = oldState;
                yield return build.Current;
                oldState = UnityEngine.Random.state;
                UnityEngine.Random.state = state;
            }

            foreach (Machine m in machines)
            {
                m.PostActivation(gameMap);
            }

            //Post activation for stairs - attach the actual stair objects
            stairs.SetupStairTiles(gameMap);

            foreach (Room r in rooms)
            {
                IEnumerator activate = r.PostActivation(gameMap);
                while (activate.MoveNext())
                {
                    state = UnityEngine.Random.state;
                    UnityEngine.Random.state = oldState;      
                    yield return activate.Current;
                    oldState = UnityEngine.Random.state;
                    UnityEngine.Random.state = state;
                }
            }


            LevelLoader.maps[index] = gameMap;

            gameMap.Setup();

            //TODO: Load monsters in
            IEnumerator monsterSpawn = MonsterSpawner.singleton.SpawnForFloor(index, gameMap, numMonsters.Evaluate());
            while (monsterSpawn.MoveNext())
            {
                state = UnityEngine.Random.state;
                UnityEngine.Random.state = oldState;
                yield return monsterSpawn.Current;
                oldState = UnityEngine.Random.state;
                UnityEngine.Random.state = state;
            }
            

            
            //Start loading items in
            IEnumerator itemSpawn = ItemSpawner.singleton.SpawnForFloor(index, gameMap, numItems.Evaluate());

            while (itemSpawn.MoveNext())
            {
                state = UnityEngine.Random.state;
                UnityEngine.Random.state = oldState;
                yield return itemSpawn.Current;
                oldState = UnityEngine.Random.state;
                UnityEngine.Random.state = state;
            }

            //Allow tiles that need after-generation modifications to do so
            gameMap.SetAllTiles();

            { //Skip
                state = UnityEngine.Random.state;
                UnityEngine.Random.state = oldState;
                yield return itemSpawn.Current;
                oldState = UnityEngine.Random.state;
                UnityEngine.Random.state = state;
            }

            //Rebuild any extra info that's changed during post-generation
            //This mostly catches edge cases from tiles spawned by rexpaint prefabs.
            gameMap.RebuildAllMapData();

            { //Skip
                state = UnityEngine.Random.state;
                UnityEngine.Random.state = oldState;
                yield return itemSpawn.Current;
                oldState = UnityEngine.Random.state;
                UnityEngine.Random.state = state;
            }

            //Refresh so that monsters and items don't show.
            gameMap.RefreshGraphics();

            { //Skip
                state = UnityEngine.Random.state;
                UnityEngine.Random.state = oldState;
                yield return itemSpawn.Current;
                oldState = UnityEngine.Random.state;
                UnityEngine.Random.state = state;
            }

            //Monsters should almost by definition be setup now. Do it again, just in case, and then have themselves attach to the floor!
            foreach (Monster m in gameMap.monsters)
            {
                m.Setup();
                m.PostSetup(gameMap);
            }

            finished = true;
            UnityEngine.Random.state = oldState;
        }

    }

    //TODO: Come back and use a texture packing algo to place these, if we fail.
    private bool FindPacking()
    {

        //Split machines into machines we need to pack, and the ones we don't.
        List<Machine> toPack = machines.FindAll(x=>!x.canShareSpace); //ToPack's not dead
        List<Machine> anywhere = machines.FindAll(x=>x.canShareSpace);

        //Rank them according to size. Larger machines should place themselves first
        toPack.Sort(CompareMachinesBySize);

        bool successful = true;
        //Stupid Solution first?
        for (int i = 0; i < numberOfAttempts; i++)
        {
            successful = true;
            for (int c = 0; c < toPack.Count; c++)
            {
                Machine m = toPack[c];

                bool placedSuccessfully = true;;
                for (int j = 0; j < attemptsPerMachine; j++)
                {
                    placedSuccessfully = true;
                    Vector2Int startBounds = bounds - m.size;
                    Vector2Int newStart = new Vector2Int(UnityEngine.Random.Range(0, startBounds.x), UnityEngine.Random.Range(0, startBounds.y));
                    m.SetPosition(newStart, bounds);

                    //Check for good placement
                    for (int check = 0; check < c; check++)
                    {
                        placedSuccessfully = placedSuccessfully && !m.Overlaps(toPack[check]);
                        //print($"{c}: ({toPack[check].start},{toPack[check].end}) vs ({m.start}, {m.end}) : {Overlaps(toPack[check], m)}");
                    }
                    if (placedSuccessfully)
                    {
                        break;
                    }
                }

                if (!placedSuccessfully)
                {
                    Debug.LogError($"Couldn't find a spot for machine {c}");
                    successful = false;
                    break;
                }
            }

            if (successful)
            {
                break;
            }
        }

        //TODO:Fallback to space packing function should go here!
        if (!successful)
        {
            Debug.LogError("Failure of the generator for find a placing.");
            return false;
        }

        //Fill in our non-caring machines.
        for (int c = 0; c < anywhere.Count; c++)
        {
            Machine m = anywhere[c];
            Vector2Int startBounds = bounds - m.size;
            Vector2Int newStart = new Vector2Int(UnityEngine.Random.Range(0, startBounds.x), UnityEngine.Random.Range(0, startBounds.y));
            m.SetPosition(newStart, bounds);
        }

        return true;
    }

    private int CompareMachinesBySize(Machine x, Machine y)
    {
        float sizeOne = x.global ? bounds.magnitude : x.size.magnitude;
        float sizeTwo = y.global ? bounds.magnitude : y.size.magnitude;

        return sizeTwo.CompareTo(sizeOne);
    }

    public void ActivateMachines()
    {
        foreach (Machine m in machines)
        {
            m.Activate();
        }
    }
}
