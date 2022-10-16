using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomTile : RogueTile
{
    public List<Sprite> spritesToChoose;
    public int chosenTile = -1;

    public override void PreSetup()
    {
        chosenTile = RogueRNG.Linear(0, spritesToChoose.Count);
        GetComponent<SpriteRenderer>().sprite = spritesToChoose[chosenTile];
    }
}
