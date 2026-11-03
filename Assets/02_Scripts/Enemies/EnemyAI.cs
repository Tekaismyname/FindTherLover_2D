using UnityEngine;
using UnityEngine.AI;
using FindTheLover.Combat;
using FindTheLover.Enemies.Data;
using System.Collections;
using Unity.AppUI.Core;
using FindTheLover.Environment;
using UnityEditor.EditorTools;

namespace FindTheLover.Enemies
{
    /// <summary>
    /// Discrete states for the Enemy Finite State Machine (FSM).
    /// </summary>
    public enum EnemyState
    {
        Idle,   // Standing still or observing
        Chase,  // Actively pathfinding towards the target player
        Attack, // In melee range, performing attacks with cooldown
        Dead    // Health reached zero, disabled and waiting for destruction
    }

    /// <summary>
    /// Logic Layer: Finite State Machine (FSM) controlling enemy perception, NavMesh pathfinding, and combat engagement.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(HealthSystem))]
    public abstract class EnemyAI : MonoBehaviour, Iknockbackable
    {
        // =========================================================================================
        // KHỐI 1: KHAI BÁO BIẾN CẤU HÌNH & THÀNH PHẦN
        // =========================================================================================
        [Header("Configuration")]
        [Tooltip("Enemy static blueprint data reference.")]
        [SerializeField] protected EnemyDataSO enemyData;

        [Header("Presentation")]
        [Tooltip("SpriteRenderer reference for horizontal directional flipping.")]
        [SerializeField] protected SpriteRenderer spriteRenderer;

        [Tooltip("Animator reference for controlling sprite animation states.")]
        [SerializeField] protected Animator animator;

        [Header("Night Frenzy Settings")]
        [Tooltip("speed multiplier applied during nighttime")]
        [SerializeField] private float nightSpeedMultiplier = 1.3f;

        [Tooltip("Damage multiplier applied during nighttime")]
        [SerializeField] private float nightDamageMultiplier = 1.5f;

        [Tooltip("Detection darius multiplier applied during nighttime")]
        [SerializeField] private float nightDetectionMultiplier = 1.6f;

        [Header("Night Frenzy Visual Feedback")]
        [Tooltip("Visual tint applied to the sprite when frenzied at night")]
        [SerializeField] private Color frenziedColor = new Color(1f, 0.45f, 1f);

        [Tooltip("Scale multiplier making the moster look larger and more threatening.")]
        [SerializeField] private float enrageScaleMultiplier = 1.2f;

        [Tooltip("Red eye glow light or visual object shining through the night fog.")]
        [SerializeField] private GameObject eyeGlowObject;

        protected EnemyState _currentState = EnemyState.Idle;
        protected NavMeshAgent _navAgent;
        protected HealthSystem _healthSystem;
        protected Transform _targetPlayer;
        protected float _lastAttackTime;
        // Dynamic runtime metrics
        protected float _currentMoveSpeed;
        protected float _currentAttackDamage;
        protected float _currentDetectRadius;
        protected bool _isNightFrenzied;
        protected Color _originalSpriteColor = Color.white;
        protected Vector3 _originalScale = Vector3.one;
        protected Coroutine _knockbackCoroutine;
        // Animator parameter hash IDs for performance
        protected static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        protected static readonly int AttackHash = Animator.StringToHash("Attack");
        protected static readonly int HurtHash = Animator.StringToHash("Hurt");
        protected static readonly int IsDeadHash = Animator.StringToHash("IsDead");
        protected static readonly int MoveXHash = Animator.StringToHash("MoveX");
        protected static readonly int MoveYHash = Animator.StringToHash("MoveY");

        public EnemyState CurrentState => _currentState;

        // =========================================================================================
        // KHỐI 2: KHỞI TẠO & ĐỒNG BỘ NAVMESH AGENT
        // =========================================================================================
        private void Awake()
        {
            _navAgent = GetComponent<NavMeshAgent>();
            _healthSystem = GetComponent<HealthSystem>();
            _originalScale = transform.localScale;

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (spriteRenderer != null)
            {
                _originalSpriteColor = spriteRenderer.color;
            }

            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }

