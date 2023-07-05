using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

enum TotemTypes
{
    Serpent,
    Jaguar,
    Eagle,
    Frog,
    Crocodile,
    Scorpion
}

[Group("World Generators")]
public class JunglePathMachine : Machine
{
    [HideInInspector]
    

    public override IEnumerator Activate()
    {
        List<TotemTypes[]> totems = new List<TotemTypes[]>();
        int[] goldenPath = Enumerable.Range(0, 7).Select(x => RogueRNG.Linear(0, 3)).ToArray();

        List<TotemTypes> value = Enum.GetValues(typeof(TotemTypes)).Cast<TotemTypes>().ToList();
        for (int i = 0; i < 7; i++)
        {
            totems.Add(value.OrderBy(x => UnityEngine.Random.value).Take(3).ToArray());
        }

        for (int i = 0; i < 7; i++)
        {
            World.current.BlackboardWrite<TotemTypes[]>($"Jungle:{i} Totems", totems[i]);
            Debug.Log($"Floor {i}: {totems[i][0]}, {totems[i][1]}, and {totems[i][2]}. Need to take {totems[i][goldenPath[i]]}.");
        }

        World.current.BlackboardWrite<int[]>($"COG Path", goldenPath);


        yield break;
    }
}
