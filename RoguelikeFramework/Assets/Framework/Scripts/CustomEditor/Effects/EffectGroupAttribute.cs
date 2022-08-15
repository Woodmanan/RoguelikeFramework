using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Class)]
public class EffectGroupAttribute : System.Attribute
{
    private string group;

    public EffectGroupAttribute(string group)
    {
        this.group = group;
    }

    public string groupName
    {
        get { return group; }
    }
}