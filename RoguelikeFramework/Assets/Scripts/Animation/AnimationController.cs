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

public class StepAnimation : RogueAnimation
{
    public const float movementDuration = .15f;
    Vector3 startLocation;
    Vector3 endLocation;
    Vector3 midPoint;
    Monster monster;

    public StepAnimation(Monster monster, Vector2Int oldLocation, Vector2Int newLocation) : base(movementDuration)
    {
        this.monster = monster;
        startLocation = new Vector3(oldLocation.x, oldLocation.y, Monster.monsterZPosition);
        endLocation = new Vector3(newLocation.x, newLocation.y, Monster.monsterZPosition);
        midPoint = (startLocation + endLocation) / 2 + Vector3.up;


    }

    public override void OnStart()
    {
        //Enforce location on creation
        monster.transform.position = startLocation;
    }

    public override void OnStep(float delta)
    {
        float t = currentDuration / MaxDuration;
        Vector3 a = Vector3.Lerp(startLocation, midPoint, t);
        Vector3 b = Vector3.Lerp(midPoint, endLocation, t);

        monster.transform.position = Vector3.Lerp(a, b, t);
    }

    public override void OnEnd()
    {
        monster.transform.position = endLocation;
    }
}

public class AttackAnimation : RogueAnimation
{
    public const float attackDuration = .3f;

    Monster monster;
    Vector3 start;
    Vector3 end;

    public AttackAnimation(Monster attacker, Monster defender) : base(attackDuration)
    {
        this.monster = attacker;
        this.start = new Vector3(attacker.location.x, attacker.location.y, Monster.monsterZPosition);
        this.end = new Vector3(defender.location.x, defender.location.y, Monster.monsterZPosition);
    }

    public override void OnStart()
    {
        //Enforce location on creation
        monster.transform.position = start;
    }

    public override void OnStep(float delta)
    {
        float t;
        float half = MaxDuration / 2;
        Vector3 midpoint = Vector3.Lerp(start, end, .5f);

        if (currentDuration < half)
        {
            t = currentDuration / half;
            monster.transform.position = Vector3.Lerp(start, midpoint, t);
        }
        else
        {
            t = (currentDuration - half) / half;
            monster.transform.position = Vector3.Lerp(midpoint, start, t);

        }
    }

    public override void OnEnd()
    {
        monster.transform.position = start;
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
