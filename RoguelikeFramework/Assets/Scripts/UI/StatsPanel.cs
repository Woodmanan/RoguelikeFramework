using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsPanel : MonoBehaviour
{
    List<StatBar> statBars;

    // Start is called before the first frame update
    void Start()
    {
        statBars = new List<StatBar>();
        for (int i = 0; i < transform.childCount; i++)
        {
            StatBar bar = transform.GetChild(i).GetComponent<StatBar>();
            if (bar)
            {
                statBars.Add(bar);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (StatBar bar in statBars)
        {
            bar.CheckForStats();
        }
    }
}
