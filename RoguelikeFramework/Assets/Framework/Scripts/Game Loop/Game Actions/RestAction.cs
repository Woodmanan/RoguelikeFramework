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

        while (true)
        {

            if (caller.view.visibleMonsters.FindAll(x => x.IsEnemy(caller)).Count > 0)
            {
                Debug.Log("Log: You cannot rest while enemies are in sight.");
                yield break;
            }

            //yield return null;

            GameAction act = new WaitAction();
            act.Setup(caller);
            while (act.action.MoveNext())
            {
                yield return act.action.Current;
            }

            if (ReadyToStop())
            {
                Debug.Log("Console: You finish resting.");
                yield break;
            }

            yield return GameAction.StateCheck;
        }
    }

    //Called after construction, but before execution!
    //This is THE FIRST spot where caller is not null! Heres a great spot to actually set things up.
    public override void OnSetup()
    {

    }

    public bool ReadyToStop()
    {
        if (caller.baseStats[HEALTH] != caller.currentStats[MAX_HEALTH])
        {
            return false;
        }

        if (caller.baseStats[MANA] != caller.currentStats[MAX_MANA])
        {
            return false;
        }

        if (caller.baseStats[HEAT] > 0)
        {
            return false;
        }

        return true;
    }
}