            InitializeAgentParameters();
        }

        private void InitializeAgentParameters()
        {
            if (enemyData == null)
            {
                Debug.LogWarning($"[EnemyAI] Missing EnemyDataSO reference on {gameObject.name}!", this);
                return;
            }

            _navAgent.speed = enemyData.moveSpeed;
            _navAgent.angularSpeed = enemyData.angularSpeed;
            _navAgent.acceleration = enemyData.acceleration;
            _navAgent.stoppingDistance = enemyData.stoppingDistance;
            _currentMoveSpeed = enemyData.moveSpeed;
            _currentAttackDamage = enemyData.attackDamage;
            _currentDetectRadius = enemyData.detectRadius;

            // In 2.5D billboard setup, lock 3D rotation to prevent sprite distortion
            _navAgent.updateRotation = false;

            // Inject max health from EnemyDataSO into the runtime HealthSystem
            if (_healthSystem != null)
            {
                _healthSystem.SetMaxHealth(enemyData.maxHealth, resetCurrentHealth: true);
            }
        }

        private void LocatePlayerTarget()
        {
            var locomotion = FindAnyObjectByType<PlayerLocomotion.PlayerLocomotion>();
            if (locomotion != null)
            {
                _targetPlayer = locomotion.transform;
                return;
            }

            var playerByTag = GameObject.FindGameObjectWithTag("Player");
            if (playerByTag != null)
            {
                _targetPlayer = playerByTag.transform;
            }
        }

        // =========================================================================================
        // KHỐI 3: VÒNG LẶP SUY NGHĨ (UPDATE & FSM EVALUATION)
        // =========================================================================================
        protected virtual void Start()
        {
            LocatePlayerTarget();

            if (DayNightCycleManager.Instance != null && DayNightCycleManager.Instance.IsNight)
            {
                ApplyNightFrenzy();
            }
        }

        private void Update()
        {
            if (_currentState == EnemyState.Dead) return;

            if (_targetPlayer == null)
            {
                LocatePlayerTarget();
                return;
            }

            EvaluateStateMachine();
            UpdateAnimationDirection();
            UpdateSpriteFacing();
        }

        protected virtual void EvaluateStateMachine()
        {
            if (enemyData == null || _targetPlayer == null) return;

            // Calculate horizontal 2D distance on ground plane (ignoring height differences)
            Vector3 flatEnemyPos = new Vector3(transform.position.x, 0f, transform.position.z);
            Vector3 flatPlayerPos = new Vector3(_targetPlayer.position.x, 0f, _targetPlayer.position.z);
            float distanceToTarget = Vector3.Distance(flatEnemyPos, flatPlayerPos);

            // Kiểm tra xem Player có đang trốn an toàn trong vùng Lửa Trại không
            bool isPlayerInSanctuary = CampfireSanctuary.Instance != null &&
                                       CampfireSanctuary.Instance.IsPositionInSanctuary(_targetPlayer.position);

            switch (_currentState)
            {
                case EnemyState.Idle:
                    // Chỉ đuổi theo nếu Player trong tầm phát hiện VÀ KHÔNG trốn trong Lửa Trại
                    if (distanceToTarget <= _currentDetectRadius && !isPlayerInSanctuary)
                    {
                        TransitionToState(EnemyState.Chase);
                    }
                    break;

                case EnemyState.Chase:
                    // Nếu Player chạy quá xa HOẶC đã chạy vào Lửa Trại -> Quái phanh lại chuyển về Idle
                    if (distanceToTarget > _currentDetectRadius || isPlayerInSanctuary)
                    {
                        TransitionToState(EnemyState.Idle);
                    }
                    else if (distanceToTarget <= enemyData.attackRange)
                    {
                        TransitionToState(EnemyState.Attack);
                    }
                    else
                    {
                        if (_navAgent.isOnNavMesh)
                        {
                            _navAgent.SetDestination(_targetPlayer.position);
                        }
                    }
                    break;

                case EnemyState.Attack:
                    // Nếu Player kịp nhảy vào Lửa Trại -> Hủy đánh lập tức
                    if (isPlayerInSanctuary)
                    {
                        TransitionToState(EnemyState.Idle);
                    }
                    else if (distanceToTarget > enemyData.attackRange)
                    {
                        TransitionToState(EnemyState.Chase);
                    }
                    else
                    {
                        ExecuteAttackBehavior();
                    }
                    break;
            }
        }

