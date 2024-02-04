using OdinSerializer;
using System.Collections.Generic;
using System;
using UnityEngine.AddressableAssets;

//Register two formatters so that players can serialze extra info!
[assembly: RegisterFormatter(typeof(AssetReferenceFormatter))]


namespace OdinSerializer
{
    using UnityEngine;

    public class AssetReferenceFormatter : MinimalBaseFormatter<AssetReference>
    {
        protected static readonly Serializer<long> LongSerializer = Serializer.Get<long>();
        protected static readonly Serializer<string> StringSerializer = Serializer.Get<string>();
        protected static readonly Serializer<Guid> GuidSerializer = Serializer.Get<Guid>();

        protected override void Read(ref AssetReference value, IDataReader reader)
        {
            value = new AssetReference(GuidSerializer.ReadValue(reader).ToString("N"));
        }

        protected override void Write(ref AssetReference value, IDataWriter writer)
        {
            //RogueSaveSystem.WriteValue("Guid", value.AssetGUID, StringSerializer, writer);
            RogueSaveSystem.WriteValue("Guid", new Guid(value.AssetGUID), GuidSerializer, writer);
        }

    }
}