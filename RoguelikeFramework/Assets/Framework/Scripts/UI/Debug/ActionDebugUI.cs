using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ActionDebugUI : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI text;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        text.text = Player.player.GetActionDebug();
    }
}
