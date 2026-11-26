using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using FindTheLover.Crafting;
using FindTheLover.Crafting.Data;
using FindTheLover.Equipment.Data;
using FindTheLover.Items;
using FindTheLover.Player;

namespace FindTheLover.UI
{
    /// <summary>
    /// Presentation Layer: Muck-Style Blacksmith Crafting Modal toggled via Key C (or interacting with Anvil).
    /// Dynamically renders recipe rows with icons, live ingredient availability coloring, and one-click CRAFT buttons.
    /// </summary>
    public class CraftingModalUI : MonoBehaviour
    {
        [Header("Modal References")]
        [SerializeField] private GameObject _modalRootPanel;
        [SerializeField] private Transform _recipeListContainer;
        [SerializeField] private Button _closeButton;

        [Header("Toggle Settings")]
        [Tooltip("Allow toggling Crafting Forge anywhere via Key C.")]
        [SerializeField] private bool _allowHotkeyToggle = true;

        private readonly List<GameObject> _spawnedRows = new List<GameObject>();
        private InventorySystem _playerInventory;
        private PlayerHotbarController _playerHotbar;
        private bool _isOpen;
        private CursorLockMode _prevCursorLock;
        private bool _prevCursorVisible;

        public bool IsOpen => _isOpen;

        private void Start()
        {
            if (_modalRootPanel != null) _modalRootPanel.SetActive(false);
            if (_closeButton != null) _closeButton.onClick.AddListener(CloseModal);

            LocatePlayerComponents();
            if (_playerInventory != null) _playerInventory.OnInventoryChanged += HandleInventoryChanged;
        }

        private void OnDestroy()
        {
            if (_closeButton != null) _closeButton.onClick.RemoveListener(CloseModal);
            if (_playerInventory != null) _playerInventory.OnInventoryChanged -= HandleInventoryChanged;
        }

        private void Update()
        {
            if (!_allowHotkeyToggle || Keyboard.current == null) return;

            if (Keyboard.current.cKey.wasPressedThisFrame)
            {
                if (_isOpen) CloseModal();
                else OpenModal();
            }
            else if (_isOpen && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                CloseModal();
            }
        }

