using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "New HeatBurst", menuName = "Abilities/Heat Mage/HeatBurst", order = 1)]
public class HeatBurst : Ability
{
    public DamagePairing damage;
    public DamagePairing wallDamage;
    public int pushRange;
	//Check activation, but for requirements that you are willing to override (IE, needs some amount of gold to cast)
    /*public override bool OnCheckActivationSoft(Monster caster)
    {
        return true;
    }*/

    //Check activation, but for requirements that MUST be present for the spell to launch correctly. (Status effects will never override)
    /*public override bool OnCheckActivationHard(Monster caster)
    {
        return true;
    }*/

    /*public override void OnRegenerateStats(Monster caster)
    {
        
    }*/

    public override void OnCast(Monster caster)
    {
        List<Monster> byDist = targeting.affected.OrderByDescending(x => x.location.GameDistance(caster.location)).ToList();
        foreach (Monster m in targeting.affected)
        {
            Vector2 direction = m.location - caster.location;
            direction = direction.normalized * pushRange;

            Vector2Int step = new Vector2Int(Mathf.RoundToInt(direction.x), Mathf.RoundToInt(direction.y));
            List<Vector2Int> steps = Bresenham.GetPointsOnLine(m.location, m.location + step).ToList();

            bool hitWall = false;
            int dist = 1;

            //Start on our own spot, then start trying to push back.
            Vector2Int finalSpot = steps[0];
            foreach (Vector2Int nextStep in steps.Skip(1))
            {
                if (!Map.current.ValidLocation(nextStep)) break;
                RogueTile tile = Map.current.GetTile(nextStep);
                if (tile.BlocksMovement())
                {
                    hitWall = true;
                    break;
                }
                else if (tile.IsInteractable() || tile.currentlyStanding != null) break;
                finalSpot = nextStep;
                dist++;
            }

            m.SetPositionSnap(finalSpot);

            m.Damage(caster, damage.damage.evaluate(), damage.type, DamageSource.ABILITY);
            if (hitWall)
            {
                m.Damage(caster, dist * wallDamage.damage.evaluate(), damage.type, DamageSource.ABILITY);
            }
        }
    }
}
