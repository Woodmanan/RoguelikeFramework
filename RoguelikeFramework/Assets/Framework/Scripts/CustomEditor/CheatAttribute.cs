using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
[AttributeUsage(AttributeTargets.Method)]
public class CheatAttribute : System.Attribute
{
    public CheatAttribute() { }
}
#endif