using OdinSerializer;
using System.Collections.Generic;
using System;
using UnityEngine.Localization;

//Register two formatters so that players can serialze extra info!
[assembly: RegisterFormatter(typeof(RogueTileFormatter))]


namespace OdinSerializer
{
    using UnityEngine;

    public class RogueTileFormatter : MinimalBaseFormatter<RogueTile>
    {
        protected static readonly Serializer<int> IntSerializer = Serializer.Get<int>();
        protected static readonly Serializer<string> StringSerializer = Serializer.Get<string>();
        protected static readonly Serializer<Guid> GuidSerializer = Serializer.Get<Guid>();
        protected static readonly Serializer<byte> byteSerializer = Serializer.Get<byte>();

        protected static readonly Serializer<Vector2Int> LocSerializer = Serializer.Get<Vector2Int>();
        protected static readonly Serializer<Visibility> VisibilitySerializer = Serializer.Get<Visibility>();
        protected static readonly Serializer<RogueHandle<Monster>> MonsterSerializer = Serializer.Get<RogueHandle<Monster>>();

        protected override void Read(ref RogueTile value, IDataReader reader)
        {
            value.location = LocSerializer.ReadValue(reader);
            value.graphicsVisibility = VisibilitySerializer.ReadValue(reader);
            value.currentlyStanding = MonsterSerializer.ReadValue(reader);
        }

        protected override void Write(ref RogueTile value, IDataWriter writer)
        {
            RogueSaveSystem.WriteValue("Location", value.location, LocSerializer, writer);
            RogueSaveSystem.WriteValue("Visibility", value.graphicsVisibility, VisibilitySerializer, writer);
            RogueSaveSystem.WriteValue("Standing", value.currentlyStanding, MonsterSerializer, writer);
            RogueSaveSystem.WriteValue("ID", (byte) 7, byteSerializer, writer);
        }
    }
}