using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RogueAnimation
{
    [HideInInspector] public float MaxDuration = 0;
    [HideInInspector] public float currentDuration = 0;

    public bool isFinished
    {
        get { return currentDuration >= MaxDuration; }
    }

    public RogueAnimation(float MaxDuration)
    {
        this.MaxDuration = MaxDuration;
        currentDuration = 0;
    }

    public void Step(float delta)
    {
        if (currentDuration == 0)
        {
            OnStart();
        }

        currentDuration += delta;
        OnStep(delta);
        if (currentDuration >= MaxDuration)
        {
            OnEnd();
        }
    }

    public void Flush()
    {
        Step(MaxDuration - currentDuration);
    }

    public virtual void OnStart()
    {

    }

    public virtual void OnStep(float delta)
    {

    }

    public virtual void OnEnd()
    {

    }
}
