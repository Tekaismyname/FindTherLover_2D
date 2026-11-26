#if UNITY_EDITOR || UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using FindTheLover.Crafting;
using FindTheLover.Crafting.Data;
using FindTheLover.Equipment.Data;
using FindTheLover.Items;
using FindTheLover.Player;
using FindTherLover.Player;

namespace FindTheLover.Tests.Crafting
{
    /// <summary>
    /// Co-located Automated Unit Test Suite for <see cref="CraftingForge"/>, <see cref="PlayerHotbarController"/>,
    /// and Modular Hand Socket <see cref="PlayerEquipment"/> swing curves.
    /// </summary>
    [TestFixture]
    public class CraftingAndHotbarTests
    {
        private GameObject _playerObj;
        private GameObject _forgeObj;
        private InventorySystem _inventory;
        private PlayerHotbarController _hotbar;
        private CraftingForge _forge;
        private ItemDataSO _wood;
        private ItemDataSO _iron;
        private EquipmentItemDataSO _ironSword;
        private CraftingRecipeSO _recipe;

        [SetUp]
        public void SetUp()
        {
            _playerObj = new GameObject("Test_Player");
            _inventory = _playerObj.AddComponent<InventorySystem>();
            _hotbar = _playerObj.AddComponent<PlayerHotbarController>();

            _forgeObj = new GameObject("Test_Forge");
            _forge = _forgeObj.AddComponent<CraftingForge>();

            _wood = ScriptableObject.CreateInstance<ItemDataSO>();
            _wood.itemId = "wood_log";
            _wood.itemName = "Wood";

            _iron = ScriptableObject.CreateInstance<ItemDataSO>();
            _iron.itemId = "iron_ore";
            _iron.itemName = "Iron Ore";

            _ironSword = ScriptableObject.CreateInstance<EquipmentItemDataSO>();
            _ironSword.itemId = "sword_iron";
            _ironSword.displayName = "Iron Sword";
            _ironSword.equipmentType = EquipmentType.Sword;
            _ironSword.materialTier = WeaponMaterialTier.Iron;
            _ironSword.weaponDamageBonus = 22f;

            _recipe = ScriptableObject.CreateInstance<CraftingRecipeSO>();
            _recipe.recipeId = "recipe_iron_sword";
            _recipe.displayName = "Iron Sword";
            _recipe.tier = WeaponMaterialTier.Iron;
            _recipe.resultEquipment = _ironSword;
            _recipe.ingredients = new List<CraftingIngredient>
            {
                new CraftingIngredient { item = _wood, requiredAmount = 5 },
                new CraftingIngredient { item = _iron, requiredAmount = 4 }
            };
        }

        [TearDown]
        public void TearDown()
        {
            if (_playerObj != null) Object.DestroyImmediate(_playerObj);
            if (_forgeObj != null) Object.DestroyImmediate(_forgeObj);
            if (_wood != null) Object.DestroyImmediate(_wood);
            if (_iron != null) Object.DestroyImmediate(_iron);
            if (_ironSword != null) Object.DestroyImmediate(_ironSword);
            if (_recipe != null) Object.DestroyImmediate(_recipe);
        }

        [Test]
        public void TC_CRAFT_01_CanCraft_RequiresExactIngredientThresholds()
        {
            _inventory.AddItem(_wood, 5);
            _inventory.AddItem(_iron, 3); // 1 Iron short
            Assert.IsFalse(_forge.CanCraft(_recipe, _inventory), "CanCraft must return false when 1 Iron Ore is missing.");

            _inventory.AddItem(_iron, 1); // Now 4 Iron
            Assert.IsTrue(_forge.CanCraft(_recipe, _inventory), "CanCraft must return true once all ingredients are met.");
        }

        [Test]
        public void TC_CRAFT_02_TryCraft_DeductsIngredientsAndUpgradesHotbarSlot()
        {
            _inventory.AddItem(_wood, 8);
            _inventory.AddItem(_iron, 4);

            bool crafted = _forge.TryCraft(_recipe, _inventory, _hotbar);

            Assert.IsTrue(crafted, "TryCraft must succeed when ingredients are sufficient.");
            Assert.AreEqual(3, _inventory.GetItemCount(_wood), "Remaining wood must be 8 - 5 = 3.");
            Assert.AreEqual(0, _inventory.GetItemCount(_iron), "Remaining iron must be 4 - 4 = 0.");
            Assert.AreEqual(_ironSword, _hotbar.GetEquipmentInSlot(0), "Crafted Iron Sword must upgrade Hotbar Slot 0 (Sword).");
        }

        [Test]
        public void TC_SOCKET_03_EvaluateSwingAngle_InterpolatesSmoothlyFromStartToEnd()
        {
            float atStart = PlayerEquipment.EvaluateSwingAngle(55f, -105f, 0f);
            float atMid = PlayerEquipment.EvaluateSwingAngle(55f, -105f, 0.5f);
            float atEnd = PlayerEquipment.EvaluateSwingAngle(55f, -105f, 1f);

            Assert.AreEqual(55f, atStart, 0.001f);
            Assert.Less(atMid, 0f, "Ease-out cubic swing must accelerate past midpoint (>50% sweep) by t=0.5.");
            Assert.AreEqual(-105f, atEnd, 0.001f);
        }
    }
}
#endif
