using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class OrderedEvent
{
    public static ReverseComparer comp = new ReverseComparer();

    public List<int> priorities = new List<int>();
    public List<ActionRef> delegates = new List<ActionRef>();
    public List<int> runData = new List<int>();
    int eventRunIndex = -1;

    public void AddEventFrame()
    {
        eventRunIndex++;
        if (eventRunIndex >= 32)
        {
            UnityEngine.Debug.LogError("Events cannot go more than 16 self calls deep!");
        }
        for (int i = 0; i < runData.Count; i++)
        {
            runData[i] &= ~(1 << eventRunIndex);
        }
    }

    public void FlagAsRun(int index)
    {
        runData[index] |= (1 << eventRunIndex);
    }

    public bool HasRun(int index)
    {
        return (runData[index] & (1 << eventRunIndex)) > 0;
    }

    public void ClearEventFrame()
    {
        eventRunIndex--;
    }

    public void AddListener(int priority, ActionRef listener)
    {
        int index = priorities.BinarySearch(priority, comp);
        if (index < 0) index = ~index;
        priorities.Insert(index, priority);
        delegates.Insert(index, listener);
        runData.Insert(index, 0);
    }

    public void AddListener(ActionRef listener)
    {
        AddListener(0, listener);
    }

    public void AddMethod(ActionRef act, Type defaultType, int priority)
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


    public void RemoveListener(ActionRef listener)
    {
        int index = delegates.IndexOf(listener);
        if (index >= 0)
        {
            priorities.RemoveAt(index);
            delegates.RemoveAt(index);
            runData.RemoveAt(index);
        }
    }

    public void Invoke()
    {
        AddEventFrame();

        for (int i = delegates.Count - 1; i >= 0; i--)
        {
            //Catch case where multiple events disconnect suddenly, so we have to skip many to get back to our place.
            if (i >= delegates.Count() || HasRun(i))
            {
                continue;
            }
            else
            {
                FlagAsRun(i);
                delegates[i](); //I LOVE that this is valid code
            }
        }

        ClearEventFrame();
    }

    public void BlendInvoke(OrderedEvent other)
    {
        if (other == null)
        {
            Invoke();
            return;
        }

        int i = delegates.Count - 1, j = other.delegates.Count - 1;

        //Add new event frame to track calls
        AddEventFrame();
        other.AddEventFrame();
        
        //Work through the lists until one of them is done
        while (i >= 0 && j >= 0)
        {
            if (i >= delegates.Count || HasRun(i))
            {
                i--;
                continue;
            }
            else if (j >= other.delegates.Count || other.HasRun(j))
            {
                j--;
                continue;
            }


            if (priorities[i] < other.priorities[j])
            {
                FlagAsRun(i);
                delegates[i]();
                i--;
            }
            else
            {
                other.FlagAsRun(j);
                other.delegates[j]();
                j--;
            }
        }
        
        //Clear out rest of our list, if it exists
        for (; i >= 0; i--)
        {
            if (i >= delegates.Count() || HasRun(i))
            {
                continue;
            }
            else
            {
                FlagAsRun(i);
                delegates[i]();
            }
        }

        //Clear out their list, if it exists
        for (; j >= 0; j--)
        {
            if (j >= other.delegates.Count || other.HasRun(j))
            {
                continue;
            }
            else
            {
                other.FlagAsRun(j);
                other.delegates[j]();
            }
        }

        //Cleanup event frame data
        ClearEventFrame();
        other.ClearEventFrame();
    }
}

public class OrderedEvent<T1>
{
    public static ReverseComparer comp = new ReverseComparer();

    public List<int> priorities = new List<int>();
    public List<ActionRef<T1>> delegates = new List<ActionRef<T1>>();
    public List<int> runData = new List<int>();
    int eventRunIndex = -1;

    public void AddEventFrame()
    {
        eventRunIndex++;
        if (eventRunIndex >= 32)
        {
            UnityEngine.Debug.LogError("Events cannot go more than 16 self calls deep!");
        }
        for (int i = 0; i < runData.Count; i++)
        {
            runData[i] &= ~(1 << eventRunIndex);
        }
    }

    public void FlagAsRun(int index)
    {
        runData[index] |= (1 << eventRunIndex);
    }

    public bool HasRun(int index)
    {
        return (runData[index] & (1 << eventRunIndex)) > 0;
    }

    public void ClearEventFrame()
    {
        eventRunIndex--;
    }

