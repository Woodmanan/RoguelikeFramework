using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingPanelController : MonoBehaviour
{
    public static List<FloatingPanel> panels = new List<FloatingPanel>();

    public Quadtree<FloatingPanel> quadtree;

    PanelComparer comp = new PanelComparer();

    public static FloatingPanelController Singleton;
    public static FloatingPanelController singleton
    {
        get
        {
            if (!Singleton)
            {
                FloatingPanelController panel = GameObject.FindObjectOfType<FloatingPanelController>();
                if (panel)
                {
                    Singleton = panel;
                }
                else
                {
                    UnityEngine.Debug.LogError("No FloatingPanelController found!");
                }
            }
            return Singleton;
        }
        set { Singleton = value; }
    }

    public int maxIterations = 40;
    public float speedPerIteration = .02f;
    public float goalSpeed = 0.01f;
    public float staticPushMultiplier = 5;

    Coroutine routine;

    bool dirty = true;

    // Start is called before the first frame update
    void Start()
    {
        if (Singleton)
        {
            if (Singleton != this)
            {
                Destroy(this.gameObject);
                return;
            }
        }
        else
        {
            Singleton = this;
        }
    }

    public void AddPanel(FloatingPanel panel)
    {
        int index = panels.BinarySearch(panel, comp);
        if (index < 0) index = ~index;
        panels.Insert(index, panel);

        if (routine != null)
        {
            StopAllCoroutines();
        }
        dirty = true;
        Debug.Log($"There are {panels.Count} panels active.");
    }

    public void RemovePanel(FloatingPanel panel)
    {
        panels.Remove(panel);
        dirty = true;
    }

    public void Reset()
    {
        foreach (FloatingPanel p in panels)
        {
            if (p.canMove)
            {
                p.rect.center = p.goal;
            }
        }
    }

    public void Rebuild()
    {
        dirty = false;

        quadtree = new Quadtree<FloatingPanel>(new Rect(0, 0, Screen.width, Screen.height));

        for (int i = 0; i < panels.Count; i++)
        {
            FloatingPanel panel = panels[i];
            Debug.Log($"Working on panel {i} with weight {panel.weight} and dim {panel.GenerateActualBounds()}");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (dirty)
        {
            Reset();
            Rebuild();
        }
    }

    void OnDestroy()
    {
        panels.Clear();
    }

    public Vector2 FastestEscapeRoute(Rect box, Rect moving)
    {
        float aspect = box.width / (box.height + box.width);
        Vector2 x = new Vector2(moving.center.x - box.center.x, 0);
        Vector2 y = new Vector2(0, moving.center.y - box.center.y);
        return x * (1 - aspect) + y * aspect;
    }
}

public class PanelComparer : IComparer<FloatingPanel>
{
    public int Compare(FloatingPanel a, FloatingPanel b)
    {
        return b.weight.CompareTo(a.weight);
    }
}
