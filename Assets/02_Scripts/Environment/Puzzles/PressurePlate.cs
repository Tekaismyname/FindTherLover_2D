using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace FindTheLover.Environment.Puzzles
{
    /// <summary>
    /// Logic Layer: Pressure-sensitive ground trigger plate.
    /// Tracks overlapping physical occupants, animates depression with spring physics,
    /// supports Momentary, TimedDelay, and PermanentLatch modes,
    /// and dispatches OnPressed / OnReleased events for puzzle mechanics (gates, traps, bridges).
    /// </summary>
    public class PressurePlate : MonoBehaviour
    {
        public enum PlateMode
        {
            Momentary,      // Springs back immediately when unstepped
            TimedDelay,     // Remains depressed for releaseDelay seconds before springing up
            PermanentLatch  // Stays depressed permanently once stepped on
        }

        [Header("Plate Behavior Mode")]
        [Tooltip("Momentary: springs back immediately; TimedDelay: stays down for duration; PermanentLatch: stays down forever")]
        [SerializeField] private PlateMode plateMode = PlateMode.TimedDelay;

        [Tooltip("Seconds the plate remains depressed after stepping off (used in TimedDelay mode)")]
        [SerializeField] private float releaseDelay = 2.5f;

        [Header("Plate Visual Feedback")]
        [Tooltip("Moving plate transform that depresses downward when stepped on")]
        [SerializeField] private Transform plateVisual;

        [Tooltip("Distance the plate depresses downward when activated")]
        [SerializeField] private float depressDistance = 0.14f;

        [Tooltip("Speed of the spring depression/release motion")]
        [SerializeField] private float springSpeed = 8.0f;

        [Header("Layer Filter")]
        [Tooltip("Layers that can trigger this pressure plate (Player, Enemies, Pushable Blocks)")]
        [SerializeField] private LayerMask triggerLayers = ~0;

        [Header("Audio Feedback")]
        [Tooltip("Sound played when stepping onto the plate")]
        [SerializeField] private AudioClip pressSFX;

        [Tooltip("Sound played when releasing the plate")]
        [SerializeField] private AudioClip releaseSFX;

        [Header("Puzzle Events")]
        [Tooltip("Fired once when the first entity steps onto the plate")]
        public UnityEvent OnPressed;

        [Tooltip("Fired once when all entities have stepped off the plate")]
        public UnityEvent OnReleased;

        // Component references
        private AudioSource _audioSource;
        private BoxCollider _plateCollider;
        private Rigidbody _rigidbody;
        private Light _runeLight;

        // Runtime state
        private HashSet<int> occupyingRootIDs = new HashSet<int>();
        private Vector3 initialLocalPos;
        private Vector3 pressedLocalPos;
        private Vector3 targetLocalPos;
        private bool isPressed = false;
        private Coroutine releaseRoutine;

        public bool IsPressed => isPressed;
        public int OccupantCount => occupyingRootIDs.Count;
        public PlateMode CurrentPlateMode => plateMode;

        private void Awake()
        {
            occupyingRootIDs.Clear();
            isPressed = false;

            _audioSource = GetComponent<AudioSource>();
            _plateCollider = GetComponent<BoxCollider>();
            _runeLight = GetComponentInChildren<Light>();

            if (_plateCollider != null)
            {
                _plateCollider.isTrigger = true;
                // Keep trigger box slightly above ground (bottom at y=0.15) to avoid scraping terrain
                if (_plateCollider.size.y < 0.8f || _plateCollider.center.y < 0.5f)
                {
                    _plateCollider.center = new Vector3(0f, 0.65f, 0f);
                    _plateCollider.size = new Vector3(1.1f, 1.0f, 1.1f);
                }
            }

            // Ensure a kinematic Rigidbody is present so Unity PhysX dispatches trigger events reliably for CharacterControllers
            _rigidbody = GetComponent<Rigidbody>();
            if (_rigidbody == null)
            {
                _rigidbody = gameObject.AddComponent<Rigidbody>();
            }
            _rigidbody.isKinematic = true;
            _rigidbody.useGravity = false;

            InitializePositions();
            ApplyVisualState(false);
        }

        private void OnEnable()
        {
            InitializePositions();
        }

        /// <summary>
        /// Recalculates resting and depressed local coordinates safely.
        /// </summary>
        private void InitializePositions()
        {
            if (plateVisual != null)
            {
                if (initialLocalPos == Vector3.zero)
                {
                    initialLocalPos = plateVisual.localPosition.y > 0.001f ? plateVisual.localPosition : new Vector3(0f, 0.15f, 0f);
                }
                pressedLocalPos = initialLocalPos - new Vector3(0f, depressDistance, 0f);
                targetLocalPos = isPressed ? pressedLocalPos : initialLocalPos;
            }

            if (_runeLight == null)
            {
                _runeLight = GetComponentInChildren<Light>();
            }
        }

        private void Update()
        {
            // Smoothly animate the plate between initial and pressed position
            if (plateVisual != null && Vector3.Distance(plateVisual.localPosition, targetLocalPos) > 0.0005f)
            {
                plateVisual.localPosition = Vector3.Lerp(plateVisual.localPosition, targetLocalPos, springSpeed * Time.deltaTime);
            }
        }

        /// <summary>
        /// Validates that only real physical actors (Player, Enemies, Pushable Rigidbodies)
        /// can depress the plate, ignoring Terrain, static geometry, and other Trigger zones.
        /// </summary>
        private bool IsValidOccupant(Collider other)
        {
            if (other == null) return false;

            // 1. Ignore other trigger zones (e.g., Campfire Sanctuary aura) and self colliders
            if (other.isTrigger || other.transform.IsChildOf(transform)) return false;

            // 2. Ignore Terrain ground
            if (other is TerrainCollider) return false;

            // 3. Check layer mask filter
            if ((triggerLayers.value & (1 << other.gameObject.layer)) == 0) return false;

            // 4. Only allow dynamic actors: CharacterController (Player), NavMeshAgent (Enemy), or non-kinematic Rigidbody (Pushable Prop)
            if (other is CharacterController) return true;
            if (other.GetComponentInParent<CharacterController>() != null) return true;
            if (other.GetComponentInParent<NavMeshAgent>() != null) return true;
            if (other.attachedRigidbody != null && !other.attachedRigidbody.isKinematic) return true;

            return false;
        }

        /// <summary>
        /// Resolves a unique instance ID for the physical actor stepping on the plate.
        /// </summary>
        private int GetOccupantID(Collider other)
        {
            var cc = other.GetComponentInParent<CharacterController>();
            if (cc != null) return cc.gameObject.GetInstanceID();

            var agent = other.GetComponentInParent<NavMeshAgent>();
            if (agent != null) return agent.gameObject.GetInstanceID();

            if (other.attachedRigidbody != null) return other.attachedRigidbody.gameObject.GetInstanceID();

            return other.gameObject.GetInstanceID();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!IsValidOccupant(other)) return;

            // Stop any pending release routine if someone steps back onto the plate
            if (releaseRoutine != null)
            {
                StopCoroutine(releaseRoutine);
                releaseRoutine = null;
            }

            int occupantID = GetOccupantID(other);

            if (occupyingRootIDs.Add(occupantID))
            {
                // When first valid entity steps onto the plate, trigger activation
                if (occupyingRootIDs.Count == 1 && !isPressed)
                {
                    SetPressedState(true);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!IsValidOccupant(other)) return;

            int occupantID = GetOccupantID(other);

            if (occupyingRootIDs.Remove(occupantID))
            {
                // When last entity steps off the plate, handle release according to PlateMode
                if (occupyingRootIDs.Count == 0 && isPressed)
                {
                    if (plateMode == PlateMode.PermanentLatch)
                    {
                        return;
                    }
                    else if (plateMode == PlateMode.TimedDelay)
                    {
                        if (releaseRoutine != null) StopCoroutine(releaseRoutine);
                        releaseRoutine = StartCoroutine(DelayedReleaseRoutine());
                    }
                    else
                    {
                        SetPressedState(false);
                    }
                }
            }
        }

        private IEnumerator DelayedReleaseRoutine()
        {
            yield return new WaitForSeconds(releaseDelay);

            if (occupyingRootIDs.Count == 0 && isPressed)
            {
                SetPressedState(false);
            }
            releaseRoutine = null;
        }

        private void SetPressedState(bool pressed)
        {
            isPressed = pressed;
            ApplyVisualState(pressed);

            if (isPressed)
            {
                PlayAudio(pressSFX);
                OnPressed?.Invoke();
                Debug.Log($"[PressurePlate] {gameObject.name} PRESSED! Active occupants: {occupyingRootIDs.Count}");
            }
            else
            {
                PlayAudio(releaseSFX);
                OnReleased?.Invoke();
                Debug.Log($"[PressurePlate] {gameObject.name} RELEASED!");
            }
        }

        private void ApplyVisualState(bool pressed)
        {
            targetLocalPos = pressed ? pressedLocalPos : initialLocalPos;

            // Presentation Layer: Dynamic Light Flare feedback around player's feet
            if (_runeLight != null)
            {
                _runeLight.intensity = pressed ? 1.6f : 0.22f;
                _runeLight.range = pressed ? 3.2f : 1.8f;
            }
        }

        private void PlayAudio(AudioClip clip)
        {
            if (_audioSource != null && clip != null)
            {
                _audioSource.PlayOneShot(clip);
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = isPressed ? Color.green : Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.up * 0.05f, new Vector3(1.4f, 0.15f, 1.4f));
        }
    }
}
