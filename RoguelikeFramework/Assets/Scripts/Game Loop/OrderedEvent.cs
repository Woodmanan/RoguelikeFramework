using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class OrderedEvent
{
    public List<int> priorities = new List<int>();
    public List<Action> delegates = new List<Action>();

    public void AddListener(int priority, Action listener)
    {
        int index = priorities.BinarySearch(priority);
        if (index < 0) index = ~index;
        priorities.Insert(index, priority);
        delegates.Insert(index, listener);
    }

    public void AddListener(Action listener)
    {
        AddListener(0, listener);
    }

    public void AddMethod(Action act, Type defaultType, int priority)
    {
        var method = act.Method;
        if (method.DeclaringType != defaultType)
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                AddListener(((PriorityAttribute)attribute).Priority, act);
            }
            else
            {
                AddListener(priority, act);
            }
        }
    }

    public void RemoveListener(Action listener)
    {
        int index = delegates.IndexOf(listener);
        if (index >= 0)
        {
            priorities.RemoveAt(index);
            delegates.RemoveAt(index);
        }
    }

    public void Invoke()
    {
        for (int i = 0; i < delegates.Count; i++)
        {
            delegates[i](); //I LOVE that this is valid code
        }
    }
}

public class OrderedEvent<T1>
{
    public List<int> priorities = new List<int>();
    public List<ActionRef<T1>> delegates = new List<ActionRef<T1>>();

    public void AddListener(int priority, ActionRef<T1> listener)
    {

        int index = priorities.BinarySearch(priority);
        if (index < 0) index = ~index;
        priorities.Insert(index, priority);
        delegates.Insert(index, listener);
    }

    public void AddListener(ActionRef<T1> listener)
    {
        AddListener(0, listener);
    }

    public void AddMethod(ActionRef<T1> act, Type defaultType, int priority)
    {
        var method = act.Method;
        if (method.DeclaringType != defaultType)
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                AddListener(((PriorityAttribute)attribute).Priority, act);
            }
            else
            {
                AddListener(priority, act);
            }
        }
    }

    public void RemoveListener(ActionRef<T1> listener)
    {
        int index = delegates.IndexOf(listener);
        if (index >= 0)
        {
            priorities.RemoveAt(index);
            delegates.RemoveAt(index);
        }
    }

    public void Invoke(ref T1 arg1)
    {
        for (int i = 0; i < delegates.Count; i++)
        {
            delegates[i](ref arg1);
        }
    }
}

public class OrderedEvent<T1, T2>
{
    public List<int> priorities = new List<int>();
    public List<ActionRef<T1, T2>> delegates = new List<ActionRef<T1, T2>>();

    public void AddListener(int priority, ActionRef<T1, T2> listener)
    {

        int index = priorities.BinarySearch(priority);
        if (index < 0) index = ~index;
        priorities.Insert(index, priority);
        delegates.Insert(index, listener);
    }

    public void AddListener(ActionRef<T1, T2> listener)
    {
        AddListener(0, listener);
    }

    public void AddMethod(ActionRef<T1, T2> act, Type defaultType, int priority)
    {
        var method = act.Method;
        if (method.DeclaringType != defaultType)
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                AddListener(((PriorityAttribute)attribute).Priority, act);
            }
            else
            {
                AddListener(priority, act);
            }
        }
    }

    public void RemoveListener(ActionRef<T1, T2> listener)
    {
        int index = delegates.IndexOf(listener);
        if (index >= 0)
        {
            priorities.RemoveAt(index);
            delegates.RemoveAt(index);
        }
    }

    public void Invoke(ref T1 arg1, ref T2 arg2)
    {
        for (int i = 0; i < delegates.Count; i++)
        {
            delegates[i](ref arg1, ref arg2);
        }
    }
}

public class OrderedEvent<T1, T2, T3>
{
    public List<int> priorities = new List<int>();
    public List<ActionRef<T1, T2, T3>> delegates = new List<ActionRef<T1, T2, T3>>();

    public void AddListener(int priority, ActionRef<T1, T2, T3> listener)
    {
        int index = priorities.BinarySearch(priority);
        if (index < 0) index = ~index;
        priorities.Insert(index, priority);
        delegates.Insert(index, listener);
    }

    public void AddListener(ActionRef<T1, T2, T3> listener)
    {
        AddListener(0, listener);
    }

    public void AddMethod(ActionRef<T1, T2, T3> act, Type defaultType, int priority)
    {
        var method = act.Method;
        if (method.DeclaringType != defaultType)
        {
            object attribute = method.GetCustomAttributes(typeof(PriorityAttribute), false).FirstOrDefault();
            if (attribute != null)
            {
                AddListener(((PriorityAttribute) attribute).Priority , act);
            }
            else
            {
                AddListener(priority, act);
            }
        }
    }

    public void RemoveListener(ActionRef<T1, T2, T3> listener)
    {
        int index = delegates.IndexOf(listener);
        if (index >= 0)
        {
            priorities.RemoveAt(index);
            delegates.RemoveAt(index);
        }
    }

    public void Invoke(ref T1 arg1, ref T2 arg2, ref T3 arg3)
    {
        for (int i = 0; i < delegates.Count; i++)
        {
            delegates[i](ref arg1, ref arg2, ref arg3);
        }
    }
}