        // =========================================================================================
        // KHỐI 4: ĐIỀU KHIỂN CHUYỂN ĐỔI BÁNH XE DI CHUYỂN
        // =========================================================================================
        protected virtual void TransitionToState(EnemyState newState)
        {
            _currentState = newState;

            if (!_navAgent.isOnNavMesh) return;

            switch (_currentState)
            {
                case EnemyState.Idle:
                    _navAgent.isStopped = true;
                    _navAgent.ResetPath();
                    if (animator != null) animator.SetBool(IsMovingHash, false);
                    break;

                case EnemyState.Chase:
                    _navAgent.isStopped = false;
                    if (animator != null) animator.SetBool(IsMovingHash, true);
                    break;

                case EnemyState.Attack:
                    _navAgent.isStopped = true;
                    if (animator != null) animator.SetBool(IsMovingHash, false);
                    break;

                case EnemyState.Dead:
                    _navAgent.isStopped = true;
                    _navAgent.ResetPath();
                    _navAgent.enabled = false;
                    if (animator != null) animator.SetBool(IsDeadHash, true);
                    break;
            }
        }

        // =========================================================================================
        // KHỐI 5: BỘ ĐẾM HỒI CHIÊU & GÂY SÁT THƯƠNG
        // =========================================================================================
        protected virtual void ExecuteAttackBehavior()
        {
            // Cooldown math check
            if (Time.time < _lastAttackTime + enemyData.attackCooldown) return;

            _lastAttackTime = Time.time;

            // Trigger attack animation
            if (animator != null)
            {
                animator.SetTrigger(AttackHash);
            }

            // Search for IDamageable in target, parent, or children hierarchy
            var targetDamageable = _targetPlayer.GetComponentInChildren<IDamageable>() ?? _targetPlayer.GetComponentInParent<IDamageable>();
            if (targetDamageable != null)
            {
                Vector3 hitPoint = _targetPlayer.position;
                Vector3 hitNormal = (transform.position - _targetPlayer.position).normalized;
                targetDamageable.TakeDamage(_currentAttackDamage, hitPoint, hitNormal);
            }
        }

        // =========================================================================================
        // KHỐI 6: LẬT ẢNH 2.5D, GỤC NGÃ & VẼ GIZMOS TRỰC QUAN
        // =========================================================================================
        private void OnEnable()
        {
            if (_healthSystem != null)
            {
                _healthSystem.OnDamaged += HandleDamaged;
                _healthSystem.OnDeath += HandleDeath;
            }

            //Register Day/Night event
            if (DayNightCycleManager.Instance != null)
            {
                DayNightCycleManager.Instance.OnNightStarted += ApplyNightFrenzy;
                DayNightCycleManager.Instance.OnDayStarted += RemoveNightFrenzy;
            }
        }

        private void OnDisable()
        {
            if (_healthSystem != null)
            {
                _healthSystem.OnDamaged -= HandleDamaged;
                _healthSystem.OnDeath -= HandleDeath;
            }

            if (DayNightCycleManager.Instance != null)
            {
                DayNightCycleManager.Instance.OnNightStarted -= ApplyNightFrenzy;
                DayNightCycleManager.Instance.OnDayStarted -= RemoveNightFrenzy;
            }
        }

