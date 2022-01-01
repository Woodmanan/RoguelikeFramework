using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

//Storage object, which is mostly here to make things nice and easy for us later on.
//Only one of these should ever exist, and it should be in the same folder as this object!

//[CreateAssetMenu(fileName = "EffectConnections", menuName = "Effect Connections", order = 1)]

[Serializable]
public struct Variable
{
    public string type;
    public string name;
}
[Serializable]
public struct Connection
{
    public string name;
    public List<Variable> types;
    public string description;
    public int priority;
}

public class EffectConnections : ScriptableObject
{
    public List<Connection> connections;

    // Start is called before the first frame update
    void Start()
    {
        Debug.LogError("This thing SHOULD NOT be in the scene.", this);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
