using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Class)]
public class GroupAttribute : System.Attribute
{
    private string group;

    public GroupAttribute(string group)
    {
        this.group = group;
    }

    public string groupName
    {
        get { return group; }
    }
}

public class ResourceGroup : System.Attribute
{
    private ResourceType resourceType;

    public ResourceGroup(ResourceType resourceType)
    {
        this.resourceType = resourceType;
    }

    public ResourceType resourceTypeValue
    {
        get { return resourceType; }
    }
}