using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;

public class MenuUIButton : MonoBehaviour, IDescribable
{
    public LocalizedString LocName;
    public LocalizedString LocDescription;

    public PlayerAction actionOnClick;

    public string GetDescription()
    {
        return LocDescription.GetLocalizedString();
    }

    public Sprite GetImage()
    {
        return GetComponent<Image>().sprite;
    }

    public string GetName(bool shorten = false)
    {
        return LocName.GetLocalizedString();
    }

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(Click);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void Click()
    {
        if (RogueUIPanel.WindowsOpen)
        {
            RogueUIPanel.ExitAllWindows();
        }
        else
        {
            InputTracking.PushAction(actionOnClick);
        }
    }
}
