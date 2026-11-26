#if UNITY_EDITOR || UNITY_INCLUDE_TESTS
using NUnit.Framework;
using UnityEngine;
using FindTheLover.UI;
using FindTheLover.UI.Data;

namespace FindTheLover.Tests.UI
{
    /// <summary>
    /// Co-located Automated Unit Test Suite for Floating Combat Damage Numbers:
    /// verifies animation math curves (lift, punch scale, fade-out) and Zero-Allocation Pool recycling.
    /// </summary>
    [TestFixture]
    public class DamageNumberTests
    {
        private GameObject _spawnerObj;
        private DamageNumberSpawner _spawner;
        private DamageNumberStyleSO _style;

        [SetUp]
        public void SetUp()
        {
            _style = ScriptableObject.CreateInstance<DamageNumberStyleSO>();
            _style.prewarmPoolSize = 4;
            _style.lifetime = 0.8f;
            _style.riseHeight = 2.0f;
            _style.fadeStartNormalized = 0.5f;

            _spawnerObj = new GameObject("Test_DamageNumberSpawner");
            _spawner = _spawnerObj.AddComponent<DamageNumberSpawner>();
            _spawner.Configure(_style);
        }

        [TearDown]
        public void TearDown()
        {
            if (_spawnerObj != null) Object.DestroyImmediate(_spawnerObj);
            if (_style != null) Object.DestroyImmediate(_style);
        }

        [Test]
        public void TC_DMGNUM_01_EvaluateAnimation_LiftsPunchesAndFadesDeterministically()
        {
            DamageNumberPopup.EvaluateAnimation(0.0f, 2.0f, 1.5f, 0.5f, out float y0, out float s0, out float a0);
            DamageNumberPopup.EvaluateAnimation(0.22f, 2.0f, 1.5f, 0.5f, out float yPeak, out float sPeak, out float aPeak);
            DamageNumberPopup.EvaluateAnimation(1.0f, 2.0f, 1.5f, 0.5f, out float yEnd, out float sEnd, out float aEnd);

            Assert.AreEqual(0f, y0, 0.001f);
            Assert.AreEqual(1f, a0, 0.001f);

            Assert.Greater(yPeak, 0.9f, "Ease-out lift must rise rapidly by t=0.22.");
            Assert.AreEqual(1.5f, sPeak, 0.05f, "Punch scale must reach peakScale at t=0.22.");
            Assert.AreEqual(1f, aPeak, 0.001f, "Popup must stay fully opaque before fadeStartNormalized.");

            Assert.AreEqual(2.0f, yEnd, 0.001f, "Popup must reach exact riseHeight at t=1.");
            Assert.AreEqual(0f, aEnd, 0.001f, "Popup alpha must fade cleanly to 0 at t=1.");
        }

        [Test]
        public void TC_DMGNUM_02_Spawner_PrewarmsPoolAndFormatsCriticalDamageText()
        {
            int initialPooled = _spawner.PooledCount;
            Assert.GreaterOrEqual(initialPooled, 4, "Pool must prewarm at least prewarmPoolSize popups.");

            DamageNumberPopup critPopup = _spawner.SpawnDamage(56.4f, Vector3.zero, DamagePopupStyle.Critical);
            Assert.IsNotNull(critPopup, "SpawnDamage must return an active pooled popup.");
            Assert.IsTrue(critPopup.IsPlaying, "Popup must be actively playing upon spawn.");
            Assert.AreEqual("56!", critPopup.GetComponent<TMPro.TextMeshPro>().text, "Critical damage must append exclamation mark.");
            Assert.AreEqual(initialPooled - 1, _spawner.PooledCount, "Acquiring a popup must decrement available pool count by 1.");
        }
    }
}
#endif
