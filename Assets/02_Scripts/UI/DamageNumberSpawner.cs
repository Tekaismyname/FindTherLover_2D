using System.Collections.Generic;
using TMPro;
using UnityEngine;
using FindTheLover.Combat;
using FindTheLover.Environment;
using FindTheLover.UI.Data;

namespace FindTheLover.UI
{
    /// <summary>
    /// Logic Layer: Zero-Allocation Object Pool Manager spawning floating combat & mining numbers.
    /// Automatically observes PlayerCombat (outgoing hits & Combo Step 3 crits) and ResourceNode
    /// (tree chopping, rock/ore mining, and missing tool feedback) with strict decoupling.
    /// </summary>
    public class DamageNumberSpawner : MonoBehaviour
    {
        public static DamageNumberSpawner Instance { get; private set; }

        [Header("Data Blueprint & Prefab")]
        [SerializeField] private DamageNumberStyleSO styleConfig;
        [SerializeField] private DamageNumberPopup popupPrefab;

        [Header("Observed Combat Source (Optional Auto-Find)")]
        [SerializeField] private PlayerCombat playerCombat;

        private readonly Queue<DamageNumberPopup> _pool = new Queue<DamageNumberPopup>();
        private Transform _poolRoot;

        public int PooledCount => _pool.Count;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            EnsurePoolRoot();
            PrewarmPool();
        }

        private void OnEnable()
        {
            if (playerCombat == null)
            {
                playerCombat = Object.FindAnyObjectByType<PlayerCombat>();
            }

            if (playerCombat != null)
            {
                playerCombat.OnEnemyStruck += HandleEnemyStruck;
            }

            ResourceNode.OnAnyResourceHarvested += HandleResourceHarvested;
        }

        private void OnDisable()
        {
            if (playerCombat != null)
            {
                playerCombat.OnEnemyStruck -= HandleEnemyStruck;
            }

            ResourceNode.OnAnyResourceHarvested -= HandleResourceHarvested;
        }

        /// <summary>
        /// Injects blueprint configuration at runtime or in automated tests.
        /// </summary>
        public void Configure(DamageNumberStyleSO style, DamageNumberPopup prefab = null)
        {
            styleConfig = style;
            if (prefab != null) popupPrefab = prefab;
            EnsurePoolRoot();
            if (_pool.Count == 0) PrewarmPool();
        }

        /// <summary>
        /// Spawns a numeric floating damage/heal popup at the specified world position.
        /// </summary>
        public DamageNumberPopup SpawnDamage(float amount, Vector3 worldPos, DamagePopupStyle style = DamagePopupStyle.Normal)
        {
            int rounded = Mathf.Max(1, Mathf.RoundToInt(amount));
            string label = style switch
            {
                DamagePopupStyle.Critical => $"{rounded}!",
                DamagePopupStyle.Heal => $"+{rounded}",
                DamagePopupStyle.PlayerHurt => $"-{rounded}",
                _ => rounded.ToString()
            };

            return SpawnText(label, worldPos, style);
        }

        /// <summary>
        /// Spawns a custom floating combat text popup (e.g. "NEED PICKAXE!", "CRIT!").
        /// </summary>
        public DamageNumberPopup SpawnText(string label, Vector3 worldPos, DamagePopupStyle style)
        {
            if (string.IsNullOrEmpty(label)) return null;

            DamageNumberPopup popup = AcquireFromPool();
            if (popup == null) return null;

            float scatter = styleConfig != null ? styleConfig.horizontalScatter : 0.55f;
            float yLift = styleConfig != null ? styleConfig.verticalSpawnOffset : 0.9f;
            float fontSize = styleConfig != null ? styleConfig.baseFontSize : 6.5f;
            float lifetime = styleConfig != null ? styleConfig.lifetime : 0.85f;
            float riseHeight = styleConfig != null ? styleConfig.riseHeight : 1.85f;
            float fadeStart = styleConfig != null ? styleConfig.fadeStartNormalized : 0.55f;
            Color color = styleConfig != null ? styleConfig.GetColorForStyle(style) : Color.white;
            float peakScale = styleConfig != null ? styleConfig.GetPeakScaleForStyle(style) : 1.25f;

            if (style == DamagePopupStyle.Blocked)
            {
                fontSize *= 0.72f;
            }
            else if (style == DamagePopupStyle.Critical)
            {
                fontSize *= 1.18f;
            }

            Vector3 spawnPos = worldPos + new Vector3(
                Random.Range(-scatter * 0.45f, scatter * 0.45f),
                yLift,
                Random.Range(-0.15f, 0.15f));

            Vector3 lateralDrift = new Vector3(
                Random.Range(-scatter, scatter),
                0f,
                Random.Range(-scatter * 0.25f, scatter * 0.25f));

            popup.Play(label, spawnPos, color, fontSize, peakScale, lifetime, riseHeight, fadeStart, lateralDrift, ReturnToPool);
            return popup;
        }

        private void HandleEnemyStruck(IDamageable target, float finalDamage, Vector3 hitPoint)
        {
            bool isFinisherCrit = playerCombat != null && playerCombat.CurrentExecutingComboStep >= 3;
            DamagePopupStyle style = isFinisherCrit ? DamagePopupStyle.Critical : DamagePopupStyle.Normal;
            SpawnDamage(finalDamage, hitPoint, style);
        }

        private void HandleResourceHarvested(float finalDamage, Vector3 hitPoint, bool blocked, HarvestToolType requiredTool)
        {
            if (blocked)
            {
                string toolLabel = requiredTool == HarvestToolType.Pickaxe ? "NEED PICKAXE [3]!" : $"NEED {requiredTool.ToString().ToUpper()}!";
                SpawnText(toolLabel, hitPoint, DamagePopupStyle.Blocked);
                return;
            }

            SpawnDamage(finalDamage, hitPoint, DamagePopupStyle.Harvest);
        }

        private void EnsurePoolRoot()
        {
            if (_poolRoot == null)
            {
                GameObject rootObj = new GameObject("DamageNumbers_Pool");
                rootObj.transform.SetParent(transform, false);
                _poolRoot = rootObj.transform;
            }
        }

        private void PrewarmPool()
        {
            int count = styleConfig != null ? styleConfig.prewarmPoolSize : 24;
            for (int i = 0; i < count; i++)
            {
                DamageNumberPopup popup = CreatePopupInstance();
                if (popup != null)
                {
                    popup.gameObject.SetActive(false);
                    _pool.Enqueue(popup);
                }
            }
        }

        private DamageNumberPopup AcquireFromPool()
        {
            while (_pool.Count > 0)
            {
                DamageNumberPopup item = _pool.Dequeue();
                if (item != null) return item;
            }

            return CreatePopupInstance();
        }

        private void ReturnToPool(DamageNumberPopup popup)
        {
            if (popup == null) return;
            popup.gameObject.SetActive(false);
            _pool.Enqueue(popup);
        }

        private DamageNumberPopup CreatePopupInstance()
        {
            EnsurePoolRoot();
            if (popupPrefab != null)
            {
                return Instantiate(popupPrefab, _poolRoot);
            }

            // Runtime fallback so standalone builds or unit tests never fail even if prefab is unassigned
            GameObject go = new GameObject("DamagePopup_Runtime");
            go.transform.SetParent(_poolRoot, false);
            TextMeshPro tmp = go.AddComponent<TextMeshPro>();
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;
            tmp.textWrappingMode = TextWrappingModes.NoWrap;
            return go.AddComponent<DamageNumberPopup>();
        }
    }
}
