// Licensed to the projects contributors.
// The license conditions are provided in the LICENSE file located in the project root

using System.Collections.Immutable;
using AutoFixture;
using AutoFixture.NUnit3;
using NuGetUtility.Extensions;

namespace NuGetUtility.Test.Extensions
{
    [TestFixture(typeof(string))]
    [TestFixture(typeof(HashSetExtensionTestObject))]
    [TestFixture(typeof(int))]
    internal class HashSetExtensionsTest<T>
    {
        [SetUp]
        public void SetUp()
        {
            _uut = new HashSet<T>(new Fixture().CreateMany<T>());
        }

        private HashSet<T>? _uut;

        [Test]
        [AutoData]
        public void AddMany_Should_AddNewElementsToHashSet(T[] newElements)
        {
            var initialElements = _uut!.ToImmutableList();
            _uut!.AddRange(newElements);

            CollectionAssert.AreEquivalent(initialElements.AddRange(newElements).Distinct(), _uut);
        }

        [Test]
        [AutoData]
        public void AddMany_Should_OnlyAddNewItems(T[] newElements)
        {
            var initialElements = _uut!.ToImmutableList();
            _uut!.AddRange(initialElements.AddRange(newElements));

            CollectionAssert.AreEquivalent(initialElements.AddRange(newElements).Distinct(), _uut);
        }

        [Test]
        public void AddMany_Should_KeepSameHashSetIfOnlyAddingSameElements()
        {
            var initialElements = _uut!.ToImmutableList();
            _uut!.AddRange(initialElements);
            _uut!.AddRange(initialElements);
            _uut!.AddRange(initialElements);

            CollectionAssert.AreEquivalent(initialElements, _uut);
        }
    }
}
