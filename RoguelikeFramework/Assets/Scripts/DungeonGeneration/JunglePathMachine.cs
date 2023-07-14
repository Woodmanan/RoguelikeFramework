using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public enum TotemType
{
    Broken,
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
        List<TotemType[]> totems = new List<TotemType[]>();
        int[] goldenPath = new int[6];

        List<TotemType> value = Enum.GetValues(typeof(TotemType))
                                     .Cast<TotemType>()
                                     .Where(x => x != TotemType.Broken)
                                     .ToList();
        for (int i = 0; i < 7; i++)
        {
            totems.Add(value.OrderBy(x => UnityEngine.Random.value).Take(3).ToArray());
        }

        for (int i = 0; i < 6; i++)
        {
            int index = -1;
            bool valid = false;
            while (!valid)
            {
                valid = true;
                index = RogueRNG.Linear(0, 3);

                if (i > 0 && (totems[i-1][goldenPath[i-1]] == totems[i][index]))
                {
                    valid = false;
                }

                if (i > 1 && (totems[i - 2][goldenPath[i - 2]] == totems[i][index]))
                {
                    valid = false;
                }

                goldenPath[i] = index;
            }
        }

        for (int i = 0; i < 7; i++)
        {
            World.current.BlackboardWrite<TotemType[]>($"Jungle:{i} Totems", totems[i]);
        }

        World.current.BlackboardWrite<int[]>("COG Path", goldenPath);


        yield break;
    }
}
