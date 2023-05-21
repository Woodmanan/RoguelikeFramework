using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;

namespace Tests
{
    public class RogueTagTests
    {
        string[] testNames = new string[]
            {
                "tag.test.other.blah",
                "other.tag.test.blah",
                "random",
                "a.b.c.d",
                "q.r.x.y",
                "tw.as.d",
                "a.",
                "as.d",
                "...",
                "Test.X.Y.Z",
                "b.a.c.d",
                ".a.a."
            };

        [Test]
        public void CanCreateWithoutError()
        {
            RogueTag.FlushTagCache();
            RogueTag tag = new RogueTag("test.test.test.test");
            Assert.AreNotEqual((ulong)0, tag.GetInternalValue());
        }

        [Test]
        public void RogueTagIncrementsCorrectly()
        {
            RogueTag.FlushTagCache();
            ulong allZeroes = 0;
            ulong one = 1;
            ulong allOnes = (one) | (one << 16) | (one << 32) | (one << 48);
            ulong two = 2;
            ulong allTwos = (two) | (two << 16) | (two << 32) | (two << 48);
            ulong three = 3;
            ulong mix = (one) | (three << 16) | (two << 32) | (two << 48);

            RogueTag tagZero = new RogueTag("INTERNAL_ZERO.INTERNAL_ZERO.INTERNAL_ZERO.INTERNAL_ZERO");
            RogueTag tagOne = new RogueTag("one.one.one.one");
            RogueTag tagTwo = new RogueTag("two.two.two.two");
            RogueTag tagDuplicate = new RogueTag("two.two.two.two");
            RogueTag tagMix = new RogueTag("one.three.two.two");

            Assert.AreEqual(allZeroes, tagZero.GetInternalValue());
            Assert.AreEqual(allOnes, tagOne.GetInternalValue());
            Assert.AreEqual(allTwos, tagTwo.GetInternalValue());
            Assert.AreEqual(allTwos, tagDuplicate.GetInternalValue());
            Assert.AreEqual(mix, tagMix.GetInternalValue());
        }

        [Test]
        public void RogueTagTriggersWarning()
        {
            RogueTag.FlushTagCache();
            for (ulong i = 0; i < RogueTag.maxValue - 1; i++)
            {
                string iString = $"{i}.{i}.{i}.{i}";
                RogueTag tag = new RogueTag(iString);
            }

            try
            {
                RogueTag tag = new RogueTag("test.test.test.test");
            }
            catch 
            {
                return;
            }

            Assert.Fail("Did not catch maximum value being called.");
        }

        [Test]
        public void RogueTagMatchesHeirarchy()
        {
            RogueTag.FlushTagCache();
            RogueTag one = new RogueTag("one");
            RogueTag two = new RogueTag("one.two");
            RogueTag three = new RogueTag("one.two.three");
            RogueTag four = new RogueTag("one.two.three.four");

            Assert.AreEqual(one.GetInternalDegree(), 1);
            Assert.AreEqual(two.GetInternalDegree(), 2);
            Assert.AreEqual(three.GetInternalDegree(), 3);
            Assert.AreEqual(four.GetInternalDegree(), 4);

            Assert.IsTrue(one.IsFamilialMatch(one));
            Assert.IsTrue(one.IsFamilialMatch(two));
            Assert.IsTrue(one.IsFamilialMatch(three));
            Assert.IsTrue(one.IsFamilialMatch(four));

            Assert.IsTrue(two.IsFamilialMatch(one));
            Assert.IsTrue(two.IsFamilialMatch(two));
            Assert.IsTrue(two.IsFamilialMatch(three));
            Assert.IsTrue(two.IsFamilialMatch(four));

            Assert.IsTrue(three.IsFamilialMatch(one));
            Assert.IsTrue(three.IsFamilialMatch(two));
            Assert.IsTrue(three.IsFamilialMatch(three));
            Assert.IsTrue(three.IsFamilialMatch(four));

            Assert.IsTrue(four.IsFamilialMatch(one));
            Assert.IsTrue(four.IsFamilialMatch(two));
            Assert.IsTrue(four.IsFamilialMatch(three));
            Assert.IsTrue(four.IsFamilialMatch(four));

            //Exact matches
            Assert.IsTrue(one.IsExactMatch(one));
            Assert.IsTrue(two.IsExactMatch(two));
            Assert.IsTrue(three.IsExactMatch(three));
            Assert.IsTrue(four.IsExactMatch(four));

            Assert.IsFalse(one.IsExactMatch(two));
            Assert.IsFalse(one.IsExactMatch(three));
            Assert.IsFalse(one.IsExactMatch(four));

            Assert.IsFalse(two.IsExactMatch(one));
            Assert.IsFalse(two.IsExactMatch(three));
            Assert.IsFalse(two.IsExactMatch(four));

            Assert.IsFalse(three.IsExactMatch(one));
            Assert.IsFalse(three.IsExactMatch(two));
            Assert.IsFalse(three.IsExactMatch(four));

            Assert.IsFalse(four.IsExactMatch(one));
            Assert.IsFalse(four.IsExactMatch(two));
            Assert.IsFalse(four.IsExactMatch(three));

            Assert.IsTrue(one.IsParentalMatch(two));
            Assert.IsFalse(two.IsParentalMatch(one));

            Assert.IsTrue(two.IsParentalMatch(three));
            Assert.IsFalse(three.IsParentalMatch(two));

            Assert.IsTrue(three.IsParentalMatch(four));
            Assert.IsFalse(four.IsParentalMatch(three));
        }

