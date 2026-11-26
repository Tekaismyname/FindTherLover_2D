#if UNITY_EDITOR || UNITY_INCLUDE_TESTS
using NUnit.Framework;
using UnityEngine;
using FindTheLover.CameraSystem;
using FindTheLover.Combat;
using FindTheLover.Combat.Data;
using FindTheLover.Environment;
using FindTherLover.Player;

namespace FindTheLover.Tests.MiningAndControls
{
    /// <summary>
    /// Co-located Automated Unit Test Suite validating:
    /// 1. Horizontal-only CameraLookController pitch lock.
    /// 2. 4-Way Modular Hand Socket facing resolution without flipX corruption.
    /// 3. Pickaxe vs Wrong Tool mining damage calculations on Stone & Iron Ore nodes.
    /// </summary>
    [TestFixture]
    public class MiningAndCameraTests
    {
        private ResourceDataSO _stoneData;
        private ResourceDataSO _ironOreData;

        [SetUp]
        public void SetUp()
        {
            _stoneData = ScriptableObject.CreateInstance<ResourceDataSO>();
            _stoneData.resourceName = "Stone Boulder";
            _stoneData.resourceType = ResourceType.Stone;
            _stoneData.requiredTool = HarvestToolType.Pickaxe;
            _stoneData.toolEfficiencyMultiplier = 2.0f;
            _stoneData.wrongToolDamageMultiplier = 0.25f;
            _stoneData.requireExactTool = false;
            _stoneData.maxHealth = 80f;

            _ironOreData = ScriptableObject.CreateInstance<ResourceDataSO>();
            _ironOreData.resourceName = "Iron Ore Vein";
            _ironOreData.resourceType = ResourceType.Iron;
            _ironOreData.requiredTool = HarvestToolType.Pickaxe;
            _ironOreData.toolEfficiencyMultiplier = 2.2f;
            _ironOreData.wrongToolDamageMultiplier = 0f;
            _ironOreData.requireExactTool = true;
            _ironOreData.maxHealth = 130f;
        }

        [TearDown]
        public void TearDown()
        {
            if (_stoneData != null) Object.DestroyImmediate(_stoneData);
            if (_ironOreData != null) Object.DestroyImmediate(_ironOreData);
        }

        [Test]
        public void TC_CAM_01_HorizontalOnlyLock_IgnoresVerticalMouseDelta()
        {
            Vector2 angles = CameraLookController.ComputeTargetAngles(
                currentTargetYaw: 45f,
                currentTargetPitch: 15f,
                lookDelta: new Vector2(10f, 50f),
                sensitivityX: 0.5f,
                sensitivityY: 0.5f,
                invertY: false,
                minPitch: -20f,
                maxPitch: 60f,
                lockVertical: true,
                lockedPitch: 15f);

            Assert.AreEqual(50f, angles.x, 0.001f, "Horizontal Yaw must still rotate freely with mouse X delta.");
            Assert.AreEqual(15f, angles.y, 0.001f, "Vertical Pitch must stay strictly locked at 15 degrees despite huge mouse Y delta.");
        }

        [Test]
        public void TC_SOCKET_02_ResolveFacingDirection_HandlesAllFourDirectionsCleanly()
        {
            Assert.AreEqual(PlayerEquipment.FacingDirection2D.Front, PlayerEquipment.ResolveFacingDirection(new Vector2(0f, -1f)));
            Assert.AreEqual(PlayerEquipment.FacingDirection2D.Back, PlayerEquipment.ResolveFacingDirection(new Vector2(0f, 1f)));
            Assert.AreEqual(PlayerEquipment.FacingDirection2D.Right, PlayerEquipment.ResolveFacingDirection(new Vector2(1f, 0f)));
            Assert.AreEqual(PlayerEquipment.FacingDirection2D.Left, PlayerEquipment.ResolveFacingDirection(new Vector2(-1f, 0f)));
        }

        [Test]
        public void TC_MINE_03_PickaxeDealsFullMiningDamage_WrongToolReducedOrBlocked()
        {
            // Soft Stone: Pickaxe deals 2x (40 DMG), Axe deals 25% chip damage (5 DMG)
            float stoneWithPickaxe = ResourceNode.ComputeHarvestDamage(20f, HarvestToolType.Pickaxe, _stoneData);
            float stoneWithAxe = ResourceNode.ComputeHarvestDamage(20f, HarvestToolType.Axe, _stoneData);
            Assert.AreEqual(40f, stoneWithPickaxe, 0.001f, "Pickaxe must deal full 2x mining damage to Stone.");
            Assert.AreEqual(5f, stoneWithAxe, 0.001f, "Non-pickaxe tool must deal reduced 0.25x chip damage to Stone.");

            // Hard Iron Ore Vein (requireExactTool = true): Pickaxe deals 2.2x (44 DMG), Axe/Hand deals 0 DMG
            float ironWithPickaxe = ResourceNode.ComputeHarvestDamage(20f, HarvestToolType.Pickaxe, _ironOreData);
            float ironWithSword = ResourceNode.ComputeHarvestDamage(20f, HarvestToolType.None, _ironOreData);
            Assert.AreEqual(44f, ironWithPickaxe, 0.001f, "Pickaxe must deal 2.2x damage to Iron Ore.");
            Assert.AreEqual(0f, ironWithSword, 0.001f, "Hard Iron Ore Vein must reject non-Pickaxe hits (0 DMG).");
        }
    }
}
#endif
