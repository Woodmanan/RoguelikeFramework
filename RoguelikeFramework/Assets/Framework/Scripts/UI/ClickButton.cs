using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ClickButton : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textbox;
    [SerializeField] private Image image;
    [SerializeField] private Image cooldownImage;
    [SerializeField] private TextMeshProUGUI cooldownText;
    private Button button;

    private bool selected = false;
    [HideInInspector] public int index;

    public bool clickable = true;

    private Color savedColor;

    public ClickDelegate onClick;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Setup(ClickDelegate onClick, int index)
    {
        selected = false;
        this.index = index;
        this.onClick = onClick;
        button = GetComponent<Button>();
    }

    public void SetDisplay(Sprite image, string description, Color color)
    {
        this.image.sprite = image;
        this.image.color = color;
        textbox.text = description;
        cooldownImage.sprite = image;
        float gray = this.image.color.grayscale;
        cooldownImage.color = new Color(gray, gray, gray);
        cooldownImage.fillAmount = 0f;
        cooldownText.text = "";
    }

    public void Select()
    {
        if (clickable)
        {
            selected = !selected;
        }
    }

    public void Disable()
    {
        if (clickable)
        {
            clickable = false;
            savedColor = image.color;
            float gray = image.color.grayscale;
            image.color = new Color(gray, gray, gray);
            button.interactable = false;
            //textbox.color = Color.gray;
        }
    }

    public void Enable()
    {
        if (!clickable)
        {
            clickable = true;
            image.color = savedColor;
            button.interactable = true;
            //textbox.color = Color.white;
        }
    }

    public void SetCooldown(Ability ability)
    {
        float cooldown = ability.currentStats[Resources.CURRENT_COOLDOWN];
        float max = ability.currentStats[Resources.COOLDOWN];

        SetCooldown(cooldown, Mathf.Max(0, max));
    }

    public void SetCooldown(float cooldown, float max)
    {
        if (cooldown == 0)
        {
            Enable();
            cooldownText.text = "";
            cooldownImage.fillAmount = 0;
        }
        else
        {
            Disable();
            image.color = savedColor;
            cooldownText.text = $"{cooldown}";
            if (max == 0)
            {
                cooldownImage.fillAmount = 1;
            }
            else
            {
                cooldownImage.fillAmount = ((float)cooldown) / max;
            }
        }
    }

    public void Click()
    {
        if (clickable)
        {
            onClick.Invoke(index);
        }
    }
}
