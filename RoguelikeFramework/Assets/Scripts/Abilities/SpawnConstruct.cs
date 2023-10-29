using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "New SpawnConstruct", menuName = "Abilities/SpawnConstruct", order = 1)]
public class SpawnConstruct : Ability
{
    public Monster toSpawn;
    [SerializeReference] Effect constructEffect;
    int numToSpawn;
    bool sharesFaction;
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
        Vector2Int start = targeting.origin - Vector2Int.one * targeting.offset;
        List<Vector2Int> validLocations = new List<Vector2Int>();
        for (int i = 0; i < targeting.area.GetLength(0); i++)
        {
            for (int j = 0; j < targeting.area.GetLength(1); j++)
            {
                Vector2Int location = new Vector2Int(i, j) + start;
                if (!Map.current.ValidLocation(location)) continue;
                RogueTile tile = Map.current.GetTile(location);
                if (targeting.area[i,j] && !tile.BlocksMovement() && tile.currentlyStanding == null)
                {
                    validLocations.Add(location);
                }
            }
        }

        validLocations = validLocations.OrderBy(x => RogueRNG.Linear(0, 100000)).ToList();

        for (int c = 0; c < Mathf.Max(validLocations.Count, numToSpawn); c++)
        {
            SpawnAt(validLocations[c], caster);
        }

        yield break;
    }

    public void SpawnAt(Vector2Int location, Monster caster)
    {
        Monster spawned = MonsterSpawner.singleton.SpawnMonsterInstantiate(toSpawn, location, Map.current);
        if (sharesFaction)
        {
            spawned.faction = caster.faction;
        }
        else
        {
            spawned.faction = (Faction)~0;
        }
        spawned.energy = 100;

        Construct construct = (constructEffect.Instantiate()) as Construct;
        if (constructEffect == null)
        {
            Debug.LogError("Wrong type of effect! Spawn Construct MUST use construct effect type.");
            return;
        }
        construct.duration = Mathf.RoundToInt(currentStats[Resources.DURATION]);
        spawned.AddEffect(construct);
    }
}
