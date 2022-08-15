using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaitAction : GameAction
{
    //Constuctor for the action; must include caller!
    public WaitAction()
    {
        //Construct me! Assigns caller by default in the base class
    }

    //The main function! This EXACT coroutine will be executed, even across frames.
    //See GameAction.cs for more information on how this function should work!
    public override IEnumerator TakeAction()
    {
        //TODO: Determine if this should be -100, or just set the energy to 0. One is more efficient,
        //but the other might give better gameplay results.

        #if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.Assert(Map.current.GetTile(caller.location).currentlyStanding == caller, "Waiting monster thinks that it is not on it's tile. Some other system has incorrectly set the currentTile.", caller);
        #endif

        caller.SetPosition(caller.location);
        caller.view.CollectEntities(Map.current);
        caller.energy -= 100f;
        yield break;
    }
}