    public void AddListener(int priority, ActionRef<T1> listener)
    {

        int index = priorities.BinarySearch(priority, comp);
        if (index < 0) index = ~index;
        priorities.Insert(index, priority);
        delegates.Insert(index, listener);
        runData.Insert(index, 0);
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
            runData.RemoveAt(index);
        }
    }

    public void Invoke(ref T1 arg1)
    {
        AddEventFrame();

        for (int i = delegates.Count - 1; i >= 0; i--)
        {
            //Catch case where multiple events disconnect suddenly, so we have to skip many to get back to our place.
            if (i >= delegates.Count() || HasRun(i))
            {
                continue;
            }
            else
            {
                FlagAsRun(i);
                delegates[i](ref arg1); //I LOVE that this is valid code
            }
        }

        ClearEventFrame();
    }

    public void BlendInvoke(OrderedEvent<T1> other, ref T1 arg1)
    {
        if (other == null)
        {
            Invoke(ref arg1);
            return;
        }

        int i = delegates.Count - 1, j = other.delegates.Count - 1;

        //Add new event frame to track calls
        AddEventFrame();
        other.AddEventFrame();

        //Work through the lists until one of them is done
        while (i >= 0 && j >= 0)
        {
            if (i >= delegates.Count || HasRun(i))
            {
                i--;
                continue;
            }
            else if (j >= other.delegates.Count || other.HasRun(j))
            {
                j--;
                continue;
            }


            if (priorities[i] < other.priorities[j])
            {
                FlagAsRun(i);
                delegates[i](ref arg1);
                i--;
            }
            else
            {
                other.FlagAsRun(j);
                other.delegates[j](ref arg1);
                j--;
            }
        }

        //Clear out rest of our list, if it exists
        for (; i >= 0; i--)
        {
            if (i >= delegates.Count() || HasRun(i))
            {
                continue;
            }
            else
            {
                FlagAsRun(i);
                delegates[i](ref arg1);
            }
        }

        //Clear out their list, if it exists
        for (; j >= 0; j--)
        {
            if (j >= other.delegates.Count || other.HasRun(j))
            {
                continue;
            }
            else
            {
                other.FlagAsRun(j);
                other.delegates[j](ref arg1);
            }
        }

        //Cleanup event frame data
        ClearEventFrame();
        other.ClearEventFrame();
    }
}

public class OrderedEvent<T1, T2>
{
    public static ReverseComparer comp = new ReverseComparer();

    public List<int> priorities = new List<int>();
    public List<ActionRef<T1, T2>> delegates = new List<ActionRef<T1, T2>>();
    public List<int> runData = new List<int>();
    int eventRunIndex = -1;

    public void AddEventFrame()
    {
        eventRunIndex++;
        if (eventRunIndex >= 32)
        {
            UnityEngine.Debug.LogError("Events cannot go more than 16 self calls deep!");
        }
        for (int i = 0; i < runData.Count; i++)
        {
            runData[i] &= ~(1 << eventRunIndex);
        }
    }

    public void FlagAsRun(int index)
    {
        runData[index] |= (1 << eventRunIndex);
    }

    public bool HasRun(int index)
    {
        return (runData[index] & (1 << eventRunIndex)) > 0;
    }

    public void ClearEventFrame()
    {
        eventRunIndex--;
    }

    public void AddListener(int priority, ActionRef<T1, T2> listener)
    {

        int index = priorities.BinarySearch(priority, comp);
        if (index < 0) index = ~index;
        priorities.Insert(index, priority);
        delegates.Insert(index, listener);
        runData.Insert(index, 0);
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
            runData.RemoveAt(index);
        }
    }

    public void Invoke(ref T1 arg1, ref T2 arg2)
    {
        AddEventFrame();

        for (int i = delegates.Count - 1; i >= 0; i--)
        {
            //Catch case where multiple events disconnect suddenly, so we have to skip many to get back to our place.
            if (i >= delegates.Count() || HasRun(i))
            {
                continue;
            }
            else
            {
                FlagAsRun(i);
                delegates[i](ref arg1, ref arg2);
            }
        }

        ClearEventFrame();
    }

    public void BlendInvoke(OrderedEvent<T1, T2> other, ref T1 arg1, ref T2 arg2)
    {
        if (other == null)
        {
            Invoke(ref arg1, ref arg2);
            return;
        }

        int i = delegates.Count - 1, j = other.delegates.Count - 1;

        //Add new event frame to track calls
        AddEventFrame();
        other.AddEventFrame();

        //Work through the lists until one of them is done
        while (i >= 0 && j >= 0)
        {
            if (i >= delegates.Count || HasRun(i))
            {
                i--;
                continue;
            }
            else if (j >= other.delegates.Count || other.HasRun(j))
            {
                j--;
                continue;
            }


            if (priorities[i] < other.priorities[j])
            {
                FlagAsRun(i);
                delegates[i](ref arg1, ref arg2);
                i--;
            }
            else
            {
                other.FlagAsRun(j);
                other.delegates[j](ref arg1, ref arg2);
                j--;
            }
        }

        //Clear out rest of our list, if it exists
        for (; i >= 0; i--)
        {
            if (i >= delegates.Count() || HasRun(i))
            {
                continue;
            }
            else
            {
                FlagAsRun(i);
                delegates[i](ref arg1, ref arg2);
            }
        }

        //Clear out their list, if it exists
        for (; j >= 0; j--)
        {
            if (j >= other.delegates.Count || other.HasRun(j))
            {
                continue;
            }
            else
            {
                other.FlagAsRun(j);
                other.delegates[j](ref arg1, ref arg2);
            }
        }

        //Cleanup event frame data
        ClearEventFrame();
        other.ClearEventFrame();
    }
}

