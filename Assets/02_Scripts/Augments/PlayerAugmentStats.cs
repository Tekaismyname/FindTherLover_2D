using UnityEngine;
using FindTheLover.Augments.Contracts;
using FindTheLover.Augments.Data;
using FindTheLover.Combat;
using FindTheLover.Player;

namespace FindTheLover.Augments
{
    /// <summary>
    /// Logic Layer: Aggregates active Bronze, Silver, and Gold TFT Augments on the player
    /// and executes passive combat triggers (Lifesteal, Chain Lightning, Regenerating Shield,
    /// Blazing Dash, Toxic Rebound Nova, Celestial Thunderstorm, and Immortal Bond).
    /// </summary>
    public class PlayerAugmentStats : MonoBehaviour, IAugmentReceiver
    {
        [Header("Combat Passive Settings")]
        [Tooltip("Layer mask scanned when triggering Chain Lightning, Toxic Nova, or Celestial Strikes.")]
        [SerializeField] private LayerMask _enemyLayerMask = ~0;

        // Cached sibling component references
        private HealthSystem _healthSystem;
        private PlayerLocomotion.PlayerLocomotion _locomotion;
        private PlayerCombat _playerCombat;
        private PlayerTorchController _torchController;

        // Aggregated stat multipliers
        private float _moveSpeedBonus = 0f;
        private float _damageBonus = 0f;
        private float _lifestealPercent = 0f;
        private float _harvestMultiplier = 1f;
        private float _torchDurationBonus = 0f;
        private float _damageReductionBonus = 0f;

        // Silver & Gold passive runtime parameters
        private int _chainLightningTargets = 0;
        private float _chainLightningDamageRatio = 0f;
        private float _maxShieldCapacity = 0f;
        private float _shieldRegenInterval = 20f;
        private float _nextShieldRegenTime = 0f;
        private float _blazingDashDamage = 0f;
        private float _nextDashTickTime = 0f;
        private float _toxicReboundDamage = 0f;
        private float _toxicReboundRadius = 4f;
        private float _celestialStrikeDamage = 0f;
        private float _celestialStrikeInterval = 6f;
        private float _nextCelestialTime = 0f;
        private bool _hasImmortalBond = false;
        private bool _immortalBondReady = false;
        private float _bonusGoldDropMultiplier = 1f;

        public float HarvestMultiplier => _harvestMultiplier;
        public float BonusGoldDropMultiplier => _bonusGoldDropMultiplier;
        public float MoveSpeedMultiplier => 1f + _moveSpeedBonus;
        public float DamageMultiplier => 1f + _damageBonus;

        private void Awake()
        {
            _healthSystem = GetComponent<HealthSystem>();
            _locomotion = GetComponent<PlayerLocomotion.PlayerLocomotion>();
            _playerCombat = GetComponent<PlayerCombat>();
            _torchController = GetComponent<PlayerTorchController>();
        }

        private void OnEnable()
        {
            if (_playerCombat != null)
            {
                _playerCombat.OnEnemyStruck += HandleEnemyStruck;
            }
            if (_healthSystem != null)
            {
                _healthSystem.OnDamaged += HandlePlayerDamaged;
                _healthSystem.OnDeath += HandlePlayerDeath;
            }
        }

        private void OnDisable()
        {
            if (_playerCombat != null)
            {
                _playerCombat.OnEnemyStruck -= HandleEnemyStruck;
            }
            if (_healthSystem != null)
            {
                _healthSystem.OnDamaged -= HandlePlayerDamaged;
                _healthSystem.OnDeath -= HandlePlayerDeath;
            }
        }

        private void Update()
        {
            UpdateRegeneratingShield();
            UpdateBlazingDash();
            UpdateCelestialThunderstorm();
        }

        /// <summary>
        /// Resets all aggregated augment bonuses back to clean base values before reloading saved GameData.
        /// Guarantees zero double-stacking when loading saves multiple times.
        /// </summary>
        public void ResetAllAugmentStats()
        {
            _moveSpeedBonus = 0f;
            _damageBonus = 0f;
            _lifestealPercent = 0f;
            _harvestMultiplier = 1f;
            _torchDurationBonus = 0f;
            _damageReductionBonus = 0f;
            _chainLightningTargets = 0;
            _chainLightningDamageRatio = 0f;
            _maxShieldCapacity = 0f;
            _blazingDashDamage = 0f;
            _toxicReboundDamage = 0f;
            _celestialStrikeDamage = 0f;
            _hasImmortalBond = false;
            _immortalBondReady = false;
            _bonusGoldDropMultiplier = 1f;

            if (_locomotion != null) _locomotion.SetSpeedMultiplier(1f);
            if (_playerCombat != null) _playerCombat.SetDamageMultiplier(1f);
            if (_healthSystem != null)
            {
                _healthSystem.SetDamageReduction(0f);
                _healthSystem.SetShieldPoints(0f);
            }
            if (_torchController != null) _torchController.SetBurnDurationMultiplier(1f);
        }

