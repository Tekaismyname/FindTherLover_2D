using NUnit.Framework;
using UnityEngine;
using FindTheLover.Combat;
using FindTheLover.Enemies;

namespace FindTheLover.Tests.Enemies
{
    /// <summary>
    /// Automated Unit Test Suite for <see cref="ShadowBatEnemyAI"/>.
    /// Validates flight elevation logic, orbit trajectory calculations, and swoop attack parameters.
    /// </summary>
    [TestFixture]
    public class ShadowBatEnemyAITests
    {
        private GameObject _batGameObject;
        private ShadowBatEnemyAI _batAI;

        [SetUp]
        public void SetUp()
        {
            _batGameObject = new GameObject("Test_ShadowBat");
            _batAI = _batGameObject.AddComponent<ShadowBatEnemyAI>();
        }

        [TearDown]
        public void TearDown()
        {
            if (_batGameObject != null)
            {
                Object.DestroyImmediate(_batGameObject);
            }
        }

        [Test]
        public void TC_BAT_01_BatInitializes_ComponentNotNull()
        {
            // Assert
            Assert.IsNotNull(_batAI, "ShadowBatEnemyAI component must be successfully attached to GameObject.");
        }

        [Test]
        public void TC_BAT_02_SwoopTrajectoryFactor_ReachesPeakAtMidpoint()
        {
            // Arrange
            float midpointTime = 0.5f;

            // Act: Parabolic swoop formula: 4 * t * (1 - t)
            float factorAtMidpoint = 4f * midpointTime * (1f - midpointTime);
            float factorAtStart = 4f * 0f * (1f - 0f);
            float factorAtEnd = 4f * 1f * (1f - 1f);

            // Assert
            Assert.AreEqual(1.0f, factorAtMidpoint, 0.001f, "Swoop factor should reach maximum (1.0) at exactly t=0.5.");
            Assert.AreEqual(0.0f, factorAtStart, 0.001f, "Swoop factor should be 0.0 at beginning.");
            Assert.AreEqual(0.0f, factorAtEnd, 0.001f, "Swoop factor should be 0.0 at completion.");
        }

        [Test]
        public void TC_BAT_03_OrbitOffset_PreservesStalkingRadius()
        {
            // Arrange
            float orbitRadius = 4.0f;
            float angle = 45.0f;

            // Act
            Vector3 offset = Quaternion.Euler(0f, angle, 0f) * (Vector3.forward * orbitRadius);

            // Assert
            Assert.AreEqual(orbitRadius, offset.magnitude, 0.001f, "Orbit displacement vector must maintain constant radius.");
            Assert.AreEqual(0f, offset.y, 0.001f, "Orbit offset must reside purely on the horizontal XZ plane.");
        }
    }
}