        /// <summary>
        /// Applies the Night Frenzy empowerment buff and visual feedback when night falls.
        /// </summary>
        public void ApplyNightFrenzy(int dayNumber = 0)
        {
            if (_isNightFrenzied || enemyData == null || _currentState == EnemyState.Dead) return;
            _isNightFrenzied = true;
            // 1. Multiply stats
            _currentMoveSpeed = enemyData.moveSpeed * nightSpeedMultiplier;
            _currentAttackDamage = enemyData.attackDamage * nightDamageMultiplier;
            _currentDetectRadius = enemyData.detectRadius * nightDetectionMultiplier;
            if (_navAgent != null) _navAgent.speed = _currentMoveSpeed;
            // 2. Visual: Red sprite + Red eyes + Scale up + Faster animation
            if (spriteRenderer != null) spriteRenderer.color = frenziedColor;

            if (TryGetComponent<DamageFlash>(out var flashNight))
            {
                flashNight.SetOriginalColor(frenziedColor);
            }

            if (eyeGlowObject != null) eyeGlowObject.SetActive(true);

            transform.localScale = _originalScale * enrageScaleMultiplier;

            if (animator != null) animator.speed = nightSpeedMultiplier;
        }
        /// <summary>
        /// Reverts monster to daytime baseline parameters when the sun rises.
        /// </summary>
        public void RemoveNightFrenzy(int dayNumber = 0)
        {
            if (!_isNightFrenzied || enemyData == null || _currentState == EnemyState.Dead) return;
            _isNightFrenzied = false;
            // 1. Reset stats
            _currentMoveSpeed = enemyData.moveSpeed;
            _currentAttackDamage = enemyData.attackDamage;
            _currentDetectRadius = enemyData.detectRadius;

            if (_navAgent != null) _navAgent.speed = _currentMoveSpeed;
            // 2. Visual: Original color + Turn off eyes + Original scale

            if (spriteRenderer != null) spriteRenderer.color = _originalSpriteColor;

            if (TryGetComponent<DamageFlash>(out var flashDay))
            {
                flashDay.SetOriginalColor(_originalSpriteColor);
            }

            if (eyeGlowObject != null) eyeGlowObject.SetActive(false);
            transform.localScale = _originalScale;

            if (animator != null) animator.speed = 1.0f;
        }

        private void HandleDamaged(Vector3 hitNormal)
        {
            if (_currentState == EnemyState.Dead) return;

            if (animator != null)
            {
                animator.SetTrigger(HurtHash);
            }
        }

        /// <summary>
        /// Calculates directional velocity and injects MoveX/MoveY into the 2D Blend Tree.
        /// </summary>
        protected virtual void UpdateAnimationDirection()
        {
            if (animator == null || _targetPlayer == null) return;

            Vector3 direction;

            // If actively chasing and moving: calculate from actual NavMesh velocity
            if (_currentState == EnemyState.Chase && _navAgent.velocity.sqrMagnitude > 0.05f)
            {
                direction = _navAgent.velocity.normalized;
            }
            // Otherwise (Idle, Attack): face directly towards target player
            else
            {
                direction = (_targetPlayer.position - transform.position).normalized;
            }

            // In 2.5D: map world X to MoveX and world Z to MoveY (depth)
            animator.SetFloat(MoveXHash, direction.x);
            animator.SetFloat(MoveYHash, direction.z);
        }

        protected virtual void UpdateSpriteFacing()
        {
            if (spriteRenderer == null || _targetPlayer == null) return;

            Vector3 direction;

            //Determine active facing direction based on current state
            if (_currentState == EnemyState.Chase && _navAgent.velocity.sqrMagnitude > 0.05f)
            {
                direction = _navAgent.velocity.normalized;
            }
            else
            {
                direction = (_targetPlayer.position - transform.position).normalized;
            }


            //Only flix X there is significant horizontal movement to prevent jitter
            if (Mathf.Abs(direction.x) > 0.1f)
            {
                //If moving left (negative X), flip sprite orizontally; ortherwise keep default facing right
                spriteRenderer.flipX = direction.x < 0f;
            }
            else
            {
                // When moving purely foward or backward, do not mirror the sprite
                spriteRenderer.flipX = false;
            }
        }

