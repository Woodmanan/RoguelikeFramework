using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Priority(8)]
public class DamageResistance : Effect
{
    [SerializeField] int numTurns;
    //Constuctor for the object; use this in code if you're not using the asset version!
    public DamageResistance()
    {
        //Construct me!
        //this.numTurns = numTurns;
    }

    //Called the moment an effect connects to a monster
    //Use this to apply effects or stats immediately, before the next frame
    /*public override void OnFirstConnection() {}*/

    //Called at the start of the global turn sequence
    [Priority(10)]
    public override void OnTurnStartGlobal() 
    {
        numTurns--;
        if (numTurns == 0)
        {
            Disconnect();
        }
    }

    //Called at the end of the global turn sequence
    /*public override void OnTurnEndGlobal() {}*/

    //Called at the start of a monster's turn
    /*public override void OnTurnStartLocal() {}*/

    //Called at the end of a monster's turn
    /*public override void OnTurnEndLocal() {}*/

    //Called whenever a monster takes a step
    /*public override void OnMove() {}*/

    //Called whenever a monster returns to full health
    /*public override void OnFullyHealed() {}*/

    //Called when a monster dies
    /*public override void OnDeath() {}*/

    //Called when a monster is looking to recheck the stats, good for adding in variable stats mid-effect
    //it gains from effects and items
    /*public override void RegenerateStats(ref StatBlock stats) {}*/

    //Called whenever a monster gains energy
    /*public override void OnEnergyGained(ref int energy) {}*/

    //Called when a monster recieves an attack type request
    /*public override void OnAttacked(ref int pierce, ref int accuracy) {}*/

    //Called when a monster takes damage from any source, good for making effects fire upon certain types of damage
    public override void OnTakeDamage(ref int damage, ref DamageType damageType, ref DamageSource source)
    {
        damage = damage - 3;
    }

    //Called when a monster recieves a healing event request
    /*public override void OnHealing(ref int healAmount) {}*/

    //Callen when a monster recieves an event with new status effects
    /*public override void OnApplyStatusEffects(ref Effect[] effects) {}*/

    //BEGIN CONNECTION
    public override void Connect(Connections c)
    {
        connectedTo = c;

        c.OnTurnStartGlobal.AddListener(10, OnTurnStartGlobal);

        c.OnTakeDamage.AddListener(8, OnTakeDamage);

        OnConnection();
    }
    //END CONNECTION

    //BEGIN DISCONNECTION
    public override void Disconnect()
    {
        OnDisconnection();

        connectedTo.OnTurnStartGlobal.RemoveListener(OnTurnStartGlobal);

        connectedTo.OnTakeDamage.RemoveListener(OnTakeDamage);

        ReadyToDelete = true;
    }
    //END DISCONNECTION
}
