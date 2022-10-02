using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class PathQueueTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void PathQueueTestsCapacityTests()
        {
            //Test creation with no elements
            PriorityQueue<Vector2Int> queue = new PriorityQueue<Vector2Int>();
            Assert.IsTrue(queue.GetCapacity() == 0);

            //Test initial expansion
            queue.Expand();
            Assert.IsTrue(queue.GetCapacity() == 4);

            //Test expansion
            int count = 4;
            for (int i = 0; i < 20; i++)
            {
                count *= 2;
                queue.Expand();
                Assert.IsTrue(queue.Count == 0);
                Assert.IsTrue(queue.GetCapacity() == count);
                Assert.IsTrue(queue.GetValueLength() == count);
            }

            queue = new PriorityQueue<Vector2Int>(4000);
            Assert.IsTrue(queue.GetCapacity() == 4000);
            Assert.IsTrue(queue.GetValueLength() == 4000);

            queue.ExpandTo(300*300);
            Assert.IsTrue(queue.GetCapacity() > (300 * 300));
        }

        [Test]
        public void PathQueueAddAndRemoveTest()
        {
            int count = 10000;
            PriorityQueue<float> queue = new PriorityQueue<float>(count / 2);
            
            for (int i = 0; i < count; i++)
            {
                float val = 20 * Random.value - 10;
                queue.Enqueue(val, val);
            }

            float last = Mathf.NegativeInfinity;

            for (int i = 0; i < count; i++)
            {
                float latest = queue.Dequeue();
                Assert.LessOrEqual(last, latest);
                last = latest;
            }       
        }

        [Test]
        public void PathQueueSpeedTestNoPrealloc()
        {
            PriorityQueue<Vector2Int> queue = new PriorityQueue<Vector2Int>();
            for (int i = 0; i < (400 * 400); i++)
            {
                queue.Enqueue(Vector2Int.one, Random.value);
            }

            for (int i = 0; i < (400 * 400); i++)
            {
                queue.Dequeue();
            }
        }

        [Test]
        public void InsertOneItemTest()
        {
            PriorityQueue<Vector2> queue = new PriorityQueue<Vector2>();
            Vector2 item = new Vector2(Random.value, Random.value);
            queue.Enqueue(item, Random.value);

            int count = 300;

            for (int i = 0; i < count; i++)
            {
                queue.Enqueue(Vector2.one * Random.value, -1 + Random.value);
            }

            for (int i = 0; i < count; i++)
            {
                queue.Dequeue();
            }

            Assert.AreEqual(item, queue.Dequeue());
        }

        [Test]
        public void PathQueueSpeedTestWithPrealloc()
        {
            PriorityQueue<Vector2Int> queue = new PriorityQueue<Vector2Int>(400*400);
            for (int i = 0; i < (400 * 400); i++)
            {
                queue.Enqueue(Vector2Int.one, Random.value);
            }

            for (int i = 0; i < (400 * 400); i++)
            {
                queue.Dequeue();
            }
        }

        [Test]
        public void PathQueueTestValidity()
        {
            int count = 1000;
            PriorityQueue<int> queue = new PriorityQueue<int>();
            for (int i = 0; i < count; i++)
            {
                queue.Enqueue(i, i);
            }

            for (int i = 0; i < count; i++)
            {
                int returned = queue.Dequeue();
                Assert.IsTrue(returned == i);
            }
        }

        [Test]
        public void PathQueueExpectedSpeedTestWithPrealloc()
        {
            PriorityQueue<Vector2Int> queue = new PriorityQueue<Vector2Int>(101 * 101);
            for (int i = 0; i < (100 * 100); i++)
            {
                queue.Enqueue(Vector2Int.one, Random.value);
            }

            for (int i = 0; i < (100 * 100); i++)
            {
                queue.Dequeue();
            }
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator PathQueueTestsWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
