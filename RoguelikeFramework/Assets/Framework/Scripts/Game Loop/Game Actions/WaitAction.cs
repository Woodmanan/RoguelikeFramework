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
        Debug.AssertFormat(Map.current.GetTile(caller[0].location).currentlyStanding == caller, "Waiting monster thinks that it is not on it's tile. Some other system has incorrectly set the currentTile.", caller[0].unity);
        #endif

        caller[0].SetPosition(caller[0].location);
        //caller.view.CollectEntities(Map.current);
        caller[0].energy -= 100f;
        caller[0].willSwap = true;
        yield break;
    }

    public override string GetDebugString()
    {
        return "Wait Action";
    }
}