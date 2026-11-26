using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using FindTheLover.Augments.Contracts;
using FindTheLover.Augments.Data;

namespace FindTheLover.Augments
{
    /// <summary>
    /// Logic Layer: Central controller for the TFT-style 3-Tier Augment system (Bronze / Silver / Gold).
    /// Handles weighted rarity rolls, duplicate prevention, active stack tracking, and receiver dispatch.
    /// </summary>
    public class AugmentManager : MonoBehaviour
    {
        public static AugmentManager Instance { get; private set; }

        [Header("Augment Catalog (Data Layer)")]
        [Tooltip("Complete pool of Bronze, Silver, and Gold AugmentDataSO assets.")]
        [SerializeField] private List<AugmentDataSO> _augmentCatalog = new List<AugmentDataSO>();

        [Header("Weighted Tier Probabilities")]
        [Range(0f, 1f)] [SerializeField] private float _bronzeWeight = 0.55f;
        [Range(0f, 1f)] [SerializeField] private float _silverWeight = 0.32f;
        [Range(0f, 1f)] [SerializeField] private float _goldWeight = 0.13f;

        [Header("QA Testing Shortcut")]
        [Tooltip("Allows pressing K in Play Mode to immediately preview a 3-card TFT Augment roll.")]
        [SerializeField] private bool _enableDebugOfferHotkey = true;

        private readonly Dictionary<string, int> _activeStacks = new Dictionary<string, int>();
        private readonly List<AugmentDataSO> _selectedHistory = new List<AugmentDataSO>();
        private AugmentDataSO[] _currentOfferedChoices;
        private bool _isSelectionPending;

        /// <summary>Broadcast when a new 3-card TFT Augment modal should be presented to the player.</summary>
        public event Action<AugmentDataSO[]> OnAugmentChoiceOffered;

        /// <summary>Broadcast when the player selects an augment card: (chosenAugment, newStackCount).</summary>
        public event Action<AugmentDataSO, int> OnAugmentSelected;

        public bool IsSelectionPending => _isSelectionPending;
        public int TotalActiveAugmentCount => _selectedHistory.Count;
        public IReadOnlyList<AugmentDataSO> SelectedHistory => _selectedHistory;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (_enableDebugOfferHotkey && !_isSelectionPending && Keyboard.current != null && Keyboard.current.kKey.wasPressedThisFrame)
            {
                OfferAugmentSelection(null);
            }
#endif
        }

        /// <summary>
        /// Replaces the catalog dynamically (used by Editor builders and automated unit tests).
        /// </summary>
        public void SetCatalog(List<AugmentDataSO> catalog)
        {
            _augmentCatalog = catalog ?? new List<AugmentDataSO>();
        }

        /// <summary>
        /// Returns the current owned stack count for a specific augment ID.
        /// </summary>
        public int GetStackCount(string augmentId)
        {
            return _activeStacks.TryGetValue(augmentId, out int count) ? count : 0;
        }

        /// <summary>
        /// Rolls up to <paramref name="choiceCount"/> distinct eligible augments honoring an optional minimum rarity tier.
        /// Safe against infinite loops even if fewer than 3 eligible augments remain in the catalog.
        /// </summary>
        public AugmentDataSO[] RollAugmentChoices(int choiceCount = 3, AugmentTier? guaranteedMinTier = null)
        {
            if (choiceCount <= 0 || _augmentCatalog == null || _augmentCatalog.Count == 0)
            {
                return Array.Empty<AugmentDataSO>();
            }

            List<AugmentDataSO> eligible = CollectEligibleAugments(guaranteedMinTier);

            // Graceful fallback: if strict minimum tier has too few cards left, relax tier constraint
            if (eligible.Count < choiceCount && guaranteedMinTier.HasValue)
            {
                List<AugmentDataSO> fallbackPool = CollectEligibleAugments(null);
                for (int i = 0; i < fallbackPool.Count; i++)
                {
                    if (!eligible.Contains(fallbackPool[i]))
                    {
                        eligible.Add(fallbackPool[i]);
                    }
                }
            }

            int actualCount = Mathf.Min(choiceCount, eligible.Count);
            AugmentDataSO[] results = new AugmentDataSO[actualCount];
            HashSet<string> pickedIds = new HashSet<string>();

            for (int slot = 0; slot < actualCount; slot++)
            {
                AugmentDataSO rolled = PickWeightedDistinct(eligible, pickedIds, guaranteedMinTier);
                if (rolled == null) break;

                results[slot] = rolled;
                pickedIds.Add(rolled.augmentId);
                eligible.Remove(rolled);
            }

            return results;
        }

        /// <summary>
        /// Opens a 3-card TFT Augment selection offer for the player.
        /// </summary>
        public bool OfferAugmentSelection(AugmentTier? guaranteedMinTier = null)
        {
            if (_isSelectionPending) return false;

            AugmentDataSO[] choices = RollAugmentChoices(3, guaranteedMinTier);
            if (choices.Length == 0)
            {
                Debug.LogWarning("[AugmentManager] No eligible augments left to offer!");
                return false;
            }

            _currentOfferedChoices = choices;
            _isSelectionPending = true;
            OnAugmentChoiceOffered?.Invoke(_currentOfferedChoices);
            return true;
        }

