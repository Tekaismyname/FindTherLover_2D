using System.Collections;
using UnityEngine;
using FindTheLover.Combat;
using FindTheLover.Environment;

namespace FindTheLover.Enemies
{
    /// <summary>
    /// Logic Layer: Specialized AI for the Zone 2 Magma Scarab (Kamikaze Beetle).
    /// Skitters rapidly toward the Player and primes its volatile magma abdomen upon entering attack range.
    /// After a telegraph wind-up (igniteWindupDuration), it detonates for massive AoE damage (45 DMG).
    /// If slain by the Player before the countdown finishes, its shell ruptures harmlessly without exploding.
    /// </summary>
    public class MagmaScarabEnemyAI : EnemyAI
    {
        // =========================================================================================
        // KHỐI 1: CẤU HÌNH KÍCH NỔ CẢM TỬ (KAMIKAZE DETONATION SETTINGS)
        // =========================================================================================
        [Header("Magma Scarab — Kamikaze Explosion")]
        [Tooltip(" AoE explosion damage dealt when the Scarab detonates (GDD: 45 DMG).")]
        [SerializeField] private float explosionDamage = 45f;

        [Tooltip("Blast radius in meters within which targets take explosion damage.")]
        [SerializeField] private float explosionRadius = 3.2f;

        [Tooltip("Telegraph wind-up time in seconds before detonation (gives Player a window to dodge or slay).")]
        [SerializeField] private float igniteWindupDuration = 0.85f;

        [Tooltip("Maximum scale multiplier reached right before detonation.")]
        [SerializeField] private float swellScaleMultiplier = 1.25f;

        [Tooltip("Glowing magma tint pulsed during the ignition countdown.")]
        [SerializeField] private Color igniteGlowColor = new Color(1f, 0.55f, 0.1f, 1f);

        [Tooltip("Standalone explosion VFX prefab (sprite animation + magma sparks) spawned on detonation.")]
        [SerializeField] private GameObject explosionVfxPrefab;

        [Tooltip("Optional audio clip played when the Scarab detonates.")]
        [SerializeField] private AudioClip explosionSound;

        [Header("Magma Scarab — Burrow Spawn Emergence")]
        [Tooltip("Duration in seconds of the ground emergence spawn animation (Scarab_Spawn_Blend = 0.5s).")]
        [SerializeField] private float spawnAnimDuration = 0.5f;

        private bool _isSpawning;
        private bool _isIgniting;
        private bool _hasExploded;
        private Coroutine _spawnCoroutine;
        private Coroutine _igniteCoroutine;

        // Extra Animator Parameter Hashes specific to MagmaScarabController
        protected static readonly int SpawnHash = Animator.StringToHash("Spawn");
        protected static readonly int IsIgnitingHash = Animator.StringToHash("IsIgniting");
        protected static readonly int ExplodeHash = Animator.StringToHash("Explode");

        public bool IsSpawning => _isSpawning;
        public bool IsIgniting => _isIgniting;
        public bool HasExploded => _hasExploded;

        protected override void Start()
        {
            base.Start();

            // Play ground emergence lock on initial scene start (Entry -> Scarab_Spawn_Blend)
            TriggerSpawnEmergence(triggerAnimator: false);
        }

        // =========================================================================================
        // KHỐI 2: VÒNG LẶP ĐÁNH GIÁ FSM (KHÓA DI CHUYỂN KHI ĐANG CHUI LÊN HOẶC NẠP NỔ)
        // =========================================================================================
        protected override void EvaluateStateMachine()
        {
            // Lock movement while emerging from underground or priming kamikaze explosion
            if (_isSpawning || _isIgniting || _hasExploded) return;

            base.EvaluateStateMachine();
        }

        private void TriggerSpawnEmergence(bool triggerAnimator)
        {
            if (_spawnCoroutine != null)
            {
                StopCoroutine(_spawnCoroutine);
            }
            _spawnCoroutine = StartCoroutine(SpawnEmergenceRoutine(triggerAnimator));
        }

