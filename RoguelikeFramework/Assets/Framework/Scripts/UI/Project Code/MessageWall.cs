using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageWall : MonoBehaviour
{
    RectTransform rectTransform;
    BoxCollider2D boxCollider;

    Vector2 stored;

    // Start is called before the first frame update
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        if (stored != rectTransform.rect.size)
        {
            boxCollider.size = rectTransform.rect.size;
            stored = rectTransform.rect.size;
        }
    }
}
