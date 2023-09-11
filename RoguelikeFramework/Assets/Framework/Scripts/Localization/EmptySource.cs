using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.SmartFormat.Core.Extensions;

public class EmptySource : ISource
{
    public string selector = "empty";

    public bool TryEvaluateSelector(ISelectorInfo selectorInfo)
    {
        if (selectorInfo.SelectorText != selector)
            return false;

        selectorInfo.Result = "";

        return true;
    }
}
