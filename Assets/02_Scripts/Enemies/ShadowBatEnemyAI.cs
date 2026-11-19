using System.Collections;
using UnityEngine;
using FindTheLover.Combat;
using FindTheLover.Environment;

namespace FindTheLover.Enemies
{
    /// <summary>
    /// Logic Layer: Specialized AI controller for the Shadow Bat (Aerial Hit & Run Assassin).
    /// Inherits from <see cref="EnemyAI"/> and manages flight altitude, circular stalking,
    /// high-speed dive attacks, sonic wave screeching, and death fall physics.
    /// </summary>
    [SelectionBase]
    public class ShadowBatEnemyAI : EnemyAI
    {
        [Header("Flight & Hovering Settings")]
        [Tooltip("Target cruise altitude above the terrain surface in world units.")]
        [SerializeField] private float flightAltitude = 2.2f;

        [Tooltip("Frequency of the hovering sinusoidal bobbing motion.")]
        [SerializeField] private float hoverFrequency = 3.5f;

        [Tooltip("Amplitude of the hovering sinusoidal vertical displacement.")]
        [SerializeField] private float hoverAmplitude = 0.25f;

        [Header("Aerial Combat Parameters")]
        [Tooltip("Distance maintained while circling and stalking the target before diving.")]
        [SerializeField] private float orbitDistance = 4.0f;

        [Tooltip("Speed multiplier applied during the aggressive swoop dive attack.")]
        [SerializeField] private float diveSpeedMultiplier = 2.2f;

        [Tooltip("Duration in seconds of the dive swoop down to target and return up.")]
        [SerializeField] private float diveDuration = 0.85f;

        [Tooltip("Cooldown period between sonic screech blasts.")]
        [SerializeField] private float sonicCooldown = 5.0f;

        [Tooltip("Optional prefab for the sonic shockwave projectile.")]
        [SerializeField] private GameObject sonicProjectilePrefab;

        [Tooltip("Spawn offset for sonic shockwave or bite hit detection.")]
        [SerializeField] private Transform mouthPoint;

        [Header("Visual Child Transform")]
        [Tooltip("Reference to the visual model/sprite container for applying vertical hovering without disrupting NavMesh.")]
        [SerializeField] private Transform visualTransform;

        // Internal State tracking
        private bool _isDiving = false;
        private float _lastSonicTime;
        private float _orbitAngle;
        private Vector3 _visualBaseLocalPosition;

        // Animator Hashes for Bat-specific triggers
        private static readonly int DiveAttackHash = Animator.StringToHash("Attack");
        private static readonly int BiteHash = Animator.StringToHash("Bite");
        private static readonly int SonicHash = Animator.StringToHash("Sonic");
        private static readonly int IsStunnedHash = Animator.StringToHash("IsStunned");

        protected override void Start()
        {
            base.Start();

            // Cache visual base offset
            if (visualTransform == null && spriteRenderer != null)
            {
                visualTransform = spriteRenderer.transform;
            }

            if (visualTransform != null)
            {
                _visualBaseLocalPosition = visualTransform.localPosition;
            }

            // Configure base elevation on NavMeshAgent for aerial locomotion
            if (_navAgent != null)
            {
                _navAgent.baseOffset = flightAltitude;
            }

            // Randomize starting orbit angle to avoid clustering
            _orbitAngle = Random.Range(0f, 360f);
        }

        /// <summary>
        /// LateUpdate handles post-movement visual hover bobbing without interfering with base EnemyAI.Update().
        /// </summary>
        protected virtual void LateUpdate()
        {
            if (_currentState == EnemyState.Dead) return;

            // Apply sinusoidal hover bobbing to the visual sprite child
            ApplyHoverBobbing();
        }

