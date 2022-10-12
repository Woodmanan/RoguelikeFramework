using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingPanel : MonoBehaviour
{
    public bool canMove = true;
    public Vector2 size;
    public float weight = 1;
    [HideInInspector] public Vector2 velocity;

    [HideInInspector] public Vector2 goal;

    RectTransform rectTrans;

    [HideInInspector] public Rect rect;

    Coroutine moveRoutine;

    public float timeToMove = .2f;

    // Start is called before the first frame update
    void Start()
    {
        rectTrans = GetComponent<RectTransform>();
        if (!canMove)
        {
            rect = new Rect(rectTrans.anchorMin, (rectTrans.anchorMax - rectTrans.anchorMin));
            Debug.Log(rect + ": rect");
            FloatingPanelController.singleton.AddPanel(this);
        }
        else
        {
            Set(new Vector2(Random.value, Random.value));
        }
    }

    public void Set(Vector2 goalPos)
    {
        //Slight random offset, to prevent perfect overlaps
        goal = goalPos + ((new Vector2(Random.value, Random.value) - (Vector2.one / 2)) / 100);
        if (weight <= .1f)
        {
            weight = 1;
            Debug.Log("Weight must be greater than .1");
        }

        rectTrans = GetComponent<RectTransform>();
        rect = new Rect(goal - size / 2, size);
        rectTrans.anchorMin = rect.min;
        rectTrans.anchorMax = rect.max;

        FloatingPanelController.singleton.AddPanel(this);
    }

    public void UpdatePositionFromRect()
    {
        if (canMove)
        {
            if (moveRoutine != null)
            {
                StopAllCoroutines();
            }
            StartCoroutine(MoveToNewRect());
        }
    }

    public IEnumerator MoveToNewRect()
    {
        Rect oldRect = new Rect(rectTrans.anchorMin, (rectTrans.anchorMax - rectTrans.anchorMin));
        for (float t = 0; t < timeToMove; t += Time.deltaTime)
        {
            rectTrans.anchorMin = Vector2.Lerp(oldRect.min, rect.min, t / timeToMove);
            rectTrans.anchorMax = Vector2.Lerp(oldRect.max, rect.max, t / timeToMove);
            yield return null;
        }

        rectTrans.anchorMin = rect.min;
        rectTrans.anchorMax = rect.max;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnDestroy()
    {
        FloatingPanelController.singleton.RemovePanel(this);
    }
}
