using OdinSerializer;

//Set up compressed formatters
[assembly: RegisterFormatter(typeof(RogueHandleFormatter<Monster>))]
[assembly: RegisterFormatter(typeof(RogueHandleFormatter<int>))]


namespace OdinSerializer
{
    using UnityEngine;

    public class RogueHandleFormatter<T> : MinimalBaseFormatter<RogueHandle<T>>
    {
        private static readonly Serializer<int> IntSerializer = Serializer.Get<int>();
        private static readonly Serializer<bool> BoolSerializer = Serializer.Get<bool>();
        private static readonly Serializer<short> ShortSerializer = Serializer.Get<short>();

        protected override void Read(ref RogueHandle<T> value, IDataReader reader)
        {
            bool valid = BoolSerializer.ReadValue(reader);
            if (valid)
            {
                value = new RogueHandle<T>(IntSerializer.ReadValue(reader));
            }
            else
            {
                value = RogueHandle<T>.Default;
            }
        }

        protected override void Write(ref RogueHandle<T> value, IDataWriter writer)
        {
            RogueSaveSystem.WriteValue("Valid", value.IsValid(), BoolSerializer, writer);
            if (value.IsValid())
            {
                RogueSaveSystem.WriteValue("offset", value.GetOffset(), IntSerializer, writer);
            }
        }
    }
}