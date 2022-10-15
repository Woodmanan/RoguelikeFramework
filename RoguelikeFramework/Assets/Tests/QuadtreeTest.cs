using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Diagnostics;

namespace Tests
{
    public class QuadtreeTest
    {
        // A Test behaves as an ordinary method
        [Test]
        public void FirstInsert()
        {
            // Use the Assert class to test conditions
            Rect testRect = new Rect(Vector2.zero, Vector2.one * 100);
            Quadtree<int> intTree = new Quadtree<int>(testRect);
            intTree.Insert(1, new Rect(Vector2.one * 25, Vector2.one * 50));
            Assert.IsTrue(intTree.held == 1);
            Assert.IsTrue(intTree.SW != null);
            Assert.IsTrue(intTree.SW.rect.size.x == 50f);

            Assert.IsTrue(intTree.rect.size.x == 100f);
        }

        [Test]
        public void FindSingleItem()
        {
            for (int i = 0; i < 100; i++)
            {
                Quadtree<int> testTree = new Quadtree<int>(new Rect(Vector2.zero, Vector2.one * 100));
                Rect itemRect = new Rect(new Vector2(Random.Range(0, 99), Random.Range(0, 99)), Vector2.one);
                testTree.Insert(0, itemRect);
                Assert.Contains(0, testTree.GetItemsIn(itemRect));
            }
        }

        [Test]
        public void AllTopLevel()
        {
            Quadtree<int> testTree = new Quadtree<int>(new Rect(Vector2.zero, Vector2.one * 100));
            //Insert a bunch of large items, confirm that none of them get dropped the wrong way.
            for (int i = 0; i < 300; i++)
            {
                Rect newRect = new Rect(Vector2.one * Random.Range(20, 40), Vector2.one * 40);
                testTree.Insert(i, newRect);
            }
            Assert.IsTrue(testTree.held == 300);
        }

        [Test]
        public void NoneTopLevel()
        {
            Quadtree<int> testTree = new Quadtree<int>(new Rect(Vector2.zero, Vector2.one * 100));
            //Insert a bunch of large items, confirm that none of them get dropped the wrong way.
            for (int i = 0; i < 300; i++)
            {
                Rect newRect = new Rect(Vector2.one * Random.Range(0, 40), Vector2.one * 10);
                testTree.Insert(i, newRect);
            }
            Assert.IsTrue(testTree.held == 0);
        }

        [Test]
        public void FindSingleItemWithNoise()
        {
            for (int i = 0; i < 100; i++)
            {
                Quadtree<int> testTree = new Quadtree<int>(new Rect(Vector2.zero, Vector2.one * 100));
                Rect itemRect = new Rect(new Vector2(Random.Range(0, 99), Random.Range(0, 99)), Vector2.one);
                testTree.Insert(0, itemRect);

                for (int j = 0; j < 100; j++)
                {
                    Rect itemRect2 = new Rect(new Vector2(Random.Range(0, 99), Random.Range(0, 99)), Vector2.one);
                    testTree.Insert(j, itemRect2);
                }

                Assert.Contains(0, testTree.GetItemsIn(itemRect));
            }
        }

        [Test]
        public void FindManyItemsAllSmall()
        {
            int count = 1000;
            (int, Rect)[] held = new (int, Rect)[count];
            Quadtree<int> testTree = new Quadtree<int>(new Rect(Vector2.zero, Vector2.one * 100));
            for (int i = 0; i < count; i++)
            {
                Rect itemRect = new Rect(new Vector2(Random.Range(0, 99), Random.Range(0, 99)), Vector2.one);
                testTree.Insert(i, itemRect);
                held[i] = (i, itemRect);
            }



            //3 ms to set up, roughly
            Stopwatch watch = new Stopwatch();
            int c = 0;
            for (int i = 0; i < count; i++)
            {
                watch.Start();
                List<int> items = testTree.GetItemsIn(held[i].Item2);
                watch.Stop();
                Assert.Contains(i, items);
                c += items.Count;
            }

            UnityEngine.Debug.Log($"Average time per item: {((float) watch.ElapsedMilliseconds) / count} ms ({watch.ElapsedMilliseconds} / {count})");
            UnityEngine.Debug.Log("Avg leaf count is " + c / count);
            UnityEngine.Debug.Log($"Tree depth is {testTree.GetDepth()}");
        }

        [Test]
        public void FindManyItemsAllSizes()
        {
            int count = 1000;
            (int, Rect)[] held = new (int, Rect)[count];
            Quadtree<int> testTree = new Quadtree<int>(new Rect(Vector2.zero, Vector2.one * 100));
            for (int i = 0; i < count; i++)
            {
                Vector2 size = new Vector2(Random.Range(1, 100), Random.Range(1, 100));
                Vector2 pos = new Vector2(Random.Range(0, 100 - size.x), Random.Range(0, 100 - size.y));
                Rect itemRect = new Rect(pos, size);
                testTree.Insert(i, itemRect);
                held[i] = (i, itemRect);
            }



            //3 ms to set up, roughly
            Stopwatch watch = new Stopwatch();
            int c = 0;
            for (int i = 0; i < count; i++)
            {
                watch.Start();
                List<int> items = testTree.GetItemsIn(held[i].Item2);
                watch.Stop();
                Assert.Contains(i, items);
                c += items.Count;
            }

            UnityEngine.Debug.Log($"Average time per item: {((float)watch.ElapsedMilliseconds) / count} ms ({watch.ElapsedMilliseconds} / {count})");
            UnityEngine.Debug.Log("Avg leaf count is " + c / count);
            UnityEngine.Debug.Log($"Tree depth is {testTree.GetDepth()}");
        }

