#if UNITY_EDITOR || UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using FindTheLover.Augments;
using FindTheLover.Augments.Contracts;
using FindTheLover.Augments.Data;

namespace FindTheLover.Tests.Augments
{
    /// <summary>
    /// Co-located Automated Unit Test Suite for <see cref="AugmentManager"/> and <see cref="AugmentDropController"/>.
    /// Verifies zero-duplicate 3-card rolling, minimum tier boss guarantees, graceful edge-case fallbacks, and receiver dispatch.
    /// </summary>
    [TestFixture]
    public class AugmentManagerTests
    {
        private GameObject _managerObj;
        private AugmentManager _manager;
        private List<AugmentDataSO> _catalog;

        private class MockAugmentReceiver : IAugmentReceiver
        {
            public AugmentDataSO LastApplied { get; private set; }
            public int LastStackCount { get; private set; }

            public void ApplyAugment(AugmentDataSO augment, int newStackCount)
            {
                LastApplied = augment;
                LastStackCount = newStackCount;
            }
        }

        [SetUp]
        public void SetUp()
        {
            _managerObj = new GameObject("Test_AugmentManager");
            _manager = _managerObj.AddComponent<AugmentManager>();
            _catalog = new List<AugmentDataSO>();

            CreateTestAugment("b1", "Bronze 1", AugmentTier.Bronze);
            CreateTestAugment("b2", "Bronze 2", AugmentTier.Bronze);
            CreateTestAugment("s1", "Silver 1", AugmentTier.Silver);
            CreateTestAugment("s2", "Silver 2", AugmentTier.Silver);
            CreateTestAugment("g1", "Gold 1", AugmentTier.Gold);
            CreateTestAugment("g2", "Gold 2", AugmentTier.Gold);
            CreateTestAugment("g3", "Gold 3", AugmentTier.Gold);

            _manager.SetCatalog(_catalog);
        }

        [TearDown]
        public void TearDown()
        {
            if (_managerObj != null) Object.DestroyImmediate(_managerObj);
            if (_catalog != null)
            {
                foreach (var item in _catalog)
                {
                    if (item != null) Object.DestroyImmediate(item);
                }
            }
        }

        private void CreateTestAugment(string id, string title, AugmentTier tier)
        {
            AugmentDataSO data = ScriptableObject.CreateInstance<AugmentDataSO>();
            data.augmentId = id;
            data.displayName = title;
            data.tier = tier;
            data.allowStacking = false;
            data.maxStacks = 1;
            _catalog.Add(data);
        }

        [Test]
        public void TC_AUG_01_RollAugmentChoices_ReturnsThreeUniqueCards()
        {
            AugmentDataSO[] rolled = _manager.RollAugmentChoices(3, null);

            Assert.AreEqual(3, rolled.Length, "Standard roll must return exactly 3 augment cards.");
            HashSet<string> ids = new HashSet<string>();
            foreach (var card in rolled)
            {
                Assert.IsNotNull(card);
                Assert.IsTrue(ids.Add(card.augmentId), $"Duplicate augment ID detected: {card.augmentId}");
            }
        }

        [Test]
        public void TC_AUG_02_RollAugmentChoices_RespectsGuaranteedGoldTierForBossOrbs()
        {
            AugmentDataSO[] rolled = _manager.RollAugmentChoices(3, AugmentTier.Gold);

            Assert.AreEqual(3, rolled.Length);
            foreach (var card in rolled)
            {
                Assert.AreEqual(AugmentTier.Gold, card.tier, "Gold Boss Orb must only roll Gold-tier cards when 3+ Gold cards remain.");
            }
        }

        [Test]
        public void TC_AUG_03_RollAugmentChoices_HandlesFewerThanThreeRemainingWithoutHanging()
        {
            // Mark 5 of 7 augments as already owned so only 2 remain
            var mock = new MockAugmentReceiver();
            for (int i = 0; i < 5; i++)
            {
                _manager.SelectAugment(_catalog[i], mock);
            }

            AugmentDataSO[] rolled = _manager.RollAugmentChoices(3, null);
            Assert.AreEqual(2, rolled.Length, "Roll must cleanly return the 2 remaining cards instead of looping infinitely.");
        }

        [Test]
        public void TC_AUG_04_SelectAugment_NotifiesReceiverAndIncrementsStack()
        {
            var mock = new MockAugmentReceiver();
            AugmentDataSO target = _catalog[4]; // Gold 1

            bool ok = _manager.SelectAugment(target, mock);

            Assert.IsTrue(ok);
            Assert.AreEqual(target, mock.LastApplied);
            Assert.AreEqual(1, mock.LastStackCount);
            Assert.AreEqual(1, _manager.GetStackCount(target.augmentId));
        }

        [Test]
        public void TC_AUG_05_DetermineDropTier_UpgradesToGoldOnCalamityNights()
        {
            Assert.AreEqual(AugmentTier.Bronze, AugmentDropController.DetermineDropTier(false, 1, false));
            Assert.AreEqual(AugmentTier.Silver, AugmentDropController.DetermineDropTier(true, 2, false));
            Assert.AreEqual(AugmentTier.Gold, AugmentDropController.DetermineDropTier(true, 5, true));
            Assert.AreEqual(AugmentTier.Gold, AugmentDropController.DetermineDropTier(true, 10, false));
        }
    }
}
#endif
