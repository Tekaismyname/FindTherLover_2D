using System.Collections;
using UnityEngine;
using FindTheLover.Combat;
using FindTheLover.Environment;

namespace FindTheLover.Enemies
{
    /// <summary>
    /// Logic Layer: Specialized AI for the Zone 2 Spore Shroom (Biological Acid Turret & Stalker).
    /// Creeps menacingly through the toxic swamp or acts as a stationary sentinel.
    /// Spits long-range acid spore projectiles (up to 8m) that inflict direct impact
    /// damage plus a lingering poison DoT debuff (2 HP/s for 5s).
    /// Features 2.5D frontal directional priority to prevent crab-walking.
    /// </summary>
    public class SporeShroomEnemyAI : EnemyAI
    {
        // =========================================================================================
        // KHỐI 1: CẤU HÌNH BẮN BÀO TỬ ĐỘC (ACID SPORE SPIT SETTINGS)
        // =========================================================================================
        [Header("Spore Shroom — Ranged Acid Spit Attack")]
        [Tooltip("Prefab containing the SporeProjectile component.")]
        [SerializeField] private SporeProjectile sporeProjectilePrefab;

        [Tooltip("Effective maximum range for spitting spore projectiles (GDD: 8.0m).")]
        [SerializeField] private float spitRange = 8.0f;

        [Tooltip("Cooldown in seconds between successive spore spit attacks.")]
        [SerializeField] private float spitCooldown = 2.5f;

        [Tooltip("Direct impact damage dealt by the projectile.")]
        [SerializeField] private float directDamage = 10f;

        [Tooltip("Damage dealt per second by lingering poison DoT (GDD: 2 HP/s).")]
        [SerializeField] private float poisonDps = 2f;

        [Tooltip("Duration in seconds of the poison DoT debuff (GDD: 5s).")]
        [SerializeField] private float poisonDuration = 5f;

        [Tooltip("Vertical height offset from the root where the spore launches.")]
        [SerializeField] private float projectileSpawnHeight = 1.4f;

        [Tooltip("Delay in seconds from triggering the spit animation to launching the projectile.")]
        [SerializeField] private float spitReleaseDelay = 0.35f;

        [Tooltip("If true, the mushroom behaves as a fixed stationary turret without moving.")]
        [SerializeField] private bool isStationaryTurret = false;

        private float _lastSpitTime = -999f;
        private bool _isSpitting;
        private Coroutine _spitCoroutine;

        public bool IsSpitting => _isSpitting;

        // =========================================================================================
        // KHỐI 2: VÒNG LẶP SUY NGHĨ (FSM EVALUATION & DYNAMIC ACTION)
        // =========================================================================================
        protected override void EvaluateStateMachine()
        {
            if (_isSpitting) return;

            if (enemyData == null || _targetPlayer == null) return;

            Vector3 flatEnemyPos = new Vector3(transform.position.x, 0f, transform.position.z);
            Vector3 flatPlayerPos = new Vector3(_targetPlayer.position.x, 0f, _targetPlayer.position.z);
            float distanceToTarget = Vector3.Distance(flatEnemyPos, flatPlayerPos);

            bool isPlayerInSanctuary = CampfireSanctuary.Instance != null &&
                                       CampfireSanctuary.Instance.IsPositionInSanctuary(_targetPlayer.position);

            switch (_currentState)
            {
                case EnemyState.Idle:
                    if (distanceToTarget <= _currentDetectRadius && !isPlayerInSanctuary)
                    {
                        TransitionToState(EnemyState.Chase);
                    }
                    break;

                case EnemyState.Chase:
                    if (distanceToTarget > _currentDetectRadius || isPlayerInSanctuary)
                    {
                        TransitionToState(EnemyState.Idle);
                    }
                    else if (distanceToTarget <= spitRange)
                    {
                        TransitionToState(EnemyState.Attack);
                    }
                    else
                    {
                        if (!isStationaryTurret && _navAgent != null && _navAgent.isOnNavMesh)
                        {
                            _navAgent.isStopped = false;
                            _navAgent.speed = _currentMoveSpeed;
                            _navAgent.SetDestination(_targetPlayer.position);
                            if (animator != null) animator.SetBool(IsMovingHash, true);
                        }
                    }
                    break;

                case EnemyState.Attack:
                    if (isPlayerInSanctuary)
                    {
                        TransitionToState(EnemyState.Idle);
                    }
                    else if (distanceToTarget > spitRange)
                    {
                        TransitionToState(EnemyState.Chase);
                    }
                    else
                    {
                        // Action Enhancement: Fire when ready, creep stalk during cooldown
                        if (Time.time >= _lastSpitTime + spitCooldown)
                        {
                            ExecuteAttackBehavior();
                        }
                        else if (!isStationaryTurret && _navAgent != null && _navAgent.isOnNavMesh)
                        {
                            // Stalk creeping if distance > 3.5m to maintain combat pressure
                            if (distanceToTarget > 3.5f)
                            {
                                _navAgent.isStopped = false;
                                _navAgent.speed = _currentMoveSpeed * 0.8f;
                                _navAgent.SetDestination(_targetPlayer.position);
                                if (animator != null) animator.SetBool(IsMovingHash, true);
                            }
                            else
                            {
                                _navAgent.isStopped = true;
                                if (animator != null) animator.SetBool(IsMovingHash, false);
                            }
                        }
                    }
                    break;
            }
        }

