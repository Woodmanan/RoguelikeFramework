using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Blink", menuName = "Abilities/BlinkMage", order = 1)]
public class Blink : Ability
{
    public RandomNumber damage;
    [SerializeReference]
    Effect punishmentEffect;
	//Check activation, but for requirements that you are willing to override (IE, needs some amount of gold to cast)
    public override bool OnCheckActivationSoft(Monster caster)
    {
        return true;
    }

    //Check activation, but for requirements that MUST be present for the spell to launch correctly. (Status effects will never override)
    public override bool OnCheckActivationHard(Monster caster)
    {
        return true;
    }

    public override void OnCast(Monster caster)
    {
        foreach (Monster hit in targeting.affected)
        {
            hit.Damage(caster, damage.Evaluate(), DamageType.PIERCING, DamageSource.ABILITY, "{name} is torn by the warping space");
        }

        Vector2Int goal = targeting.points[0];
        RogueTile goalTile = Map.current.GetTile(goal);
        if (goalTile.currentlyStanding == null)
        {
            caster.SetPositionSnap(goal);
        }
        else
        {
            Debug.Log($"Console: The blink fails because {goalTile.currentlyStanding.displayName} is standing there.");
        }

        caster.AddEffect(punishmentEffect.Instantiate());
    }
}