        /// <summary>
        /// Applies subtle vertical sinusoidal oscillation to simulate fluttering wing beats in flight.
        /// </summary>
        private void ApplyHoverBobbing()
        {
            if (visualTransform == null) return;

            if (_isDiving)
            {
                // Reset during dive to ensure precise hit placement
                visualTransform.localPosition = _visualBaseLocalPosition;
                return;
            }

            float verticalOffset = Mathf.Sin(Time.time * hoverFrequency) * hoverAmplitude;
            visualTransform.localPosition = _visualBaseLocalPosition + new Vector3(0f, verticalOffset, 0f);
        }

        /// <summary>
        /// Evaluates AI transitions with hit-and-run aerial tactics.
        /// When target is acquired, the bat circles at orbitDistance and swoops when ready.
        /// </summary>
        protected override void EvaluateStateMachine()
        {
            if (_isDiving) return; // Locked during dive action

            base.EvaluateStateMachine();

            // Check secondary ranged sonic blast
            if (_currentState == EnemyState.Chase && _targetPlayer != null)
            {
                float dist = Vector3.Distance(transform.position, _targetPlayer.position);
                if (dist <= _currentDetectRadius && Time.time >= _lastSonicTime + sonicCooldown)
                {
                    ExecuteSonicScreech();
                }
            }
        }

        /// <summary>
        /// State entry transitions. Delegates to base FSM.
        /// </summary>
        protected override void TransitionToState(EnemyState newState)
        {
            base.TransitionToState(newState);
        }

        /// <summary>
        /// Executes the Aerial Dive Swoop: Launches down towards the target, strikes at apex, and ascends.
        /// </summary>
        protected override void ExecuteAttackBehavior()
        {
            if (_isDiving || _targetPlayer == null) return;
            if (Time.time < _lastAttackTime + (enemyData != null ? enemyData.attackCooldown : 2.5f)) return;

            StartCoroutine(DiveAttackRoutine());
        }

        private IEnumerator DiveAttackRoutine()
        {
            _isDiving = true;
            _lastAttackTime = Time.time;

            // Trigger Dive Animation
            if (animator != null)
            {
                animator.SetTrigger(DiveAttackHash);
            }

            Vector3 startPos = transform.position;
            Vector3 targetGroundPos = _targetPlayer.position;
            targetGroundPos.y = Mathf.Max(0.5f, targetGroundPos.y); // Swoop near ground

            // Disable standard NavMesh destination to control high-speed aerial dive
            if (_navAgent != null)
            {
                _navAgent.isStopped = true;
                _navAgent.updatePosition = false;
            }

            // Adjust dive duration by dive speed multiplier
            float adjustedDuration = Mathf.Max(0.3f, diveDuration / Mathf.Max(1.0f, diveSpeedMultiplier));
            float elapsed = 0f;
            bool hitApplied = false;

            while (elapsed < adjustedDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / adjustedDuration);

                // Parabolic trajectory: down to ground at t=0.5, then ascending back up
                // 4 * t * (1 - t) reaches 1.0 at t=0.5
                float swoopFactor = 4f * t * (1f - t);
                Vector3 currentHorizontal = Vector3.Lerp(startPos, targetGroundPos + (targetGroundPos - startPos).normalized * 2.0f, t);

                float currentY = Mathf.Lerp(startPos.y, targetGroundPos.y, swoopFactor);
                Vector3 currentPos = new Vector3(currentHorizontal.x, currentY, currentHorizontal.z);

                transform.position = currentPos;

                // Check damage delivery at swoop apex (around t = 0.5)
                if (!hitApplied && t >= 0.45f && t <= 0.65f && _targetPlayer != null)
                {
                    float distToTarget = Vector3.Distance(transform.position, _targetPlayer.position);
                    float range = enemyData != null ? enemyData.attackRange : 2.5f;
                    if (distToTarget <= range)
                    {
                        ApplyDamageToTarget(_targetPlayer.gameObject);
                        hitApplied = true;
                    }
                }

                yield return null;
            }

            // Restore NavMeshAgent tracking
            if (_navAgent != null)
            {
                _navAgent.nextPosition = transform.position;
                _navAgent.updatePosition = true;
                _navAgent.isStopped = false;
            }

