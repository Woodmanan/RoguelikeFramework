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

        bool shouldFinish = false;
        float clampedDelta = Mathf.Min(delta, MaxDuration - currentDuration);
        currentDuration += delta;
        if (currentDuration >= MaxDuration)
        {
            shouldFinish = true;
            currentDuration = MaxDuration;
        }

        OnStep(clampedDelta);
        if (shouldFinish)
        {
            OnEnd();
        }
    }

    public void Flush()
    {
        Step(MaxDuration);
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

    //Should this animation be played? Only visible animations get queued.
    public virtual bool IsVisible()
    {
        return true;
    }
}
