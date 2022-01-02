using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DungeonOrchestrator : MonoBehaviour
{
    public Vector2Int bounds;
    public List<Machine> machines;
    [HideInInspector] public List<Room> rooms;

    //Generation parameters
    public int numberOfAttempts;
    public int attemptsPerMachine;

    public int[,] map;

    [SerializeField] private Map gameMap;
    [SerializeField] private Player player;

    // Start is called before the first frame update
    void Start()
    {
        machines = machines.FindAll(x=>x != null);
        foreach (Machine m in machines)
        {
            m.Connect(this);
        }

        if (FindPacking())
        {
            map = new int[bounds.x, bounds.y];
            ActivateMachines();
            PassMap();

            //TODO: For testing purposes only, remove when infrastructure is ready.
            FindStartingPoint();
        }
    }

    private void FindStartingPoint()
    {
        bool success = false;
        for (int i = 0; i < 100; i++)
        {
            Vector2Int spot = new Vector2Int(Random.Range(0, bounds.x), Random.Range(0, bounds.y));
            if (map[spot.x, spot.y] == 1)
            {
                player.location = spot;
                success = true;
                break;
            }
        }
        if (!success)
        {
            for (int i = 0; i < bounds.x * bounds.y; i++)
            {
                Vector2Int spot = new Vector2Int(i % bounds.x, i / bounds.x);
                if (map[spot.x, spot.y] == 1)
                {
                    player.location = spot;
                    break;
                }
            }
        }
    }

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

                print($"Starting with machine {c}, with bounds {m.size}");
                bool placedSuccessfully = true;;
                for (int j = 0; j < attemptsPerMachine; j++)
                {
                    placedSuccessfully = true;
                    Vector2Int startBounds = bounds - m.size;
                    Vector2Int newStart = new Vector2Int(Random.Range(0, startBounds.x), Random.Range(0, startBounds.y));
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
                print("Placement found!");
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
            Vector2Int newStart = new Vector2Int(Random.Range(0, startBounds.x), Random.Range(0, startBounds.y));
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

    public void PassMap()
    {
        gameMap.BuildFromTemplate(map);
    }
}
