using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AutoAttackAction : GameAction
{
    public Monster target;
    //Constuctor for the action; must include caller!
    public AutoAttackAction()
    {
        //Construct me! Assigns caller by default in the base class
    }

    //The main function! This EXACT coroutine will be executed, even across frames.
    //See GameAction.cs for more information on how this function should work!
    public override IEnumerator TakeAction()
    {
        if (target)
        {
            yield return SubAction(new PathfindAction(target.location));
        }
        else
        {
            Debug.Log("Console (player only): Can't auto-fight with no enemies in sight!");
            yield break;
        }
    }

    public override string GetDebugString()
    {
        return $"Auto Attack action : {target}";
    }

    //Called after construction, but before execution!
    //This is THE FIRST spot where caller is not null! Heres a great spot to actually set things up.
    public override void OnSetup()
    {
        List<Monster> enemies = caller.view.visibleEnemies;
        if (enemies.Count > 0)
        {
            target = enemies.OrderBy(x => x.DistanceFrom(caller)).First();
        }
        else
        {
            target = null;
        }
    }
}
