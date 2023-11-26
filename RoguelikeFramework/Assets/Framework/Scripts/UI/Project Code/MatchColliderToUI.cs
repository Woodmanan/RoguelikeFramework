using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MatchColliderToUI : MonoBehaviour
{
    Vector2 currentDimensions;
    BoxCollider2D boxCollider;

    RectTransform rTransform;
    // Start is called before the first frame update
    void Start()
    {
        rTransform = transform as RectTransform;
        boxCollider = GetComponent<BoxCollider2D>();
        FixCollider();
    }

    // Update is called once per frame
    void Update()
    {
        FixCollider();
    }

    void FixCollider()
    {
        //Extract worldspace corners
        Vector3[] worldCorners = new Vector3[4];
        rTransform.GetWorldCorners(worldCorners);

        Vector3 size = worldCorners[2] - worldCorners[0];
        Vector2 pivotCorrection = (Vector2.one / 2) - rTransform.pivot;
        Vector2 pos = new Vector2(size.x * pivotCorrection.x, size.y * pivotCorrection.y);

        boxCollider.size = size;
        boxCollider.offset = pos;

        currentDimensions = size;
    }
}
