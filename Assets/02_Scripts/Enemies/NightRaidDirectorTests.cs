#if UNITY_EDITOR || UNITY_INCLUDE_TESTS
using NUnit.Framework;
using UnityEngine;
using FindTheLover.Enemies;
using FindTheLover.Enemies.Data;

namespace FindTheLover.Tests.Enemies
{
    /// <summary>
    /// Co-located Automated Unit Test Suite for <see cref="NightRaidDirector"/>.
    /// Verifies escalating night budget math, calamity boss day detection, and annular ring bounds.
    /// </summary>
    [TestFixture]
    public class NightRaidDirectorTests
    {
        private GameObject _directorObject;
        private NightRaidDirector _director;
        private NightRaidConfigSO _config;

        [SetUp]
        public void SetUp()
        {
            _directorObject = new GameObject("Test_NightRaidDirector");
            _director = _directorObject.AddComponent<NightRaidDirector>();
            _config = ScriptableObject.CreateInstance<NightRaidConfigSO>();
            _config.baseNightBudget = 10;
            _config.budgetPerDayMultiplier = 6;
            _config.calamityBudgetMultiplier = 1.5f;
            _config.calamityBossDays = new int[] { 5, 10, 15, 20 };
            _config.minSpawnRadius = 25f;
            _config.maxSpawnRadius = 35f;
            _director.SetConfig(_config);
        }

        [TearDown]
        public void TearDown()
        {
            if (_directorObject != null) Object.DestroyImmediate(_directorObject);
            if (_config != null) Object.DestroyImmediate(_config);
        }

        [Test]
        public void TC_RAID_01_CalculateNightBudget_ScalesLinearlyOnStandardDays()
        {
            Assert.AreEqual(10, _director.CalculateNightBudget(1), "Day 1 must equal baseNightBudget (10).");
            Assert.AreEqual(16, _director.CalculateNightBudget(2), "Day 2 must equal 10 + 6 = 16.");
            Assert.AreEqual(28, _director.CalculateNightBudget(4), "Day 4 must equal 10 + 3*6 = 28.");
        }

        [Test]
        public void TC_RAID_02_CalculateNightBudget_AppliesCalamityMultiplierOnMilestoneDays()
        {
            // Day 5 raw = 10 + 4*6 = 34; with 1.5x calamity boost = 51
            Assert.IsTrue(_director.IsCalamityBossNight(5), "Day 5 must be recognized as a Calamity Boss Night.");
            Assert.AreEqual(51, _director.CalculateNightBudget(5), "Day 5 budget should apply 1.5x calamity multiplier.");
            Assert.IsFalse(_director.IsCalamityBossNight(7), "Day 7 must not be a Calamity Night.");
        }

        [Test]
        public void TC_RAID_03_CalculateNightBudget_ClampsNegativeOrZeroDaySafely()
        {
            Assert.AreEqual(10, _director.CalculateNightBudget(0), "Day 0 must safely clamp to Day 1 base budget.");
            Assert.AreEqual(10, _director.CalculateNightBudget(-5), "Negative days must never produce negative budgets.");
        }

        [Test]
        public void TC_RAID_04_GetRandomRingPosition_StaysStrictlyWithinDonutBounds()
        {
            Vector3 origin = new Vector3(10f, 5f, -15f);
            for (int i = 0; i < 100; i++)
            {
                Vector3 pos = _director.GetRandomRingPosition(origin);
                float horizontalDist = Vector2.Distance(new Vector2(origin.x, origin.z), new Vector2(pos.x, pos.z));
                Assert.GreaterOrEqual(horizontalDist, 24.99f, "Spawn distance must never enter inner camera radius.");
                Assert.LessOrEqual(horizontalDist, 35.01f, "Spawn distance must never exceed outer island ring.");
                Assert.AreEqual(origin.y, pos.y, 0.001f, "Raw ring offset must preserve center elevation before NavMesh snap.");
            }
        }
    }
}
#endif
