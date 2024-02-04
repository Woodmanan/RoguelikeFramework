using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Resources;

public class RestAction : GameAction
{
    //Constuctor for the action
    public RestAction()
    {
        //Construct me! Don't need caller here, that will get assigned during Setup.
    }

    //The main function! This EXACT coroutine will be executed, even across frames.
    //See GameAction.cs for more information on how this function should work!
    public override IEnumerator TakeAction()
    {
        if (ReadyToStop())
        {
            yield break;
        }

        while (!ReadyToStop())
        {
            if (caller[0].view.visibleEnemies.Count > 0)
            {
                Debug.Log("Log: You cannot rest while enemies are in sight.");
                yield break;
            }

            //Check for player escape
            if (InputTracking.NumOfUnmatchedActions(PlayerAction.NONE, PlayerAction.AUTO_EXPLORE) > 0)
            {
                InputTracking.Clear();
                yield return GameAction.AbortAll;
            }

            yield return SubAction(new WaitAction());
            yield return GameAction.StateCheck;
        }

        Debug.Log("Console: You finish resting.");
    }

    public override string GetDebugString()
    {
        return "Rest Action";
    }

    //Called after construction, but before execution!
    //This is THE FIRST spot where caller is not null! Heres a great spot to actually set things up.
    public override void OnSetup()
    {

    }

    public bool ReadyToStop()
    {
        if (caller[0].baseStats[HEALTH] != caller[0].currentStats[MAX_HEALTH])
        {
            return false;
        }

        if (caller[0].baseStats[MANA] != caller[0].currentStats[MAX_MANA])
        {
            return false;
        }

        if (caller[0].baseStats[HEAT] > 0)
        {
            return false;
        }

        return true;
    }
}