using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingPanel : MonoBehaviour
{
    public bool canMove = true;
    public float weight = 1;
    [HideInInspector] public Vector2 velocity;

    public Vector2 goal;

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
            Set();
        }
    }

    public void Set()
    {
        //Slight random offset, to prevent perfect overlaps
        if (weight <= .1f)
        {
            weight = 1;
            Debug.Log("Weight must be greater than .1");
        }

        rectTrans = GetComponent<RectTransform>();
        rect = rectTrans.rect;

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

    public Rect GenerateActualBounds()
    {
        Vector3[] corners = new Vector3[4];
        rectTrans.GetLocalCorners(corners);

        Vector2 min = (Vector2)corners[0];
        Vector2 max = (Vector2)corners[2];

        return new Rect((Vector2) rectTrans.localPosition, (max - min));
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
