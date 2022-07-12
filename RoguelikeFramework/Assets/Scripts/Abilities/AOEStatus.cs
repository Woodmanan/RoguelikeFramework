using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "New AOEStatus", menuName = "Abilities/AOEStatus", order = 1)]
public class AOEStatus : Ability
{
    [SerializeReference] public List<Effect> toApply;
    [SerializeField] Sprite[] sprites;

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
        AnimationController.AddAnimation(new ExplosionAnimation(caster.location, targeting.radius, targeting, sprites));
        foreach (Monster m in targeting.affected)
        {
            foreach (Effect e in toApply)
            {
                Effect inst = e.Instantiate();
                inst.credit = caster;
                m.AddEffect(inst);
            }
        }
    }
}
