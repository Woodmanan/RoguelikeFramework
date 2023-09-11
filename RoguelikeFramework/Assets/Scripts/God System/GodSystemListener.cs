using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using static Resources;

//[Group("Sample Group/Sample Subgroup")]
[Priority(10)]
public class GodSystemListener : Effect
{
    List<GodDefinition> watching;
    GodDefinition sponsor;
    List<GodScoringOperator> operators;
    /*public override string GetName(bool shorten = false) { return name.GetLocalizedString(this); }*/

    /*public override string GetDescription() { return description.GetLocalizedString(this); }*/

    /*public override Sprite GetImage() { return image; }*/

    /*public override bool ShouldDisplay() { return !name.IsEmpty && !description.IsEmpty; }*/

    /*public override string GetUISubtext() { return ""; }*/

    /*public override float GetUIFillPercent() { return 0.0f; }*/

    //Constuctor for the object; use this in code if you're not using the asset version!
    //Generally nice to include, just for future feature proofing
    public GodSystemListener()
    {
        //Construct me!
    }

    public void AttachGod(GodDefinition god)
    {
        foreach (GodScoringOperator scoring in god.scoring)
        {
            scoring.connectedMonster = connectedTo.monster;
        }
        operators.AddRange(god.scoring);
        watching.Add(god);
    }

    public void DetachGod(GodDefinition god)
    {
        foreach (GodScoringOperator scoring in god.scoring)
        {
            scoring.connectedMonster = null;
            operators.Remove(scoring);
        }
        watching.Remove(god);
    }

    public bool HasSponsor()
    {
        return sponsor != null;
    }

    public GodDefinition GetSponsor()
    {
        return sponsor;
    }

    public List<GodDefinition> GetWatching()
    {
        return watching;
    }

    //Called the moment an effect connects to a monster
    //Use this to apply effects or stats immediately, before the next frame
    public override void OnConnection()
    {
        watching = new List<GodDefinition>();
        operators = new List<GodScoringOperator>();
    }

    //Called when an effect gets disconnected from a monster
    /*public override void OnDisconnection() {} */

    //Called when an effect "Clashes" with an effect of the same type
    /*public override void OnStack(Effect other, ref bool addThisEffect) {} */

    //Called when a connected entity wants to localize - use this to insert information it might want.
    //public override void OnGenerateLocalizedString(ref Dictionary<string, object> arguments) {}

    //Called at the start of the global turn sequence
    //public override void OnTurnStartGlobal() {}

    //Called at the end of the global turn sequence
    public override void OnTurnEndGlobal()
    {
        foreach (GodScoringOperator scoring in operators)
        {
            scoring.OnTurnEnd();
        }
    }

    //Called at the start of a monster's turn
    //public override void OnTurnStartLocal() {}

    //Called at the end of a monster's turn
    //public override void OnTurnEndLocal() {}

    //Called whenever a monster wants to take a step.
    //public override void OnMoveInitiated(ref Vector2Int newLocation, ref bool canMove) {}

    //Called whenever a monster sucessfully takes a step.
    //public override void OnMove() {}

    //Called whenever a monster returns to full health
    //public override void OnFullyHealed() {}

    //Called after this monster has registered its death.
    public override void OnPostDeath(ref Monster killer)
    {
        foreach (GodScoringOperator scoring in operators)
        {
            scoring.OnKilled(killer);
        }

        for (int i = watching.Count - 1; i >= 0; i--)
        {
            watching[i].EvaluateAllCandidates();
        }
    }

    //Called when the connected monster dies
    //public override void OnDeath() {}

    //Called when a monster is killed by this unit.
    public override void OnKillMonster(ref Monster monster, ref DamageType type, ref DamageSource source)
    {
        foreach (GodScoringOperator scoring in operators)
        {
            scoring.OnKillMonster(monster);
        }
    }

    //Called often, whenever a monster needs up-to-date stats.
    //public override void RegenerateStats(ref Stats stats) {}

    //Called wenever a monster gains energy
    //public override void OnEnergyGained(ref int energy) {}

    //Called when a monster gets attacked (REWORKING SOON!)
    //public override void OnAttacked(ref int pierce, ref int accuracy) {}

    //Called by the dealer of damage, when applicable. Modifications here happen before damage is dealt.
    public override void OnDealDamage(ref float damage, ref DamageType damageType, ref DamageSource source)
    {
        foreach (GodScoringOperator scoring in operators)
        {
            scoring.OnDealDamage(damage, damageType, source);
        }
    }

    //Called when a monster takes damage from any source, good for making effects fire upon certain types of damage
    //public override void OnTakeDamage(ref float damage, ref DamageType damageType, ref DamageSource source) {}

    //Called when a monster recieves a healing event request
    //public override void OnHealing(ref float healAmount) {}

    //Called when new status effects are added. All status effects coming through are bunched together as a list.
    //public override void OnApplyStatusEffects(ref Effect[] effects) {}

    //Called when this monster attempts to activate an item.
    //public override void OnActivateItem(ref Item item, ref bool canContinue) {}

    //Called when a spell is cast. Modify spell, or set continue to false in order to cancel the action!
    //public override void OnCastAbility(ref AbilityAction action, ref bool canContinue) {}

    //Called when this monster gains resources. (Different from healing, but can give health)
    //public override void OnGainResources(ref Stats resources) {}

    //Called when this monster gains XP from any source.
    //public override void OnGainXP(ref float XPAmount) {}

    //Called when this monster levels up! Level CANNOT be modified.
    //public override void OnLevelUp(ref int Level) {}

    //Called when this monster loses resources. (Different from damage, but can take health)
    //public override void OnLoseResources(ref Stats resources) {}

