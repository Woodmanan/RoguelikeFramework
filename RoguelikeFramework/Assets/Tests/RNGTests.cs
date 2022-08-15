using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class RNGTests
    {
        public static bool ApproxEqual(float a, float b, float eps)
        {
            return Mathf.Abs(a - b) < eps;
        }

        // A Test behaves as an ordinary method
        [Test]
        public void ExponentialTest()
        {
            for (int c = 0; c < 50; c++)
            {
                float sum = 0.0f;
                int count = 1000000;
                float mean = Random.Range(2, 100);
                for (int i = 0; i < count; i++)
                {
                    sum += RogueRNG.Exponential(mean);
                }
                Assert.IsTrue(ApproxEqual(sum / count, mean, mean / 100f));
            }
        }

        [Test]
        public void CutBoundExponentialTest()
        {
            // Use the Assert class to test conditions
            int count = 1000;
            float max = 100.0f;
            float min = 30.0f;
            for (int i = 0; i < count; i++)
            {
                float val = RogueRNG.BoundedExponentialCut(min, max, 50f);
                Assert.IsTrue(val <= max);
                Assert.IsTrue(val >= min);
            }
        }

        [Test]
        public void BoundExponentialTest()
        {
            for (int i = 0; i < 1; i++)
            {
                float sum = 0.0f;
                int count = 1000000;
                float min = Random.Range(0, 20);
                float max = min + Random.Range(20, 40);
                float mean = Random.Range(min + 10, max - 10);

                for (int j = 0; j < count; j++)
                {
                    float val = RogueRNG.BoundedExponential(min, max, mean);
                    sum += val;
                    Assert.IsTrue(val <= max);
                    Assert.IsTrue(val >= min);
                }

                Assert.IsTrue(ApproxEqual(sum / count, mean, 3f));
            }
        }

        [Test]
        public void ParetoTest()
        {
            for (int a = 1; a < 20; a++)
            {
                float max = 0;
                float sum = 0;
                float min = 1;

                int count = 10000;
                for (int i = 0; i < count; i++)
                {
                    float val = RogueRNG.Pareto(1f, a);
                    Assert.GreaterOrEqual(val, min);
                    sum += val;
                    max = Mathf.Max(val, max);
                }

                if (a > 1)
                {
                    Assert.IsTrue(ApproxEqual(sum / count, min * (a) / (a - 1), .1f));
                }
            }
            
        }

        [Test]
        public void CutParetoTest()
        {
            for (int a = 1; a < 20; a++)
            {
                float min = 1;
                float max = 20;

                int count = 10000;
                for (int i = 0; i < count; i++)
                {
                    float val = RogueRNG.BoundedParetoCut(min, max, 1f, a);
                    Assert.GreaterOrEqual(val, min);
                    Assert.GreaterOrEqual(max, val);
                }
            }
        }

        [Test]
        public void BoundedParetoTest()
        {
            for (int a = 1; a < 20; a++)
            {
                float min = 0;
                float max = 100;
                float largest = 0.0f;
                float sum = 0;

                int count = 10000;
                for (int i = 0; i < count; i++)
                {
                    float val = RogueRNG.BoundedPareto(min, max, 1f, a);
                    largest = Mathf.Max(largest, val);
                    Assert.GreaterOrEqual(val, min);
                    Assert.GreaterOrEqual(max, val);
                    sum += val;
                }

                //Great for testing values out! Uncomment to get a nice readout
                //Debug.Log($"{a} - largest is {largest}");
                //Debug.Log($"{a} avg is {sum / count}");
            }
        }

        [Test]
        public void BinomialTest()
        {
            for (int i = 0; i < 50; i++)
            {
                int n = Random.Range(1, 100);
                float p = Random.value;
                int sum = 0;
                int count = 10000;
                for (int c = 0; c < count; c++)
                {
                    int val = RogueRNG.Binomial(n, p);
                    Assert.GreaterOrEqual(val, 0, "Binomial returned less than 1");
                    Assert.GreaterOrEqual(n + 1, val, "Binomial returned more than n");
                    sum += val;
                }

                Assert.IsTrue(ApproxEqual(((float)sum) / count, n * p, .2f));
            }
        }

        [Test]
        public void GeometricTest()
        {
            for (int i = 0; i < 10; i++)
            {
                int mean = RogueRNG.BoundedExponential(1, 50, 10);
                int count = 100000;
                int sum = 0;
                int max = 1000;
                for (int c = 0; c < count; c++)
                {
                    int val = RogueRNG.Geometric(mean, max);
                    Assert.GreaterOrEqual(val, 0, "Geometric returned less than 1?");
                    Assert.GreaterOrEqual(max, val, "Geometric exceed max value");
                    sum += val;
                }

                Assert.IsTrue(ApproxEqual(((float)sum) / count, mean, .25f));
            }
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator RNGTestsWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
