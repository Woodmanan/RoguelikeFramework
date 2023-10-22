using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static Resources;

public class MonsterUIController : MonoBehaviour
{
    [SerializeField] Canvas canvas;
    [SerializeField] RectTransform pipContainer;
    [SerializeField] GameObject PipIconPrefab;
    Monster connectedTo;

    [SerializeField] Image healthForeground;
    [SerializeField] Image manaForeground;

    Dictionary<Effect, PipIcon> heldEffects;

    // Start is called before the first frame update
    void Start()
    {
        heldEffects = new Dictionary<Effect, PipIcon>(4);
        ConnectToMonster(transform.parent.GetComponent<Monster>());
    }

    // Update is called once per frame
    void Update()
    {
        if (connectedTo == null)
        {
            return;
        }

        if (connectedTo.IsDead())
        {
            DisconnectFromMonster();
            gameObject.SetActive(false);
            return;
        }

        if (connectedTo.currentTile is RogueTile tile)
        {
            canvas.enabled = tile.isVisible;
        }

        healthForeground.fillAmount = (connectedTo.baseStats[HEALTH] / connectedTo.currentStats[MAX_HEALTH]);
        manaForeground.fillAmount = (connectedTo.baseStats[MANA] / connectedTo.currentStats[MAX_MANA]);
    }

    public void ConnectToMonster(Monster monster)
    {
        connectedTo = monster;
        monster.connections.OnApplyStatusEffects.AddListener(3000, OnEffectAdded);
        Update();
    }

    public void DisconnectFromMonster()
    {
        connectedTo.connections.OnApplyStatusEffects.RemoveListener(OnEffectAdded);
    }

    public void OnEffectAdded(ref Effect[] effects)
    {
        foreach (Effect effect in effects)
        {
            if (effect is PipEffect pip)
            {
                pip.SetUIController(this);
            }
        }
    }

    public void UpdateEffect(PipEffect effect, int delta)
    {
        heldEffects[effect].UpdateCount(delta);
    }

    public void AddEffect(PipEffect effect)
    {
        PipIcon pip = Instantiate(PipIconPrefab, pipContainer).GetComponent<PipIcon>();
        pip.SetupForEffect(effect);
        heldEffects.Add(effect, pip);
    }

    public void RemoveEffect(PipEffect effect)
    {
        PipIcon outIcon;
        if (heldEffects.TryGetValue(effect, out outIcon))
        {
            Destroy(outIcon.gameObject);
            heldEffects.Remove(effect);
        }
    }
}
