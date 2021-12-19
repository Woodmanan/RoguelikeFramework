using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAction
{

    //--------------Static runtime variables----------------------

    //A permanent object that can be yielded, in order to allow the main
    //loop to perform a validity check, ending the turn if necessary
    public static YieldInstruction StateCheck = new WaitForSeconds(1.0f); 

    //----------Shared variables per instance---------------------
    public Monster caller;
    public IEnumerator action;
    public bool finished;

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
        Debug.LogError("You forgot to override the TakeAction function! Skipping their turn for it.", caller.gameObject);
        caller.energy -= 100;
        if (false)
        {
            yield return null;
        }
    }

    public IEnumerator RunAction()
    {
        finished = false;
        IEnumerator runState = TakeAction();
        while (runState.MoveNext())
        {
            yield return runState.Current;
        }

        finished = true;
    }

    public void Setup(Monster caller)
    {
        this.caller = caller;
        OnSetup();
        action = RunAction();
    }

    public virtual void OnSetup()
    {

    }
}
