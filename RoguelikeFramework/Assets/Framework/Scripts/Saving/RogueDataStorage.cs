using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public struct RogueHandle<T>
{
    [SerializeField]
    private int offset;

#if UNITY_EDITOR
    [SerializeField]
    public T serialValue;
#endif

    public static RogueHandle<T> Default = new RogueHandle<T>(-1);

    public static RogueHandle<T> Create()
    {
        return RogueDataArena<T>.arena.Allocate();
    }

    public static RogueHandle<T> Create(T toInsert)
    {
        return RogueDataArena<T>.arena.Insert(toInsert);
    }

    public static RogueHandle<T> Create<T2>() where T2 : T
    {
        T2 hold = Activator.CreateInstance<T2>();
        return RogueDataArena<T>.arena.Insert(hold);
    }

    public static RogueHandle<T> Create<T2>(T2 toInsert) where T2 : T
    {
        return RogueDataArena<T>.arena.Insert(toInsert);
    }

    public static RogueHandle<T> Cast<T2>(RogueHandle<T2> other) where T2 : T
    {
        return new RogueHandle<T>(other.offset);
    }

    public RogueHandle(int offset = -1)
    {
        this.offset = offset;
#if UNITY_EDITOR
        this.serialValue = default(T);
        SetSerialValue();
#endif
    }

#if UNITY_EDITOR
    public void SetSerialValue()
    {
        if (IsValid())
        {
            serialValue = RogueDataArena<T>.arena[offset];
        }
        else
        {
            serialValue = default(T);
        }
    }

#endif

    public bool IsValid()
    {
        return offset >= 0 && offset < RogueDataArena<T>.arena.Count;
    }

    public static implicit operator bool(RogueHandle<T> handle)
    {
        return handle.IsValid();
    }

    public static implicit operator T(RogueHandle<T> handle)
    {
        return handle.value;
    }

    public T Get()
    {
        return RogueDataArena<T>.arena[offset];
    }

    public T2 Get<T2>() where T2 : class, T
    {
        return Get() as T2;
    }

    public T this[int i]
    {
        get { return RogueDataArena<T>.arena[offset]; }
        set { RogueDataArena<T>.arena[offset] = value; }
    }

    public static bool operator==(RogueHandle<T> one, RogueHandle<T> two)
    {
        return one.offset == two.offset;
    }

    public static bool operator !=(RogueHandle<T> one, RogueHandle<T> two)
    {
        return one.offset != two.offset;
    }

    public T value
    {
        get { return RogueDataArena<T>.arena[offset]; }
        set { RogueDataArena<T>.arena[offset] = value; }
    }

    public int GetOffset()
    {
        return offset;
    }
}

[System.Serializable]
public abstract class RogueDataArena
{
    public abstract void Clear();
    public abstract void InitializeFromPersistence();
}

[System.Serializable]
public class RogueDataArena<T> : RogueDataArena
{
    public static RogueDataArena<T> arena = new RogueDataArena<T>();
    private const int baseCapacity = 64;
    [SerializeField]
    private List<T> memory;

    public RogueDataArena(int capacity = 4)
    {
        
    }

    private void CheckInitialize()
    {

        if (memory == null)
        {
            memory = new List<T>(baseCapacity);

            //Register must always be the last step
            RogueDataStorage.RegisterArena<T>();
        }
    }

    public RogueHandle<T> Allocate()
    {
        CheckInitialize();
        memory.Add(Activator.CreateInstance<T>());
        Debug.Assert(memory.Count - 1 >= 0);
        return new RogueHandle<T>(memory.Count - 1);
    }

    public RogueHandle<T> Insert(T value)
    {
        CheckInitialize();
        memory.Add(value);
        Debug.Assert(memory.Count - 1 >= 0);
        return new RogueHandle<T>(memory.Count - 1);
    }

    public override void Clear()
    {
        memory = null;
    }

    public override void InitializeFromPersistence()
    {
        arena = this;
    }

    public T this[int index]
    {
        get { return memory[index]; }
        set { memory[index] = value; }
    }

    private static int NextPowerOfTwo(int valueToReach, int scaling, int subtraction)
    {
        int numMuls = 0;
        while ((scaling - subtraction) < valueToReach)
        {
            scaling *= 2;
            numMuls++;
        }

        return numMuls;
    }

    public static void PrepareBufferForInserts(int capacity)
    {
        arena.CheckInitialize();

        arena.memory.Capacity = NextPowerOfTwo(capacity, arena.memory.Capacity, arena.memory.Count);
    }

    public int Count => memory.Count;
}

[Serializable]
public class RogueDataStorage
{
    private static RogueDataStorage _storage = new RogueDataStorage();
    [SerializeField]
    private List<RogueDataArena> arenas = new List<RogueDataArena>();

    public static void RegisterArena<T>()
    {
        Debug.Assert(!_storage.arenas.Contains(RogueDataArena<T>.arena));
        _storage.arenas.Add(RogueDataArena<T>.arena);
    }

    public static void ReleaseAll()
    {
        foreach (RogueDataArena arena in _storage.arenas)
        {
            arena.Clear();
        }
        _storage.arenas.Clear();
    }

    public static void SaveArenas()
    {
        Debug.Assert(RogueSaveSystem.isSaving);
        RogueSaveSystem.Write(_storage);
    }

    public static void RetrieveArenas()
    {
        Debug.Assert(RogueSaveSystem.isReading);
        RogueSaveSystem.Read(out _storage);
        foreach (RogueDataArena arena in _storage.arenas)
        {
            //Static members don't know that we reloaded - need to reset them
            arena.InitializeFromPersistence();
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
