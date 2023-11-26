using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Group("Room Placers")]
public class PrefabFloorMachine : Machine
{
    public Room floorLayout;

    public override IEnumerator Activate()
    {
        Room working = Room.Instantiate(floorLayout);
        working.Setup();
        working.SetPosition(Vector2Int.zero);
        working.Write(generator);
        generator.rooms.Add(working);
        yield break;
    }
}

