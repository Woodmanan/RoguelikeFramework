using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SingleResourceBar : MonoBehaviour
{
    Monster player;
    Material mat;
    Image image;

    public Color fillColor;
    public Resources resource;

    float currentFillAmount;

    public TextMeshProUGUI valueText;
    public TextMeshProUGUI percentText;

    public int baseAmount;
    public int raisedExponent;

    public bool canBeNegative;

    // Start is called before the first frame update
    void Start()
    {
        image = transform.GetComponent<Image>();
        mat = Instantiate<Material>(image.material);
        image.material = mat;
        mat.SetColor("_fillColor", fillColor);
        currentFillAmount = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
        {
            player = Player.player;
        }

        if (player != null && mat != null)
        {
            float amount = player.currentStats[resource];

            float goalFillAmount = (1 - 1 / Mathf.Pow(baseAmount, amount / raisedExponent));
            if (amount < 0 && !canBeNegative)
            {
                goalFillAmount = 0;
            }
            currentFillAmount = Mathf.Lerp(currentFillAmount, Mathf.Clamp(Mathf.Abs(goalFillAmount), 0, 1), .05f);
            if (Mathf.Abs(goalFillAmount - currentFillAmount) < .005)
            {
                currentFillAmount = goalFillAmount;
            }

            mat.SetFloat("_fillAmount", currentFillAmount);
            valueText.text = amount.ToString("0.0");
            percentText.text = $"{(goalFillAmount * 100).ToString("0.0")}%";
        }
    }
}
