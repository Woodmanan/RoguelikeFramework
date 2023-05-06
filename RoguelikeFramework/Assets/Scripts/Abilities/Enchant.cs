using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enchant", menuName = "Abilities/Enchant", order = 1)]
public class Enchant : Ability
{
    public int numberToEnchant = 1;
    public ItemType allowedTypes = (ItemType) (-1);
    public RandomNumber EnchantmentToAdd;
    

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

    public override void OnRegenerateStats(Monster caster)
    {
        
    }

    public override IEnumerator OnCast(Monster caster)
    {
        List<int> indices = new List<int>();
        UIController.singleton.OpenInventorySelect((x) => x.held[0].CanAddEnchantment() && (x.type & allowedTypes) > 0, (x) => indices = x,  numberToEnchant);
        yield return new WaitUntil(() => !UIController.WindowsOpen);

        foreach (int index in indices)
        {
            caster.inventory[index].held[0].AddEnchantment(EnchantmentToAdd.Evaluate());
        }
    }
}
