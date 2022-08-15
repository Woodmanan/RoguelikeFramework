using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractAction : GameAction
{
    InteractableTile tile;
    //Constuctor for the action; must include caller!
    public InteractAction(CustomTile tile)
    {
        this.tile = tile as InteractableTile;
        if (this.tile == null)
        {
            Debug.Log($"Tile at {tile.location} is not interactable, and shouldn't be used for interact action!");
        }
    }

    public InteractAction(InteractableTile tile)
    {
        this.tile = tile;
    }

    //The main function! This EXACT coroutine will be executed, even across frames.
    //See GameAction.cs for more information on how this function should work!
    public override IEnumerator TakeAction()
    {
        if (tile == null)
        {
            Debug.Log("Monster tried to interact wtih a null tile, aborting!");
            yield return GameAction.Abort;
        }
        IEnumerator interactAction = tile.Interact(caller);
        while (interactAction.MoveNext())
        {
            yield return interactAction.Current;
        }
    }

    //Called after construction, but before execution!
    //This is THE FIRST spot where caller is not null! Heres a great spot to actually set things up.
    public override void OnSetup()
    {

    }
}
