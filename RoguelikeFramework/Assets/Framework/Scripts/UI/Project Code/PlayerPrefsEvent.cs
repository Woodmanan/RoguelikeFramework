using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerPrefsEvent : MonoBehaviour
{
    public string key;
    public int defaultValue;

    int value = -1;

    public UnityEvent<int> passEvent;

    // Start is called before the first frame update
    void Start()
    {
        value = PlayerPrefs.GetInt(key, defaultValue);
        Debug.Log("Starting value is " + value);
        passEvent.Invoke(value);
    }

    public void StoreAndPass(int newValue)
    {
        if (value == newValue) return;
        Debug.Log("Setting value to " + value);
        PlayerPrefs.SetInt(key, newValue);
        value = newValue;
        passEvent.Invoke(value);
    }
}
