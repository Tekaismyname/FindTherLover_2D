using UnityEngine;
using UnityEngine.AI;
using FindTheLover.Combat;
using FindTheLover.Enemies.Data;

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
    public class EnemyAI : MonoBehaviour
    {
        // =========================================================================================
        // KHỐI 1: KHAI BÁO BIẾN CẤU HÌNH & THÀNH PHẦN
        // =========================================================================================
        [Header("Configuration")]
        [Tooltip("Enemy static blueprint data reference.")]
        [SerializeField] private EnemyDataSO enemyData;

        [Header("Presentation")]
        [Tooltip("SpriteRenderer reference for horizontal directional flipping.")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Tooltip("Animator reference for controlling sprite animation states.")]
        [SerializeField] private Animator animator;

        private EnemyState _currentState = EnemyState.Idle;
        private NavMeshAgent _navAgent;
        private HealthSystem _healthSystem;
        private Transform _targetPlayer;
        private float _lastAttackTime;

        // Animator parameter hash IDs for performance
        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
        private static readonly int AttackHash = Animator.StringToHash("Attack");
        private static readonly int HurtHash = Animator.StringToHash("Hurt");
        private static readonly int IsDeadHash = Animator.StringToHash("IsDead");
        private static readonly int MoveXHash = Animator.StringToHash("MoveX");
        private static readonly int MoveYHash = Animator.StringToHash("MoveY");

        public EnemyState CurrentState => _currentState;

        // =========================================================================================
        // KHỐI 2: KHỞI TẠO & ĐỒNG BỘ NAVMESH AGENT
        // =========================================================================================
        private void Awake()
        {
            _navAgent = GetComponent<NavMeshAgent>();
            _healthSystem = GetComponent<HealthSystem>();

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
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
        private void Start()
        {
            LocatePlayerTarget();
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

        private void EvaluateStateMachine()
        {
            if (enemyData == null || _targetPlayer == null) return;

            // Calculate horizontal 2D distance on ground plane (ignoring height differences)
            Vector3 flatEnemyPos = new Vector3(transform.position.x, 0f, transform.position.z);
            Vector3 flatPlayerPos = new Vector3(_targetPlayer.position.x, 0f, _targetPlayer.position.z);
            float distanceToTarget = Vector3.Distance(flatEnemyPos, flatPlayerPos);

            switch (_currentState)
            {
                case EnemyState.Idle:
                    if (distanceToTarget <= enemyData.detectRadius)
                    {
                        TransitionToState(EnemyState.Chase);
                    }
                    break;

                case EnemyState.Chase:
                    if (distanceToTarget > enemyData.detectRadius)
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
                    if (distanceToTarget > enemyData.attackRange)
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
        private void TransitionToState(EnemyState newState)
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
        private void ExecuteAttackBehavior()
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
                targetDamageable.TakeDamage(enemyData.attackDamage, hitPoint, hitNormal);
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
        }

        private void OnDisable()
        {
            if (_healthSystem != null)
            {
                _healthSystem.OnDamaged -= HandleDamaged;
                _healthSystem.OnDeath -= HandleDeath;
            }
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
        private void UpdateAnimationDirection()
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

        private void UpdateSpriteFacing()
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

        private void HandleDeath()
        {
            TransitionToState(EnemyState.Dead);

            var colliders = GetComponentsInChildren<Collider>();
            foreach (var col in colliders)
            {
                col.enabled = false;
            }

            Destroy(gameObject, 1.5f);
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