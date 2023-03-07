using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "New TeleportOther", menuName = "Abilities/TeleportOther", order = 1)]
public class TeleportOther : Ability
{
    public int teleportRange;
    public Sprite[] sprites;
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

    public override IEnumerator OnCast(Monster caster)
    {
        
        List<Vector2Int> validLocations = new List<Vector2Int>();
        if (TeleportAnchor.anchor != null)
        {
            validLocations.AddRange(
                TeleportAnchor.anchor.GetValidLocations()
                      .OrderBy(x => RogueRNG.Linear(0, 100000)));
        }

        if (validLocations.Count < targeting.affected.Count)
        {
            validLocations.AddRange(
                Map.current.GetOpenLocationsAround(caster.location, teleportRange)
                           .OrderBy(x => RogueRNG.Linear(0, 100000)));
        }

        int count = Mathf.Min(validLocations.Count, targeting.affected.Count);

        //Prep anim for movement
        AnimationController.AddAnimation(new TeleportAnimation(targeting.affected.Take(count).ToList(), validLocations.Take(count).ToList(), caster.location, targeting.radius, sprites));

        //Move as many targets as we can
        for (int c = 0; c < count; c++)
        {
            targeting.affected[c].SetPositionNoGraphicsUpdate(validLocations[c]);
        }

        yield break;
    }
}
