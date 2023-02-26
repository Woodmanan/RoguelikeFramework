using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FloatingMessage : MonoBehaviour
{
    public RectTransform rectTransform;
    public Rigidbody2D rigid;
    public TargetJoint2D targetJoint;
    public TextMeshProUGUI body;

    [HideInInspector]
    public FloatingController controller;

    public bool usesRealWorldCoords;
    public Vector2 realWorldCoord;
    Camera cam;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(string message, Vector2 anchorPoint, float duration)
    {
        cam = Camera.main;
        targetJoint.target = anchorPoint;
        rectTransform.position = anchorPoint;
        body.text = message;
        StartCoroutine(CloseMessage(duration));
    }

    public void SetupRealWorld(string message, Vector2 anchorPoint, float duration)
    {
        cam = Camera.main;
        usesRealWorldCoords = true;
        Vector2 UIPos = cam.WorldToScreenPoint(anchorPoint);
        realWorldCoord = anchorPoint;
        rectTransform.position = UIPos;
        body.text = message;
        StartCoroutine(CloseMessage(duration));
    }

    private void LateUpdate()
    {
        if (usesRealWorldCoords)
        {
            Vector2 UIPos = cam.WorldToScreenPoint(realWorldCoord);
            targetJoint.target = UIPos;
        }
    }

    IEnumerator CloseMessage(float duration)
    {
        yield return new WaitForSeconds(duration);
        controller.RemoveMessage(this);
        Destroy(gameObject);
    }
}
