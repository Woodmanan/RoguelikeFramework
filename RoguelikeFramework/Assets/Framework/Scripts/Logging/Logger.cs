using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Logger : MonoBehaviour
{
    private static List<String> logs = new List<string>();

    public static void Log(string msg)
    {
        logs.Add(msg);
        if (logs.Count > 50)
        {
            logs.RemoveRange(0, logs.Count - 50);
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