        [Test]
        public void RogueTagSimpleNoMatch()
        {
            RogueTag.FlushTagCache();

            RogueTag one = new RogueTag("one.two.three.four");
            RogueTag two = new RogueTag("one.two.three.five");

            Assert.IsFalse(one.IsExactMatch(two));
            Assert.IsFalse(one.IsFamilialMatch(two));
            Assert.IsFalse(one.IsParentalMatch(two));

            Assert.IsFalse(two.IsExactMatch(one));
            Assert.IsFalse(two.IsFamilialMatch(one));
            Assert.IsFalse(two.IsParentalMatch(one));
        }

        [Test]
        public void RogueTagCheckNames()
        {
            RogueTag.FlushTagCache();

            RogueTag[] tags = testNames.Select(x => new RogueTag(x)).ToArray();

            for (int i = 0; i < testNames.Length; i++)
            {
                Assert.IsTrue(testNames[i].Equals(tags[i].GetHumanName()));
            }
        } 

        [Test]
        public void RogueTagEqualityCheck()
        {
            RogueTag.FlushTagCache();
            for (int i = 0; i < testNames.Length - 1; i++)
            {
                Assert.AreEqual(new RogueTag(testNames[i]), new RogueTag(testNames[i]));
                Assert.AreNotEqual(new RogueTag(testNames[i]), new RogueTag(testNames[i + 1]));
                Assert.AreNotEqual(new RogueTag(testNames[i + 1]), new RogueTag(testNames[i]));
            }
        }

        [Test]
        public void TagContainerSimpleCheck()
        {
            RogueTag.FlushTagCache();
            RogueTagContainer container = new RogueTagContainer();

            RogueTag original = new RogueTag("Test.X.Y.Z");

            container.AddTag("Test.X.Y.Z");

            RogueTag duplicate = new RogueTag("Test.X.Y.Z");

            Assert.IsTrue(container.HasTag(original));
            Assert.IsTrue(container.HasTag(duplicate));
        }

        [Test]
        public void TagInclusionCheck()
        {
            RogueTag.FlushTagCache();
            RogueTagContainer container = new RogueTagContainer();

            foreach (string s in testNames)
            {
                container.AddTag(s);
                Assert.IsTrue(container.HasTag(s));
            }

            foreach (string s in testNames)
            {
                Assert.IsTrue(container.HasTag(s));
            }

            foreach (string s in testNames)
            {
                container.RemoveTag(s);
                Assert.IsFalse(container.HasTag(s));
            }

            foreach (string s in testNames)
            {
                Assert.IsFalse(container.HasTag(s));
            }
        }

        [Test]
        public void RogueTagAllocateALot()
        {
            RogueTag.FlushTagCache();
            RogueTagContainer container = new RogueTagContainer();
            for (int i = 0; i < 60000; i++)
            {
                container.AddTag("Test.X.Y.Z");
            }
        }

        [Test]
        public void RogueTagAllocateManyCopies()
        {
            RogueTag.FlushTagCache();
            RogueTagContainer container = new RogueTagContainer();

            for (int i = 0; i < 60000; i++)
            {
                foreach (string s in testNames)
                {
                    container.AddTag(s);
                }
            }

            foreach (string s in testNames)
            {
                Assert.IsTrue(container.HasTag(s));
            }
        }

        [Test]
        public void TagContainerInclusion()
        {
            RogueTag.FlushTagCache();
            RogueTagContainer container = new RogueTagContainer();
            container.AddTag("Testing.X.Y.Z");
            container.AddTag("Testing.X.A.B");
            container.AddTag("Testing.X.Y.W");
            container.AddTag("Testing.X.X.X");
            container.AddTag("Testing.X.Y.Y");
            container.AddTag("Testing.X");
            container.AddTag("Testing.X.");
            container.AddTag("Testing.X..");
            container.AddTag("Testing.X.Y.Z"); //Add duplicate
            container.RemoveTag("Testing.X.Y.W");

            Assert.IsTrue(container.MatchAnyTags(new RogueTag("Testing.X.A.B"), TagMatch.Exact));
            Assert.IsTrue(container.MatchAnyTags(new RogueTag("Testing.X.Y.Z"), TagMatch.Exact));

            Assert.IsFalse(container.MatchAnyTags(new RogueTag("Testing.X.Y.W"), TagMatch.Exact));

            Assert.IsTrue(container.MatchAnyTags(new RogueTag("Testing"), TagMatch.Parental));
            Assert.IsTrue(container.MatchAnyTags(new RogueTag("Testing.X"), TagMatch.Parental));

            Assert.IsFalse(container.MatchAnyTags(new RogueTag("Testing.Y.W.B"), TagMatch.Familial));

            Assert.IsTrue(container.MatchAllTags(new RogueTag("Testing"), TagMatch.Parental));
            Assert.IsFalse(container.MatchAllTags(new RogueTag("Testing.X.Y"), TagMatch.Parental));

            RogueTagContainer ParentContainer = new RogueTagContainer();
            ParentContainer.AddTag("Testing");

            Assert.IsTrue(ParentContainer.MatchAllTags(container, TagMatch.Parental));
            Assert.IsTrue(ParentContainer.MatchAllTags(container, TagMatch.Familial));
            Assert.IsTrue(ParentContainer.MatchAnyTags(container, TagMatch.Parental));
            Assert.IsTrue(ParentContainer.MatchAnyTags(container, TagMatch.Familial));

            ParentContainer.AddTag("Other.Tag");

            Assert.IsTrue(ParentContainer.MatchAnyTags(container, TagMatch.Parental));
            Assert.IsTrue(ParentContainer.MatchAnyTags(container, TagMatch.Familial));
            Assert.IsFalse(ParentContainer.MatchAllTags(container, TagMatch.Parental));
            Assert.IsFalse(ParentContainer.MatchAllTags(container, TagMatch.Familial));
        }
    }
}
