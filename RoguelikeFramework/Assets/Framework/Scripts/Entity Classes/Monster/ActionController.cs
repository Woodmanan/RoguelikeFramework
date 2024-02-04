using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ActionController : MonoBehaviour
{
    public GameAction nextAction = null;
    public IEnumerator selection;
    [HideInInspector] public RogueHandle<Monster> monster;

    public void ClearAction()
    {
        nextAction = null;
        selection = DetermineAction();
    }

    public virtual void Setup()
    {
        monster = GetComponent<UnityMonster>().monsterHandle;
        Debug.Assert(monster.IsValid());
    }

    public virtual IEnumerator DetermineAction()
    {
        Debug.LogWarning("A monster used an ActionController, instead of it's derivative classes!");
        nextAction = new WaitAction();
        yield break;
    }

    public virtual IEnumerator DetermineTarget(Targeting targeting, BoolDelegate setValidityTo, Func<RogueHandle<Monster>, bool> TargetCheck = null)
    {
        Debug.Log("Action controllers need to override this method!");
        setValidityTo.Invoke(false);
        yield break;
    }
}