        [Test]
        public void FindManyItemsRegularUseCase()
        {
            int count = 150;
            (int, Rect)[] held = new (int, Rect)[count];
            Quadtree<int> testTree = new Quadtree<int>(new Rect(Vector2.zero, Vector2.one * 100));
            for (int i = 0; i < count; i++)
            {
                Rect itemRect = new Rect(new Vector2(Random.Range(0, 99), Random.Range(0, 99)), Vector2.one);
                testTree.Insert(i, itemRect);
                held[i] = (i, itemRect);
            }



            //3 ms to set up, roughly
            Stopwatch watch = new Stopwatch();
            int c = 0;
            for (int i = 0; i < count; i++)
            {
                watch.Start();
                List<int> items = testTree.GetItemsIn(held[i].Item2);
                watch.Stop();
                Assert.Contains(i, items);
                c += items.Count;
            }

            UnityEngine.Debug.Log($"Average time per item: {((float)watch.ElapsedMilliseconds) / count} ms ({watch.ElapsedMilliseconds} / {count})");
            UnityEngine.Debug.Log("Avg leaf count is " + c / count);
            UnityEngine.Debug.Log($"Tree depth is {testTree.GetDepth()}");
        }

        [Test]
        public void GetsExactAnswersPredicatable()
        {
            Vector2Int bounds = new Vector2Int(40, 40);
            Quadtree<int> testTree = new Quadtree<int>(new Rect(Vector2.zero, bounds));
            for (int i = 0; i < bounds.x; i++)
            {
                for (int j = 0; j < bounds.y; j++)
                {
                    testTree.Insert(i + j * bounds.x, new Rect(new Vector2Int(i, j), Vector2.one));
                }
            }

            for (int size = 1; size < 10; size++)
            {
                for (int i = 0; i < bounds.x - size; i++)
                {
                    for (int j = 0; j < bounds.y - size; j++)
                    {
                        Rect searchRect = new Rect(new Vector2Int(i, j), Vector2.one * size);
                        List<int> found = testTree.GetItemsIn(new Rect(new Vector2Int(i, j), Vector2.one * size));
                        if (found.Count != (size * size))
                        {
                            foreach (int index in found)
                            {
                                UnityEngine.Debug.Log($"Found {index % bounds.x}, {index / bounds.x} instead");
                            }
                            Assert.IsTrue(false, $"Failed: found {found.Count} items in {searchRect} instead of {size * size}");
                        }
                        
                    }
                }
            }
        }

        [Test] 
        public void GetsExactAnswersRandom()
        {
            int itemCount = 1000;
            int testCount = 100;

            (int, Rect)[] held = new (int, Rect)[itemCount];
            Quadtree<int> testTree = new Quadtree<int>(new Rect(Vector2.zero, Vector2.one * 100));
            for (int i = 0; i < itemCount; i++)
            {
                Vector2 size = new Vector2(Random.Range(0.1f, 100), Random.Range(0.1f, 100));
                Vector2 pos = new Vector2(Random.Range(0, 100 - size.x), Random.Range(0, 100 - size.y));
                Rect itemRect = new Rect(pos, size);
                testTree.Insert(i, itemRect);
                held[i] = (i, itemRect);
            }

            for (int i = 0; i < testCount; i++)
            {
                Vector2 size = new Vector2(Random.Range(0, 100), Random.Range(0, 100));
                Vector2 pos = new Vector2(Random.Range(0, 100 - size.x), Random.Range(0, 100 - size.y));
                Rect testRect = new Rect(pos, size);

                int numFound = testTree.GetItemsIn(testRect).Count;
                int actual = 0;
                foreach ((int j, Rect rect) in held)
                {
                    if (rect.Overlaps(testRect)) actual++;
                }
                Assert.IsTrue(numFound == actual, $"Expected {actual} but got {numFound}");
            }
        }

        [Test]
        public void CorrectlyInsertsDepth()
        {
            Quadtree<int> testTree = new Quadtree<int>(new Rect(Vector2.zero, Vector2.one * 100));
            int lastDepth = 0;
            for (float size = 100; size > .025f; size = size / 2)
            {
                testTree.Insert(0, new Rect(Vector2.zero, Vector2.one * size));
                Assert.Greater(testTree.GetDepth(), lastDepth);
                lastDepth = testTree.GetDepth();
                UnityEngine.Debug.Log($"Size {size} is depth {lastDepth}");
            }
        }
    }
}