        protected virtual void HandleDeath()
        {
            TransitionToState(EnemyState.Dead);

            var colliders = GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                col.enabled = false;
            }

            StartCoroutine(ReturnToPoolRoutine(1.5f));
        }

        /// <summary>
        /// Pushes the enemy backward safely along the NavMesh surface.
        /// </summary>
        public void ApplyKnockBack(Vector3 direction, float force)
        {
            if (_currentState == EnemyState.Dead || force <= 0) return;

            // Stop existing knockback to rpevent conflicting displacement
            if (_knockbackCoroutine != null)
            {
                StopCoroutine(_knockbackCoroutine);
            }

            _knockbackCoroutine = StartCoroutine(KnockBackRoutine(direction.normalized, force));
        }


        private IEnumerator KnockBackRoutine(Vector3 direction, float force)
        {
            // 1. temporarily pause normal A pathfiding velocity
            if (_navAgent != null && _navAgent.isOnNavMesh)
            {
                _navAgent.isStopped = true;
                _navAgent.velocity = Vector3.zero;
            }

            float duration = 0.2f;
            float elapsed = 0f;

            // Flatten direction onto horizontal XZ plane so enemy doesn't fly into ait
            direction.y = 0f;
            direction = direction.normalized;

            while (elapsed < duration)
            {
                if (_currentState == EnemyState.Dead) yield break;

                // Quadratic friction falloff curve: fast initial burst, smooth halt
                float remainingRatio = 1f - (elapsed / duration);
                float currentSpeed = force * remainingRatio * remainingRatio;

                // Safely displace agent within baked NavMesh borders ( prevents all clipping)
                if (_navAgent != null && _navAgent.isOnNavMesh)
                {
                    _navAgent.Move(direction * currentSpeed * Time.deltaTime);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            // 2. Resume normal pathfinding if enemy is still alive and chasing 
            if (_currentState != EnemyState.Dead && _navAgent != null && _navAgent.isOnNavMesh)
            {
                if (_currentState == EnemyState.Chase)
                {
                    _navAgent.isStopped = false;
                }
            }

            _knockbackCoroutine = null;
        }

        /// <summary>
        /// Resets and deploys the pooled enemy onto the island NavMesh at a specified position.
        /// </summary>
        public virtual void SpawnAt(Vector3 spawnPosition)
        {
            gameObject.SetActive(true);

            // Bật lại Collider bị tắt lúc chết
            var colliders = GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                col.enabled = true;
            }

            // Bật lại NavMeshAgent và dịch chuyển an toàn
            if (_navAgent != null)
            {
                _navAgent.enabled = true;
                _navAgent.Warp(spawnPosition);
                _navAgent.isStopped = false;
                _navAgent.velocity = Vector3.zero;
            }

            // Hồi đầy 100% máu và xóa cờ chết
            if (_healthSystem != null && enemyData != null)
            {
                _healthSystem.SetMaxHealth(enemyData.maxHealth, resetCurrentHealth: true);
            }

            // Tắt hoạt ảnh chết
            if (animator != null)
            {
                animator.SetBool(IsDeadHash, false);
            }

            _currentState = EnemyState.Idle;
            LocatePlayerTarget();

            // Thêm vào cuối hàm SpawnAt():
            if (DayNightCycleManager.Instance != null && DayNightCycleManager.Instance.IsNight)
            {
                ApplyNightFrenzy();
            }
            else
            {
                RemoveNightFrenzy();
            }
        }

        /// <summary>
        /// Delays disabling and returns this enemy to the Object Pool after death animation.
        /// </summary>
        private System.Collections.IEnumerator ReturnToPoolRoutine(float delay)
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

        private void OnDrawGizmosSelected()
        {
            if (enemyData == null) return;

            // Yellow: Sensory detection sphere
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, enemyData.detectRadius);

            // Red: Melee engagement attack sphere
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, enemyData.attackRange);
        }
    }
}