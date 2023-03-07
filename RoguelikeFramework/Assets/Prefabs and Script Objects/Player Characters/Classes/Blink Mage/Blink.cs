using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Blink", menuName = "Abilities/BlinkMage", order = 1)]
public class Blink : Ability
{
    public RandomNumber damage;
    [SerializeReference]
    Effect punishmentEffect;

    public bool landingZoneOnly;
	//Check activation, but for requirements that you are willing to override (IE, needs some amount of gold to cast)
    public override bool OnCheckActivationSoft(Monster caster)
    {
        if (targeting.range <= 0)
        {
            return false;
        }
        return true;
    }

    //Check activation, but for requirements that MUST be present for the spell to launch correctly. (Status effects will never override)
    public override bool OnCheckActivationHard(Monster caster)
    {
        return true;
    }

    public override IEnumerator OnCast(Monster caster)
    {
        Vector2Int goal = targeting.points[0];
        RogueTile goalTile = Map.current.GetTile(goal);
        if (landingZoneOnly)
        {
            if (goalTile.currentlyStanding)
            {
                goalTile.currentlyStanding.Damage(caster, damage.Evaluate(), DamageType.MAGICAL, DamageSource.ABILITY, "{name} is torn by the warping space");
            }
        }
        else
        {
            foreach (Monster hit in targeting.affected)
            {
                hit.Damage(caster, damage.Evaluate(), DamageType.MAGICAL, DamageSource.ABILITY, "{name} is torn by the warping space");
            }
        }

        if (goalTile.currentlyStanding == null)
        {
            caster.SetPositionSnap(goal);
            caster.UpdateLOS();
        }
        else
        {
            Debug.Log($"Console: The blink fails because {goalTile.currentlyStanding.GetLocalizedName()} is standing there.");
        }

        caster.AddEffect(punishmentEffect.Instantiate());
        yield break;
    }
}