        /// <summary>
        /// IAugmentReceiver implementation: applies the chosen augment blueprint onto the player.
        /// </summary>
        public void ApplyAugment(AugmentDataSO augment, int newStackCount)
        {
            if (augment == null) return;

            switch (augment.effectType)
            {
                // --- BRONZE TIER ---
                case AugmentEffectType.MoveSpeedPercent:
                    _moveSpeedBonus += augment.primaryValue;
                    if (_locomotion != null) _locomotion.SetSpeedMultiplier(MoveSpeedMultiplier);
                    break;

                case AugmentEffectType.MaxHealthFlat:
                    if (_healthSystem != null)
                    {
                        float oldHealth = _healthSystem.CurrentHealth;
                        _healthSystem.SetMaxHealth(_healthSystem.MaxHealth + augment.primaryValue, resetCurrentHealth: false);
                        _healthSystem.Heal(augment.primaryValue + Mathf.Max(0f, oldHealth - _healthSystem.CurrentHealth));
                    }
                    break;

                case AugmentEffectType.LifestealPercent:
                    _lifestealPercent += augment.primaryValue;
                    break;

                case AugmentEffectType.HarvestYieldMultiplier:
                    _harvestMultiplier += augment.primaryValue;
                    break;

                case AugmentEffectType.TorchDurationPercent:
                    _torchDurationBonus += augment.primaryValue;
                    if (_torchController != null) _torchController.SetBurnDurationMultiplier(1f + _torchDurationBonus);
                    break;

                case AugmentEffectType.DamageReductionPercent:
                    _damageReductionBonus += augment.primaryValue;
                    if (_healthSystem != null) _healthSystem.SetDamageReduction(_damageReductionBonus);
                    break;

                // --- SILVER TIER ---
                case AugmentEffectType.ChainLightningOnHit:
                    _chainLightningTargets += Mathf.Max(1, Mathf.RoundToInt(augment.secondaryValue));
                    _chainLightningDamageRatio += augment.primaryValue;
                    break;

                case AugmentEffectType.BlazingDashTrail:
                    _blazingDashDamage += augment.primaryValue;
                    break;

                case AugmentEffectType.RegeneratingShield:
                    _maxShieldCapacity += augment.primaryValue;
                    _shieldRegenInterval = augment.secondaryValue > 0f ? augment.secondaryValue : 20f;
                    if (_healthSystem != null) _healthSystem.SetShieldPoints(_maxShieldCapacity);
                    _nextShieldRegenTime = Time.time + _shieldRegenInterval;
                    break;

                case AugmentEffectType.ToxicReboundNova:
                    _toxicReboundDamage += augment.primaryValue;
                    if (augment.secondaryValue > 0f) _toxicReboundRadius = augment.secondaryValue;
                    break;

                case AugmentEffectType.PartnerDamageAura:
                case AugmentEffectType.WindSlashFinisher:
                    _damageBonus += augment.primaryValue;
                    if (_playerCombat != null) _playerCombat.SetDamageMultiplier(DamageMultiplier);
                    break;

                // --- GOLD TIER ---
                case AugmentEffectType.ImmortalLoverBond:
                    _hasImmortalBond = true;
                    _immortalBondReady = true;
                    break;

                case AugmentEffectType.CelestialThunderstorm:
                    _celestialStrikeDamage += augment.primaryValue;
                    _celestialStrikeInterval = augment.secondaryValue > 0f ? augment.secondaryValue : 6f;
                    _nextCelestialTime = Time.time + _celestialStrikeInterval;
                    break;

                case AugmentEffectType.GoldenMidasTouch:
                    _bonusGoldDropMultiplier += augment.primaryValue;
                    _harvestMultiplier += 0.5f;
                    break;

                case AugmentEffectType.EternalRingProtection:
                    _damageReductionBonus += augment.primaryValue;
                    if (_healthSystem != null) _healthSystem.SetDamageReduction(_damageReductionBonus);
                    break;

                case AugmentEffectType.BerserkerFrenzy:
                    _damageBonus += augment.primaryValue;
                    _moveSpeedBonus += augment.secondaryValue;
                    if (_playerCombat != null) _playerCombat.SetDamageMultiplier(DamageMultiplier);
                    if (_locomotion != null) _locomotion.SetSpeedMultiplier(MoveSpeedMultiplier);
                    break;

                case AugmentEffectType.ArcaneOverdrive:
                    _damageBonus += augment.primaryValue;
                    _lifestealPercent += augment.secondaryValue;
                    if (_playerCombat != null) _playerCombat.SetDamageMultiplier(DamageMultiplier);
                    break;
            }

            Debug.Log($"<color=#FFD700>[PlayerAugmentStats]</color> Applied [{augment.tier}] {augment.displayName} (Stack x{newStackCount}).");
        }

