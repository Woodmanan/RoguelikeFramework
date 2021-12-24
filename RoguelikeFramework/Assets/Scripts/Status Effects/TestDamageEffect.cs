using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New TestDamageEffect", menuName = "Status Effects/TestDamageEffect", order = 1)]
[EffectGroup("Damaging Effects")]
public class TestDamageEffect : Effect
{
    [SerializeField] int damagePerTurn;
    [SerializeField] int numTurns;

    Monster target;

    public override int priority { get { return 8; } }

    //Constuctor for the object; use this in code if you're not using the asset version!
    public TestDamageEffect(int damagePerTurn, int numTurns)
    {
        this.damagePerTurn = damagePerTurn;
        this.numTurns = numTurns;
    }

    //Called the moment an effect connects to a monster
    //Use this to apply effects or stats immediately, before the next frame
    public override void OnConnection()
    {
        Debug.Log("ConnectedTo is null?");
        target = connectedTo.monster;
        if (target == null)
        {
            Debug.Log("No monster, so I quit!");
            Disconnect();
        }
    }

    //Called at the start of the global turn sequence
    [Priority(6)]
    public override void OnTurnStartLocal() 
    {
        target.TakeDamage(damagePerTurn, DamageType.PIERCING); //Ah yes, potion of piercing damage
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
    /*public override void OnAttacked(ref int pierce, ref int accuracy, ref int damage) {}*/

    //Called when a monster recieves a healing event request
    /*public override void OnHealing(ref int healAmount) {}*/

    //Callen when a monster recieves an event with new status effects
    /*public override void OnApplyStatusEffects(ref Effect[] effects) {}*/
}
