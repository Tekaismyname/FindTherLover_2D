using System.Collections;
using UnityEngine;
using FindTheLover.Combat;
using FindTheLover.Environment;

namespace FindTheLover.Enemies
{
    /// <summary>
    /// Logic Layer: Specialized AI for the Zone 2 Skeleton Goblin.
    /// Extends EnemyAI with two signature mechanics:
    /// 1. Mid-Range Bone Throw: Hurls a spinning bone projectile when chasing within [minThrowRange, maxThrowRange].
    /// 2. False Death & Bone Pile Reassembly: Collapses into a 1-HP Bone Pile on first death and revives
    ///    after reassembleDelay seconds unless struck a final blow while collapsed.
    /// </summary>
    public class SkeletonEnemyAI : EnemyAI
    {
        // =========================================================================================
        // KHỐI 1: CẤU HÌNH NÉM XƯƠNG (MID-RANGE BONE THROW)
        // =========================================================================================
        [Header("Skeleton — Bone Throw Ranged Attack")]
        [Tooltip("Prefab containing the BoneProjectile component.")]
        [SerializeField] private BoneProjectile boneProjectilePrefab;

        [Tooltip("Minimum distance to Player required to trigger a bone throw.")]
        [SerializeField] private float minThrowRange = 3.0f;

        [Tooltip("Maximum distance to Player within which the Skeleton can throw a bone.")]
        [SerializeField] private float maxThrowRange = 8.5f;

        [Tooltip("Cooldown in seconds between successive bone throws.")]
        [SerializeField] private float throwCooldown = 4.0f;

        [Tooltip("Damage dealt by a thrown bone projectile.")]
        [SerializeField] private float throwDamage = 10f;

        [Tooltip("Vertical height offset from the Skeleton root where the bone spawns.")]
        [SerializeField] private float projectileSpawnHeight = 0.2f;

        [Tooltip("Delay in seconds from triggering the throw animation to spawning the projectile.")]
        [SerializeField] private float throwReleaseDelay = 0.25f;

        // =========================================================================================
        // KHỐI 2: CẤU HÌNH ĐỐNG XƯƠNG HỒI SINH (BONE PILE REASSEMBLY)
        // =========================================================================================
        [Header("Skeleton — Bone Pile Reassembly")]
        [Tooltip("Maximum number of times this Skeleton can reassemble per spawn lifecycle.")]
        [SerializeField] private int maxReassembleCount = 1;

        [Tooltip("Time in seconds the Skeleton stays collapsed as a vulnerable 1-HP bone pile.")]
        [SerializeField] private float reassembleDelay = 4.0f;

        [Tooltip("Duration of the reassembly rising animation before resuming Chase.")]
        [SerializeField] private float reassembleAnimDuration = 0.65f;

        [Tooltip("Fraction of maxHealth restored after successful reassembly (0.5 = 50% HP).")]
        [Range(0.1f, 1f)]
        [SerializeField] private float reviveHealthRatio = 0.5f;

        [Tooltip("Brief invulnerability window right after collapsing so the killing blow does not instantly shatter the pile.")]
        [SerializeField] private float bonePileGracePeriod = 0.4f;

        private float _lastThrowTime = -999f;
        private bool _isThrowingBone;
        private bool _isBonePile;
        private int _currentReassembleCount;
        private Coroutine _reassembleCoroutine;
        private Coroutine _throwCoroutine;

        // Extra Animator Parameter Hashes specific to SkeletonController
        protected static readonly int BoneThrowHash = Animator.StringToHash("BoneThrow");
        protected static readonly int IsBonePileHash = Animator.StringToHash("IsBonePile");
        protected static readonly int ReassembleHash = Animator.StringToHash("Reassemble");

        public bool IsBonePile => _isBonePile;
        public int CurrentReassembleCount => _currentReassembleCount;

