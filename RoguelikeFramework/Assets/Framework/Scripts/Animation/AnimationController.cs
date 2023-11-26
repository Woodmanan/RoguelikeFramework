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

public class AnimGroup
{
    public List<RogueAnimation> animations = new List<RogueAnimation>();
    public HashSet<Object> blockedObjects = new HashSet<Object>();
    public bool isEmpty
    {
        get { return animations.Count == 0; }
    }

    public bool CanAddAnimForObject(Object toAdd)
    {
        return !blockedObjects.Contains(toAdd);
    }

    public void AddAnim(RogueAnimation anim, Object pairedObject = null)
    {
        animations.Add(anim);
        if (pairedObject)
        {
            blockedObjects.Add(pairedObject);
        }
    }

    public void StepAll()
    {
        for (int i = 0; i < animations.Count; i++)
        {
            animations[i].Step(Time.deltaTime);
        }

        for (int i = animations.Count - 1; i >= 0; i--)
        {
            if (animations[i].isFinished)
            {
                animations.RemoveAt(i);
            }
        }
    }

    public void Flush()
    {
        for (int i = 0; i < animations.Count; i++)
        {
            animations[i].Flush();
        }

        for (int i = animations.Count - 1; i >= 0; i--)
        {
            if (animations[i].isFinished)
            {
                animations.RemoveAt(i);
            }
        }

        if (animations.Count != 0)
        {
            Debug.LogError("Flush did not clear out all anims!");
        }
    }
}

public class AnimationController : MonoBehaviour
{
    private static List<AnimGroup> animations;

    //The last animation group is considered "in progress" - it's the only modifiable one
    private static AnimGroup workingGroup
    {
        get
        {
            if (animations.Count == 0)
            {
                PushNewAnimGroup();
            }
            return animations[animations.Count - 1];
        }
    }

    private static AnimGroup activeGroup
    {
        get
        {
            if (animations.Count > 0)
            {
                return animations[0];
            }
            return null;
        }
    }

    public Sprite sprite;
    public static int Count
    {
        get { return animations.Count; }
    }

    public static bool hasAnimations
    {
        get { return animations.Count > 0; }
    }
    
    public static void PushNewAnimGroup()
    {
        animations.Add(new AnimGroup());
    }

    public static void AddAnimation(RogueAnimation anim)
    {
        workingGroup.AddAnim(anim);
    }

    public static void AddAnimationForMonster(RogueAnimation anim, Monster monster, params Monster[] secondary)
    {
        if (monster.renderer.enabled && monster.currentTile != null && monster.currentTile.isVisible)
        {
            AddAnimationForObject(anim, monster);
            foreach (Monster blocked in secondary)
            {
                workingGroup.blockedObjects.Add(blocked);
            }
        }
    }

    public static void AddAnimationForObject(RogueAnimation anim, Object pairedObject)
    {
        if (!workingGroup.CanAddAnimForObject(pairedObject))
        {
            PushNewAnimGroup();
        }
        workingGroup.AddAnim(anim, pairedObject);
    }

    public static void AddAnimationSolo(RogueAnimation anim)
    {
        BeginSoloGroup();
        workingGroup.AddAnim(anim);
        EndSoloGroup();
    }

    public static void AddAnimationSolo(params RogueAnimation[] anims)
    {
        BeginSoloGroup();
        foreach (RogueAnimation anim in anims)
        {
            workingGroup.AddAnim(anim);
        }
        EndSoloGroup();
    }

    public static void AddAnimationSolo(List<RogueAnimation> anims)
    {
        BeginSoloGroup();
        foreach (RogueAnimation anim in anims)
        {
            workingGroup.AddAnim(anim);
        }
        EndSoloGroup();
    }

    public static void BeginSoloGroup()
    {
        if (!workingGroup.isEmpty)
        {
            PushNewAnimGroup();
        }
    }

    public static void EndSoloGroup()
    {
        if (!workingGroup.isEmpty)
        {
            PushNewAnimGroup();
        }
    }

    private static void MoveToNextGroup()
    {
        if (animations.Count == 0)
        {
            Debug.LogError("Animation groups have gotten out of sync!");
            return;
        }
        animations.RemoveAt(0);
    }

    public static void StepAnimations()
    {
        if (hasAnimations)
        {
            activeGroup.StepAll();
            if (activeGroup.isEmpty)
            {
                MoveToNextGroup();
            }
        }
    }

    public static void Flush()
    {
        while (hasAnimations)
        {
            activeGroup.Flush();
            MoveToNextGroup();
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        
        animations = new List<AnimGroup>();
    }

    // Update is called once per frame
    void Update()
    {
        Time.timeScale = Mathf.Min(16, 1.0f * Mathf.Pow(1.5f, animations.Count) * Mathf.Pow(2f, InputTracking.actions.Count));

        if (Time.timeScale > 10)
        {
            Flush();
            Time.timeScale = 10;
        }

        StepAnimations();
    }
}
