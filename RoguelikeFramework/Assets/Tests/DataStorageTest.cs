using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class DataStorageTest
    {
        class A
        {
            public int valueOne;
            public int valueTwo;
        }

        class B : A
        {
            public int valueThree;
        }

        struct AStruct
        {
            public int valueOne;
            public float valueTwo;
            public string valueThree;
        }

        // A Test behaves as an ordinary method
        [Test]
        public void DataStorageTestSimplePasses()
        {
            int numA = 1000;
            int numB = 1000;

            RogueDataStorage.ReleaseAll();

            List<RogueHandle<A>> handles = new List<RogueHandle<A>>();
            
            for (int i = 0; i < numA; i++)
            {
                RogueHandle<A> handle = RogueHandle<A>.Create();
                handle.Get().valueOne = 5;
                handle.Get().valueTwo = i;
                handles.Add(handle);
            }

            for (int i = 0; i < numB; i++)
            {
                RogueHandle<A> handle = RogueHandle<A>.Create<B>();
                handle.Get().valueOne = 5;
                handle.Get().valueTwo = i + numA;
                handle.Get<B>().valueThree = 7;
                handles.Add(handle);
            }

            for (int i = 0; i < numA + numB; i++)
            {
                Assert.AreEqual(handles[i].Get().valueOne, 5);
                Assert.AreEqual(handles[i].Get().valueTwo, i);
            }

            for (int i = numA; i < numA + numB; i++)
            {
                Assert.AreEqual(handles[i].Get().valueOne, 5);
                Assert.AreEqual(handles[i].Get().valueTwo, i);
                Assert.AreEqual(handles[i].Get<B>().valueThree, 7);
            }
        }

        [Test]
        public void DataStorageStructTests()
        {
            int width = 500;
            int size = width * width;
            RogueDataStorage.ReleaseAll();

            List<RogueHandle<Vector2Int>> positions = new List<RogueHandle<Vector2Int>>();


            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    positions.Add(RogueHandle<Vector2Int>.Create(new Vector2Int(x, y)));
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    RogueHandle<Vector2Int> handle = positions[x * width + y];
                    Assert.IsTrue(handle.value.x == x && handle.value.y == y);
                }
            }
        }

        [Test]
        public void DataStorageStructTestsPrealloc()
        {
            int width = 500;
            int size = width * width;
            RogueDataStorage.ReleaseAll();

            List<RogueHandle<Vector2Int>> positions = new List<RogueHandle<Vector2Int>>(size);

            RogueDataArena<int>.PrepareBufferForInserts(size);


            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    positions.Add(RogueHandle<Vector2Int>.Create(new Vector2Int(x, y)));
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < width; y++)
                {
                    RogueHandle<Vector2Int> handle = positions[x * width + y];
                    Assert.IsTrue(handle.value.x == x && handle.value.y == y);
                }
            }
        }

        [Test]
        public void DataStorageClassTest()
        {
            int size = 1000;
            RogueDataStorage.ReleaseAll();

            List<RogueHandle<AStruct>> structs = new List<RogueHandle<AStruct>>(size);

            for (int i = 0; i < size; i++)
            {
                structs.Add(RogueHandle<AStruct>.Create());
                AStruct insert;
                insert.valueOne = i;
                insert.valueTwo = i + 1;
                insert.valueThree = "test";
                RogueHandle<AStruct> test = structs[i];
                test[0] = insert;
            }

            for (int i = 0; i < size; i++)
            {
                Assert.AreEqual(structs[i].value.valueOne, i);
                Assert.AreEqual(structs[i].value.valueTwo, i + 1);
                Assert.AreEqual(structs[i].value.valueThree, "test");
            }
        }

        [Test]
        public void SaveTest()
        {
            int size = 100;

            RogueSaveSystem.BeginWriteSaveFile("IntTest3");
            List<int> values = new List<int>(size);

            for (int i = 0; i < size; i++)
            {
                values.Add(RogueRNG.Linear(0, 1000));
                RogueSaveSystem.Write(values[i]);
            }

            RogueSaveSystem.CloseSaveFile();

            RogueSaveSystem.BeginReadSaveFile("IntTest3");

            try
            {
                for (int i = 0; i < size; i++)
                {
                    //int readValue;
                    //RogueSaveSystem.Read(out readValue);
                    if (i == size - 1)
                    {
                        Debug.Log("Got here!");
                    }

                    int value = RogueSaveSystem.Read<int>();
                    Debug.Log($"{value} vs {values[i]}");
                    Assert.AreEqual(value, values[i]);
                }
            }
            finally
            {
                RogueSaveSystem.CloseSaveFile();
            }
            
        }

        [System.Serializable]
        public class baseClass
        {
            public int valueOne = 8;

            [SerializeField]
            protected string valueTwo = "base";
        }

        [System.Serializable]
        public class extensionClass : baseClass
        {
            public extensionClass(string test)
            {
                valueTwo = test;
                valueThree = 5;
                valueOne = 4;
            }
            public int valueThree;

            [SerializeField]
            private float otherValue = 1032;
        }

        [Test]
        public void SaveClassExtensionTest()
        {
            string fileName = "ExtensionTest";
            
            RogueSaveSystem.BeginWriteSaveFile(fileName);

            baseClass testOne = new baseClass();
            baseClass testTwo = new extensionClass("exension!");
            extensionClass testThree = new extensionClass("also extension!");

            RogueSaveSystem.Write(testOne);
            RogueSaveSystem.Write(testTwo);
            RogueSaveSystem.Write(testThree);

            RogueSaveSystem.CloseSaveFile();

            RogueSaveSystem.BeginReadSaveFile(fileName);

            RogueSaveSystem.Read<baseClass>(out testOne);
            extensionClass recover = (extensionClass) RogueSaveSystem.Read<baseClass>();
            RogueSaveSystem.Read<extensionClass>(out testThree);

            Debug.Log(recover.GetType());

            Debug.Assert(recover.valueThree == 5);
            Debug.Assert(testThree.valueThree == 5);

            RogueSaveSystem.CloseSaveFile();
        }
        
        [Test]
        public void ArenaSerializationTest()
        {
            RogueDataStorage.ReleaseAll();
            RogueHandle<baseClass> hold = new RogueHandle<baseClass>();

            for (int i = 0; i < 4000; i++)
            {
                RogueHandle<baseClass> test = RogueHandle<baseClass>.Create<extensionClass>(new extensionClass("agh!"));
                if (i == 2510) hold = test;
            }

            RogueSaveSystem.BeginWriteSaveFile("ArenaTest");
            RogueSaveSystem.Write(hold);
            RogueDataStorage.SaveArenas();
            RogueSaveSystem.CloseSaveFile();

            hold = new RogueHandle<baseClass>();

            //Read operation!
            RogueSaveSystem.BeginReadSaveFile("ArenaTest");
            RogueSaveSystem.Read(out hold);
            RogueDataStorage.RetrieveArenas();
            RogueSaveSystem.CloseSaveFile();

            Assert.IsTrue(hold.IsValid());
            Assert.IsTrue(hold.value.valueOne == 4);
        }

        [Test]
        public void ArenaSerializationTestLarge()
        {
            RogueDataStorage.ReleaseAll();
            RogueHandle<baseClass> hold = new RogueHandle<baseClass>();

            for (int i = 0; i < 4000; i++)
            {
                RogueHandle<baseClass> test = RogueHandle<baseClass>.Create<extensionClass>(new extensionClass("agh!"));
                if (i == 2510) hold = test;
            }

            for (int i = 0; i < 4000; i++)
            {
                RogueHandle<float> test = RogueHandle<float>.Create(i);
            }

            for (int i = 0; i < 4000; i++)
            {
                RogueHandle<int> test = RogueHandle<int>.Create(i);
            }

            for (int i = 0; i < 4000; i++)
            {
                RogueHandle<int> test = RogueHandle<int>.Create(i);
            }

            RogueSaveSystem.BeginWriteSaveFile("LargeArenaTest");
            RogueSaveSystem.Write(hold);
            RogueDataStorage.SaveArenas();
            RogueSaveSystem.CloseSaveFile();

            hold = new RogueHandle<baseClass>();
            RogueDataStorage.ReleaseAll();

            //Read operation!
            RogueSaveSystem.BeginReadSaveFile("LargeArenaTest");
            RogueSaveSystem.Read(out hold);
            RogueDataStorage.RetrieveArenas();
            RogueSaveSystem.CloseSaveFile();

            Assert.IsTrue(hold.IsValid());
            Assert.IsTrue(hold.value.valueOne == 4);
        }

        [System.Serializable]
        public class HandleA
        {
            [SerializeField]
            RogueHandle<HandleB> handle;
            
            public void Setup(RogueHandle<HandleA> myHandle)
            {
                handle = RogueHandle<HandleB>.Create();
                HandleB hold = handle.value;
                hold.Setup(myHandle);
                handle.value = hold;
            }

            public bool CheckCircular()
            {
                return handle.value.CheckCircular(this);
            }
        }

        [System.Serializable]
        public struct HandleB
        {
            [SerializeField]
            RogueHandle<HandleA> handle;

            public void Setup(RogueHandle<HandleA> owner)
            {
                handle = owner;
            }

            public bool CheckCircular(HandleA owner)
            {
                return handle.value == owner;
            }
        }

        [Test]
        public void CircularClassTest()
        {
            RogueDataStorage.ReleaseAll();

            List<RogueHandle<HandleA>> handles = new List<RogueHandle<HandleA>>();

            for (int i = 0; i < 1000; i++)
            {
                handles.Add(RogueHandle<HandleA>.Create());
                handles[i].value.Setup(handles[i]);
                Assert.IsTrue(handles[i].value.CheckCircular());
            }

            RogueSaveSystem.BeginWriteSaveFile("CircularTest");
            RogueSaveSystem.Write(handles);
            RogueDataStorage.SaveArenas();
            RogueSaveSystem.CloseSaveFile();

            handles = null;
            RogueDataStorage.ReleaseAll();

            RogueSaveSystem.BeginReadSaveFile("CircularTest");
            RogueSaveSystem.Read(out handles);
            RogueDataStorage.RetrieveArenas();
            RogueSaveSystem.CloseSaveFile(false);

            for (int i = 0; i < 1000; i++)
            {
                Assert.IsTrue(handles[i].IsValid());
                Assert.IsTrue(handles[i].value.CheckCircular());
            }
        }

        [Test]
        public void CompressedHandleTest()
        {
            RogueDataStorage.ReleaseAll();

            List<RogueHandle<Monster>> handles = new List<RogueHandle<Monster>>();

            for (int i = 0; i < 1000; i++)
            {
                handles.Add(RogueHandle<Monster>.Create());
                handles[i].value.ID = i;
                handles[i].value.baseStats = new Stats();
                handles[i].value.baseStats[Resources.MR] = i;
            }

            RogueSaveSystem.BeginWriteSaveFile("CompressedHandleTest");
            RogueSaveSystem.Write(handles);
            RogueDataStorage.SaveArenas();
            RogueSaveSystem.CloseSaveFile();

            handles = null;
            RogueDataStorage.ReleaseAll();

            RogueSaveSystem.BeginReadSaveFile("CompressedHandleTest");
            RogueSaveSystem.Read(out handles);
            RogueDataStorage.RetrieveArenas();
            RogueSaveSystem.CloseSaveFile(false);

            for (int i = 0; i < 1000; i++)
            {
                Assert.IsTrue(handles[i].IsValid());
                Assert.IsTrue(handles[i].value.ID == i);
                Assert.IsTrue(handles[i].value.baseStats[Resources.MR] == i);
            }
        }

        [System.Serializable]
        class ClassTest
        {
            [SerializeField]
            ClassTest inner;

            public void SetDepth(int i)
            {
                if (i > 0)
                {
                    inner = new ClassTest();
                    inner.SetDepth(i - 1);
                }
                else
                {
                    inner = this;
                }
            }

            public bool TestDepth(int i)
            {
                if (i > 0)
                {
                    Assert.NotNull(inner);
                    return inner.TestDepth(i - 1);
                }
                else
                {
                    return inner == this;
                }
            }
        }

        [Test]
        public void RecursiveSelfReferenceTest()
        {
            ClassTest test = new ClassTest();
            test.SetDepth(100);

            RogueSaveSystem.BeginWriteSaveFile("Recursive");
            RogueSaveSystem.Write(test);
            RogueSaveSystem.CloseSaveFile();

            test = null;

            RogueSaveSystem.BeginReadSaveFile("Recursive");
            RogueSaveSystem.Read(out test);
            RogueSaveSystem.CloseSaveFile();

            Assert.IsTrue(test.TestDepth(100));
        }

        [Test]
        public void SimpleValueReadWrite()
        {
            Assert.IsTrue(RogueSaveSystem.TestDeserialization(16) == 16);
            Assert.IsTrue(RogueSaveSystem.TestDeserialization('a') == 'a');
            Assert.IsTrue(RogueSaveSystem.TestDeserialization(15.13f) == 15.13f);
            Assert.IsTrue(RogueSaveSystem.TestDeserialization("") == "");

            RogueSaveSystem.BeginWriteSaveFile("Simple2");
            RogueSaveSystem.Write(100f);
            RogueSaveSystem.Write("AGH TEST AGAIN");
            RogueSaveSystem.Write('a');
            RogueSaveSystem.Write(long.MaxValue);
            RogueSaveSystem.Write(false);
            RogueSaveSystem.Write("");
            RogueSaveSystem.Write(new Vector2Int(20, 42));
            RogueSaveSystem.CloseSaveFile();

            RogueSaveSystem.BeginReadSaveFile("Simple2");
            Assert.IsTrue(RogueSaveSystem.Read<float>() == 100f);
            Assert.IsTrue(RogueSaveSystem.Read<string>() == "AGH TEST AGAIN");
            Assert.IsTrue(RogueSaveSystem.Read<char>() == 'a');
            Assert.IsTrue(RogueSaveSystem.Read<long>() == long.MaxValue);
            Assert.IsFalse(RogueSaveSystem.Read<bool>());
            Assert.IsTrue(RogueSaveSystem.Read<string>() == "");
            Assert.IsTrue(RogueSaveSystem.Read<Vector2Int>() == new Vector2Int(20, 42));
            RogueSaveSystem.CloseSaveFile();
        }
    }
}
