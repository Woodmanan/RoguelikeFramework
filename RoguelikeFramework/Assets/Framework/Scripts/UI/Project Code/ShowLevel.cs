using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShowLevel : MonoBehaviour
{
    TextMeshProUGUI text;
    int cachedLevel = 1;

    // Start is called before the first frame update
    void Start()
    {
        text = GetComponent<TextMeshProUGUI>();
        text.text = "1";
    }

    // Update is called once per frame
    void Update()
    {
        if (Player.player)
        {
            if (cachedLevel != Player.player[0].level)
            {
                cachedLevel = Player.player[0].level;
                text.text = $"{cachedLevel}";
            }
        }
    }
}
