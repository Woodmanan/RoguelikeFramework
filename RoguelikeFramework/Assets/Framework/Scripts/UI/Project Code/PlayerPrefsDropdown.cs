using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(TMP_Dropdown))]
public class PlayerPrefsDropdown : MonoBehaviour
{
    TMP_Dropdown dropdown;
    public string key;
    public int defaultValue;

    // Start is called before the first frame update
    void Start()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        dropdown.value = PlayerPrefs.GetInt(key, defaultValue);
        dropdown.onValueChanged.AddListener(UpdateValue);
    }

    void UpdateValue(int newValue)
    {
        PlayerPrefs.SetInt(key, newValue);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
