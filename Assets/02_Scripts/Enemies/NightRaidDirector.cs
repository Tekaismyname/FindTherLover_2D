using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using FindTheLover.Environment;
using FindTheLover.Enemies.Data;

namespace FindTheLover.Enemies
{
    /// <summary>
    /// Logic Layer: Muck-like Night Raid Director coordinating escalating nightly monster budgets,
    /// multi-archetype object pooling, off-camera annular ring spawning, and Calamity Nights (Days 5, 10, 15, 20).
    /// Subscribes cleanly to DayNightCycleManager events without tight coupling.
    /// </summary>
    public class NightRaidDirector : MonoBehaviour
    {
        public static NightRaidDirector Instance { get; private set; }

        [Header("Data Configuration")]
        [Tooltip("ScriptableObject defining budget scaling, spawn ring metrics, and enemy roster.")]
        [SerializeField] private NightRaidConfigSO _config;

        [Header("Runtime Anchor")]
        [Tooltip("Target transform (Player or Campfire) around which the spawn ring is centered. Auto-located if empty.")]
        [SerializeField] private Transform _spawnCenterTransform;

        [Header("Audio Source")]
        [Tooltip("Optional AudioSource for playing dusk alarms and calamity horns.")]
        [SerializeField] private AudioSource _alarmAudioSource;

        // Multi-archetype object pools keyed by prefab reference to avoid cross-type contamination
        private readonly Dictionary<EnemyAI, Queue<EnemyAI>> _archetypePools = new Dictionary<EnemyAI, Queue<EnemyAI>>();
        private readonly List<EnemyAI> _activeRaiders = new List<EnemyAI>();
        private readonly List<NightEnemyRosterEntry> _eligibleCache = new List<NightEnemyRosterEntry>();

        private Coroutine _raidCoroutine;
        private int _currentNightBudgetTotal;
        private int _currentNightBudgetRemaining;
        private bool _isRaidActive;
        private bool _isCalamityNight;

        /// <summary>Fired when a night raid starts: (day, totalBudget, isCalamityNight).</summary>
        public event Action<int, int, bool> OnRaidStarted;

        /// <summary>Fired whenever raid budget is spent: (remainingBudget, totalBudget).</summary>
        public event Action<int, int> OnRaidBudgetChanged;

        /// <summary>Fired when dawn breaks and the night raid concludes: (survivedDay).</summary>
        public event Action<int> OnRaidEnded;

        /// <summary>Fired when players survive past the target marathon day (e.g., Day 20).</summary>
        public event Action<int> OnSurvivalGoalReached;

        public NightRaidConfigSO Config => _config;
        public bool IsRaidActive => _isRaidActive;
        public bool IsCalamityNight => _isCalamityNight;
        public int RemainingBudget => _currentNightBudgetRemaining;
        public int TotalBudget => _currentNightBudgetTotal;
        public int ActiveRaiderCount => _activeRaiders.Count;
        public int TargetSurvivalDays => _config != null ? _config.targetSurvivalDays : 20;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            LocateSpawnCenter();

            if (DayNightCycleManager.Instance != null)
            {
                DayNightCycleManager.Instance.OnNightStarted += HandleNightStarted;
                DayNightCycleManager.Instance.OnDayStarted += HandleDayStarted;

                if (DayNightCycleManager.Instance.IsNight)
                {
                    HandleNightStarted(DayNightCycleManager.Instance.CurrentDay);
                }
            }
        }

        private void OnDestroy()
        {
            if (DayNightCycleManager.Instance != null)
            {
                DayNightCycleManager.Instance.OnNightStarted -= HandleNightStarted;
                DayNightCycleManager.Instance.OnDayStarted -= HandleDayStarted;
            }
        }

        /// <summary>
        /// Injects a configuration blueprint dynamically (useful for automated unit tests).
        /// </summary>
        public void SetConfig(NightRaidConfigSO config)
        {
            _config = config;
        }

        /// <summary>
        /// Calculates the escalating spawn budget for a specific survival day.
        /// Formula: (Base + Max(0, Day - 1) * Multiplier) * CalamityMultiplier(if milestone night).
        /// </summary>
        public int CalculateNightBudget(int day)
        {
            if (_config == null)
            {
                return 10 + Mathf.Max(0, day - 1) * 6;
            }

            int clampedDay = Mathf.Max(1, day);
            float rawBudget = _config.baseNightBudget + (clampedDay - 1) * _config.budgetPerDayMultiplier;

            if (IsCalamityBossNight(clampedDay))
            {
                rawBudget *= _config.calamityBudgetMultiplier;
            }

            return Mathf.RoundToInt(rawBudget);
        }

