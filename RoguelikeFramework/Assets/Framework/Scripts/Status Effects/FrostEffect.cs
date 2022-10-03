using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Group("Elemental Effects/Cold")]
[Priority(10)]
public class FrostEffect : Effect
{
    [SerializeField] int numTurns;

    /* The default priority of all functions in this class - the order in which they'll be called
     * relative to other status effects
     * 
     * To override for individual functions, use the [Priority(int)] attribute 
     */

    //Constuctor for the object; use this in code if you're not using the asset version!
    //Generally nice to include, just for future feature proofing
    public FrostEffect()
    {
        //Construct me!
    }

    //Called the moment an effect connects to a monster
    //Use this to apply effects or stats immediately, before the next frame
    /*public override void OnConnection() {}*/

    //Called when an effect gets disconnected from a monster
    /*public override void OnDisconnection() {} */

    //Called at the start of the global turn sequence
    //public override void OnTurnStartGlobal() {}

    //Called at the end of the global turn sequence
    //public override void OnTurnEndGlobal() {}

    //Called at the start of a monster's turn
    public override void OnTurnStartLocal()
    {
        Debug.Log("Frost effect on the start of your turn!");
        numTurns--;
        if (numTurns == 0)
        {
            Debug.Log("Frost effect dcing!");
            Disconnect();
        }
    }

    //Called at the end of a monster's turn
    //public override void OnTurnEndLocal() {}

    //Called whenever a monster takes a step
    public override void OnMove()
    {
        connectedTo.monster?.Damage(credit, 1, DamageType.CUTTING, DamageSource.EFFECT);
    }

    //Called whenever a monster returns to full health
    //public override void OnFullyHealed() {}

    //Called when the connected monster dies
    //public override void OnDeath() {}

    //Called often, whenever a monster needs up-to-date stats.
    //public override void RegenerateStats(ref StatBlock stats) {}

    //Called wenever a monster gains energy
    public override void OnEnergyGained(ref int energy)
    {
        energy = energy / 2;
    }

    //Called when a monster gets attacked (REWORKING SOON!)
    //public override void OnAttacked(ref int pierce, ref int accuracy) {}

    //Called when a monster takes damage from any source, good for making effects fire upon certain types of damage
    //public override void OnTakeDamage(ref int damage, ref DamageType damageType) {}

    //Called when a monster recieves a healing event request
    //public override void OnHealing(ref int healAmount) {}

    //Called when new status effects are added. All status effects coming through are bunched together as a list.
    //public override void OnApplyStatusEffects(ref Effect[] effects) {}

    //BEGIN CONNECTION
    public override void Connect(Connections c)
    {
        connectedTo = c;

        c.OnTurnStartLocal.AddListener(10, OnTurnStartLocal);

        c.OnMove.AddListener(10, OnMove);

        c.OnEnergyGained.AddListener(10, OnEnergyGained);

        OnConnection();
    }
    //END CONNECTION

    //BEGIN DISCONNECTION
    public override void Disconnect()
    {
        OnDisconnection();

        connectedTo.OnTurnStartLocal.RemoveListener(OnTurnStartLocal);

        connectedTo.OnMove.RemoveListener(OnMove);

        connectedTo.OnEnergyGained.RemoveListener(OnEnergyGained);

        ReadyToDelete = true;
    }
    //END DISCONNECTION
}
