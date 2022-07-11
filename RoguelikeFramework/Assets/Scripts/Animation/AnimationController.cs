using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Animation System
 * 
 * Goals: Create a fast, powerful system for running code-driven,
 * dynamic animations. For example, moving a character, or having 
 * spell explosion that dynamically fills the target area. These
 * are not elegantly expressed in Unity AFAIK, and the dynamic
 * nature is hard to capture with the tradional tools. Additionally,
 * we need support for a powerful queueing system. Some animations
 * (like movement) should be allowed to run in parallel with similar
 * movements. Others, like spell casts, should wait for animations
 * before them to work, while also preventing animations behind them
 * from running.
 */

public class RogueAnimation
{
    public float MaxDuration = 0;
    public float currentDuration = 0;
    public bool isBlocking = false;

    public RogueAnimation(float MaxDuration, bool isBlocking = false)
    {
        this.MaxDuration = MaxDuration;
        this.isBlocking = isBlocking;
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

public class AnimationController : MonoBehaviour
{
    private static List<(RogueAnimation, int)> animations;
    private static int currentTurn;

    public Sprite sprite;
    public static int Count
    {
        get { return animations.Count; }
    }

    public static void AddAnimation(RogueAnimation anim)
    {
        animations.Add((anim, GameController.singleton.turn));
    }

    public static void StepAll()
    {
        if (animations.Count > 0)
        {
            currentTurn = animations[0].Item2;

            int max = 0;
            for (int i = 0; i < animations.Count; i++)
            {
                if (animations[i].Item1.isBlocking || animations[i].Item2 != currentTurn)
                {
                    break;
                }
                max++;
            }

            //Correct for first anim being blocking
            max = Mathf.Max(0, max - 1);

            for (int i = 0; i <= max; i++)
            {
                animations[i].Item1.Step(Time.deltaTime);
            }

            for (int i = max; i >= 0; i--)
            {
                if (animations[i].Item1.currentDuration >= animations[i].Item1.MaxDuration)
                {
                    animations.RemoveAt(i);
                }
            }
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        
        animations = new List<(RogueAnimation, int)>();
        currentTurn = 0;
    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = Mathf.Min(99, Mathf.Pow(2f, InputTracking.actions.Count));
        StepAll();
    }
}
