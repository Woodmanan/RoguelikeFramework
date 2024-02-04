using OdinSerializer;
using System.Collections.Generic;
using System;

//Register two formatters so that players can serialze extra info!
[assembly: RegisterFormatter(typeof(RogueTagFormatter))]
[assembly: RegisterFormatter(typeof(RogueTagContainerFormatter))]


namespace OdinSerializer
{
    using UnityEngine;

    public class RogueTagFormatter : MinimalBaseFormatter<RogueTag>
    {

        protected static readonly Serializer<string> StringSerializer = Serializer.Get<string>();


        /// <summary>
        /// Reads into the specified value using the specified reader.
        /// </summary>
        /// <param name="value">The value to read into.</param>
        /// <param name="reader">The reader to use.</param>
        protected override void Read(ref RogueTag value, IDataReader reader)
        {
            value = new RogueTag(StringSerializer.ReadValue(reader));
        }

        protected override void Write(ref RogueTag value, IDataWriter writer)
        {
            RogueSaveSystem.WriteValue("Tag", value.GetHumanName(), StringSerializer, writer);
        }
    }

    public class RogueTagContainerFormatter : MinimalBaseFormatter<RogueTagContainer>
    {
        protected static readonly Serializer<int> IntSerializer = Serializer.Get<int>();
        protected static readonly Serializer<RogueTag> TagSerializer = Serializer.Get<RogueTag>();

        protected override void Read(ref RogueTagContainer value, IDataReader reader)
        {
            int length = IntSerializer.ReadValue(reader);
            value = new RogueTagContainer();

            for (int i = 0; i < length; i++)
            {
                RogueTag tag = TagSerializer.ReadValue(reader);
                int count = IntSerializer.ReadValue(reader);
                value.counts.Add(tag, count);
            }
        }

        protected override void Write(ref RogueTagContainer value, IDataWriter writer)
        {
            RogueSaveSystem.WriteValue("Length", value.counts.Count, IntSerializer, writer);
            foreach (KeyValuePair<RogueTag, int> pair in value.counts)
            {
                RogueSaveSystem.WriteValue("Tag", pair.Key, TagSerializer, writer);
                RogueSaveSystem.WriteValue("Count", pair.Value, IntSerializer, writer);
            }
        }
    }
}