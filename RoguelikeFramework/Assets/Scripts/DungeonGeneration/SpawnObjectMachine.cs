using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnObjectMachine : Machine
{
    public GameObject toSpawn;

    public override void PostActivation(Map m)
    {
        toSpawn = GameObject.Instantiate(toSpawn);
        toSpawn.transform.parent = m.transform;
    }
}
