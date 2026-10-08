using UnityEngine;
using UnityEngine.InputSystem;
using FindTheLover.Combat;

namespace FindTheLover.PlayerLocomotion
{
    /// <summary>
    /// Controls 3D character movement across terrain slopes using CharacterController,
    /// calculates camera-relative directional motion, applies physical gravity,
    /// and synchronizes animation BlendTree parameters (PosX, PosY).
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerLocomotion : MonoBehaviour
    {
        [Header("Input Actions (New Input System)")]
        [Tooltip("Reference to the 2D movement action (Value > Vector2)")]
        [SerializeField] private InputActionReference moveAction;
        [Tooltip("Reference to the sprint action (Button)")]
        [SerializeField] private InputActionReference sprintAction;

        [Header("Movement Speeds")]
        [Tooltip("Standard walking speed in units per second")]
        [SerializeField] private float walkSpeed = 4.0f;
        [Tooltip("Running sprint speed in units per second")]
        [SerializeField] private float runSpeed = 7.0f;
        [Tooltip("Acceleration smoothing factor")]
        [SerializeField] private float acceleration = 12.0f;

        [Header("Physics & Ground Snapping")]
        [Tooltip("Gravity multiplier applied when falling")]
        [SerializeField] private float gravityMultiplier = 2.0f;
        [Tooltip("Downward grounding force to keep player glued to slopes")]
        [SerializeField] private float groundStickForce = 3.0f;

        [Header("Sprite Settings")]
        [Tooltip("Automatically flip sprite horizontally when moving left")]
        [SerializeField] private bool autoFlipX = false;

        // Component references
        private CharacterController characterController;
        private Animator animator;
        private SpriteRenderer spriteRenderer;
        private PlayerCombat playerCombat;
        private UnityEngine.Camera mainCamera;

        // Runtime state
        private Vector2 moveInput;
        private Vector3 currentVelocity;
        private float verticalVelocity;
        private bool isSprinting;

        private Vector2 lastFacingDirection = new Vector2(0f, -1f);
        private Vector3 lastWorldFacingDirection = Vector3.back;


        public Vector2 LastFacingDirection => lastFacingDirection;
        public Vector3 LastWorldFacingDirection => lastWorldFacingDirection;
        // Pre-computed Animator parameter hashes for optimal performance
        private static readonly int PosXHash = Animator.StringToHash("PosX");
        private static readonly int PosYHash = Animator.StringToHash("PosY");
        private static readonly int IsMovingHash = Animator.StringToHash("IsMoving");

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            playerCombat = GetComponent<PlayerCombat>();
            mainCamera = UnityEngine.Camera.main;

            // Configure CharacterController defaults if not set
            if (characterController != null)
            {
                characterController.slopeLimit = 50f;
                characterController.stepOffset = 0.5f;
                if (characterController.height < 1.0f)
                {
                    characterController.height = 1.8f;
                    characterController.center = new Vector3(0f, 0.9f, 0f);
                    characterController.radius = 0.4f;
                }
            }
        }

        private void OnEnable()
        {
            if (moveAction != null && moveAction.action != null) moveAction.action.Enable();
            if (sprintAction != null && sprintAction.action != null) sprintAction.action.Enable();
        }

        private void OnDisable()
        {
            if (moveAction != null && moveAction.action != null) moveAction.action.Disable();
            if (sprintAction != null && sprintAction.action != null) sprintAction.action.Disable();
        }

        private void Update()
        {
            if (mainCamera == null) mainCamera = UnityEngine.Camera.main;

            ProcessInput();
            ApplyMovement();
            UpdateAnimation();
        }

        /// <summary>
        /// Reads directional movement vector and sprint button from New Input System.
        /// </summary>
        private void ProcessInput()
        {
            if (moveAction != null && moveAction.action != null)
            {
                moveInput = moveAction.action.ReadValue<Vector2>();
                if (moveInput.sqrMagnitude > 1f) moveInput.Normalize();
            }
            else
            {
                moveInput = Vector2.zero;
            }

            isSprinting = sprintAction != null && sprintAction.action != null && sprintAction.action.IsPressed();

            // Handle horizontal sprite flipping
            if (autoFlipX && spriteRenderer != null)
            {
                if (moveInput.x > 0.05f) spriteRenderer.flipX = false;
                else if (moveInput.x < -0.05f) spriteRenderer.flipX = true;
            }

            if (moveInput.sqrMagnitude > 0.05f)
            {
                lastFacingDirection = moveInput.normalized;
            }
        }

        /// <summary>
        /// Calculates camera-relative 3D motion vector and moves CharacterController.
        /// </summary>
        private void ApplyMovement()
        {
            if (characterController == null) return;

            // Check if player is currently locked in an attack animation
            if (playerCombat != null && playerCombat.IsMovementLocked())
            {
                moveInput = Vector2.zero;
            }

            // 1. Calculate camera-relative movement direction in X-Z ground plane
            Vector3 camForward = mainCamera != null ? mainCamera.transform.forward : Vector3.forward;
            Vector3 camRight = mainCamera != null ? mainCamera.transform.right : Vector3.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 targetDirection = (camRight * moveInput.x + camForward * moveInput.y);
            if (targetDirection.sqrMagnitude > 0.05f)
            {
                lastWorldFacingDirection = targetDirection.normalized;
            }
            // 2. Target horizontal speed
            float targetSpeed = (moveInput.sqrMagnitude > 0.01f) ? (isSprinting ? runSpeed : walkSpeed) : 0f;
            Vector3 targetHorizontalVelocity = targetDirection * targetSpeed;

            // Smooth horizontal velocity acceleration
            currentVelocity = Vector3.MoveTowards(currentVelocity, targetHorizontalVelocity, acceleration * Time.deltaTime);

            // 3. Gravity and slope grounding
            if (characterController.isGrounded)
            {
                // Firm downward stick force so player doesn't float down slopes
                verticalVelocity = -groundStickForce;
            }
            else
            {
                verticalVelocity += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
            }

            // 4. Combine horizontal motion and vertical gravity
            Vector3 finalMotion = currentVelocity;
            finalMotion.y = verticalVelocity;

            characterController.Move(finalMotion * Time.deltaTime);
        }

        /// <summary>
        /// Updates the 2D Animator BlendTree with directional coordinates.
        /// </summary>
        private void UpdateAnimation()
        {
            if (animator == null) return;

            // Không ghi đè khi nhân vật đang vung kiếm
            if (playerCombat != null && playerCombat.IsMovementLocked()) return;

            if (moveInput.sqrMagnitude > 0.01f)
            {
                // ĐANG DI CHUYỂN
                animator.SetBool(IsMovingHash, true);

                float blendMultiplier = isSprinting ? 2.0f : 1.0f;
                animator.SetFloat(PosXHash, moveInput.x * blendMultiplier);
                animator.SetFloat(PosYHash, moveInput.y * blendMultiplier);
            }
            else
            {
                // ĐỨNG YÊN: Giữ nguyên hướng nhìn cuối cùng để Animator phát đúng Idle tương ứng!
                animator.SetBool(IsMovingHash, false);

                animator.SetFloat(PosXHash, lastFacingDirection.x);
                animator.SetFloat(PosYHash, lastFacingDirection.y);
            }
        }
    }
}