        // =========================================================================================
        // KHỐI 3: VÒNG LẶP ĐÁNH GIÁ FSM (BỔ SUNG NÉM XƯƠNG TẦM TRUNG)
        // =========================================================================================
        protected override void EvaluateStateMachine()
        {
            // Freeze AI decision-making while collapsed as a bone pile or mid-throw windup
            if (_isBonePile || _isThrowingBone) return;

            if (enemyData == null || _targetPlayer == null) return;

            Vector3 flatEnemyPos = new Vector3(transform.position.x, 0f, transform.position.z);
            Vector3 flatPlayerPos = new Vector3(_targetPlayer.position.x, 0f, _targetPlayer.position.z);
            float distanceToTarget = Vector3.Distance(flatEnemyPos, flatPlayerPos);

            bool isPlayerInSanctuary = CampfireSanctuary.Instance != null &&
                                       CampfireSanctuary.Instance.IsPositionInSanctuary(_targetPlayer.position);

            // Check mid-range bone throw opportunity while chasing
            if (_currentState == EnemyState.Chase && !isPlayerInSanctuary && boneProjectilePrefab != null)
            {
                bool inThrowBand = distanceToTarget >= minThrowRange && distanceToTarget <= maxThrowRange;
                bool cooldownReady = Time.time >= _lastThrowTime + throwCooldown;

                if (inThrowBand && cooldownReady)
                {
                    _throwCoroutine = StartCoroutine(PerformBoneThrowRoutine());
                    return;
                }
            }

            // Delegate standard Idle / Chase / Melee Bite Attack logic to base EnemyAI
            base.EvaluateStateMachine();
        }

        private IEnumerator PerformBoneThrowRoutine()
        {
            _isThrowingBone = true;
            _lastThrowTime = Time.time;

            // Pause NavMesh movement while winding up the throw
            if (_navAgent != null && _navAgent.isOnNavMesh)
            {
                _navAgent.isStopped = true;
                _navAgent.velocity = Vector3.zero;
            }

            if (animator != null)
            {
                animator.SetBool(IsMovingHash, false);
                animator.SetTrigger(BoneThrowHash);
            }

            yield return new WaitForSeconds(throwReleaseDelay);

            // Spawn the spinning bone projectile if still alive and not collapsed
            if (_currentState != EnemyState.Dead && !_isBonePile && _targetPlayer != null && boneProjectilePrefab != null)
            {
                Vector3 spawnPos = transform.position + Vector3.up * projectileSpawnHeight;
                Vector3 throwDir = (_targetPlayer.position - transform.position);
                throwDir.y = 0f;

                BoneProjectile projectile = Instantiate(boneProjectilePrefab, spawnPos, Quaternion.identity);
                float finalDamage = _isNightFrenzied ? throwDamage * 1.5f : throwDamage;
                projectile.Initialize(throwDir, finalDamage, gameObject);
            }

            yield return new WaitForSeconds(0.25f);

            _isThrowingBone = false;

            // Resume chasing if still in Chase state
            if (_currentState == EnemyState.Chase && !_isBonePile && _navAgent != null && _navAgent.isOnNavMesh)
            {
                _navAgent.isStopped = false;
                if (animator != null) animator.SetBool(IsMovingHash, true);
            }
        }

        // =========================================================================================
        // KHỐI 4: CƠ CHẾ GIẢ CHẾT (BONE PILE) & TỰ RÁP XƯƠNG HỒI SINH
        // =========================================================================================
        protected override void HandleDeath()
        {
            // Stop any active throw windup
            if (_throwCoroutine != null)
            {
                StopCoroutine(_throwCoroutine);
                _throwCoroutine = null;
                _isThrowingBone = false;
            }

            // Case 1: Skeleton still has a reassembly charge and is not yet in Bone Pile state
            if (!_isBonePile && _currentReassembleCount < maxReassembleCount)
            {
                EnterBonePileState();
                return;
            }

            // Case 2: Struck while in Bone Pile state OR out of reassemble charges -> Permanent Death
            ShatterPermanently();
        }

