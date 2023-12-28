using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

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

    public void StepAll(float animationSpeed)
    {
        for (int i = 0; i < animations.Count; i++)
        {
            animations[i].Step(animationSpeed);
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

[System.Serializable]
public struct AnimationSpeed
{
    public int groupCount;
    public int inputCount;
    public float speed;
}

public class AnimationController : MonoBehaviour
{
    private static List<AnimGroup> animations;
    public List<AnimationSpeed> speeds;
    public float flushSpeed;
    public static float animationSpeed;

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

    public static int numGroups
    {
        get { return animations.Count; }
    }

    public static int GetNumAnimsInGroup(int index)
    {
        return animations[index].animations.Count;
    }
    
    public static void PushNewAnimGroup()
    {
        animations.Add(new AnimGroup());
    }

    public static void AddAnimation(RogueAnimation anim)
    {
        if (anim.IsVisible())
        {
            workingGroup.AddAnim(anim);
        }
    }

    public static void AddAnimationForMonster(RogueAnimation anim, Monster monster, params Monster[] secondary)
    {
        if (anim.IsVisible())
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
        if (anim.IsVisible())
        {
            if (!workingGroup.CanAddAnimForObject(pairedObject))
            {
                PushNewAnimGroup();
            }
            workingGroup.AddAnim(anim, pairedObject);
        }
    }

    public static void AddAnimationSolo(RogueAnimation anim)
    {
        if (anim.IsVisible())
        {
            BeginSoloGroup();
            workingGroup.AddAnim(anim);
            EndSoloGroup();
        }
    }

    public static void AddAnimationSolo(params RogueAnimation[] anims)
    {
        if (anims.Any(x => x.IsVisible()))
        {
            BeginSoloGroup();
            foreach (RogueAnimation anim in anims)
            {
                if (anim.IsVisible())
                {
                    workingGroup.AddAnim(anim);
                }
            }
            EndSoloGroup();
        }
    }

    public static void AddAnimationSolo(List<RogueAnimation> anims)
    {
        if (anims.Any(x => x.IsVisible()))
        {
            BeginSoloGroup();
            foreach (RogueAnimation anim in anims)
            {
                if (anim.IsVisible())
                {
                    workingGroup.AddAnim(anim);
                }
            }
            EndSoloGroup();
        }
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

    public static void StepAnimations(float animationSpeed)
    {
        if (hasAnimations)
        {
            activeGroup.StepAll(animationSpeed * Time.deltaTime);
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

    public static void FlushSingleAnimation()
    {
        if (hasAnimations)
        {
            activeGroup.Flush();
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
        int inputCount = InputTracking.actions.Count;
        int animationCount = animations.Count;

        animationSpeed = 1;

        for (int i = 0; i < speeds.Count; i++)
        {
            if (inputCount >= speeds[i].inputCount || animationCount >= speeds[i].groupCount)
            {
                animationSpeed = speeds[i].speed;
            }
        }

        if (animationSpeed < flushSpeed)
        {
            StepAnimations(animationSpeed);
        }
        else
        {
            FlushSingleAnimation();
        }
        
    }
}
