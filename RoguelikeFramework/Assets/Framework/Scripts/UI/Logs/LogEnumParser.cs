using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;

public class LogEnumParser : MonoBehaviour
{
    public int everythingIndex;
    public UnityEvent<int> onValueChanged;
    int held;

    public RectTransform toggleParent;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetValue(int value)
    {
        held = value;
        for (int i = 0; i < toggleParent.childCount; i++)
        {
            Toggle toggle = toggleParent.GetChild(i).GetComponent<Toggle>();
            toggle.SetIsOnWithoutNotify((value & (1 << i)) > 0);
        }
    }

    public void OnValueSelected(int value)
    {
        held ^= value;
        onValueChanged.Invoke(held);
    }
}
