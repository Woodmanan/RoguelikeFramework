using OdinSerializer;
using System.Collections.Generic;
using System;
using UnityEngine.Localization;

//Register two formatters so that players can serialze extra info!
[assembly: RegisterFormatter(typeof(LocalizedStringFormatter))]


namespace OdinSerializer
{
    using UnityEngine;

    public class LocalizedStringFormatter : MinimalBaseFormatter<LocalizedString>
    {
        protected static readonly Serializer<long> LongSerializer = Serializer.Get<long>();
        protected static readonly Serializer<string> StringSerializer = Serializer.Get<string>();
        protected static readonly Serializer<Guid> GuidSerializer = Serializer.Get<Guid>();


        protected override void Read(ref LocalizedString value, IDataReader reader)
        {
            UnityEngine.Localization.Tables.TableReference tableReference;
            if (RogueSaveSystem.isDebug)
            {
                tableReference = StringSerializer.ReadValue(reader);
            }
            else
            {
                tableReference = GuidSerializer.ReadValue(reader);
            }

            UnityEngine.Localization.Tables.TableEntryReference entryReference = LongSerializer.ReadValue(reader);

            value = new LocalizedString(tableReference, entryReference);
        }

        protected override void Write(ref LocalizedString value, IDataWriter writer)
        {
            if (RogueSaveSystem.isDebug)
            {
                RogueSaveSystem.WriteValue("Table Name", value.TableReference.TableCollectionName, StringSerializer, writer);
            }
            else
            {
                RogueSaveSystem.WriteValue("GUID", value.TableReference.TableCollectionNameGuid, GuidSerializer, writer);
            }
            
            RogueSaveSystem.WriteValue("Entry", value.TableEntryReference.KeyId, LongSerializer, writer);
        }
    }
}