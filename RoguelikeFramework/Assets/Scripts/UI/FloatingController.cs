using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingController : MonoBehaviour
{

    public static FloatingController Singleton;
    public static FloatingController singleton
    {
        get
        {
            if (!Singleton)
            {
                FloatingController controller = FindObjectOfType<FloatingController>();
                if (controller)
                {
                    Singleton = controller;
                }
                else
                {
                    UnityEngine.Debug.LogError("No Floating message controller found!");
                }
            }

            return Singleton;
        }
        set { Singleton = value; }
    }
    RectTransform rectTransform;
    Camera camera;

    public GameObject BasicMessagePrefab;

    List<FloatingMessage> messages = new List<FloatingMessage>();

    // Start is called before the first frame update
    void Start()
    {
        if (Singleton == null)
        {
            Singleton = this;
        }
        if (Singleton != this)
        {
            Destroy(gameObject);
            return;
        }
        camera = Camera.main;
        rectTransform = GetComponent<RectTransform>();

        #if !UNITY_EDITOR
        gameObject.SetActive(false);
        #endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void LateUpdate()
    {
        //rectTransform.sizeDelta = new Vector2(camera.scaledPixelWidth, camera.scaledPixelHeight);
    }

    public void AddBasicMessage(string text, Vector2 anchor)
    {
        if (!gameObject.activeSelf)
        {
            return;
        }
        FloatingMessage message = Instantiate(BasicMessagePrefab, transform).GetComponent<FloatingMessage>();
        message.controller = this;
        messages.Add(message);

        message.Setup(text, anchor, 5f);
    }

    public void AddWorldMessage(string text, Vector2 anchor)
    {
        if (!gameObject.activeSelf)
        {
            return;
        }
        FloatingMessage message = Instantiate(BasicMessagePrefab, transform).GetComponent<FloatingMessage>();
        message.controller = this;
        messages.Add(message);

        message.SetupRealWorld(text, anchor, 5f);
    }

    public void RemoveMessage(FloatingMessage message)
    {
        messages.Remove(message);
    }
}
