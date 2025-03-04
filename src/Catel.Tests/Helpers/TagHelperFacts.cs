﻿namespace Catel.Tests
{
    using Data;

    using NUnit.Framework;

    public class TagHelperFacts
    {
        [TestFixture]
        public class TheAreTagsEqualMethod
        {
            [TestCase]
            public void ReturnsTrueForBothNull()
            {
                Assert.IsTrue(TagHelper.AreTagsEqual(null, null));
            }

            [TestCase]
            public void ReturnsTrueForEqualStrings()
            {
                Assert.IsTrue(TagHelper.AreTagsEqual("Catel", "Catel"));
            }

            [TestCase]
            public void ReturnsFalseForDifferentStrings()
            {
                Assert.IsFalse(TagHelper.AreTagsEqual("Catel", "mvvm"));
            }

            [TestCase]
            public void ReturnsFalseForDifferentCasingStrings()
            {
                Assert.IsFalse(TagHelper.AreTagsEqual("Catel", "catel"));
            }

            [TestCase]
            public void ReturnsTrueForEqualInstances()
            {
                var firstEntry = ModelBaseTestHelper.CreateIniEntryObject("A", "B", "C");
                var secondEntry = ModelBaseTestHelper.CreateIniEntryObject("A", "B", "C");

                // References equal
                Assert.IsTrue(TagHelper.AreTagsEqual(firstEntry, firstEntry));

                // Objects equal
                Assert.IsTrue(TagHelper.AreTagsEqual(firstEntry, secondEntry));
            }

            [TestCase]
            public void ReturnsFalseForDifferentInstances()
            {
                var firstEntry = ModelBaseTestHelper.CreateIniEntryObject("A", "B", "C");
                var secondEntry = ModelBaseTestHelper.CreateIniEntryObject("D", "E", "F");

                Assert.IsFalse(TagHelper.AreTagsEqual(firstEntry, secondEntry));
            }
        }
    }
}