        private void HandleEnemyStruck(IDamageable primaryTarget, float dealtDamage, Vector3 hitPoint)
        {
            // 1. Lifesteal healing
            if (_lifestealPercent > 0f && _healthSystem != null)
            {
                float healAmount = dealtDamage * _lifestealPercent;
                if (healAmount > 0.1f) _healthSystem.Heal(healAmount);
            }

            // 2. Silver Chain Lightning bounce to nearby enemies
            if (_chainLightningTargets > 0 && _chainLightningDamageRatio > 0f)
            {
                float chainDamage = dealtDamage * _chainLightningDamageRatio;
                Collider[] nearby = Physics.OverlapSphere(hitPoint, 6.5f, _enemyLayerMask);
                int bounced = 0;

                for (int i = 0; i < nearby.Length && bounced < _chainLightningTargets; i++)
                {
                    if (nearby[i].transform.root == transform.root) continue;
                    IDamageable secondary = nearby[i].GetComponentInParent<IDamageable>();
                    if (secondary != null && secondary != primaryTarget && !secondary.IsDead)
                    {
                        secondary.TakeDamage(chainDamage, nearby[i].transform.position, Vector3.up);
                        bounced++;
                    }
                }
            }
        }

        private void HandlePlayerDamaged(Vector3 hitNormal)
        {
            if (_toxicReboundDamage <= 0f) return;

            // Emit AoE retaliation shockwave around player when struck
            Collider[] nearby = Physics.OverlapSphere(transform.position, _toxicReboundRadius, _enemyLayerMask);
            for (int i = 0; i < nearby.Length; i++)
            {
                if (nearby[i].transform.root == transform.root) continue;
                IDamageable enemy = nearby[i].GetComponentInParent<IDamageable>();
                if (enemy != null && !enemy.IsDead)
                {
                    enemy.TakeDamage(_toxicReboundDamage, nearby[i].transform.position, Vector3.up);
                }
            }
        }

        private void HandlePlayerDeath()
        {
            // Gold Augment: Immortal Lover Bond grants one emergency second-wind revival with brief immunity
            if (_hasImmortalBond && _immortalBondReady && _healthSystem != null)
            {
                _immortalBondReady = false;
                _healthSystem.Revive(_healthSystem.MaxHealth * 0.6f, customIFrameDuration: 3.0f);
                Debug.Log("<color=#FFD700>[PlayerAugmentStats] IMMORTAL LOVER BOND TRIGGERED! Revived with 60% HP & 3s Immunity!</color>");
            }
        }

        private void UpdateRegeneratingShield()
        {
            if (_maxShieldCapacity <= 0f || _healthSystem == null || _healthSystem.IsDead) return;

            if (Time.time >= _nextShieldRegenTime)
            {
                _healthSystem.SetShieldPoints(_maxShieldCapacity);
                _nextShieldRegenTime = Time.time + _shieldRegenInterval;
            }
        }

        private void UpdateBlazingDash()
        {
            if (_blazingDashDamage <= 0f || _locomotion == null || !_locomotion.IsSprinting) return;
            if (Time.time < _nextDashTickTime) return;

            _nextDashTickTime = Time.time + 0.5f;
            Collider[] trails = Physics.OverlapSphere(transform.position, 2.2f, _enemyLayerMask);
            for (int i = 0; i < trails.Length; i++)
            {
                if (trails[i].transform.root == transform.root) continue;
                IDamageable enemy = trails[i].GetComponentInParent<IDamageable>();
                if (enemy != null && !enemy.IsDead)
                {
                    enemy.TakeDamage(_blazingDashDamage, trails[i].transform.position, Vector3.up);
                }
            }
        }

        private void UpdateCelestialThunderstorm()
        {
            if (_celestialStrikeDamage <= 0f || Time.time < _nextCelestialTime) return;

            _nextCelestialTime = Time.time + _celestialStrikeInterval;
            Collider[] enemies = Physics.OverlapSphere(transform.position, 18f, _enemyLayerMask);
            IDamageable bestTarget = null;
            float highestHealth = -1f;
            Vector3 strikePos = transform.position;

            for (int i = 0; i < enemies.Length; i++)
            {
                if (enemies[i].transform.root == transform.root) continue;
                HealthSystem enemyHealth = enemies[i].GetComponentInParent<HealthSystem>();
                if (enemyHealth != null && !enemyHealth.IsDead && enemyHealth.CurrentHealth > highestHealth)
                {
                    highestHealth = enemyHealth.CurrentHealth;
                    bestTarget = enemyHealth;
                    strikePos = enemyHealth.transform.position;
                }
            }

            if (bestTarget != null)
            {
                bestTarget.TakeDamage(_celestialStrikeDamage, strikePos, Vector3.down);
                Debug.Log($"<color=#FFD700>[CelestialThunderstorm]</color> Struck target for {_celestialStrikeDamage} damage!");
            }
        }
    }
}
