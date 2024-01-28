#if UNITY_EDITOR || DEVELOPMENT_BUILD
#define JSON
#endif

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using OdinSerializer;

//Wrapper for JSON serializer - it can't handle simple values that aren't in a struct
[System.Serializable]
public struct JSONValueWrapper<T>
{
    public JSONValueWrapper(T inValue)
    {
        value = inValue;
    }

    public T value;
}

public class RogueSaveSystem
{
    const string fileMagic = "RSFL"; //Rogue SaveFile
    const int Version = 1;

    private static string savePath;

    private static Stream stream;
    private static IDataWriter iWriter;
    private static IDataReader iReader;

    public static bool isSaving => (iWriter != null);
    public static bool isReading => (iReader != null);

    public static void BeginWriteSaveFile(string fileName)
    {
        savePath = Path.Combine(Application.persistentDataPath, fileName + ".rsf");
        Debug.Log("Begin writing save file at " + savePath);
        //writer = new BinaryWriter(File.Open(savePath, FileMode.Create));
        stream = File.Open(savePath, FileMode.Create);
#if JSON
        iWriter = SerializationUtility.CreateWriter(stream, null, DataFormat.JSON);
#else
        iWriter = SerializationUtility.CreateWriter(stream, null, DataFormat.Binary);
#endif
        Write(fileMagic);
        Write(Version);
    }

    public static void BeginReadSaveFile(string fileName)
    {
        savePath = Path.Combine(Application.persistentDataPath, fileName + ".rsf");
        Debug.Log("Begin reading save file at " + savePath);
        //reader = new BinaryReader(File.Open(savePath, FileMode.Open));
        stream = File.Open(savePath, FileMode.Open);
#if JSON
        iReader = SerializationUtility.CreateReader(stream, null, DataFormat.JSON);
#else
        iReader = SerializationUtility.CreateReader(stream, null, DataFormat.Binary);
#endif

        int fileVersion;
        Debug.Assert(Read<string>() == fileMagic);
        Read(out fileVersion);

        if (fileVersion != Version)
        {
            Debug.LogError("File format is out of date!");
        }
    }

    public static void CloseSaveFile(bool delete = true)
    {
        if (iWriter != null)
        {
            //Bug with the reader implementation - it will drop the last value!
            //Write a cute lil @ as an EOF telomere
            Write('@');
            iWriter.FlushToStream();
            iWriter.Dispose();
            iWriter = null;
            stream.Flush();
            stream.Dispose();
        }
        

        if (iReader != null)
        {
            iReader.Dispose();
            iReader = null;

            stream.Dispose();
            if (delete)
            {
                File.Delete(savePath);
            }
        }    

        savePath = null;
    }

    public static void Write<T>(T toSave)
    {
        SerializationUtility.SerializeValue(toSave, iWriter);
    }

    public static void Read<T>(out T value)
    {
        value = Read<T>();
    }

    public static T Read<T>()
    {
        return SerializationUtility.DeserializeValue<T>(iReader);
    }

    public static T TestDeserialization<T>(T value)
    {
        return SerializationUtility.DeserializeValue<T>(SerializationUtility.SerializeValue<T>(value, DataFormat.Binary), DataFormat.Binary);
    }
}
