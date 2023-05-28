using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Resources;
using System.Linq;
using UnityEngine.Localization;

[CreateAssetMenu(fileName = "New CorpseExplosion", menuName = "Abilities/Classes/Necromancer/CorpseExplosion", order = 1)]
public class CorpseExplosion : Ability
{
    [SerializeField]
    DamagePairing executeThreshold;
    [SerializeField]
    DamagePairing nonExecuteDamage;

    [SerializeField]
    DamagePairing explosionDamage;

    [SerializeField]
    LocalizedString explosionString;
    [SerializeField]
    LocalizedString failedExplosionString;

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

    public override bool IsValidTarget(Monster target)
    {
        return target.tags.MatchAnyTags(new RogueTag("Monster.Undead"), TagMatch.Parental);
    }

    public override void OnRegenerateStats(Monster caster)
    {
        
    }

    public override IEnumerator OnCast(Monster caster)
    {
        Vector2Int mainTargetPoint = targeting.points[0];
        Monster mainTarget = Map.current.GetTile(mainTargetPoint).currentlyStanding;

        int executeNumber = executeThreshold.damage.evaluate();

        if (mainTarget.baseStats[HEALTH] < executeNumber)
        {
            mainTarget.Damage(caster, 999, executeThreshold.type, DamageSource.ABILITY);

            if (mainTarget.baseStats[HEALTH] <= 0)
            {
                RogueLog.singleton.Log(explosionString.GetLocalizedString(), null, LogPriority.HIGH);
                targeting.affected.Remove(mainTarget);
                foreach (Monster target in targeting.affected)
                {
                    target.Damage(caster, explosionDamage.damage.evaluate(), explosionDamage.type, DamageSource.ABILITY);
                }
            }
        }
        else
        {
            RogueLog.singleton.Log(failedExplosionString.GetLocalizedString(), null, LogPriority.HIGH);
            mainTarget.Damage(caster, nonExecuteDamage.damage.evaluate(), nonExecuteDamage.type, DamageSource.ABILITY);
        }
        yield break;
    }
}
