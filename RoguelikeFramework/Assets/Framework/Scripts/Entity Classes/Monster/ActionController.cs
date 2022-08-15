using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionController : MonoBehaviour
{
    public GameAction nextAction = null;
    public IEnumerator selection;
    [HideInInspector] public Monster monster;
    // Start is called before the first frame update
    protected void Awake()
    {
        monster = GetComponent<Monster>();
    }

    public void ClearAction()
    {
        nextAction = null;
        selection = DetermineAction();
    }

    public virtual void Setup()
    {

    }

    public virtual IEnumerator DetermineAction()
    {
        Debug.LogWarning("A monster used an ActionController, instead of it's derivative classes!");
        nextAction = new WaitAction();
        yield break;
    }

    public virtual IEnumerator DetermineTarget(Targeting targeting, BoolDelegate setValidityTo)
    {
        Debug.Log("Action controllers need to override this method!");
        setValidityTo.Invoke(false);
        yield break;
    }
}