        /// <summary>
        /// Confirms the player's card selection, updates stacks, and applies the augment to the player receiver.
        /// </summary>
        public bool SelectAugment(AugmentDataSO chosen, IAugmentReceiver explicitReceiver = null)
        {
            if (chosen == null) return false;

            int newStack = GetStackCount(chosen.augmentId) + 1;
            _activeStacks[chosen.augmentId] = newStack;
            _selectedHistory.Add(chosen);
            _isSelectionPending = false;
            _currentOfferedChoices = null;

            IAugmentReceiver receiver = explicitReceiver ?? LocatePlayerReceiver();
            if (receiver != null)
            {
                receiver.ApplyAugment(chosen, newStack);
            }

            OnAugmentSelected?.Invoke(chosen, newStack);
            return true;
        }

        /// <summary>
        /// Convenience method to select an offered choice by card index (0, 1, or 2).
        /// </summary>
        public bool SelectOfferedChoiceByIndex(int index)
        {
            if (!_isSelectionPending || _currentOfferedChoices == null) return false;
            if (index < 0 || index >= _currentOfferedChoices.Length) return false;
            return SelectAugment(_currentOfferedChoices[index]);
        }

        private List<AugmentDataSO> CollectEligibleAugments(AugmentTier? minTier)
        {
            List<AugmentDataSO> pool = new List<AugmentDataSO>();
            for (int i = 0; i < _augmentCatalog.Count; i++)
            {
                AugmentDataSO item = _augmentCatalog[i];
                if (item == null || string.IsNullOrEmpty(item.augmentId)) continue;

                int owned = GetStackCount(item.augmentId);
                int maxAllowed = item.allowStacking ? Mathf.Max(1, item.maxStacks) : 1;
                if (owned >= maxAllowed) continue;

                if (minTier.HasValue && item.tier < minTier.Value) continue;

                pool.Add(item);
            }
            return pool;
        }

        private AugmentDataSO PickWeightedDistinct(List<AugmentDataSO> pool, HashSet<string> pickedIds, AugmentTier? minTier)
        {
            float totalWeight = 0f;
            for (int i = 0; i < pool.Count; i++)
            {
                if (pickedIds.Contains(pool[i].augmentId)) continue;
                totalWeight += GetTierWeight(pool[i].tier, minTier);
            }

            if (totalWeight <= 0f) return null;

            float roll = UnityEngine.Random.Range(0f, totalWeight);
            float cumulative = 0f;
            for (int i = 0; i < pool.Count; i++)
            {
                if (pickedIds.Contains(pool[i].augmentId)) continue;
                cumulative += GetTierWeight(pool[i].tier, minTier);
                if (roll <= cumulative)
                {
                    return pool[i];
                }
            }

            return pool[pool.Count - 1];
        }

        private float GetTierWeight(AugmentTier tier, AugmentTier? minTier)
        {
            if (minTier.HasValue && tier < minTier.Value)
            {
                return 0.05f; // Low fallback weight only when higher tiers are exhausted
            }

            switch (tier)
            {
                case AugmentTier.Gold: return Mathf.Max(0.01f, _goldWeight);
                case AugmentTier.Silver: return Mathf.Max(0.01f, _silverWeight);
                default: return Mathf.Max(0.01f, _bronzeWeight);
            }
        }

        /// <summary>
        /// Returns a read-only snapshot of owned augment IDs and stack counts for GameData persistence.
        /// </summary>
        public IReadOnlyDictionary<string, int> GetActiveStacksSnapshot()
        {
            return _activeStacks;
        }

        /// <summary>
        /// Clears active stacks and re-applies saved augments cleanly from GameData (preventing double-stacking).
        /// </summary>
        public void RestoreAugmentsFromData(List<(AugmentDataSO augment, int stacks)> savedAugments, IAugmentReceiver explicitReceiver = null)
        {
            _activeStacks.Clear();
            _selectedHistory.Clear();
            _isSelectionPending = false;
            _currentOfferedChoices = null;

            IAugmentReceiver receiver = explicitReceiver ?? LocatePlayerReceiver();
            if (savedAugments == null) return;

            for (int i = 0; i < savedAugments.Count; i++)
            {
                AugmentDataSO aug = savedAugments[i].augment;
                int finalLevel = Mathf.Clamp(savedAugments[i].stacks, 1, aug != null ? aug.MaxLevel : 1);
                if (aug == null) continue;

                _activeStacks[aug.augmentId] = finalLevel;
                for (int s = 1; s <= finalLevel; s++)
                {
                    _selectedHistory.Add(aug);
                }
                if (receiver != null)
                {
                    receiver.ApplyAugment(aug, finalLevel);
                }
            }
        }

        private IAugmentReceiver LocatePlayerReceiver()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                IAugmentReceiver receiver = player.GetComponentInChildren<IAugmentReceiver>() ?? player.GetComponent<IAugmentReceiver>();
                if (receiver == null)
                {
                    receiver = player.AddComponent<PlayerAugmentStats>();
                }
                return receiver;
            }
            return null;
        }
    }
}