        // =========================================================================================
        // KHỐI 3: THI HÀNH ĐÒN PHUN BÀO TỬ ĐỘC
        // =========================================================================================
        protected override void ExecuteAttackBehavior()
        {
            if (_isSpitting || _currentState == EnemyState.Dead) return;

            if (Time.time < _lastSpitTime + spitCooldown) return;

            _spitCoroutine = StartCoroutine(PerformSpitRoutine());
        }

        private IEnumerator PerformSpitRoutine()
        {
            _isSpitting = true;
            _lastSpitTime = Time.time;

            if (_navAgent != null && _navAgent.isOnNavMesh)
            {
                _navAgent.isStopped = true;
                _navAgent.velocity = Vector3.zero;
            }

            if (animator != null)
            {
                animator.SetBool(IsMovingHash, false);
                animator.SetTrigger(AttackHash);
            }

            yield return new WaitForSeconds(spitReleaseDelay);

            if (_currentState != EnemyState.Dead && _targetPlayer != null && sporeProjectilePrefab != null)
            {
                Vector3 spawnPos = transform.position + Vector3.up * projectileSpawnHeight;
                Vector3 spitDir = (_targetPlayer.position - transform.position);
                spitDir.y = 0f;

                SporeProjectile projectile = Instantiate(sporeProjectilePrefab, spawnPos, Quaternion.identity);
                float finalDamage = _isNightFrenzied ? directDamage * 1.5f : directDamage;
                float finalDps = _isNightFrenzied ? poisonDps * 1.5f : poisonDps;

                projectile.Initialize(spitDir, finalDamage, gameObject, finalDps, poisonDuration);
            }

            yield return new WaitForSeconds(0.2f);

            _isSpitting = false;
            _spitCoroutine = null;

            if (_currentState == EnemyState.Chase && _navAgent != null && _navAgent.isOnNavMesh && !isStationaryTurret)
            {
                _navAgent.isStopped = false;
                if (animator != null) animator.SetBool(IsMovingHash, true);
            }
        }

        // =========================================================================================
        // KHỐI 4: 2.5D FRONTAL PRIORITY DIRECTIONAL ANIMATION (CHỐNG LỆCH HƯỚNG / ĐI CUA NGANG)
        // =========================================================================================
        protected override void UpdateAnimationDirection()
        {
            if (animator == null || _targetPlayer == null) return;

            // Always calculate facing direction directly towards the player target
            // (Never use _navAgent.velocity, which drifts sideways during pathfinding/avoidance)
            Vector3 direction = (_targetPlayer.position - transform.position).normalized;

            float moveX = 0f;
            float moveY = 0f;

            // 2.5D Perspective Priority:
            // With camera pitched down from the south, the player-facing forward presentation (fw)
            // is visually dominant. We give Front View a wide priority arc (~140 degrees downward):
            // Whenever player is southward or diagonal-forward (direction.z <= 0.25f and not extreme 90-degree lateral)
            if (direction.z <= 0.25f && Mathf.Abs(direction.x) < 0.92f)
            {
                // Front View (Facing camera / player directly)
                moveY = -1.0f;
                moveX = 0.0f;
            }
            else if (direction.z > 0.5f && direction.z > Mathf.Abs(direction.x))
            {
                // Back View (Strictly when player is distinctly behind the mushroom)
                moveY = 1.0f;
                moveX = 0.0f;
            }
            else
            {
                // Lateral Side Views (Right or Left)
                moveX = direction.x > 0f ? 1.0f : -1.0f;
                moveY = 0.0f;
            }

            animator.SetFloat(MoveXHash, moveX);
            animator.SetFloat(MoveYHash, moveY);
        }

        protected override void UpdateSpriteFacing()
        {
            // SporeShroom has dedicated 4-directional clips (fw, bw, r, l) mapped to (-1,0) and (1,0)
            // in all Blend Trees. SpriteRenderer.flipX must remain false to prevent inverted mirroring.
            if (spriteRenderer != null && spriteRenderer.flipX)
            {
                spriteRenderer.flipX = false;
            }
        }

        protected override void HandleDeath()
        {
            if (_spitCoroutine != null)
            {
                StopCoroutine(_spitCoroutine);
                _spitCoroutine = null;
                _isSpitting = false;
            }

            base.HandleDeath();
        }

        public override void SpawnAt(Vector3 spawnPosition)
        {
            if (_spitCoroutine != null)
            {
                StopCoroutine(_spitCoroutine);
                _spitCoroutine = null;
            }

            _isSpitting = false;

            base.SpawnAt(spawnPosition);
        }

        private void OnDrawGizmosSelected()
        {
            if (enemyData == null) return;

            // Yellow: Sensory detection sphere
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, enemyData.detectRadius);

            // Green: Spore Spit range (8m)
            Gizmos.color = new Color(0.2f, 1f, 0.2f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, spitRange);
        }
    }
}
