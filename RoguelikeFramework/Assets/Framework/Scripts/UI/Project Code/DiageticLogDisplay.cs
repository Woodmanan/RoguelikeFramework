using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class DiageticLogDisplay : MonoBehaviour
{
    List<GameObject> messages = new List<GameObject>();

    public GameObject basicMessage;

    public RectTransform spawnAreas;
    int currentArea = -1;

    public float defaultShowTime;

    RectTransform canvas;
    Camera camera;

    // Start is called before the first frame update
    void Start()
    {
        canvas = GetComponentInParent<Canvas>().transform as RectTransform;
        RogueLog.singleton.OnDisplayLogAdded += OnShouldDisplayMessage;
    }

    // Update is called once per frame
    void Update()
    {
        if (camera == null)
        {
            camera = Camera.main;
        }
    }

    void OnShouldDisplayMessage(ref RogueLogMessage message)
    {
        GameObject newMessage = Instantiate(basicMessage, transform);
        newMessage.GetComponentInChildren<TextMeshProUGUI>().text = message.message;
        Vector2 spawn = GetSpawnPoint();
        newMessage.GetComponent<RectTransform>().position = spawn ;
        newMessage.GetComponent<UIToCollider>().targetPoint = spawn;
        StartCoroutine(ShowMessageForXSeconds(newMessage, defaultShowTime));
    }

    IEnumerator ShowMessageForXSeconds(GameObject message, float showTime)
    {
        messages.Add(message);
        yield return new WaitForSeconds(showTime);
        messages.Remove(message);
        Destroy(message);
    }

    private Vector2 GetSpawnPoint()
    {
        currentArea = (currentArea + 1) % spawnAreas.childCount;
        return GetRandomPointInRect(spawnAreas.GetChild(currentArea) as RectTransform);
    }

    private Vector2 GetRandomPointInRect(RectTransform rect)
    {
        Vector2 min = rect.anchorMin;
        Vector2 max = rect.anchorMax;

        Vector2 pos = new Vector2(Random.Range(min.x, max.x), Random.Range(min.y, max.y));

        Vector2 start = new Vector2(Screen.width, Screen.height) * pos;

        return start;// + offset;
    }
}
