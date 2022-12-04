using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Condensed version of applyable and targetable items - both of those essentially just did
 * ability usage, but were completely limited to that domain. This fixes that problem by
 * just converting items to actually use abilites.
 * 
 * Items will no lnoger be able to be both consumed, fired, and targeted, but that was frankly
 * too much and was never used anyways. Having just two uses instead of three makes things a lot
 * more manageable, and should impose a healthy constraint on the design.
 */
[System.Flags]
public enum ActivateType
{
    Ability = 1 << 1,
    Effect  = 1 << 2
}
public class ActivatableItem : MonoBehaviour
{
    public ActivateType activateType;
    public Ability abilityOnActivation;
    [SerializeReference]
    public List<Effect> activationEffects;
    public bool ConsumedOnUse;

    // Start is called before the first frame update
    void Start()
    {
        this.enabled = false;
        if (abilityOnActivation)
        {
            abilityOnActivation = abilityOnActivation.Instantiate();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }


}
