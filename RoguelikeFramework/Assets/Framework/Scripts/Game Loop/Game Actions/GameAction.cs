using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAction
{

    //--------------Static runtime variables----------------------

    //A permanent object that can be yielded, in order to allow the main
    //loop to perform a validity check, ending the turn if necessary
    public static YieldInstruction StateCheck = new WaitForSeconds(1.0f);
    public static YieldInstruction Abort = new WaitForSeconds(2.0f);
    public static YieldInstruction AbortAll = new WaitForSeconds(3.0f);
    public static YieldInstruction StateCheckAllowExit = new WaitForSeconds(4.0f); //Asks players if they'd like to stop
    public static YieldInstruction StateCheckNoExit = new WaitForSeconds(5.0f); //Willing to stop for a turn, but NOT willing to stop for visible enemies.

    //----------Shared variables per instance---------------------
    public RogueHandle<Monster> caller;
    public IEnumerator action;
    public bool finished;
    public bool successful = true;
    public bool stopsOnVisible = false;
    public bool checksOnVisible = false;
    public bool hasPreventedStop = false;

    //Empty contructor, because the flow works better if you make an object
    //and then just let the monster itself figure out how to use it
    public GameAction()
    {

    }

    /*
     * The big one! This function does all the fanciness for GameActions.
     * 
     * Essentially, this is the place where all of the actual code for an action goes.
     * Yields in this function propogate back to the main game loop, so don't be afraid to do
     * things like yield and wait for a UI window. Everything has been built to be as flexible as
     * possible!
     * 
     * GameActions can persist through multiple turns, but only if they yield at some point. I.E.,
     * a turn ends when the monster is at <= 0 energy, so if a yield is detected and the turn is over,
     * the game will continue on without the coroutine. This should never really affect most actions, as
     * they'll happen in one frame regardless of energy, or wait without subtracting energy. If you want to,
     * though, this behaviour lets us do all sort of fun behaviour, like having a 400 cost action that's 
     * interuptable across turns, like donning heavy armor. Best of all, it's all the same workflow, so you 
     * shouldn't have to do anything fancy!
     */
    public virtual IEnumerator TakeAction()
    {
        Debug.LogError("You forgot to override the TakeAction function! Skipping their turn for it.", caller[0].unity.gameObject);
        caller[0].energy -= 100;
        yield break; //This is stupid, but it's needed for the compiler to count this.
    }

    public IEnumerator RunAction()
    {
        finished = false;
        IEnumerator runState = TakeAction();
        while (runState.MoveNext())
        {
            if (runState.Current == GameAction.Abort)
            {
                successful = false;
                break;
            }
            else
            {
                stopsOnVisible = (runState.Current != GameAction.StateCheckNoExit);
                checksOnVisible = (runState.Current == GameAction.StateCheckAllowExit);
                yield return runState.Current;
            }
        }

        finished = true;
    }

    public void Setup(RogueHandle<Monster> caller)
    {
        this.caller = caller;
        OnSetup();
        action = RunAction();
    }

    public virtual void OnSetup()
    {

    }

    public virtual string GetDebugString()
    {
        return "No Debug String";
    }

    protected YieldInstruction SubAction(GameAction action)
    {
        action.Setup(caller);
        caller[0].AddSubAction(action);
        return null;
    }

    public static bool IsSpecialInstruction(YieldInstruction instruction)
    {
        return instruction != GameAction.StateCheck &&
               instruction != GameAction.StateCheckAllowExit &&
               instruction != GameAction.StateCheckNoExit;
    }

    public static bool HasSpecialInstruction(IEnumerator enumerator)
    {
        return enumerator.Current == GameAction.StateCheck ||
               enumerator.Current == GameAction.StateCheckAllowExit ||
               enumerator.Current == GameAction.StateCheckNoExit;
    }
}
