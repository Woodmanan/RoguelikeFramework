using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
[AttributeUsage(AttributeTargets.Method)]
public class CheatAttribute : System.Attribute
{
    bool auto;

    public CheatAttribute(bool hasAutoComplete = false)
    {
        auto = hasAutoComplete;
    }

    public bool hasAutoComplete
    {
        get { return auto; }
    }
}
#endif