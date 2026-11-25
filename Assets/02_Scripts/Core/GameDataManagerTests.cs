#if UNITY_EDITOR || UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using FindTheLover.Augments;
using FindTheLover.Augments.Data;
using FindTheLover.Core;
using FindTheLover.Core.Data;
using FindTheLover.Equipment.Data;
using FindTheLover.Items;

namespace FindTheLover.Tests.Core
{
    /// <summary>
    /// Co-located Automated Unit Test Suite for <see cref="GameDataManager"/> and <see cref="GameCatalogSO"/>.
    /// Verifies JSON round-trip persistence, catalog lookup resilience, and zero double-stacking on reload.
    /// </summary>
    [TestFixture]
    public class GameDataManagerTests
    {
        private GameCatalogSO _catalog;
        private ItemDataSO _woodItem;
        private EquipmentItemDataSO _torchEquip;
        private AugmentDataSO _sneakersAug;
        private const string TestSlot = "__unit_test_gamedata_slot";

        [SetUp]
        public void SetUp()
        {
            _catalog = ScriptableObject.CreateInstance<GameCatalogSO>();
            _woodItem = ScriptableObject.CreateInstance<ItemDataSO>();
            _woodItem.itemId = "wood_log";
            _woodItem.itemName = "Wood Log";

            _torchEquip = ScriptableObject.CreateInstance<EquipmentItemDataSO>();
            _torchEquip.itemId = "item_torch";
            _torchEquip.displayName = "Survival Torch";

            _sneakersAug = ScriptableObject.CreateInstance<AugmentDataSO>();
            _sneakersAug.augmentId = "aug_bronze_sneakers";
            _sneakersAug.displayName = "Wind Sneakers";
            _sneakersAug.effectType = AugmentEffectType.MoveSpeedPercent;
            _sneakersAug.primaryValue = 0.20f;

            _catalog.items.Add(_woodItem);
            _catalog.equipmentItems.Add(_torchEquip);
            _catalog.augments.Add(_sneakersAug);
            _catalog.RebuildLookups();
        }

        [TearDown]
        public void TearDown()
        {
            string path = GameDataManager.GetSaveFilePath(TestSlot);
            if (File.Exists(path)) File.Delete(path);

            if (_catalog != null) Object.DestroyImmediate(_catalog);
            if (_woodItem != null) Object.DestroyImmediate(_woodItem);
            if (_torchEquip != null) Object.DestroyImmediate(_torchEquip);
            if (_sneakersAug != null) Object.DestroyImmediate(_sneakersAug);
        }

        [Test]
        public void TC_DATA_01_GameSaveData_JsonRoundTripPreservesAllFields()
        {
            GameSaveData original = new GameSaveData();
            original.world.worldSeed = 777777;
            original.world.currentDay = 12;
            original.player.position = new Vector3(42.5f, 8.0f, -18.25f);
            original.player.currentHealth = 85f;
            original.player.equippedItemId = "item_torch";
            original.inventorySlots.Add(new InventorySlotSaveData { itemId = "wood_log", quantity = 37 });
            original.ownedAugments.Add(new OwnedAugmentSaveData { augmentId = "aug_bronze_sneakers", stackCount = 2 });
            original.worldObjectStates.Add(new WorldObjectStateSaveData { objectId = "brazier:Brazier_Checkpoint_01", isActivated = true });

            string json = JsonUtility.ToJson(original, true);
            GameSaveData restored = JsonUtility.FromJson<GameSaveData>(json);

            Assert.AreEqual(777777, restored.world.worldSeed);
            Assert.AreEqual(12, restored.world.currentDay);
            Assert.AreEqual(42.5f, restored.player.position.x, 0.001f);
            Assert.AreEqual("item_torch", restored.player.equippedItemId);
            Assert.AreEqual(1, restored.inventorySlots.Count);
            Assert.AreEqual(37, restored.inventorySlots[0].quantity);
            Assert.AreEqual(2, restored.ownedAugments[0].stackCount);
            Assert.IsTrue(restored.worldObjectStates[0].isActivated);
        }

        [Test]
        public void TC_DATA_02_GameCatalog_ResolvesKnownIdsAndSafelyIgnoresMissingIds()
        {
            Assert.AreEqual(_woodItem, _catalog.GetItemById("wood_log"));
            Assert.AreEqual(_torchEquip, _catalog.GetEquipmentById("item_torch"));
            Assert.AreEqual(_sneakersAug, _catalog.GetAugmentById("aug_bronze_sneakers"));
            Assert.IsNull(_catalog.GetItemById("deprecated_id_999"), "Unknown IDs must cleanly return null instead of throwing.");
            Assert.IsNull(_catalog.GetAugmentById(string.Empty));
        }

        [Test]
        public void TC_DATA_03_PlayerAugmentStats_ResetPreventsDoubleStackingOnReload()
        {
            GameObject playerObj = new GameObject("Test_PlayerStats");
            try
            {
                PlayerAugmentStats stats = playerObj.AddComponent<PlayerAugmentStats>();
                stats.ApplyAugment(_sneakersAug, 1);
                Assert.AreEqual(1.20f, stats.MoveSpeedMultiplier, 0.001f);

                // Simulate reloading save: reset first, then re-apply 1 stack -> must stay 1.20f (NOT 1.40f!)
                stats.ResetAllAugmentStats();
                stats.ApplyAugment(_sneakersAug, 1);
                Assert.AreEqual(1.20f, stats.MoveSpeedMultiplier, 0.001f, "Reloading saved augments must never double-stack stats.");
            }
            finally
            {
                Object.DestroyImmediate(playerObj);
            }
        }
    }
}
#endif
