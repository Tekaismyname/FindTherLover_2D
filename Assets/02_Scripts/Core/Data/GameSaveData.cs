using System;
using System.Collections.Generic;
using UnityEngine;

namespace FindTheLover.Core.Data
{
    /// <summary>
    /// Serializable snapshot of macro survival world progression (Seed, Day/Night cycle, Shrines).
    /// </summary>
    [Serializable]
    public class WorldSaveData
    {
        public int worldSeed = 20261119;
        public int currentDay = 1;
        public float normalizedTime = 0.25f;
        public int shrinesActivated = 0;
        public int totalDownedCount = 0;
    }

    /// <summary>
    /// Serializable snapshot of player transform, health/shield vitals, torch flame, and equipped gear ID.
    /// </summary>
    [Serializable]
    public class PlayerSaveData
    {
        public Vector3 position = Vector3.zero;
        public float rotationY = 0f;
        public float currentHealth = 100f;
        public float maxHealth = 100f;
        public float shieldPoints = 0f;
        public int torchFlameType = 1; // 0=Unlit, 1=NormalFlame, 2=SoulFlame
        public float torchRemainingFuel = 40f;
        public string equippedItemId = string.Empty;
    }

    /// <summary>
    /// Lightweight inventory slot record referencing ItemDataSO via stable itemId.
    /// </summary>
    [Serializable]
    public struct InventorySlotSaveData
    {
        public string itemId;
        public int quantity;
    }

    /// <summary>
    /// Lightweight owned TFT Augment record referencing AugmentDataSO via stable augmentId and stack/level.
    /// </summary>
    [Serializable]
    public struct OwnedAugmentSaveData
    {
        public string augmentId;
        public int stackCount;
    }

    /// <summary>
    /// Generic persistent boolean flag for world puzzles, ancient braziers, gates, and levers.
    /// </summary>
    [Serializable]
    public struct WorldObjectStateSaveData
    {
        public string objectId;
        public bool isActivated;
    }

    /// <summary>
    /// Root JSON-serializable Game Data container representing a complete playthrough snapshot.
    /// Compatible with Unity JsonUtility across Windows Standalone (.exe) and Android (.apk).
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        public int schemaVersion = 1;
        public string lastSavedUtc = string.Empty;

        public WorldSaveData world = new WorldSaveData();
        public PlayerSaveData player = new PlayerSaveData();
        public List<InventorySlotSaveData> inventorySlots = new List<InventorySlotSaveData>();
        public List<OwnedAugmentSaveData> ownedAugments = new List<OwnedAugmentSaveData>();
        public List<WorldObjectStateSaveData> worldObjectStates = new List<WorldObjectStateSaveData>();
    }
}