        /// <summary>
        /// Checks whether the specified day is a Calamity Boss Night (default: Days 5, 10, 15, 20).
        /// </summary>
        public bool IsCalamityBossNight(int day)
        {
            if (_config == null || _config.calamityBossDays == null)
            {
                return day > 0 && day % 5 == 0;
            }

            for (int i = 0; i < _config.calamityBossDays.Length; i++)
            {
                if (_config.calamityBossDays[i] == day)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Computes enemy health multiplier for the given survival day.
        /// </summary>
        public float CalculateHealthScale(int day)
        {
            float scalePerDay = _config != null ? _config.dailyHealthScalePercent : 0.12f;
            return 1f + Mathf.Max(0, day - 1) * scalePerDay;
        }

        /// <summary>
        /// Computes enemy damage multiplier for the given survival day.
        /// </summary>
        public float CalculateDamageScale(int day)
        {
            float scalePerDay = _config != null ? _config.dailyDamageScalePercent : 0.08f;
            return 1f + Mathf.Max(0, day - 1) * scalePerDay;
        }

        /// <summary>
        /// Pure geometric helper converting polar angle and radius into an XZ plane ring offset.
        /// </summary>
        public static Vector3 CalculateRingOffset(float angleRadians, float radius)
        {
            float safeRadius = Mathf.Max(0f, radius);
            return new Vector3(Mathf.Cos(angleRadians) * safeRadius, 0f, Mathf.Sin(angleRadians) * safeRadius);
        }

        /// <summary>
        /// Generates a randomized annular ring position around the given center point.
        /// </summary>
        public Vector3 GetRandomRingPosition(Vector3 center)
        {
            float minR = _config != null ? _config.minSpawnRadius : 25f;
            float maxR = _config != null ? _config.maxSpawnRadius : 35f;
            if (maxR < minR) maxR = minR;

            float angleRad = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
            float radius = UnityEngine.Random.Range(minR, maxR);
            return center + CalculateRingOffset(angleRad, radius);
        }

        private void LocateSpawnCenter()
        {
            if (_spawnCenterTransform != null) return;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _spawnCenterTransform = player.transform;
                return;
            }

            if (CampfireSanctuary.Instance != null)
            {
                _spawnCenterTransform = CampfireSanctuary.Instance.transform;
            }
        }

        private void HandleNightStarted(int currentDay)
        {
            if (_config == null)
            {
                Debug.LogWarning("[NightRaidDirector] Missing NightRaidConfigSO! Cannot start night raid.", this);
                return;
            }

            _isRaidActive = true;
            _isCalamityNight = IsCalamityBossNight(currentDay);
            _currentNightBudgetTotal = CalculateNightBudget(currentDay);
            _currentNightBudgetRemaining = _currentNightBudgetTotal;

            PlaySound(_isCalamityNight && _config.calamityWarningClip != null
                ? _config.calamityWarningClip
                : _config.duskAlarmClip);

            string tag = _isCalamityNight ? "<color=#FF3366>CALAMITY BOSS NIGHT</color>" : "<color=#FF8844>NIGHT RAID</color>";
            Debug.Log($"[NightRaidDirector] {tag} Day {currentDay} started! Budget: {_currentNightBudgetTotal}");

            OnRaidStarted?.Invoke(currentDay, _currentNightBudgetTotal, _isCalamityNight);
            OnRaidBudgetChanged?.Invoke(_currentNightBudgetRemaining, _currentNightBudgetTotal);

            if (_raidCoroutine != null) StopCoroutine(_raidCoroutine);
            _raidCoroutine = StartCoroutine(NightWaveRoutine(currentDay));
        }

        private void HandleDayStarted(int currentDay)
        {
            if (_raidCoroutine != null)
            {
                StopCoroutine(_raidCoroutine);
                _raidCoroutine = null;
            }

            if (_isRaidActive)
            {
                _isRaidActive = false;
                _isCalamityNight = false;
                PlaySound(_config != null ? _config.dawnReliefClip : null);
                OnRaidEnded?.Invoke(Mathf.Max(1, currentDay - 1));
            }

            if (_config != null && currentDay > _config.targetSurvivalDays)
            {
                Debug.Log($"<color=#FFD700>[NightRaidDirector] SURVIVAL GOAL REACHED! Survived {_config.targetSurvivalDays} days!</color>");
                OnSurvivalGoalReached?.Invoke(_config.targetSurvivalDays);
            }
        }

        private IEnumerator NightWaveRoutine(int currentDay)
        {
            bool guaranteedEliteDeployed = false;

            while (_isRaidActive && _currentNightBudgetRemaining > 0)
            {
                yield return new WaitForSeconds(_config.spawnIntervalSeconds);

                // Remove deactivated or destroyed enemies before checking capacity
                _activeRaiders.RemoveAll(e => e == null || !e.gameObject.activeInHierarchy);

                if (_activeRaiders.Count >= _config.maxSimultaneousRaiders)
                {
                    continue;
                }

                // Guarantee at least one Elite commander spawns on Calamity Nights (Days 5, 10, 15, 20)
                bool forceElite = _isCalamityNight && !guaranteedEliteDeployed;
                if (TryDeployRaider(currentDay, forceElite))
                {
                    if (forceElite) guaranteedEliteDeployed = true;
                }
            }
        }

        private bool TryDeployRaider(int currentDay, bool forceElite)
        {
            LocateSpawnCenter();
            if (_spawnCenterTransform == null) return false;

            if (!TrySelectRosterEntry(currentDay, _currentNightBudgetRemaining, out NightEnemyRosterEntry chosen))
            {
                return false;
            }

            // Sample up to 6 candidate points on the annular ring until a valid NavMesh point is found
            Vector3 validSpawnPos = Vector3.zero;
            bool foundNavPos = false;
            for (int attempt = 0; attempt < 6; attempt++)
            {
                Vector3 candidate = GetRandomRingPosition(_spawnCenterTransform.position);
                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, _config.navMeshSnapDistance, NavMesh.AllAreas))
                {
                    validSpawnPos = hit.position;
                    foundNavPos = true;
                    break;
                }
            }