        private IEnumerator SpawnEmergenceRoutine(bool triggerAnimator)
        {
            _isSpawning = true;

            if (_navAgent != null && _navAgent.isOnNavMesh)
            {
                _navAgent.isStopped = true;
                _navAgent.velocity = Vector3.zero;
            }

            if (animator != null)
            {
                animator.SetBool(IsMovingHash, false);
                if (triggerAnimator)
                {
                    animator.SetTrigger(SpawnHash);
                }
            }

            yield return new WaitForSeconds(spawnAnimDuration);

            _isSpawning = false;
            _spawnCoroutine = null;
        }

        // =========================================================================================
        // KHỐI 3: KÍCH HOẠT ĐẾM NGƯỢC PHÁT NỔ KHI VÀO TẦM TẤN CÔNG
        // =========================================================================================
        protected override void ExecuteAttackBehavior()
        {
            if (_isIgniting || _hasExploded || _currentState == EnemyState.Dead) return;

            _igniteCoroutine = StartCoroutine(IgniteAndExplodeRoutine());
        }

        private IEnumerator IgniteAndExplodeRoutine()
        {
            _isIgniting = true;

            // 1. Brake NavMeshAgent immediately so the beetle plants its feet and swells
            if (_navAgent != null && _navAgent.isOnNavMesh)
            {
                _navAgent.isStopped = true;
                _navAgent.ResetPath();
                _navAgent.velocity = Vector3.zero;
            }

            // 2. Switch Animator to Scarab_Ignite_Blend
            if (animator != null)
            {
                animator.SetBool(IsMovingHash, false);
                animator.SetBool(IsIgnitingHash, true);
            }

            Vector3 startScale = transform.localScale;
            Color baseColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
            float elapsed = 0f;

            // 3. Telegraph Loop: Swell body scale & rapidly pulse bright magma orange-yellow
            while (elapsed < igniteWindupDuration)
            {
                if (_currentState == EnemyState.Dead) yield break;

                float t = elapsed / igniteWindupDuration;
                transform.localScale = Vector3.Lerp(startScale, startScale * swellScaleMultiplier, t);

                if (spriteRenderer != null)
                {
                    float pulse = Mathf.PingPong(elapsed * 16f, 1f);
                    spriteRenderer.color = Color.Lerp(baseColor, igniteGlowColor, pulse);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            if (_currentState == EnemyState.Dead) yield break;

            // 4. Detonate!
            Detonate();
        }

        private void Detonate()
        {
            _hasExploded = true;
            _isIgniting = false;
            _currentState = EnemyState.Dead;

            // Restore scale and color so the Impact shockwave sprite renders cleanly
            transform.localScale = _isNightFrenzied ? _originalScale * 1.2f : _originalScale;
            if (spriteRenderer != null)
            {
                spriteRenderer.color = _isNightFrenzied ? new Color(1f, 0.45f, 1f) : _originalSpriteColor;
            }

            // Trigger Scarab_Impact_Blend animation on the beetle
            if (animator != null)
            {
                animator.SetBool(IsIgnitingHash, false);
                animator.SetBool(IsDeadHash, true);
                animator.SetTrigger(ExplodeHash);
            }

            // Spawn standalone Magma Explosion VFX (Magma_Explosion_FX sprite + lava particles)
            if (explosionVfxPrefab != null)
            {
                GameObject vfx = Instantiate(explosionVfxPrefab, transform.position, Quaternion.identity);
                Destroy(vfx, 1.2f);
            }

            if (explosionSound != null)
            {
                AudioSource.PlayClipAtPoint(explosionSound, transform.position);
            }

            // Deal AoE explosion damage (45 DMG baseline, 1.5x during Night Frenzy)
            float finalDamage = _isNightFrenzied ? explosionDamage * 1.5f : explosionDamage;
            ApplyExplosionDamage(finalDamage);

            // Disable colliders and NavMeshAgent
            if (_navAgent != null && _navAgent.isOnNavMesh)
            {
                _navAgent.isStopped = true;
                _navAgent.enabled = false;
            }

            Collider[] cols = GetComponentsInChildren<Collider>();
            for (int i = 0; i < cols.Length; i++)
            {
                cols[i].enabled = false;
            }

            Debug.Log($"<color=orange>[MagmaScarabEnemyAI]</color> {gameObject.name} DETONATED for {finalDamage} AoE damage!");

            // Return to pool after Impact animation finishes (~0.6s)
            StartCoroutine(DelayedPoolReturnRoutine(0.7f));
        }

        private void ApplyExplosionDamage(float damage)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
            bool damagedPlayer = false;

            for (int i = 0; i < hits.Length; i++)
            {
                Collider hit = hits[i];
                if (hit.gameObject == gameObject || hit.transform.IsChildOf(transform)) continue;

                // Check if hit target is the Player
                if (!damagedPlayer && (hit.CompareTag("Player") || hit.GetComponentInParent<PlayerLocomotion.PlayerLocomotion>() != null))
                {
                    // Respect Campfire Sanctuary protection
                    if (CampfireSanctuary.Instance != null &&
                        CampfireSanctuary.Instance.IsPositionInSanctuary(hit.transform.position))
                    {
                        continue;
                    }

                    IDamageable damageable = hit.GetComponentInChildren<IDamageable>() ??
                                             hit.GetComponentInParent<IDamageable>();
                    if (damageable != null)
                    {
                        damagedPlayer = true;
                        Vector3 hitPoint = hit.ClosestPoint(transform.position);
                        Vector3 hitNormal = (hit.transform.position - transform.position).normalized;
                        damageable.TakeDamage(damage, hitPoint, hitNormal);
                    }
                }
            }
        }

        // =========================================================================================
        // KHỐI 4: XỬ LÝ KHI BỊ NGƯỜI CHƠI TIÊU DIỆT TRƯỚC KHI KỊP NỔ (RUPTURE DEATH)
        // =========================================================================================
        protected override void HandleDeath()
        {
            // If already detonated via Detonate(), ignore redundant OnDeath callback
            if (_hasExploded) return;

            // Cancel ignition countdown if the Player killed the Scarab in time!
            if (_igniteCoroutine != null)
            {
                StopCoroutine(_igniteCoroutine);
                _igniteCoroutine = null;
            }

            _isIgniting = false;
            transform.localScale = _isNightFrenzied ? _originalScale * 1.2f : _originalScale;

            if (animator != null)
            {
                animator.SetBool(IsIgnitingHash, false);
            }

            Debug.Log($"<color=green>[MagmaScarabEnemyAI]</color> {gameObject.name} was slain before it could detonate! Shell ruptured harmlessly.");

            // Plays Scarab_Rupture_Blend via IsDead = true in base.HandleDeath()
            base.HandleDeath();
        }

        private IEnumerator DelayedPoolReturnRoutine(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (EnemyPoolManager.Instance != null)
            {
                EnemyPoolManager.Instance.ReturnEnemy(this);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        // =========================================================================================
        // KHỐI 5: RESET TRẠNG THÁI KHI TÁI SỬ DỤNG TỪ OBJECT POOL
        // =========================================================================================
        public override void SpawnAt(Vector3 spawnPosition)
        {
            if (_spawnCoroutine != null)
            {
                StopCoroutine(_spawnCoroutine);
                _spawnCoroutine = null;
            }

            if (_igniteCoroutine != null)
            {
                StopCoroutine(_igniteCoroutine);
                _igniteCoroutine = null;
            }

            _isSpawning = false;
            _isIgniting = false;
            _hasExploded = false;
            transform.localScale = _originalScale;

            if (animator != null)
            {
                animator.SetBool(IsIgnitingHash, false);
            }

            base.SpawnAt(spawnPosition);

            // Play burrow-up emergence animation every time the Scarab is deployed from the Object Pool
            TriggerSpawnEmergence(triggerAnimator: true);
        }

        private void OnDrawGizmosSelected()
        {
            if (enemyData == null) return;

            // Yellow: Detection radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, enemyData.detectRadius);

            // Red: Trigger ignition range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, enemyData.attackRange);

            // Orange: AoE Explosion blast radius
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.9f);
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}