        private void EnterBonePileState()
        {
            _isBonePile = true;
            _currentReassembleCount++;

            // 1. Stop NavMeshAgent movement, but KEEP Collider enabled so the Player can land the finishing blow!
            if (_navAgent != null && _navAgent.isOnNavMesh)
            {
                _navAgent.isStopped = true;
                _navAgent.ResetPath();
                _navAgent.velocity = Vector3.zero;
            }

            // 2. Grant 1 HP to the collapsed bone pile with a brief grace period
            if (_healthSystem != null)
            {
                _healthSystem.Revive(1f, bonePileGracePeriod);
            }

            // 3. Play collapse animation and hold in Skeleton_Death_Blend
            if (animator != null)
            {
                animator.SetBool(IsMovingHash, false);
                animator.SetBool(IsBonePileHash, true);
                animator.SetBool(IsDeadHash, true);
            }

            Debug.Log($"<color=orange>[SkeletonEnemyAI]</color> {gameObject.name} collapsed into a Bone Pile! Strike within {reassembleDelay}s to finish it off!");

            // 4. Start countdown timer to reassemble
            if (_reassembleCoroutine != null)
            {
                StopCoroutine(_reassembleCoroutine);
            }
            _reassembleCoroutine = StartCoroutine(BonePileReassembleRoutine());
        }

        private IEnumerator BonePileReassembleRoutine()
        {
            yield return new WaitForSeconds(reassembleDelay);

            // Safety check in case it was shattered during the wait
            if (!_isBonePile || _currentState == EnemyState.Dead) yield break;

            // Trigger Reassemble animation (Skeleton_Death_Blend -> Skeleton_Reassemble_Blend)
            if (animator != null)
            {
                animator.SetBool(IsDeadHash, false);
                animator.SetBool(IsBonePileHash, false);
                animator.SetTrigger(ReassembleHash);
            }

            yield return new WaitForSeconds(reassembleAnimDuration);

            if (_currentState == EnemyState.Dead) yield break;

            // Restore partial health pool (e.g., 50% max HP)
            float restoredHp = enemyData != null ? enemyData.maxHealth * reviveHealthRatio : 25f;
            if (_healthSystem != null)
            {
                _healthSystem.Revive(restoredHp, 0.25f);
            }

            _isBonePile = false;
            _reassembleCoroutine = null;

            Debug.Log($"<color=yellow>[SkeletonEnemyAI]</color> {gameObject.name} reassembled with {restoredHp} HP!");

            // Immediately resume chasing the player
            TransitionToState(EnemyState.Chase);
        }

        private void ShatterPermanently()
        {
            if (_reassembleCoroutine != null)
            {
                StopCoroutine(_reassembleCoroutine);
                _reassembleCoroutine = null;
            }

            _isBonePile = false;

            if (animator != null)
            {
                animator.SetBool(IsBonePileHash, false);
            }

            Debug.Log($"<color=red>[SkeletonEnemyAI]</color> {gameObject.name} was permanently shattered!");

            // Execute standard permanent death & Object Pool return from base EnemyAI
            base.HandleDeath();
        }

        // =========================================================================================
        // KHỐI 5: RESET TRẠNG THÁI KHI TÁI SỬ DỤNG TỪ OBJECT POOL
        // =========================================================================================
        public override void SpawnAt(Vector3 spawnPosition)
        {
            _isBonePile = false;
            _isThrowingBone = false;
            _currentReassembleCount = 0;

            if (_reassembleCoroutine != null)
            {
                StopCoroutine(_reassembleCoroutine);
                _reassembleCoroutine = null;
            }

            if (_throwCoroutine != null)
            {
                StopCoroutine(_throwCoroutine);
                _throwCoroutine = null;
            }

            if (animator != null)
            {
                animator.SetBool(IsBonePileHash, false);
            }

            base.SpawnAt(spawnPosition);
        }

        private void OnDrawGizmosSelected()
        {
            if (enemyData == null) return;

            // Yellow: Detection radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, enemyData.detectRadius);

            // Cyan: Mid-range Bone Throw band (min & max)
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, minThrowRange);
            Gizmos.DrawWireSphere(transform.position, maxThrowRange);

            // Red: Melee Bite range
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, enemyData.attackRange);
        }
    }
}