            if (!foundNavPos) return false;

            EnemyAI instance = GetPooledArchetype(chosen.prefab);
            if (instance == null) return false;

            bool isElite = forceElite || UnityEngine.Random.value < _config.eliteSpawnChance;
            instance.SpawnAt(validSpawnPos);
            instance.ApplyNightRaidScaling(CalculateHealthScale(currentDay), CalculateDamageScale(currentDay), isElite);

            _activeRaiders.Add(instance);
            _currentNightBudgetRemaining = Mathf.Max(0, _currentNightBudgetRemaining - chosen.budgetCost);
            OnRaidBudgetChanged?.Invoke(_currentNightBudgetRemaining, _currentNightBudgetTotal);

            Debug.Log($"[NightRaidDirector] Deployed {(isElite ? "ELITE " : "")}{chosen.label} at {validSpawnPos} (Remaining Budget: {_currentNightBudgetRemaining}/{_currentNightBudgetTotal})");
            return true;
        }

        private bool TrySelectRosterEntry(int currentDay, int remainingBudget, out NightEnemyRosterEntry selected)
        {
            selected = default;
            if (_config == null || _config.enemyRoster == null || _config.enemyRoster.Count == 0) return false;

            _eligibleCache.Clear();
            float totalWeight = 0f;

            for (int i = 0; i < _config.enemyRoster.Count; i++)
            {
                NightEnemyRosterEntry entry = _config.enemyRoster[i];
                if (entry.prefab == null) continue;
                if (entry.minDayToAppear <= currentDay && entry.budgetCost <= remainingBudget)
                {
                    _eligibleCache.Add(entry);
                    totalWeight += Mathf.Max(0.1f, entry.spawnWeight);
                }
            }

            if (_eligibleCache.Count == 0) return false;

            float roll = UnityEngine.Random.Range(0f, totalWeight);
            float cumulative = 0f;
            for (int i = 0; i < _eligibleCache.Count; i++)
            {
                cumulative += Mathf.Max(0.1f, _eligibleCache[i].spawnWeight);
                if (roll <= cumulative)
                {
                    selected = _eligibleCache[i];
                    return true;
                }
            }

            selected = _eligibleCache[_eligibleCache.Count - 1];
            return true;
        }

        private EnemyAI GetPooledArchetype(EnemyAI prefab)
        {
            if (!_archetypePools.TryGetValue(prefab, out Queue<EnemyAI> pool))
            {
                pool = new Queue<EnemyAI>();
                _archetypePools[prefab] = pool;
            }

            EnemyAI enemy = null;
            while (pool.Count > 0 && enemy == null)
            {
                enemy = pool.Dequeue();
            }

            if (enemy == null)
            {
                enemy = Instantiate(prefab, transform);
            }

            // Inject return-to-pool callback so each archetype always returns to its own pool
            EnemyAI capturedPrefab = prefab;
            enemy.OnReturnToPoolCallback = (returnedEnemy) => ReturnPooledArchetype(capturedPrefab, returnedEnemy);
            return enemy;
        }

        private void ReturnPooledArchetype(EnemyAI prefabKey, EnemyAI enemy)
        {
            if (enemy == null) return;
            enemy.gameObject.SetActive(false);
            enemy.transform.SetParent(transform);

            if (!_archetypePools.TryGetValue(prefabKey, out Queue<EnemyAI> pool))
            {
                pool = new Queue<EnemyAI>();
                _archetypePools[prefabKey] = pool;
            }

            if (!pool.Contains(enemy))
            {
                pool.Enqueue(enemy);
            }
        }

        private void PlaySound(AudioClip clip)
        {
            if (clip == null) return;

            if (_alarmAudioSource != null)
            {
                _alarmAudioSource.PlayOneShot(clip);
            }
            else if (_spawnCenterTransform != null)
            {
                AudioSource.PlayClipAtPoint(clip, _spawnCenterTransform.position);
            }
        }
    }
}
