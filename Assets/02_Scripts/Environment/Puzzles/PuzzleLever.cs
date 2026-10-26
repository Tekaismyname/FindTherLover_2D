using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace FindTheLover.Environment.Puzzles
{
    /// <summary>
    /// Logic Layer: Interactive puzzle lever switch.
    /// Detects player proximity, listens for interaction input (Key E / New Input System),
    /// rotates mechanical handle, and triggers OnActivated / OnDeactivated events for gates and traps.
    /// </summary>
    public class PuzzleLever : MonoBehaviour
    {
        [Header("Interaction Settings")]
        [Tooltip("Maximum distance from player to interact")]
        [SerializeField] private float interactionRadius = 1.8f;

        [Tooltip("Reference to the Interact Input Action (Key E)")]
        [SerializeField] private InputActionReference interactAction;

        [Tooltip("If true, lever can be toggled on and off; if false, activates only once")]
        [SerializeField] private bool isReusable = true;

        [Header("Lever Visuals & Animation")]
        [Tooltip("Transform of the rotating lever stick/handle")]
        [SerializeField] private Transform leverHandle;

        [Tooltip("Rotation angle when lever is OFF (degrees)")]
        [SerializeField] private float offAngle = -35.0f;

        [Tooltip("Rotation angle when lever is ON (degrees)")]
        [SerializeField] private float onAngle = 35.0f;

        [Tooltip("Speed of the smooth handle flip rotation")]
        [SerializeField] private float flipSpeed = 10.0f;

        [Header("UI Prompt (Optional)")]
        [Tooltip("Visual prompt GameObject shown when player is in range (e.g., Press E icon)")]
        [SerializeField] private GameObject interactPrompt;

        [Header("Audio Feedback")]
        [Tooltip("Sound played when pulling the lever")]
        [SerializeField] private AudioClip leverPullSFX;

        [Header("Puzzle Events")]
        [Tooltip("Fired when the lever is pulled into the ON state")]
        public UnityEvent OnActivated;

        [Tooltip("Fired when the lever is pulled back into the OFF state")]
        public UnityEvent OnDeactivated;

        // Component references
        private AudioSource _audioSource;

        // Runtime state
        private bool isActivated = false;
        private bool isPlayerInRange = false;
        private Quaternion targetRotation;
        private Transform playerTransform;

        public bool IsActivated => isActivated;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();

            if (leverHandle != null)
            {
                targetRotation = Quaternion.Euler(offAngle, 0f, 0f);
                leverHandle.localRotation = targetRotation;
            }

            if (interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (interactAction != null && interactAction.action != null)
            {
                interactAction.action.Enable();
            }
        }

        private void OnDisable()
        {
            if (interactAction != null && interactAction.action != null)
            {
                interactAction.action.Disable();
            }
        }

        private void Update()
        {
            CheckPlayerProximity();
            HandleInput();
            AnimateHandle();
        }

        private void CheckPlayerProximity()
        {
            // Find the active moving player CharacterController (e.g., Quene_Player) if reference is not cached
            if (playerTransform == null)
            {
                CharacterController cc = FindFirstObjectByType<CharacterController>();
                if (cc != null && cc.CompareTag("Player"))
                {
                    playerTransform = cc.transform;
                }
                else
                {
                    GameObject player = GameObject.FindGameObjectWithTag("Player");
                    if (player != null) playerTransform = player.transform;
                }
            }

            if (playerTransform == null) return;

            // Measure horizontal XZ distance so tall 2.5D CharacterController center offset doesn't block interaction
            Vector2 leverXZ = new Vector2(transform.position.x, transform.position.z);
            Vector2 playerXZ = new Vector2(playerTransform.position.x, playerTransform.position.z);
            float horizontalDist = Vector2.Distance(leverXZ, playerXZ);
            float verticalDiff = Mathf.Abs(transform.position.y - playerTransform.position.y);

            bool inRange = horizontalDist <= interactionRadius && verticalDiff <= 4.0f;

            if (inRange != isPlayerInRange)
            {
                isPlayerInRange = inRange;
                if (interactPrompt != null)
                {
                    // Only show prompt if lever can still be used
                    interactPrompt.SetActive(isPlayerInRange && (isReusable || !isActivated));
                }
            }
        }

        private void HandleInput()
        {
            if (!isPlayerInRange) return;
            if (!isReusable && isActivated) return;

            bool wasPressed = false;

            if (interactAction != null && interactAction.action != null)
            {
                wasPressed = interactAction.action.WasPressedThisFrame();
            }
            else
            {
                wasPressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
            }

            if (wasPressed)
            {
                ToggleLever();
            }
        }

        public void ToggleLever()
        {
            if (!isReusable && isActivated) return;

            isActivated = !isActivated;
            targetRotation = Quaternion.Euler(isActivated ? onAngle : offAngle, 0f, 0f);

            PlayAudio(leverPullSFX);

            if (isActivated)
            {
                OnActivated?.Invoke();
                Debug.Log($"[PuzzleLever] {gameObject.name} ACTIVATED (ON)!");
            }
            else
            {
                OnDeactivated?.Invoke();
                Debug.Log($"[PuzzleLever] {gameObject.name} DEACTIVATED (OFF)!");
            }

            // Update prompt visibility if single-use
            if (!isReusable && interactPrompt != null)
            {
                interactPrompt.SetActive(false);
            }
        }

        private void AnimateHandle()
        {
            if (leverHandle != null && Quaternion.Angle(leverHandle.localRotation, targetRotation) > 0.5f)
            {
                leverHandle.localRotation = Quaternion.Slerp(leverHandle.localRotation, targetRotation, flipSpeed * Time.deltaTime);
            }
        }

        private void PlayAudio(AudioClip clip)
        {
            if (_audioSource != null && clip != null)
            {
                _audioSource.PlayOneShot(clip);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, interactionRadius);
        }
    }
}
