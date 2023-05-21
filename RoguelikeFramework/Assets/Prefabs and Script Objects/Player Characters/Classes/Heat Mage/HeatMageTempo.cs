using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using System.Linq;

[Group("Class Effects/Heat Mage")]
[Priority(0)]
public class HeatMageTempo : Effect
{
    [HideInInspector]
    public bool active;

    public RandomNumber lossPerTurn;
    public RandomNumber gainPerHit;

    public float fireDamageAfter;
    public float fireDamageUntil;
    public int fireDamageLow;
    public int fireDamageHigh;

    [Range(0, 100)]
    public float fireDamageConversion;
    [Range(0, 100)]
    public float fireDamageEfficiency;

    [Header("Overheat")]
    [HideInInspector]
    public bool isOverheating = false;
    public LocalizedString overheatDescription;
    public Sprite overheatImage;
    public float burnPerTurn;
    public int overheatMaxDuration;
    public int overheatCurrentDuration;

    [HideInInspector]
    public int cooldown = 0;
    public int maxCooldown;
    /* The default priority of all functions in this class - the order in which they'll be called
     * relative to other status effects
     * 
     * To override for individual functions, use the [Priority(int)] attribute 
     */
    //public override int priority { get { return 10; } }

    /*public overrde string GetName() { return name.GetLocalizedString(this); }*/

    public override string GetDescription()
    {
        if (isOverheating)
        {
            return overheatDescription.GetLocalizedString(this);
        }
        return base.GetDescription();
    }

    public override Sprite GetImage()
    {
        if (isOverheating)
        {
            return overheatImage;
        }
        return base.GetImage();
    }

    /* public override string GetDescription() { return description.GetLocalizedString(this); }*/

    public override string GetUISubtext()
    {
        if (isOverheating)
        {
            return overheatCurrentDuration.ToString();
        }
        return $"{cooldown}";
    }

    public override float GetUIFillPercent()
    {
        if (isOverheating)
        {
            return ((float)overheatCurrentDuration) / overheatMaxDuration;
        }
        return 0.0f;
    }

    //Constuctor for the object; use this in code if you're not using the asset version!
    //Generally nice to include, just for future feature proofing
    public HeatMageTempo()
    {
        //Construct me!
    }

    //Helper function - adds heat and resets the clock
    public void AddHeat(float amount)
    {
        connectedTo.monster.AddBaseStat(Resources.HEAT, amount);
        if (connectedTo.monster.view.visibleEnemies.Count > 0)
        {
            cooldown = maxCooldown;
        }
        active = true;
    }

    //Called the moment an effect connects to a monster
    //Use this to apply effects or stats immediately, before the next frame
    /*public override void OnConnection() {}*/

    //Called when an effect gets disconnected from a monster
    /*public override void OnDisconnection() {} */

    //Called when an effect "Clashes" with an effect of the same type
    public override void OnStack(Effect other, ref bool addThisEffect)
    {
        addThisEffect = false;
    }

    //Called at the start of the global turn sequence
    //public override void OnTurnStartGlobal() {}

