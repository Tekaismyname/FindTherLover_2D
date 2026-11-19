#if UNITY_EDITOR || UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using FindTheLover.Environment;
using FindTheLover.Environment.Data;

namespace FindTheLover.Tests.Environment
{
    /// <summary>
    /// Co-located Automated Unit Test Suite for <see cref="SeededWorldTerrainManager"/>.
    /// Verifies Co-op seed determinism, seed variety, Early-Game vs End-Game relief differentiation, and anchor flattening.
    /// </summary>
    [TestFixture]
    public class SeededWorldTerrainTests
    {
        private SeededTerrainProfileSO _earlyProfile;
        private SeededTerrainProfileSO _endGameProfile;

        [SetUp]
        public void SetUp()
        {
            _earlyProfile = ScriptableObject.CreateInstance<SeededTerrainProfileSO>();
            _earlyProfile.archetype = TerrainZoneArchetype.EarlyGameMeadow;
            _earlyProfile.islandCenterXZ = Vector2.zero;
            _earlyProfile.baseGroundHeight = 8.0f;
            _earlyProfile.islandBaseRadius = 200f;
            _earlyProfile.interiorReliefAmplitude = 3.5f;
            _earlyProfile.perimeterRimHeight = 24f;
            _earlyProfile.protectedAnchors = new List<TerrainAnchorZone>
            {
                new TerrainAnchorZone { label = "Campfire", worldCenterXZ = Vector2.zero, flatRadius = 24f, blendWidth = 16f, targetWorldHeight = 8.0f }
            };

            _endGameProfile = ScriptableObject.CreateInstance<SeededTerrainProfileSO>();
            _endGameProfile.archetype = TerrainZoneArchetype.EndGameCalamity;
            _endGameProfile.islandCenterXZ = new Vector2(450f, 0f);
            _endGameProfile.baseGroundHeight = 8.0f;
            _endGameProfile.islandBaseRadius = 175f;
            _endGameProfile.interiorReliefAmplitude = 9.5f;
            _endGameProfile.perimeterRimHeight = 46f;
            _endGameProfile.ridgeSharpness = 2.4f;
            _endGameProfile.protectedAnchors = new List<TerrainAnchorZone>
            {
                new TerrainAnchorZone { label = "Boss Caldera", worldCenterXZ = new Vector2(585f, 0f), flatRadius = 32f, blendWidth = 14f, targetWorldHeight = 8.5f }
            };
        }

        [TearDown]
        public void TearDown()
        {
            if (_earlyProfile != null) Object.DestroyImmediate(_earlyProfile);
            if (_endGameProfile != null) Object.DestroyImmediate(_endGameProfile);
        }

        [Test]
        public void TC_SEED_01_IdenticalSeed_ProducesByteIdenticalHeightAcrossRuns()
        {
            Vector2 off1 = SeededWorldTerrainManager.GenerateSeedOffset(20261119, 1);
            Vector2 off2 = SeededWorldTerrainManager.GenerateSeedOffset(20261119, 1);
            Vector2 coast1 = SeededWorldTerrainManager.GenerateSeedOffset(20261119, 2);
            Vector2 coast2 = SeededWorldTerrainManager.GenerateSeedOffset(20261119, 2);

            float hA = SeededWorldTerrainManager.EvaluateWorldHeight(75f, -40f, _earlyProfile, off1, coast1);
            float hB = SeededWorldTerrainManager.EvaluateWorldHeight(75f, -40f, _earlyProfile, off2, coast2);

            Assert.AreEqual(hA, hB, 0.0001f, "Identical seeds must yield identical heightmaps for multiplayer Co-op sync.");
        }

        [Test]
        public void TC_SEED_02_DifferentSeeds_ProduceDistinctTerrainRelief()
        {
            Vector2 offA = SeededWorldTerrainManager.GenerateSeedOffset(1001, 1);
            Vector2 coastA = SeededWorldTerrainManager.GenerateSeedOffset(1001, 2);
            Vector2 offB = SeededWorldTerrainManager.GenerateSeedOffset(99999, 1);
            Vector2 coastB = SeededWorldTerrainManager.GenerateSeedOffset(99999, 2);

            float diffSum = 0f;
            for (int x = 40; x <= 120; x += 20)
            {
                float hA = SeededWorldTerrainManager.EvaluateWorldHeight(x, 35f, _earlyProfile, offA, coastA);
                float hB = SeededWorldTerrainManager.EvaluateWorldHeight(x, 35f, _earlyProfile, offB, coastB);
                diffSum += Mathf.Abs(hA - hB);
            }

            Assert.Greater(diffSum, 1.5f, "Different world seeds must visibly vary hill elevations across the island.");
        }

        [Test]
        public void TC_SEED_03_ProtectedAnchors_StayStrictlyFlatAtTargetHeightAcrossAllSeeds()
        {
            int[] testSeeds = { 1, 42, 7777, 20261119 };
            foreach (int s in testSeeds)
            {
                Vector2 off = SeededWorldTerrainManager.GenerateSeedOffset(s, 1);
                Vector2 coast = SeededWorldTerrainManager.GenerateSeedOffset(s, 2);

                float campfireH = SeededWorldTerrainManager.EvaluateWorldHeight(0f, 0f, _earlyProfile, off, coast);
                float calderaH = SeededWorldTerrainManager.EvaluateWorldHeight(585f, 0f, _endGameProfile, off, coast);

                Assert.AreEqual(8.0f, campfireH, 0.01f, $"Campfire anchor height must stay 8.0m on seed {s}.");
                Assert.AreEqual(8.5f, calderaH, 0.01f, $"End-Game Boss Caldera height must stay 8.5m on seed {s}.");
            }
        }

        [Test]
        public void TC_SEED_04_EndGameProfile_HasHigherReliefThanEarlyGameProfile()
        {
            Assert.Greater(_endGameProfile.perimeterRimHeight, _earlyProfile.perimeterRimHeight,
                "End-Game perimeter ridges must tower higher than Early-Game meadow cliffs.");
            Assert.Greater(_endGameProfile.interiorReliefAmplitude, _earlyProfile.interiorReliefAmplitude,
                "End-Game terrain must have sharper, more rugged elevation changes than Early-Game terrain.");
        }
    }
}
#endif
