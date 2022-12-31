using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AbilityButton : MonoBehaviour
{
    public int index;
    public Image backgroundImage;
    public Image mask;
    public Image cooldownImage;
    public TextMeshProUGUI cooldownText;
    public TextMeshProUGUI numberText;
    public ExamineDescription description;

    public bool locked = true;
    public bool set = false;

    // Start is called before the first frame update
    void Start()
    {
        float blendAmount = locked ? 1 : 0;
        mask.material = Instantiate(mask.material);
        backgroundImage.material = Instantiate(backgroundImage.material);

        mask.material.SetFloat("_blendAmount", blendAmount);
        backgroundImage.material.SetFloat("_blendAmount", blendAmount);

        mask.SetMaterialDirty();
        cooldownImage.enabled = false;
        cooldownText.enabled = false;
        numberText.text = $"{(index + 1)%10}";
    }

    // Update is called once per frame
    void Update()
    {
        //Check for setup
        if (!set)
        {
            if (Player.player != null && Player.player.abilities.HasAbility(index))
            {
                backgroundImage.sprite = Player.player.abilities[index].image;
                backgroundImage.color = Player.player.abilities[index].color;
                description.locName = Player.player.abilities[index].locName;
                description.locDescription = Player.player.abilities[index].locDescription;
                set = true;
            }
            else
            {
                return;
            }
        }
        

        if (locked && Player.player.level >= (index + 1))
        {
            locked = false;
            StartCoroutine(UnlockShift(1f));
        }

        Ability ability = Player.player.abilities[index];

        if (ability.castable)
        {
            cooldownImage.enabled = false;
        }
        else
        {
            cooldownImage.enabled = true;
            cooldownImage.fillAmount = 1;
        }

        if (ability.currentCooldown > 0)
        {
            cooldownImage.fillAmount = (ability.currentCooldown / ability.currentStats[AbilityResources.MAX_COOLDOWN]);
            cooldownText.enabled = true;
            cooldownText.text = $"{ability.currentCooldown}";
        }
        else
        {
            cooldownText.enabled = false;
        }
    }

    IEnumerator UnlockShift(float time)
    {
        float blendAmount = 1;
        for (float t = 0; t < time; t+= Time.deltaTime)
        {
            blendAmount = 1 - (t / time);
            mask.materialForRendering.SetFloat("_blendAmount", blendAmount);
            backgroundImage.material.SetFloat("_blendAmount", blendAmount);
            Color color = Color.white * (t / time);
            color.a = 1;
            backgroundImage.material.SetColor("_fillColor", color);
            mask.SetMaterialDirty();
            yield return null;
        }

        mask.materialForRendering.SetFloat("_blendAmount", blendAmount);
        backgroundImage.material.SetFloat("_blendAmount", 0);
        mask.SetMaterialDirty();
    }

    public void Cast()
    {
        Player.player.SetAction(new AbilityAction(index));
    }
}