        public void OpenModal()
        {
            if (_modalRootPanel == null) return;
            LocatePlayerComponents();

            _isOpen = true;
            _modalRootPanel.SetActive(true);
            RebuildRecipeList();

            _prevCursorLock = Cursor.lockState;
            _prevCursorVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void CloseModal()
        {
            _isOpen = false;
            if (_modalRootPanel != null) _modalRootPanel.SetActive(false);

            Cursor.lockState = _prevCursorLock;
            Cursor.visible = _prevCursorVisible;
        }

        /// <summary>
        /// Rebuilds the visible recipe rows based on CraftingForge.Instance.Recipes and live Inventory counts.
        /// </summary>
        public void RebuildRecipeList()
        {
            if (_recipeListContainer == null || CraftingForge.Instance == null) return;
            LocatePlayerComponents();

            for (int i = 0; i < _spawnedRows.Count; i++)
            {
                if (_spawnedRows[i] != null) Destroy(_spawnedRows[i]);
            }
            _spawnedRows.Clear();

            var recipes = CraftingForge.Instance.Recipes;
            for (int i = 0; i < recipes.Count; i++)
            {
                CraftingRecipeSO recipe = recipes[i];
                if (recipe == null) continue;
                _spawnedRows.Add(BuildRecipeRow(recipe));
            }
        }

        private GameObject BuildRecipeRow(CraftingRecipeSO recipe)
        {
            bool canCraft = CraftingForge.Instance != null && CraftingForge.Instance.CanCraft(recipe, _playerInventory);
            Color tierColor = EquipmentItemDataSO.GetCanonicalTierColor(recipe.tier);

            GameObject row = new GameObject($"RecipeRow_{recipe.recipeId}", typeof(RectTransform));
            row.transform.SetParent(_recipeListContainer, false);

            LayoutElement le = row.AddComponent<LayoutElement>();
            le.preferredHeight = 64f;
            le.minHeight = 64f;

            Image rowBg = row.AddComponent<Image>();
            rowBg.color = canCraft ? new Color(0.14f, 0.20f, 0.28f, 0.95f) : new Color(0.10f, 0.11f, 0.16f, 0.88f);

            // 1. Icon Box
            GameObject iconObj = new GameObject("Icon", typeof(RectTransform));
            iconObj.transform.SetParent(row.transform, false);
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0f, 0.5f);
            iconRect.anchoredPosition = new Vector2(12f, 0f);
            iconRect.sizeDelta = new Vector2(46f, 46f);
            Image iconImg = iconObj.AddComponent<Image>();
            iconImg.sprite = recipe.GetResultIcon();
            iconImg.color = recipe.GetResultTint();

            // 2. Title + Ingredients Summary
            GameObject textObj = new GameObject("InfoText", typeof(RectTransform));
            textObj.transform.SetParent(row.transform, false);
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.offsetMin = new Vector2(72f, 4f);
            textRect.offsetMax = new Vector2(-130f, -4f);

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 15f;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.text = FormatRecipeText(recipe, tierColor);

            // 3. Craft Button
            GameObject btnObj = new GameObject("CraftButton", typeof(RectTransform));
            btnObj.transform.SetParent(row.transform, false);
            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(1f, 0.5f);
            btnRect.anchorMax = new Vector2(1f, 0.5f);
            btnRect.pivot = new Vector2(1f, 0.5f);
            btnRect.anchoredPosition = new Vector2(-12f, 0f);
            btnRect.sizeDelta = new Vector2(106f, 40f);

            Image btnImg = btnObj.AddComponent<Image>();
            btnImg.color = canCraft ? new Color(0.20f, 0.72f, 0.36f, 1f) : new Color(0.32f, 0.34f, 0.40f, 0.85f);

            Button btn = btnObj.AddComponent<Button>();
            btn.interactable = canCraft;
            CraftingRecipeSO captured = recipe;
            btn.onClick.AddListener(() =>
            {
                if (CraftingForge.Instance != null && CraftingForge.Instance.TryCraft(captured, _playerInventory, _playerHotbar))
                {
                    RebuildRecipeList();
                }
            });

            GameObject btnTextObj = new GameObject("Label", typeof(RectTransform));
            btnTextObj.transform.SetParent(btnObj.transform, false);
            RectTransform btRect = btnTextObj.GetComponent<RectTransform>();
            btRect.anchorMin = Vector2.zero;
            btRect.anchorMax = Vector2.one;
            btRect.offsetMin = Vector2.zero;
            btRect.offsetMax = Vector2.zero;

            TextMeshProUGUI btnTmp = btnTextObj.AddComponent<TextMeshProUGUI>();
            btnTmp.text = canCraft ? "CRAFT" : "LOCKED";
            btnTmp.fontSize = 15f;
            btnTmp.fontStyle = FontStyles.Bold;
            btnTmp.alignment = TextAlignmentOptions.Center;
            btnTmp.color = Color.white;

            return row;
        }

        private string FormatRecipeText(CraftingRecipeSO recipe, Color tierColor)
        {
            string hex = ColorUtility.ToHtmlStringRGB(tierColor);
            StringBuilder sb = new StringBuilder();
            sb.Append($"<b><color=#{hex}>[{recipe.tier.ToString().ToUpper()}] {recipe.displayName}</color></b>\n<size=13>");

            for (int i = 0; i < recipe.ingredients.Count; i++)
            {
                var ing = recipe.ingredients[i];
                if (ing.item == null) continue;
                int owned = _playerInventory != null ? _playerInventory.GetItemCount(ing.item) : 0;
                string col = owned >= ing.requiredAmount ? "#77FF99" : "#FF7777";
                if (i > 0) sb.Append("   |   ");
                sb.Append($"<color={col}>{ing.item.itemName}: {owned}/{ing.requiredAmount}</color>");
            }
            sb.Append("</size>");
            return sb.ToString();
        }

        private void HandleInventoryChanged()
        {
            if (_isOpen) RebuildRecipeList();
        }

        private void LocatePlayerComponents()
        {
            if (_playerHotbar == null) _playerHotbar = FindAnyObjectByType<PlayerHotbarController>();
            if (_playerInventory == null)
            {
                _playerInventory = _playerHotbar != null
                    ? _playerHotbar.GetComponent<InventorySystem>()
                    : FindAnyObjectByType<InventorySystem>();
            }
        }
    }
}