public class OrderedEvent<T1, T2, T3>
{
    public static ReverseComparer comp = new ReverseComparer();

    public List<int> priorities = new List<int>();
    public List<ActionRef<T1, T2, T3>> delegates = new List<ActionRef<T1, T2, T3>>();
    public List<int> runData = new List<int>();
    int eventRunIndex = -1;

    public void AddEventFrame()
    {
        eventRunIndex++;
        if (eventRunIndex >= 32)
        {
            UnityEngine.Debug.LogError("Events cannot go more than 16 self calls deep!");
        }
        for (int i = 0; i < runData.Count; i++)
        {
            runData[i] &= ~(1 << eventRunIndex);
        }
    }

    public void FlagAsRun(int index)
    {
        runData[index] |= (1 << eventRunIndex);
    }

    public bool HasRun(int index)
    {
        return (runData[index] & (1 << eventRunIndex)) > 0;
    }

    public void ClearEventFrame()
    {
        eventRunIndex--;
    }

    public void AddListener(int priority, ActionRef<T1, T2, T3> listener)
    {
        int index = priorities.BinarySearch(priority, comp);
        if (index < 0) index = ~index;
        priorities.Insert(index, priority);
        delegates.Insert(index, listener);
        runData.Insert(index, 0);
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
            runData.RemoveAt(index);
        }
    }

    public void Invoke(ref T1 arg1, ref T2 arg2, ref T3 arg3)
    {
        AddEventFrame();

        for (int i = delegates.Count - 1; i >= 0; i--)
        {
            //Catch case where multiple events disconnect suddenly, so we have to skip to get back to our place.
            if (i >= delegates.Count() || HasRun(i))
            {
                continue;
            }
            else
            {
                FlagAsRun(i);
                delegates[i](ref arg1, ref arg2, ref arg3);
            }
        }

        ClearEventFrame();
    }

    public void BlendInvoke(OrderedEvent<T1, T2, T3> other, ref T1 arg1, ref T2 arg2, ref T3 arg3)
    {
        if (other == null)
        {
            Invoke(ref arg1, ref arg2, ref arg3);
            return;
        }

        int i = delegates.Count - 1, j = other.delegates.Count - 1;

        //Add new event frame to track calls
        AddEventFrame();
        other.AddEventFrame();

        //Work through the lists until one of them is done
        while (i >= 0 && j >= 0)
        {
            if (i >= delegates.Count || HasRun(i))
            {
                i--;
                continue;
            }
            else if (j >= other.delegates.Count || other.HasRun(j))
            {
                j--;
                continue;
            }


            if (priorities[i] < other.priorities[j])
            {
                FlagAsRun(i);
                delegates[i](ref arg1, ref arg2, ref arg3);
                i--;
            }
            else
            {
                other.FlagAsRun(j);
                other.delegates[j](ref arg1, ref arg2, ref arg3);
                j--;
            }
        }

        //Clear out rest of our list, if it exists
        for (; i >= 0; i--)
        {
            if (i >= delegates.Count() || HasRun(i))
            {
                continue;
            }
            else
            {
                FlagAsRun(i);
                delegates[i](ref arg1, ref arg2, ref arg3);
            }
        }

        //Clear out their list, if it exists
        for (; j >= 0; j--)
        {
            if (j >= other.delegates.Count || other.HasRun(j))
            {
                continue;
            }
            else
            {
                other.FlagAsRun(j);
                other.delegates[j](ref arg1, ref arg2, ref arg3);
            }
        }

        //Cleanup event frame data
        ClearEventFrame();
        other.ClearEventFrame();
    }
}

public class ReverseComparer : IComparer<int>
{
    public int Compare(int a, int b)
    {
        return b.CompareTo(a);
    }
}
