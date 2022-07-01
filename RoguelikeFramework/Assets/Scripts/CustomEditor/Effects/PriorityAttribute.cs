using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class PriorityAttribute : System.Attribute
{
    private int priority;
    
    public PriorityAttribute(int priority)
    {
        this.priority = priority;
    }

    public int Priority
    {
        get { return priority; }
    }
}
