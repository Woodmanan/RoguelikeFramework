using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class StatusEffect
{
    public Effect heldEffect; //Your scriptable object goes here!

    public Effect Instantiate()
    {
        return Effect.Instantiate(heldEffect);
    }
}
