using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingPanelController : MonoBehaviour
{
    public static List<FloatingPanel> panels = new List<FloatingPanel>();

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
        panels.Add(panel);
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

        for (int count = 0; count < maxIterations; count++)
        {
            bool hasMoves = false;
            foreach (FloatingPanel panel in panels)
            {
                if (panel.canMove)
                {
                    panel.velocity = Vector2.zero;
                    foreach (FloatingPanel other in panels)
                    {
                        if (other.rect.Overlaps(panel.rect) && other != panel)
                        {

                            Vector2 offset = FastestEscapeRoute(other.rect, panel.rect);
                            if (!other.canMove)
                            {
                                offset *= staticPushMultiplier;
                            }
                            panel.velocity += offset;
                        }
                    }

                    if (panel.velocity.magnitude > 0)
                    {
                        panel.velocity = panel.velocity.normalized * speedPerIteration / panel.weight;
                        panel.velocity += (panel.goal - panel.rect.center).normalized * goalSpeed / panel.weight;
                        hasMoves = true;
                    }

                    if (panel.rect.min.x < 0 || panel.rect.min.y < 0 || panel.rect.max.x > 1 || panel.rect.max.y > 1)
                    {
                        panel.velocity += ((Vector2.one / 2) - panel.rect.center).normalized * speedPerIteration;
                        hasMoves = true;
                    }
                }
            }

            if (!hasMoves)
            {
                break;
            }

            foreach (FloatingPanel panel in panels)
            {
                if (panel.canMove)
                {
                    panel.rect.position += panel.velocity;
                }
            }
        }

        foreach (FloatingPanel panel in panels)
        {
            panel.UpdatePositionFromRect();
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