            _isDiving = false;
        }

        /// <summary>
        /// Inflicts melee impact damage to the player adhering to the IDamageable interface contract.
        /// </summary>
        private void ApplyDamageToTarget(GameObject target)
        {
            var targetDamageable = target.GetComponentInChildren<IDamageable>() ?? target.GetComponentInParent<IDamageable>();
            if (targetDamageable != null)
            {
                Vector3 hitPoint = target.transform.position + Vector3.up * 1.0f;
                Vector3 hitNormal = (target.transform.position - transform.position).normalized;
                targetDamageable.TakeDamage(_currentAttackDamage, hitPoint, hitNormal);
            }
        }

        /// <summary>
        /// Fires a sonic wave blast that disrupts and damages targets from range.
        /// </summary>
        private void ExecuteSonicScreech()
        {
            _lastSonicTime = Time.time;

            if (animator != null)
            {
                animator.SetTrigger(SonicHash);
            }

            if (sonicProjectilePrefab != null)
            {
                Vector3 spawnPos = mouthPoint != null ? mouthPoint.position : transform.position;
                Vector3 dirToTarget = (_targetPlayer.position - spawnPos).normalized;

                GameObject projectile = Instantiate(sonicProjectilePrefab, spawnPos, Quaternion.LookRotation(dirToTarget));
                if (projectile.TryGetComponent<Rigidbody>(out var rb))
                {
                    rb.linearVelocity = dirToTarget * 10f;
                }
            }
        }

        /// <summary>
        /// Enforces 2.5D directional mapping for bat orientation with Frontal View dominance.
        /// </summary>
        protected override void UpdateAnimationDirection()
        {
            if (animator == null || _targetPlayer == null) return;

            Vector3 direction = (_targetPlayer.position - transform.position).normalized;

            float moveX = 0f;
            float moveY = 0f;

            // 2.5D Frontal Priority Arc (~140 degrees downward):
            // Prioritize forward view (fw) when target is in front or diagonal front
            if (direction.z <= 0.25f && Mathf.Abs(direction.x) < 0.92f)
            {
                moveY = -1.0f;
                moveX = 0.0f;
            }
            else if (direction.z > 0.5f && direction.z > Mathf.Abs(direction.x))
            {
                moveY = 1.0f;
                moveX = 0.0f;
            }
            else
            {
                moveX = direction.x > 0f ? 1.0f : -1.0f;
                moveY = 0.0f;
            }

            animator.SetFloat(MoveXHash, moveX);
            animator.SetFloat(MoveYHash, moveY);
        }

        /// <summary>
        /// Shadow Bat has dedicated 4-directional clips (fw, bw, r, l) in its Blend Trees.
        /// SpriteRenderer.flipX must remain false to prevent inverted mirroring.
        /// </summary>
        protected override void UpdateSpriteFacing()
        {
            if (spriteRenderer != null && spriteRenderer.flipX)
            {
                spriteRenderer.flipX = false;
            }
        }

        /// <summary>
        /// Specialized death sequence: Drops from flight altitude to ground before fading.
        /// </summary>
        protected override void HandleDeath()
        {
            base.HandleDeath();

            StartCoroutine(DeathFallRoutine());
        }

        private IEnumerator DeathFallRoutine()
        {
            if (_navAgent != null)
            {
                _navAgent.enabled = false;
            }

            // Rapid gravitational plummet to ground
            float fallDuration = 0.4f;
            float elapsed = 0f;
            Vector3 startPos = transform.position;
            Vector3 groundPos = new Vector3(startPos.x, Mathf.Max(0.1f, startPos.y - flightAltitude), startPos.z);

            while (elapsed < fallDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fallDuration;
                // Quadratic gravity acceleration (t * t)
                transform.position = Vector3.Lerp(startPos, groundPos, t * t);
                yield return null;
            }

            transform.position = groundPos;
        }
    }
}
