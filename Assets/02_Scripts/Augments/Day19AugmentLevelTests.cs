#if UNITY_EDITOR || UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using FindTheLover.Augments;
using FindTheLover.Augments.Data;

namespace FindTheLover.Tests.Augments
{
    /// <summary>
    /// Co-located Automated Unit Test Suite validating 3-Level TFT Augment upgrades (Lv.1 -> Lv.2 -> Lv.3),
    /// deterministic zero-drift RecalculateAllStats(), and TFT upgrade badge formatting.
    /// </summary>
    [TestFixture]
    public class Day19AugmentLevelTests
    {
        private GameObject _mgrObj;
        private GameObject _playerObj;
        private AugmentManager _manager;
        private PlayerAugmentStats _stats;
        private AugmentDataSO _speedAug;

        [SetUp]
        public void SetUp()
        {
            _mgrObj = new GameObject("Test_AugmentManager");
            _manager = _mgrObj.AddComponent<AugmentManager>();

            _playerObj = new GameObject("Test_Player");
            _stats = _playerObj.AddComponent<PlayerAugmentStats>();

            _speedAug = ScriptableObject.CreateInstance<AugmentDataSO>();
            _speedAug.augmentId = "test_wind_sneakers";
            _speedAug.displayName = "Wind Sneakers";
            _speedAug.tier = AugmentTier.Bronze;
            _speedAug.effectType = AugmentEffectType.MoveSpeedPercent;
            _speedAug.primaryValue = 0.20f;
            _speedAug.allowStacking = true;
            _speedAug.maxStacks = 3;
            _speedAug.levelPrimaryMultipliers = new float[] { 1.0f, 1.75f, 2.75f };

            _manager.SetCatalog(new List<AugmentDataSO> { _speedAug });
        }

        [TearDown]
        public void TearDown()
        {
            if (_mgrObj != null) Object.DestroyImmediate(_mgrObj);
            if (_playerObj != null) Object.DestroyImmediate(_playerObj);
            if (_speedAug != null) Object.DestroyImmediate(_speedAug);
        }

        [Test]
        public void TC_AUGLV_01_UpgradingAugmentFromLv1ToLv3_ScalesValueAndStopsOfferingAtMax()
        {
            // Lv.1 -> +20% (1.20x)
            _manager.SelectAugment(_speedAug, _stats);
            Assert.AreEqual(1, _manager.GetStackCount(_speedAug.augmentId));
            Assert.AreEqual(1.20f, _stats.MoveSpeedMultiplier, 0.001f);

            // Still eligible for Lv.2
            Assert.AreEqual(1, _manager.RollAugmentChoices(3).Length);

            // Lv.2 -> 0.20 * 1.75 = +35% (1.35x)
            _manager.SelectAugment(_speedAug, _stats);
            Assert.AreEqual(2, _manager.GetStackCount(_speedAug.augmentId));
            Assert.AreEqual(1.35f, _stats.MoveSpeedMultiplier, 0.001f);

            // Lv.3 MAX -> 0.20 * 2.75 = +55% (1.55x)
            _manager.SelectAugment(_speedAug, _stats);
            Assert.AreEqual(3, _manager.GetStackCount(_speedAug.augmentId));
            Assert.AreEqual(1.55f, _stats.MoveSpeedMultiplier, 0.001f);

            // Once Lv.3 MAX is reached, augment must no longer be offered
            Assert.AreEqual(0, _manager.RollAugmentChoices(3).Length, "Maxed Lv.3 augment must be excluded from future rolls.");
        }

        [Test]
        public void TC_AUGLV_02_RecalculateAllStats_IsIdempotentWithZeroDriftAcrossReloads()
        {
            var saved = new List<(AugmentDataSO, int)> { (_speedAug, 2) };

            // Simulate loading the same JSON save 3 times consecutively
            _manager.RestoreAugmentsFromData(saved, _stats);
            float firstLoad = _stats.MoveSpeedMultiplier;

            _manager.RestoreAugmentsFromData(saved, _stats);
            float secondLoad = _stats.MoveSpeedMultiplier;

            _manager.RestoreAugmentsFromData(saved, _stats);
            float thirdLoad = _stats.MoveSpeedMultiplier;

            Assert.AreEqual(1.35f, firstLoad, 0.001f);
            Assert.AreEqual(firstLoad, secondLoad, 0.0001f, "Reloading JSON save must never double-stack stats.");
            Assert.AreEqual(firstLoad, thirdLoad, 0.0001f, "Repeated reloads must stay 100% deterministic.");
        }

        [Test]
        public void TC_AUGLV_03_FormatUpgradeBadge_ReturnsNewLv2AndMaxLv3Labels()
        {
            Assert.AreEqual("NEW  (LV.1)", AugmentDataSO.FormatUpgradeBadge(0, 3));
            Assert.AreEqual("UPGRADE  LV.1 -> LV.2", AugmentDataSO.FormatUpgradeBadge(1, 3));
            Assert.AreEqual("MAX UPGRADE  LV.2 -> LV.3", AugmentDataSO.FormatUpgradeBadge(2, 3));
            Assert.AreEqual("MAXED  (LV.3)", AugmentDataSO.FormatUpgradeBadge(3, 3));
        }
    }
}
#endif
