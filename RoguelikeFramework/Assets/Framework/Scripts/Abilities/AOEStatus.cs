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
    public override bool OnCheckActivationSoft(RogueHandle<Monster> caster)
    {
        return true;
    }

    //Check activation, but for requirements that MUST be present for the spell to launch correctly. (Status effects will never override)
    public override bool OnCheckActivationHard(RogueHandle<Monster> caster)
    {
        return true;
    }

    public override IEnumerator OnCast(RogueHandle<Monster> caster)
    {
        AnimationController.AddAnimationSolo(new ExplosionAnimation(caster.value.location, targeting.radius, targeting, sprites));
        foreach (RogueHandle<Monster> m in targeting.affected)
        {
            Monster monster = m.value;
            foreach (Effect e in toApply)
            {
                Effect inst = e.Instantiate();
                inst.credit = caster;
                monster.AddEffectInstantiate(inst);
            }
        }
        yield break;
    }
}
