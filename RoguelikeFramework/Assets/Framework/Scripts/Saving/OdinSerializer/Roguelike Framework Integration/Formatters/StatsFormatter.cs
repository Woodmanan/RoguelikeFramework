using OdinSerializer;
using System.Collections.Generic;
using System;

//Register two formatters so that players can serialze extra info!
[assembly: RegisterFormatter(typeof(StatsFormatter))]


namespace OdinSerializer
{
    using UnityEngine;

    public class StatsFormatter : MinimalBaseFormatter<Stats>
    {
        protected static readonly Serializer<Dictionary<global::Resources, float>> DictSerializer = Serializer.Get<Dictionary<global::Resources, float>>();
        protected static readonly Serializer<global::Resources> EnumSerializer = Serializer.Get<global::Resources>();
        protected static readonly Serializer<float> FloatSerializer = Serializer.Get<float>();
        protected static readonly Serializer<int> IntSerializer = Serializer.Get<int>();
        protected static readonly Serializer<string> StringSerializer = Serializer.Get<string>();


        /// <summary>
        /// Reads into the specified value using the specified reader.
        /// </summary>
        /// <param name="value">The value to read into.</param>
        /// <param name="reader">The reader to use.</param>
        protected override void Read(ref Stats value, IDataReader reader)
        {
            int count = IntSerializer.ReadValue(reader);
            value.dictionary = new Dictionary<global::Resources, float>();

            for (int i = 0; i < count; i++)
            {
                global::Resources key = global::Resources.HEALTH;
                if (RogueSaveSystem.isDebug)
                {
                    string name = StringSerializer.ReadValue(reader);
                    Enum.TryParse(name, out key);
                }
                else
                {
                    key = EnumSerializer.ReadValue(reader);
                }
                
                float val = FloatSerializer.ReadValue(reader);
                value.dictionary.Add(key, val);
            }
        }

        protected override void Write(ref Stats value, IDataWriter writer)
        {
            RogueSaveSystem.WriteValue("Dict Length", value.dictionary.Count, IntSerializer, writer);
            foreach (KeyValuePair<global::Resources, float> pair in value.dictionary)
            {
                if (RogueSaveSystem.isDebug)
                {
                    RogueSaveSystem.WriteValue("Key Name", pair.Key.ToString(), StringSerializer, writer);
                }
                else
                {
                    RogueSaveSystem.WriteValue("Key", pair.Key, EnumSerializer, writer);
                }
                RogueSaveSystem.WriteValue("Value", pair.Value, FloatSerializer, writer);
            }
        }
    }
}