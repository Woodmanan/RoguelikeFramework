using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterRest : GameAction
{
    //Constuctor for the action
    public MonsterRest()
    {
        //Construct me! Don't need caller here, that will get assigned during Setup.
    }

    //The main function! This EXACT coroutine will be executed, even across frames.
    //See GameAction.cs for more information on how this function should work!
    public override IEnumerator TakeAction()
    {
        if (caller.resources.health == caller.stats.resources.health)
        {
            yield break;
        }

        while (true)
        {

            if (caller.view.visibleMonsters.FindAll(x => x.IsEnemy(caller)).Count > 0)
            {
                Debug.Log($"{caller.GetFormattedName()} stops resting.");
                yield break;
            }

            yield return null;

            GameAction act = new WaitAction();
            act.Setup(caller);
            while (act.action.MoveNext())
            {
                yield return act.action.Current;
            }

            caller.Heal(1, true);

            if (caller.resources.health == caller.stats.resources.health)
            {
                Debug.Log($"Log: {caller.GetFormattedName()} stops resting.");
                yield break;
            }
        }
    }

    //Called after construction, but before execution!
    //This is THE FIRST spot where caller is not null! Heres a great spot to actually set things up.
    public override void OnSetup()
    {

    }
}