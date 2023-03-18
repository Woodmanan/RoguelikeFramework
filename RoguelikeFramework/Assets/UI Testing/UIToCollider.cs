using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIToCollider : MonoBehaviour
{
    public static Transform parent;

    public RectTransform rectTransform;

    public bool tracks;
    public Vector2 targetPoint;
    public bool IsWorldSpace;

    [Range(0, 1)]
    public float lerpAmount = 1;

    GameObject pair;

    BoxCollider2D boxCollider;
    TargetJoint2D target;
    Rigidbody2D rigid;

    bool setup = false;

    Camera camera;

    CanvasScaler scaler;

    Vector2 cachedSize = Vector2.zero;
    Vector3 cachedPos = Vector2.zero;
    float cachedCamSize = 0;
    float cachedCamAspect = 0;

    // Start is called before the first frame update
    void Start()
    {
        Setup();
    }

    void Setup()
    {
        if (!setup)
        {
            
            rectTransform = transform as RectTransform;

            if (parent == null)
            {
                parent = GameObject.FindGameObjectWithTag("UICollision").transform; //Cached so it's amortized out
                if (parent == null)
                {
                    Debug.LogError("UI colliders require an object tagged with 'UICollision' to function!");
                    return;
                }
            }

            pair = new GameObject($"Collision pair : {gameObject.name}", typeof(BoxCollider2D), typeof(TargetJoint2D));
            pair.transform.parent = parent;
            boxCollider = pair.GetComponent<BoxCollider2D>();
            target = pair.GetComponent<TargetJoint2D>();
            rigid = pair.GetComponent<Rigidbody2D>();
            scaler = GetComponentInParent<CanvasScaler>();

            camera = Camera.main;

            target.enabled = tracks;

            { //Setup rigid
                rigid.gravityScale = 0;
                rigid.constraints = RigidbodyConstraints2D.FreezeRotation;
                if (!tracks)
                {
                    rigid.constraints = rigid.constraints | RigidbodyConstraints2D.FreezePosition;
                }    
            }

            UpdateCollider();

            setup = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        CheckColliderMatch();
        UpdateTarget();
    }

    void LateUpdate()
    {
        UpdateUIPosition();
    }


    public void CheckColliderMatch()
    {
        if (SizeChanged())
        {
            UpdateCollider();
        }
    }

    public void UpdateCollider()
    {
        Vector2 updatedSize = cachedSize / Screen.width;
        //Using scaler transform - x,y,z should all be equal here since rect width and height control the actual scale
        boxCollider.size = 2 * updatedSize * camera.orthographicSize * camera.aspect * scaler.transform.localScale.x;

        if (!tracks || !setup)
        {
            //Update Position
            Vector2 pos = ScreenToWorld(rectTransform.position);

            //TODO: Incorporate pivot into this calculation

            pair.transform.position = pos;
        }
    }

    public bool SizeChanged()
    {
        bool hasChanged = (cachedSize != rectTransform.rect.size || rectTransform.position != cachedPos || camera.aspect != cachedCamAspect || camera.orthographicSize != cachedCamSize);
        if (hasChanged)
        {
            cachedSize = rectTransform.rect.size;
            cachedCamAspect = camera.aspect;
            cachedCamSize = camera.orthographicSize;
            cachedPos = rectTransform.position;
            return true;
        }
        return false;
    }

    Vector2 ScreenToWorld(Vector2 screenPoint)
    {
        return camera.ScreenToWorldPoint(screenPoint);
    }

    Vector3 WorldToScreen(Vector3 position)
    {
        return camera.WorldToScreenPoint(position);
    }

    public void UpdateTarget()
    {
        if (tracks != target.enabled)
        {
            target.enabled = tracks;
        }

        if (tracks)
        {
            Vector2 point = targetPoint;
            if (!IsWorldSpace)
            {
                point = ScreenToWorld(point);
            }
            target.target = point;
        }
    }

    public void UpdateUIPosition()
    {
        if (tracks)
        {
            Vector2 screen = (Vector2)WorldToScreen(pair.transform.position);
            if (Vector2.Distance(rectTransform.position, screen) < 1)
            {
                rectTransform.position = screen;
            }
            else
            {
                rectTransform.position = Vector2.Lerp(rectTransform.position, screen, lerpAmount);
            }
            
        }
    }
}
