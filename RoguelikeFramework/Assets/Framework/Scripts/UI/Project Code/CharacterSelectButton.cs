using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CharacterSelectButton : MonoBehaviour
{
    [SerializeField] ClassGenerator generator;

    TextMeshProUGUI textbox;

    // Start is called before the first frame update
    void Start()
    {
        textbox = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        UpdateText();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateText()
    {
        textbox.text = $"Begin game as a {generator.GetCurrentChoiceName()}?";
    }
}
