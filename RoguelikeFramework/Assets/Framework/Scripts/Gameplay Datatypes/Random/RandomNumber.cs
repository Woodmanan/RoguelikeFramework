using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static RNGType;

public enum RNGType
{
    Constant,
    Linear
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
            default:
                Debug.LogError($"RandomNumber using invalid rng type - {rngType}");
                return 1;
        }
    }
}
