using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "New Monster Spawn", menuName = "Spawning/New Monster", order = 2)]
public class MonsterSpawnParams : ScriptableObject
{
    public AssetReference baseUnityObject;

    [ResourceGroup(ResourceType.Monster)]
    public Stats baseStats;

    public DamageType resistances;
    public DamageType weaknesses;
    public DamageType immunities;

    [Header("Logging controls")]
    public LocalizedString localName;
    public LocalizedString localDescription;
    public string friendlyName;
    [Tooltip("Controls what kind of verbs are used. 'You cast' (true) vs 'It casts' (false)")]
    public bool singular;
    [Tooltip("Is this a generic insance of a monster? 'The goblin' (true) vs 'Grim Timothy' (false)")]
    public bool named;
    public bool nameRequiresPluralVerbs; //Useful for the player!

    public Faction faction = Faction.STANDARD;
    public int ID;
    public int minDepth;
    public int maxDepth;

    public RogueTagContainer tags = new RogueTagContainer();

    [SerializeReference]
    public Effect[] baseEffects;

    public int visionRadius;

    public int energyPerStep;

    public Sprite sprite;

    public int XPFromKill;

    public int level;

    public List<Item> baseItems;

    public List<Ability> baseAbilities;

    public RogueHandle<Monster> SpawnMonster()
    {
        return SpawnMonster_Internal<Monster>();
    }

    public RogueHandle<Monster> SpawnPlayer()
    {
        return SpawnMonster_Internal<Player>();
    }

    private RogueHandle<Monster> SpawnMonster_Internal<T>() where T : Monster
    {
        //Todo: move this into the monster code, so the unity setup happens after serialization is done - allows more async loads
        UnityMonster unityMonster = baseUnityObject.InstantiateAsync().WaitForCompletion().GetComponent<UnityMonster>();
        unityMonster.name = friendlyName;
        RogueHandle<Monster> monsterHandle = RogueHandle<Monster>.Create<T>();

        unityMonster.monsterHandle = monsterHandle;

        //TODO: Add self as addressable! Figure out how to do this

        Monster monster = monsterHandle.value;
        monster.selfHandle = monsterHandle;

        monster.baseStats = baseStats.Copy();
        monster.resistances = resistances;
        monster.weaknesses = weaknesses;
        monster.immunities = immunities;

        monster.localName = localName;
        monster.localDescription = localDescription;
        monster.friendlyName = friendlyName;

        monster.singular = singular;
        monster.named = named;
        monster.nameRequiresPluralVerbs = nameRequiresPluralVerbs;
        monster.faction = faction;
        monster.ID = ID;
        monster.tags = tags.Copy();

        monster.visionRadius = visionRadius;
        monster.energyPerStep = energyPerStep;
        monster.XPFromKill = XPFromKill;
        monster.level = level;

        monster.unity = unityMonster;

        //Perform monster setup
        monster.Setup();

        //Todo: Initialize Inventory

        //TODO: Initiliaze Equipment

        //TODO: Initialize Abilities

        unityMonster.renderer.sprite = sprite;

        monster.AddEffectInstantiate(baseEffects);

        return monsterHandle;
    }
}
