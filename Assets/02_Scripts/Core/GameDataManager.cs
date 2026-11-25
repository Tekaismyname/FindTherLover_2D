using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using FindTheLover.Augments;
using FindTheLover.Augments.Data;
using FindTheLover.Combat;
using FindTheLover.Core.Data;
using FindTheLover.Environment;
using FindTheLover.Environment.Puzzles;
using FindTheLover.Equipment.Data;
using FindTheLover.Items;
using FindTheLover.Player;
using FindTherLover.Player;

namespace FindTheLover.Core
{
    /// <summary>
    /// Central Logic Layer Hub linking all gameplay systems (Player transform, Health, Inventory,
    /// Equipment, Torch, TFT Augments, Day/Night Cycle, Seeded Terrain, and Puzzles) to unified GameData.
    /// Provides atomic JSON persistence to Application.persistentDataPath for Standalone (.exe / .apk) builds.
    /// </summary>
    public class GameDataManager : MonoBehaviour
    {
        public static GameDataManager Instance { get; private set; }

        [Header("Master Static Blueprint Catalog")]
        [Tooltip("Central catalog mapping itemId, equipmentId, and augmentId strings to ScriptableObjects.")]
        [SerializeField] private GameCatalogSO _masterCatalog;

        [Header("Persistence Settings")]
        [Tooltip("Default save file slot name (stored as <slot>.json in Application.persistentDataPath).")]
        [SerializeField] private string _defaultSaveSlot = "survival_save_01";

        [Tooltip("Automatically saves GameData to disk whenever a new survival day dawns.")]
        [SerializeField] private bool _autoSaveOnNewDay = true;

        [Tooltip("QA hotkeys (Editor & Dev Builds only): F5 = Quick Save, F9 = Quick Load.")]
        [SerializeField] private bool _enableDevQuickSaveHotkeys = true;

        [Header("Live Runtime GameData (Inspector Viewable)")]
        [SerializeField] private GameSaveData _currentData = new GameSaveData();

        // Cached player gameplay component links
        private Transform _playerTransform;
        private CharacterController _playerController;
        private HealthSystem _playerHealth;
        private InventorySystem _playerInventory;
        private PlayerEquipment _playerEquipment;
        private PlayerTorchController _playerTorch;
        private PlayerAugmentStats _playerAugmentStats;

        /// <summary>Fired whenever live GameData is synchronized or loaded.</summary>
        public event Action<GameSaveData> OnGameDataUpdated;
        public event Action<string> OnGameSaved;
        public event Action<string> OnGameLoaded;

        public GameCatalogSO MasterCatalog => _masterCatalog;
        public GameSaveData CurrentData => _currentData;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (_masterCatalog != null)
            {
                _masterCatalog.RebuildLookups();
            }
        }

        private void Start()
        {
            BindGameplaySystems();
            CaptureSnapshotFromWorld();
        }

        private void OnDestroy()
        {
            UnbindGameplaySystems();
        }