    //Called at the end of the global turn sequence
    public override void OnTurnEndGlobal()
    {
        if (!isOverheating)
        {
            if (active)
            {
                cooldown--;
                if (cooldown <= 0)
                {
                    active = false;
                }
            }
            else
            {
                connectedTo.monster.AddBaseStat(Resources.HEAT, -lossPerTurn.Evaluate());
            }

            //Check for overheat condition
            if (connectedTo.monster.currentStats[Resources.HEAT] == connectedTo.monster.currentStats[Resources.MAX_HEAT])
            {
                isOverheating = true;
                overheatCurrentDuration = overheatMaxDuration;
                active = false;
                RogueLog.singleton.Log("You begin overheating!", priority: LogPriority.HIGH, display: LogDisplay.DANGER);
            }
        }
        else if (isOverheating)
        {
            connectedTo.monster.Damage(connectedTo.monster, burnPerTurn, DamageType.TRUE & DamageType.FIRE, DamageSource.EFFECT);
            connectedTo.monster.AddBaseStat(Resources.HEAT, -connectedTo.monster.currentStats[Resources.MAX_HEAT] / overheatMaxDuration);
            overheatCurrentDuration--;
            if (overheatCurrentDuration == 0)
            {
                isOverheating = false;
                active = false;
                cooldown = 0;
            }
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

    //Called when the connected monster dies
    //public override void OnDeath() {}

    //Called when a monster is killed by this unit.
    //public override void OnKillMonster(ref Monster monster, ref DamageType type, ref DamageSource source) {}

    //Called often, whenever a monster needs up-to-date stats.
    //public override void RegenerateStats(ref Stats stats) {}

    //Called wenever a monster gains energy
    //public override void OnEnergyGained(ref int energy) {}

    //Called when a monster gets attacked (REWORKING SOON!)
    //public override void OnAttacked(ref int pierce, ref int accuracy) {}

    //Called by the dealer of damage, when applicable. Modifications here happen before damage is dealt.
    //public override void OnDealDamage(ref float damage, ref DamageType damageType, ref DamageSource source) {}

    //Called when a monster takes damage from any source, good for making effects fire upon certain types of damage
    public override void OnTakeDamage(ref float damage, ref DamageType damageType, ref DamageSource source)
    {
        if (((damageType & DamageType.FIRE) > 0) && ((damageType & DamageType.TRUE) == 0))
        {
            float stored = damage * (fireDamageConversion/100) * (fireDamageEfficiency/100);
            damage *= (1 - (fireDamageConversion/100));
            if (stored > 0)
            {
                AddHeat(stored);
            }
        }

        if (((damageType & DamageType.ICE) > 0) && ((damageType & DamageType.TRUE) == 0))
        {
            AddHeat(-damage);
        }
    }

    //Called when a monster recieves a healing event request
    //public override void OnHealing(ref float healAmount) {}

    //Called when new status effects are added. All status effects coming through are bunched together as a list.
    //public override void OnApplyStatusEffects(ref Effect[] effects) {}

    //Called when this monster attempts to activate an item.
    //public override void OnActivateItem(ref Item item, ref bool canContinue) {}

    //Called when a spell is cast. Modify spell, or set continue to false in order to cancel the action!
    public override void OnCastAbility(ref AbilityAction action, ref bool canContinue)
    {
        AddHeat(1);
    }

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
    public override void OnEndUnarmedAttack(ref EquipmentSlot slot, ref AttackAction action, ref AttackResult result)
    {
        if (result == AttackResult.HIT && slot.type.Contains(EquipSlotType.PRIMARY_HAND))
        {
            AddHeat(gainPerHit.Evaluate());
        }


        if (connectedTo.monster.baseStats[Resources.HEAT] > fireDamageAfter)
        {
            float damageLerp = Mathf.InverseLerp(fireDamageAfter, fireDamageUntil, connectedTo.monster.baseStats[Resources.HEAT]);
            float damage = Mathf.Lerp(fireDamageLow, fireDamageHigh, damageLerp);
            action.target.Damage(connectedTo.monster, damage, DamageType.FIRE, DamageSource.EFFECT);
        }
    }

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

        c.OnTurnEndGlobal.AddListener(0, OnTurnEndGlobal);

        c.OnTakeDamage.AddListener(0, OnTakeDamage);

        c.OnCastAbility.AddListener(0, OnCastAbility);

        c.OnEndUnarmedAttack.AddListener(0, OnEndUnarmedAttack);

        OnConnection();
    }
    //END CONNECTION

    //BEGIN DISCONNECTION
    public override void Disconnect()
    {
        OnDisconnection();

        connectedTo.OnTurnEndGlobal.RemoveListener(OnTurnEndGlobal);

        connectedTo.OnTakeDamage.RemoveListener(OnTakeDamage);

        connectedTo.OnCastAbility.RemoveListener(OnCastAbility);

        connectedTo.OnEndUnarmedAttack.RemoveListener(OnEndUnarmedAttack);

        ReadyToDelete = true;
    }
    //END DISCONNECTION
}
