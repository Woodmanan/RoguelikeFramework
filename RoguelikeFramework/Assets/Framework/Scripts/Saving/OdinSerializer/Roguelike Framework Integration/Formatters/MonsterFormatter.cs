using OdinSerializer;
using System.Collections.Generic;
using UnityEngine.Localization;
using UnityEngine.AddressableAssets;

//Register two formatters so that players can serialze extra info!
[assembly: RegisterFormatter(typeof(MonsterFormatter<Monster>))]
[assembly: RegisterFormatter(typeof(PlayerFormatter))]


namespace OdinSerializer
{
    using UnityEngine;

    public class MonsterFormatter<T> : MinimalBaseFormatter<T> where T : Monster
    {
        protected static readonly Serializer<bool> BoolSerializer = Serializer.Get<bool>();
        protected static readonly Serializer<int> IntSerializer = Serializer.Get<int>();
        protected static readonly Serializer<float> FloatSerializer = Serializer.Get<float>();
        protected static readonly Serializer<string> StringSerializer = Serializer.Get<string>();
        protected static readonly Serializer<Stats> StatsSerializer = Serializer.Get<Stats>();
        protected static readonly Serializer<DamageType> DamageTypeSerializer = Serializer.Get<DamageType>();
        protected static readonly Serializer<Faction> FactionSerializer = Serializer.Get<Faction>();
        protected static readonly Serializer<RogueTagContainer> TagSerializer = Serializer.Get<RogueTagContainer>();
        protected static readonly Serializer<List<Effect>> EffectSerializer = Serializer.Get<List<Effect>>();
        protected static readonly Serializer<Vector2Int> Vec2IntSerializer = Serializer.Get<Vector2Int>();
        protected static readonly Serializer<Visibility> VisibilitySerializer = Serializer.Get<Visibility>();
        protected static readonly Serializer<LocalizedString> LocalStringSerializer = Serializer.Get<LocalizedString>();
        protected static readonly Serializer<AssetReference> SpawnSerializer = Serializer.Get<AssetReference>();
        protected static readonly Serializer<RogueHandle<Monster>> MonsterSerializer = Serializer.Get<RogueHandle<Monster>>();

        /// <summary>
        /// Reads into the specified value using the specified reader.
        /// </summary>
        /// <param name="value">The value to read into.</param>
        /// <param name="reader">The reader to use.</param>
        protected override void Read(ref T value, IDataReader reader)
        {
            //value.spawnParams = SpawnSerializer.ReadValue(reader);

            value.selfHandle = MonsterSerializer.ReadValue(reader);

            value.baseStats = StatsSerializer.ReadValue(reader);
            value.resistances = DamageTypeSerializer.ReadValue(reader);
            value.weaknesses = DamageTypeSerializer.ReadValue(reader);
            value.immunities = DamageTypeSerializer.ReadValue(reader);

            value.localName = LocalStringSerializer.ReadValue(reader);
            value.localDescription = LocalStringSerializer.ReadValue(reader);
            value.friendlyName = StringSerializer.ReadValue(reader);

            value.singular = BoolSerializer.ReadValue(reader);
            value.named = BoolSerializer.ReadValue(reader);
            value.nameRequiresPluralVerbs = BoolSerializer.ReadValue(reader);

            value.faction = FactionSerializer.ReadValue(reader);
            value.ID = IntSerializer.ReadValue(reader);

            value.tags = TagSerializer.ReadValue(reader);

            value.energy = FloatSerializer.ReadValue(reader);

            value.location = Vec2IntSerializer.ReadValue(reader);
            value.visionRadius = IntSerializer.ReadValue(reader);
            value.energyPerStep = IntSerializer.ReadValue(reader);

            value.effects = EffectSerializer.ReadValue(reader);

            value.XPFromKill = IntSerializer.ReadValue(reader);
            value.level = IntSerializer.ReadValue(reader);

            value.graphicsVisibility = VisibilitySerializer.ReadValue(reader);


            ReadExtraValues(ref value, reader);
        }

        /// <summary>
        /// Writes from the specified value using the specified writer.
        /// </summary>
        /// <param name="value">The value to write from.</param>
        /// <param name="writer">The writer to use.</param>
        protected override void Write(ref T value, IDataWriter writer)
        {
            //RogueSaveSystem.WriteValue("Spawn Params", value.spawnParams, SpawnSerializer, writer);

            RogueSaveSystem.WriteValue("Self Handle", value.selfHandle, MonsterSerializer, writer);

            RogueSaveSystem.WriteValue("Base Stats", value.baseStats, StatsSerializer, writer);
            RogueSaveSystem.WriteValue("Resistances", value.resistances, DamageTypeSerializer, writer);
            RogueSaveSystem.WriteValue("Weaknesses", value.weaknesses, DamageTypeSerializer, writer);
            RogueSaveSystem.WriteValue("Immunities", value.immunities, DamageTypeSerializer, writer);

            RogueSaveSystem.WriteValue("Local Name", value.localName, LocalStringSerializer, writer);
            RogueSaveSystem.WriteValue("Local Desc", value.localDescription, LocalStringSerializer, writer);
            RogueSaveSystem.WriteValue("Friendly Name", value.friendlyName, StringSerializer, writer);

            RogueSaveSystem.WriteValue("Singular", value.singular, BoolSerializer, writer);
            RogueSaveSystem.WriteValue("Named", value.named, BoolSerializer, writer);
            RogueSaveSystem.WriteValue("Requires Plural", value.nameRequiresPluralVerbs, BoolSerializer, writer);


            RogueSaveSystem.WriteValue("Faction", value.faction, FactionSerializer, writer);

            RogueSaveSystem.WriteValue("ID", value.ID, IntSerializer, writer);

            RogueSaveSystem.WriteValue("Tags", value.tags, TagSerializer, writer);

            RogueSaveSystem.WriteValue("Energy", value.energy, FloatSerializer, writer);

            RogueSaveSystem.WriteValue("Location", value.location, Vec2IntSerializer, writer);
            RogueSaveSystem.WriteValue("Vision Radius", value.visionRadius, IntSerializer, writer);
            RogueSaveSystem.WriteValue("Energy Per Step", value.energyPerStep, IntSerializer, writer);

            RogueSaveSystem.WriteValue("Effects", value.effects, EffectSerializer, writer);

            RogueSaveSystem.WriteValue("XP From Kill", value.XPFromKill, IntSerializer, writer);
            RogueSaveSystem.WriteValue("Level", value.level, IntSerializer, writer);

            RogueSaveSystem.WriteValue("G Visibility", value.graphicsVisibility, VisibilitySerializer, writer);

            WriteExtraValues(ref value, writer);
        }

        public virtual void ReadExtraValues(ref T value, IDataReader reader)
        {

        }

        public virtual void WriteExtraValues(ref T value, IDataWriter writer)
        {

        }
    }

    public class PlayerFormatter : MonsterFormatter<Player>
    {
        public override void ReadExtraValues(ref Player value, IDataReader reader)
        {
            value.energy = FloatSerializer.ReadValue(reader);
        }

        public override void WriteExtraValues(ref Player value, IDataWriter writer)
        {
            RogueSaveSystem.WriteValue("Energy", value.energy, FloatSerializer, writer);
        }
    }
}
