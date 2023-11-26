using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static RNGType;

public enum RNGType
{
    Constant,
    Linear,
    Exponential
}

[System.Serializable]
public struct RandomNumber
{
    public RNGType rngType;
    public int min;
    public int max;

    public int Evaluate()
    {
        switch (rngType)
        {
            case Constant:
                return Mathf.Max(min, max);
            case Linear:
                return RogueRNG.Linear(min, max + 1);
            case Exponential:
                return Mathf.RoundToInt(RogueRNG.Exponential(Mathf.Max(min, max)));
            default:
                Debug.LogError($"RandomNumber using invalid rng type - {rngType}");
                return 1;
        }
    }

    public float EvaluateFloat()
    {
        switch (rngType)
        {
            case Constant:
                return Mathf.Max((float)min, max);
            case Linear:
                return RogueRNG.Linear((float)min, max);
            case Exponential:
                return RogueRNG.Exponential(Mathf.Max(min, max));
            default:
                Debug.LogError($"RandomNumber using invalid rng type - {rngType}");
                return 1f;
        }
    }

    public int Range()
    {
        if (rngType == RNGType.Linear)
        {
            return Mathf.Abs((max + 1) - min);
        }
        return Mathf.Abs(max - min);
    }
}