        private void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (_enableDevQuickSaveHotkeys && Keyboard.current != null)
            {
                if (Keyboard.current.f5Key.wasPressedThisFrame)
                {
                    SaveToDisk(_defaultSaveSlot);
                }
                else if (Keyboard.current.f9Key.wasPressedThisFrame)
                {
                    LoadFromDisk(_defaultSaveSlot);
                }
            }
#endif
        }

        /// <summary>
        /// Injects a catalog reference dynamically (used by Editor builders and automated unit tests).
        /// </summary>
        public void SetMasterCatalog(GameCatalogSO catalog)
        {
            _masterCatalog = catalog;
            if (_masterCatalog != null) _masterCatalog.RebuildLookups();
        }

        /// <summary>
        /// Locates the active Player entity and subscribes to live system change events.
        /// </summary>
        public void BindGameplaySystems()
        {
            UnbindGameplaySystems();

            var locomotion = FindAnyObjectByType<PlayerLocomotion.PlayerLocomotion>();
            GameObject playerObj = locomotion != null ? locomotion.gameObject : GameObject.FindGameObjectWithTag("Player");

            if (playerObj != null)
            {
                _playerTransform = playerObj.transform;
                _playerController = playerObj.GetComponentInChildren<CharacterController>() ?? playerObj.GetComponentInParent<CharacterController>();
                _playerHealth = playerObj.GetComponentInChildren<HealthSystem>() ?? playerObj.GetComponentInParent<HealthSystem>();
                _playerInventory = playerObj.GetComponentInChildren<InventorySystem>() ?? playerObj.GetComponentInParent<InventorySystem>();
                _playerEquipment = playerObj.GetComponentInChildren<PlayerEquipment>() ?? playerObj.GetComponentInParent<PlayerEquipment>();
                _playerTorch = playerObj.GetComponentInChildren<PlayerTorchController>() ?? playerObj.GetComponentInParent<PlayerTorchController>();
                _playerAugmentStats = playerObj.GetComponentInChildren<PlayerAugmentStats>() ?? playerObj.GetComponentInParent<PlayerAugmentStats>();
            }

            if (_playerInventory != null) _playerInventory.OnInventoryChanged += HandleLiveDataChanged;
            if (_playerEquipment != null) _playerEquipment.OnEquipmentChanged += HandleEquipmentChanged;
            if (_playerHealth != null) _playerHealth.OnHealthChanged += HandleHealthChanged;
            if (AugmentManager.Instance != null) AugmentManager.Instance.OnAugmentSelected += HandleAugmentSelected;
            if (DayNightCycleManager.Instance != null) DayNightCycleManager.Instance.OnDayStarted += HandleNewDayStarted;
            if (SeededWorldTerrainManager.Instance != null) SeededWorldTerrainManager.Instance.OnWorldSeedGenerated += HandleWorldSeedChanged;
        }

        private void UnbindGameplaySystems()
        {
            if (_playerInventory != null) _playerInventory.OnInventoryChanged -= HandleLiveDataChanged;
            if (_playerEquipment != null) _playerEquipment.OnEquipmentChanged -= HandleEquipmentChanged;
            if (_playerHealth != null) _playerHealth.OnHealthChanged -= HandleHealthChanged;
            if (AugmentManager.Instance != null) AugmentManager.Instance.OnAugmentSelected -= HandleAugmentSelected;
            if (DayNightCycleManager.Instance != null) DayNightCycleManager.Instance.OnDayStarted -= HandleNewDayStarted;
            if (SeededWorldTerrainManager.Instance != null) SeededWorldTerrainManager.Instance.OnWorldSeedGenerated -= HandleWorldSeedChanged;
        }

        /// <summary>
        /// Captures the complete live state from all gameplay systems into <see cref="CurrentData"/>.
        /// </summary>
        public GameSaveData CaptureSnapshotFromWorld()
        {
            if (_playerTransform == null) BindGameplaySystems();
            if (_currentData == null) _currentData = new GameSaveData();

            _currentData.schemaVersion = 1;
            _currentData.lastSavedUtc = DateTime.UtcNow.ToString("O");

            // 1. World Seed & Day/Night Progression
            if (SeededWorldTerrainManager.Instance != null)
            {
                _currentData.world.worldSeed = SeededWorldTerrainManager.Instance.WorldSeed;
            }
            if (DayNightCycleManager.Instance != null)
            {
                _currentData.world.currentDay = DayNightCycleManager.Instance.CurrentDay;
                _currentData.world.normalizedTime = DayNightCycleManager.Instance.NormalizedTime;
            }

            // 2. Player Position, Vitals, Torch & Equipment
            if (_playerTransform != null)
            {
                _currentData.player.position = _playerTransform.position;
                _currentData.player.rotationY = _playerTransform.eulerAngles.y;
            }
            if (_playerHealth != null)
            {
                _currentData.player.currentHealth = _playerHealth.CurrentHealth;
                _currentData.player.maxHealth = _playerHealth.MaxHealth;
                _currentData.player.shieldPoints = _playerHealth.ShieldPoints;
            }
            if (_playerTorch != null)
            {
                _currentData.player.torchFlameType = (int)_playerTorch.CurrentFlameType;
                _currentData.player.torchRemainingFuel = _playerTorch.RemainingBurnTime;
            }
            if (_playerEquipment != null)
            {
                _currentData.player.equippedItemId = _playerEquipment.CurrentEquippedData != null
                    ? _playerEquipment.CurrentEquippedData.itemId
                    : string.Empty;
            }

            // 3. Inventory Items
            _currentData.inventorySlots.Clear();
            if (_playerInventory != null)
            {
                var slots = _playerInventory.Slots;
                for (int i = 0; i < slots.Count; i++)
                {
                    if (slots[i] != null && slots[i].itemData != null && slots[i].quantity > 0)
                    {
                        _currentData.inventorySlots.Add(new InventorySlotSaveData
                        {
                            itemId = slots[i].itemData.itemId,
                            quantity = slots[i].quantity
                        });
                    }
                }
            }

            // 4. Owned TFT Augments
            _currentData.ownedAugments.Clear();
            if (AugmentManager.Instance != null)
            {
                foreach (var kv in AugmentManager.Instance.GetActiveStacksSnapshot())
                {
                    _currentData.ownedAugments.Add(new OwnedAugmentSaveData
                    {
                        augmentId = kv.Key,
                        stackCount = kv.Value
                    });
                }
            }

            // 5. Persistent World Puzzles (Ancient Braziers & Thorn Barrier Gates)
            _currentData.worldObjectStates.Clear();
            foreach (var brazier in FindObjectsByType<AncientBrazier>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                _currentData.worldObjectStates.Add(new WorldObjectStateSaveData
                {
                    objectId = $"brazier:{brazier.gameObject.name}",
                    isActivated = brazier.IsLit
                });
            }
            foreach (var gate in FindObjectsByType<ThornBarrierGate>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                _currentData.worldObjectStates.Add(new WorldObjectStateSaveData
                {
                    objectId = $"thorngate:{gate.gameObject.name}",
                    isActivated = gate.IsBurned
                });
            }

            OnGameDataUpdated?.Invoke(_currentData);
            return _currentData;
        }

        /// <summary>
        /// Restores the entire live world state cleanly from a <see cref="GameSaveData"/> snapshot.
        /// </summary>
        public void ApplySnapshotToWorld(GameSaveData data)
        {
            if (data == null) return;
            _currentData = data;
            if (_playerTransform == null) BindGameplaySystems();

            // 1. Regenerate Seeded Terrain first so terrain elevation matches the saved seed
            if (SeededWorldTerrainManager.Instance != null &&
                SeededWorldTerrainManager.Instance.WorldSeed != data.world.worldSeed)
            {
                SeededWorldTerrainManager.Instance.GenerateWorldWithSeed(data.world.worldSeed);
            }

            // 2. Restore Day/Night Cycle
            if (DayNightCycleManager.Instance != null)
            {
                DayNightCycleManager.Instance.RestoreCycleState(data.world.currentDay, data.world.normalizedTime);
            }

            // 3. Restore Player Transform safely (temporarily disable CharacterController)
            if (_playerTransform != null)
            {
                if (_playerController != null) _playerController.enabled = false;
                _playerTransform.position = data.player.position;
                _playerTransform.rotation = Quaternion.Euler(0f, data.player.rotationY, 0f);
                if (_playerController != null) _playerController.enabled = true;
            }

            // 4. Restore Owned TFT Augments cleanly (Reset first -> prevents double-stacking!)
            if (_playerAugmentStats != null)
            {
                _playerAugmentStats.ResetAllAugmentStats();
            }
            if (AugmentManager.Instance != null && _masterCatalog != null)
            {
                var restoredAugments = new List<(AugmentDataSO augment, int stacks)>();
                for (int i = 0; i < data.ownedAugments.Count; i++)
                {
                    AugmentDataSO aug = _masterCatalog.GetAugmentById(data.ownedAugments[i].augmentId);
                    if (aug != null)
                    {
                        restoredAugments.Add((aug, Mathf.Max(1, data.ownedAugments[i].stackCount)));
                    }
                    else
                    {
                        Debug.LogWarning($"[GameDataManager] Skipping unknown augmentId '{data.ownedAugments[i].augmentId}' during load.");
                    }
                }
                AugmentManager.Instance.RestoreAugmentsFromData(restoredAugments, _playerAugmentStats);
            }

            // 5. Restore Player Health, Shield & Torch (after augments so maxHealth & torch cap match)
            if (_playerHealth != null)
            {
                _playerHealth.RestoreHealthState(data.player.maxHealth, data.player.currentHealth, data.player.shieldPoints);
            }
            if (_playerTorch != null)
            {
                _playerTorch.RestoreTorchState((TorchFlameType)data.player.torchFlameType, data.player.torchRemainingFuel);
            }

            // 6. Restore Equipped Item
            if (_playerEquipment != null && _masterCatalog != null)
            {
                EquipmentItemDataSO eq = _masterCatalog.GetEquipmentById(data.player.equippedItemId);
                _playerEquipment.EquipItem(eq);
            }

            // 7. Restore Inventory Slots
            if (_playerInventory != null && _masterCatalog != null)
            {
                var restoredSlots = new List<ItemStack>();
                for (int i = 0; i < data.inventorySlots.Count; i++)
                {
                    ItemDataSO item = _masterCatalog.GetItemById(data.inventorySlots[i].itemId);
                    if (item != null && data.inventorySlots[i].quantity > 0)
                    {
                        restoredSlots.Add(new ItemStack(item, data.inventorySlots[i].quantity));
                    }
                }
                _playerInventory.ClearAndLoadSlots(restoredSlots);
            }

            // 8. Restore World Puzzles (Braziers & Thorn Gates)
            var stateMap = new Dictionary<string, bool>();
            for (int i = 0; i < data.worldObjectStates.Count; i++)
            {
                stateMap[data.worldObjectStates[i].objectId] = data.worldObjectStates[i].isActivated;
            }
            foreach (var brazier in FindObjectsByType<AncientBrazier>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (stateMap.TryGetValue($"brazier:{brazier.gameObject.name}", out bool lit))
                {
                    brazier.ForceSetLitState(lit);
                }
            }
            foreach (var gate in FindObjectsByType<ThornBarrierGate>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (stateMap.TryGetValue($"thorngate:{gate.gameObject.name}", out bool burned))
                {
                    gate.ForceSetBurnedState(burned);
                }
            }

            OnGameDataUpdated?.Invoke(_currentData);
        }

        /// <summary>
        /// Returns the full persistent file path for a given save slot (works in Editor, Windows .exe, and Android .apk).
        /// </summary>
        public static string GetSaveFilePath(string slotName)
        {
            string safeSlot = string.IsNullOrWhiteSpace(slotName) ? "survival_save_01" : slotName.Trim();
            return Path.Combine(Application.persistentDataPath, $"{safeSlot}.json");
        }

        public bool HasSave(string slotName = null)
        {
            return File.Exists(GetSaveFilePath(slotName ?? _defaultSaveSlot));
        }

        /// <summary>
        /// Captures the live world snapshot and writes it atomically as JSON to persistent disk storage.
        /// </summary>
        public bool SaveToDisk(string slotName = null)
        {
            string targetSlot = slotName ?? _defaultSaveSlot;
            string path = GetSaveFilePath(targetSlot);
            try
            {
                GameSaveData snapshot = CaptureSnapshotFromWorld();
                string json = JsonUtility.ToJson(snapshot, true);
                string tempPath = path + ".tmp";
                File.WriteAllText(tempPath, json);
                if (File.Exists(path)) File.Delete(path);
                File.Move(tempPath, path);

                Debug.Log($"<color=#00FFCC>[GameDataManager]</color> Saved GameData to '{path}' (Day {snapshot.world.currentDay}, Items={snapshot.inventorySlots.Count}, Augments={snapshot.ownedAugments.Count}).");
                OnGameSaved?.Invoke(targetSlot);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameDataManager] Failed to save slot '{targetSlot}': {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Reads JSON GameData from disk and restores the complete world state.
        /// </summary>
        public bool LoadFromDisk(string slotName = null)
        {
            string targetSlot = slotName ?? _defaultSaveSlot;
            string path = GetSaveFilePath(targetSlot);
            if (!File.Exists(path))
            {
                Debug.LogWarning($"[GameDataManager] Save file not found at '{path}'.");
                return false;
            }

            try
            {
                string json = File.ReadAllText(path);
                GameSaveData loaded = JsonUtility.FromJson<GameSaveData>(json);
                if (loaded == null) return false;

                ApplySnapshotToWorld(loaded);
                Debug.Log($"<color=#00FFCC>[GameDataManager]</color> Loaded GameData from '{path}' (Day {loaded.world.currentDay}, Seed {loaded.world.worldSeed}).");
                OnGameLoaded?.Invoke(targetSlot);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[GameDataManager] Failed to load slot '{targetSlot}': {ex.Message}");
                return false;
            }
        }

        public bool DeleteSave(string slotName = null)
        {
            string path = GetSaveFilePath(slotName ?? _defaultSaveSlot);
            if (!File.Exists(path)) return false;
            File.Delete(path);
            return true;
        }

        private void HandleLiveDataChanged() => CaptureSnapshotFromWorld();
        private void HandleEquipmentChanged(EquipmentItemDataSO eq) => CaptureSnapshotFromWorld();
        private void HandleHealthChanged(float current, float max)
        {
            if (_currentData == null) return;
            _currentData.player.currentHealth = current;
            _currentData.player.maxHealth = max;
        }
        private void HandleAugmentSelected(AugmentDataSO augment, int newStack) => CaptureSnapshotFromWorld();
        private void HandleWorldSeedChanged(int newSeed) => CaptureSnapshotFromWorld();
        private void HandleNewDayStarted(int newDay)
        {
            CaptureSnapshotFromWorld();
            if (_autoSaveOnNewDay)
            {
                SaveToDisk(_defaultSaveSlot);
            }
        }
    }
}
