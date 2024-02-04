using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization;

public class StatBar : MonoBehaviour, IDescribable
{
    Monster player;
    Material mat;
    public Image barImage;
    TextMeshProUGUI text;

    public Resources main;
    public Resources max;
    public Gradient gradient;
    public bool alwaysShow;

    public LocalizedString LocName;
    public LocalizedString LocDescription;

    float currentFillAmount;
    // Start is called before the first frame update
    void Start()
    {
        mat = Instantiate<Material>(barImage.material);
        barImage.material = mat;
        currentFillAmount = 0;
    }

    // Update is called once per frame
    public void CheckForStats()
    {
        if (player == null)
        {
            player = Player.player;
        }

        if (player != null && mat != null)
        {
            if (player.currentStats[max] > 0 || alwaysShow)
            {
                if (!barImage.enabled)
                {
                    transform.parent.gameObject.SetActive(true);
                    barImage.enabled = true;
                }
                float goalFillAmount = player.baseStats[main] / Mathf.Max(player.currentStats[max], 0.001f);
                currentFillAmount = Mathf.Lerp(currentFillAmount, goalFillAmount, .05f);
                if (Mathf.Abs(goalFillAmount - currentFillAmount) < .005)
                {
                    currentFillAmount = goalFillAmount;
                }
                mat.SetFloat("_fillAmount", currentFillAmount);
                mat.SetColor("_fillColor", gradient.Evaluate(currentFillAmount));
            }
            else
            {
                if (barImage.enabled)
                {
                    barImage.enabled = false;
                    //transform.parent.gameObject.SetActive(false);
                }
            }
        }
    }

    public string GetDescription()
    {
        Dictionary<string, string> values = new Dictionary<string, string>();
        values.Add("current", Mathf.CeilToInt(Player.player[0].baseStats[main]).ToString());
        values.Add("max", Mathf.CeilToInt(Player.player[0].currentStats[max]).ToString());
        return LocDescription.GetLocalizedString(values);
    }

    public Sprite GetImage()
    {
        return null;
    }

    public string GetName(bool shorten = false)
    {
        Dictionary<string, string> values = new Dictionary<string, string>();
        values.Add("current", Mathf.CeilToInt(Player.player[0].baseStats[main]).ToString());
        values.Add("max", Mathf.CeilToInt(Player.player[0].currentStats[max]).ToString());
        return LocName.GetLocalizedString(values);
    }
}