    //Called when new status effects are added. All status effects coming through are bunched together as a list.
    //public override void OnRegenerateAbilityStats(ref Monster caster, ref AbilityStats abilityStats, ref Ability ability) {}

    //Called by spells, in order to determine whether they are allowed to be cast.
    //public override void OnCheckAvailability(ref Ability abilityToCheck, ref bool available) {}

    //Called by spells once targets are selected.
    //public override void OnTargetsSelected(ref Targeting targeting, ref Ability ability) {}

    //Called before spell is cast
    //public override void OnPreCast(ref Ability ability) {}

    //Called after a spell is cast.
    //public override void OnPostCast(ref Ability ability) {}

    //Called when this monster is selected to be hit by a cast. (Right before hit)
    //public override void OnTargetedByAbility(ref AbilityAction action) {}

    //Called after an ability is cast on this monster. (Right after hit)
    //public override void OnHitByAbility(ref AbilityAction action) {}

    //Called when this monster starts an attack action
    //public override void OnStartAttack(ref AttackAction action, ref bool canContinue) {}

    //Called when this monster is the primary target of an attack action.
    //public override void OnStartAttackTarget(ref AttackAction action, ref bool canContinue) {}

    //Called when an attack has collected the weapons that it will use.
    //public override void OnGenerateArmedAttacks(ref AttackAction attack, ref List<Weapon> primaryWeapons, ref List<Weapon> secondaryWeapons) {}

    //Called before a primary attack happens
    //public override void OnBeginPrimaryAttack(ref Weapon weapon, ref AttackAction action) {}

    //Called once a primary attack has generated a result. (Before result is used)
    //public override void OnPrimaryAttackResult(ref Weapon weapon, ref AttackAction action, ref AttackResult result) {}

    //Called after an attack has completely finished - results are final
    //public override void OnEndPrimaryAttack(ref Weapon weapon, ref AttackAction action, ref AttackResult result) {}

    //Called before a secondary attack happens
    //public override void OnBeginSecondaryAttack(ref Weapon weapon, ref AttackAction action) {}

    //Called once a primary attack has generated a result. (Before result is used)
    //public override void OnSecondaryAttackResult(ref Weapon weapon, ref AttackAction action, ref AttackResult result) {}

    //Called after a seconary attack has completely finished - results are final
    //public override void OnEndSecondaryAttack(ref Weapon weapon, ref AttackAction action, ref AttackResult result) {}

    //Called when an attack has collected the unarmed slots that it will use.
    //public override void OnGenerateUnarmedAttacks(ref AttackAction attack, ref List<EquipmentSlot> slots) {}

    //Called before an unarmed attack begins.
    //public override void OnBeginUnarmedAttack(ref EquipmentSlot slot, ref AttackAction action) {}

    //Called when an unarmed attack has a determined a result, before that result is used.
    //public override void OnUnarmedAttackResult(ref EquipmentSlot slot, ref AttackAction action, ref AttackResult result) {}

    //Called when an unarmed attack has a determined a result, after that result is used.
    //public override void OnEndUnarmedAttack(ref EquipmentSlot slot, ref AttackAction action, ref AttackResult result) {}

    //Called before this monster is hit by a primary attack from another monster.
    //public override void OnBeforePrimaryAttackTarget(ref Weapon weapon, ref AttackAction action, ref AttackResult result) {}

    //Called after this monster is hit by a primary attack from another monster. (Can't modify anymore)
    //public override void OnAfterPrimaryAttackTarget(ref Weapon weapon, ref AttackAction action, ref AttackResult result) {}

    //Called before this monster is hit by a secondary attack from another monster.
    //public override void OnBeforeSecondaryAttackTarget(ref Weapon weapon, ref AttackAction action, ref AttackResult result) {}

    //Called after this monster is hit by a secondary attack from another monster. (Can't modify anymore)
    //public override void OnAfterSecondaryAttackTarget(ref Weapon weapon, ref AttackAction action, ref AttackResult result) {}

    //Called before this monster is hit by an unarmed attack from another monster.
    //public override void OnBeforeUnarmedAttackTarget(ref EquipmentSlot slot, ref AttackAction action, ref AttackResult result) {}

    //Called after this monster is hit by an unarmed attack from another monster. (Can't modify anymore)
    //public override void OnAfterUnarmedAttackTarget(ref EquipmentSlot slot, ref AttackAction action, ref AttackResult result) {}

    //Called after this monster generates LOS, but before visible entity collection.
    //public override void OnGenerateLOSPreCollection(ref LOSData view) {}

    //Called after this monster generates LOS and visible entities.
    //public override void OnGenerateLOSPostCollection(ref LOSData view) {}


    //BEGIN CONNECTION
    public override void Connect(Connections c)
    {
        connectedTo = c;

        c.OnTurnEndGlobal.AddListener(10, OnTurnEndGlobal);

        c.OnPostDeath.AddListener(10, OnPostDeath);

        c.OnKillMonster.AddListener(10, OnKillMonster);

        c.OnDealDamage.AddListener(10, OnDealDamage);

        OnConnection();
    }
    //END CONNECTION

    //BEGIN DISCONNECTION
    public override void Disconnect()
    {
        OnDisconnection();

        connectedTo.OnTurnEndGlobal.RemoveListener(OnTurnEndGlobal);

        connectedTo.OnPostDeath.RemoveListener(OnPostDeath);

        connectedTo.OnKillMonster.RemoveListener(OnKillMonster);

        connectedTo.OnDealDamage.RemoveListener(OnDealDamage);

        ReadyToDelete = true;
    }
    //END DISCONNECTION
}
