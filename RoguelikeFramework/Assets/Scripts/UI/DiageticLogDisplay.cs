using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class DiageticLogDisplay : MonoBehaviour
{
    List<GameObject> messages = new List<GameObject>();

    public GameObject basicMessage;

    public List<RectTransform> spawnAreas;

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
        Debug.Log($"Spawn point is {spawn}");
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
        float[] areas = spawnAreas.Select(x => x.rect.width * x.rect.height).ToArray();
        float area = areas.Sum();
        float choice = Random.Range(0, area);
        for (int i = 0; i < areas.Count(); i++)
        {
            if (choice < areas[i])
            {
                return GetRandomPointInRect(spawnAreas[i]);
            }
            choice -= areas[i];
        }

        return new Vector2(Screen.width, Screen.height) / 2;
    }

    private Vector2 GetRandomPointInRect(RectTransform rect)
    {
        Vector2 min = rect.anchorMin;
        Vector2 max = rect.anchorMax;

        Vector2 pos = new Vector2(Random.Range(min.x, max.x), Random.Range(min.y, max.y));

        Vector2 start = new Vector2(Screen.width, Screen.height) * pos;
        Debug.Log(start);


        return start;// + offset;
    }
}
