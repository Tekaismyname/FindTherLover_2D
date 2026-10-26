using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FindTheLover.Combat;

namespace FindTheLover.Environment.Hazards
{
    /// <summary>
    /// Logic Layer: Manages the 4-phase state cycle of ground spike traps (Inactive -> Warning -> Extended -> Retracting).
    /// Detects overlapping entities and inflicts physical damage via the universal IDamageable interface.
    /// </summary>
    public class SpikeTrap : MonoBehaviour
    {
        public enum TrapMode
        {
            PeriodicTimer,  // Bẫy tự động bật/tắt tuần hoàn theo thời gian
            StepTriggered   // Bẫy nằm chờ, có người dẫm lên mới bắt đầu kích hoạt
        }

        public enum TrapState
        {
            Inactive,
            Warning,
            Extended,
            Retracting
        }

        [Header("Trap Mode & Behavior")]
        [Tooltip("Periodic: automatically cycles; StepTriggered: springs when stepped on")]
        [SerializeField] private TrapMode trapMode = TrapMode.PeriodicTimer;

        [Header("Phase Timers (Seconds)")]
        [Tooltip("Duration spikes remain safely underground")]
        [SerializeField] private float inactiveDuration = 2.0f;

        [Tooltip("Duration of warning rattle before deadly thrust")]
        [SerializeField] private float warningDuration = 0.5f;

        [Tooltip("Duration spikes stay fully extended inflicting damage")]
        [SerializeField] private float extendedDuration = 1.0f;

        [Tooltip("Cooldown period before trap can be triggered again")]
        [SerializeField] private float resetCooldown = 1.5f;

        [Header("Combat & Damage")]
        [Tooltip("Health deducted when impaled by spikes")]
        [SerializeField] private float spikeDamage = 25f;

        [Tooltip("Layer mask filtering who can be damaged (Player, Enemies)")]
        [SerializeField] private LayerMask damageableLayers = ~0;

        [Header("Spike Movement Animation")]
        [Tooltip("Child transform containing spike meshes to raise/lower")]
        [SerializeField] private Transform spikesRoot;

        [Tooltip("Local Y position when hidden underground")]
        [SerializeField] private float retractedPosY = -0.5f;

        [Tooltip("Local Y position during warning telegraph rumble")]
        [SerializeField] private float warningPosY = -0.1f;

        [Tooltip("Local Y position when spikes are fully thrust upward")]
        [SerializeField] private float extendedPosY = 0.6f;

        [Header("Audio Feedback (Optional)")]
        [Tooltip("Sound played during warning vibration")]
        [SerializeField] private AudioClip warningSFX;

        [Tooltip("Sound played when spikes shoot up violently")]
        [SerializeField] private AudioClip thrustSFX;

        // Component references
        private AudioSource _audioSource;
        private Collider _triggerCollider;

        // Runtime state tracking
        private TrapState currentState = TrapState.Inactive;
        private HashSet<IDamageable> currentOccupants = new HashSet<IDamageable>();
        private Coroutine trapRoutine;

        public TrapState CurrentState => currentState;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _triggerCollider = GetComponent<Collider>();

            // Ensure collider is configured as trigger
            if (_triggerCollider != null)
            {
                _triggerCollider.isTrigger = true;
            }

            // Set initial spike position
            SetSpikeHeight(retractedPosY);
        }

        private void OnEnable()
        {
            if (trapMode == TrapMode.PeriodicTimer)
            {
                trapRoutine = StartCoroutine(PeriodicCycleRoutine());
            }
        }

        private void OnDisable()
        {
            if (trapRoutine != null)
            {
                StopCoroutine(trapRoutine);
                trapRoutine = null;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // Verify if colliding object belongs to target damageable layer
            if ((damageableLayers.value & (1 << other.gameObject.layer)) == 0) return;

            var damageable = other.GetComponentInParent<IDamageable>() ?? other.GetComponentInChildren<IDamageable>();
            if (damageable != null && !damageable.IsDead)
            {
                // Register entity currently standing inside the trap zone
                currentOccupants.Add(damageable);

                // If trap is StepTriggered and currently idle, spring the trap!
                if (trapMode == TrapMode.StepTriggered && currentState == TrapState.Inactive && trapRoutine == null)
                {
                    trapRoutine = StartCoroutine(TriggeredCycleRoutine());
                }
                // If trap is ALREADY extended, instantly punish anyone who walks in!
                else if (currentState == TrapState.Extended)
                {
                    DamageEntity(damageable);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var damageable = other.GetComponentInParent<IDamageable>() ?? other.GetComponentInChildren<IDamageable>();
            if (damageable != null)
            {
                // Remove entity that has stepped out of trap zone
                currentOccupants.Remove(damageable);
            }
        }

        /// <summary>
        /// Coroutine driving the continuous clockwork cycle of periodic traps.
        /// </summary>
        private IEnumerator PeriodicCycleRoutine()
        {
            while (true)
            {
                // Phase 1: Inactive (Safe)
                yield return ExecutePhase(TrapState.Inactive, retractedPosY, inactiveDuration);

                // Phase 2: Warning Telegraph (Spikes rattle slightly)
                PlayAudio(warningSFX);
                yield return ExecuteWarningPhase();

                // Phase 3: Deadly Thrust (Spikes shoot up and impale occupants)
                PlayAudio(thrustSFX);
                yield return ExecuteThrustPhase();

                // Phase 4: Retract back underground
                yield return ExecutePhase(TrapState.Retracting, retractedPosY, 0.3f);
            }
        }

        /// <summary>
        /// Coroutine driving the one-shot triggered cycle when stepped on.
        /// </summary>
        private IEnumerator TriggeredCycleRoutine()
        {
            // Phase 2: Warning Telegraph (Click sound & rumble)
            PlayAudio(warningSFX);
            yield return ExecuteWarningPhase();

            // Phase 3: Deadly Thrust
            PlayAudio(thrustSFX);
            yield return ExecuteThrustPhase();

            // Phase 4: Retract
            yield return ExecutePhase(TrapState.Retracting, retractedPosY, 0.4f);

            // Cooldown before trap can be triggered again
            currentState = TrapState.Inactive;
            yield return new WaitForSeconds(resetCooldown);

            trapRoutine = null;

            // If an entity is STILL standing on the trap after reset, trigger again!
            if (currentOccupants.Count > 0)
            {
                trapRoutine = StartCoroutine(TriggeredCycleRoutine());
            }
        }

        private IEnumerator ExecutePhase(TrapState targetState, float targetY, float duration)
        {
            currentState = targetState;
            SetSpikeHeight(targetY);
            yield return new WaitForSeconds(duration);
        }

        private IEnumerator ExecuteWarningPhase()
        {
            currentState = TrapState.Warning;
            float elapsed = 0f;

            // Rumble and vibrate slightly around warning height
            while (elapsed < warningDuration)
            {
                elapsed += Time.deltaTime;
                float vibrateOffset = Mathf.Sin(Time.time * 40f) * 0.02f;
                SetSpikeHeight(warningPosY + vibrateOffset);
                yield return null;
            }
        }

        private IEnumerator ExecuteThrustPhase()
        {
            currentState = TrapState.Extended;
            SetSpikeHeight(extendedPosY);

            // Inflict damage to all entities currently standing on the trap
            DamageAllOccupants();

            yield return new WaitForSeconds(extendedDuration);
        }

        private void DamageAllOccupants()
        {
            // Clean up null or dead occupants before dealing damage
            currentOccupants.RemoveWhere(item => item == null || item.IsDead);

            foreach (var victim in currentOccupants)
            {
                DamageEntity(victim);
            }
        }

        private void DamageEntity(IDamageable victim)
        {
            if (victim == null || victim.IsDead) return;

            Vector3 hitPoint = transform.position + Vector3.up * 0.5f;
            Vector3 hitNormal = Vector3.up;

            // Apply physical damage via the IDamageable contract
            victim.TakeDamage(spikeDamage, hitPoint, hitNormal);
        }

        private void SetSpikeHeight(float localY)
        {
            if (spikesRoot != null)
            {
                Vector3 current = spikesRoot.localPosition;
                current.y = localY;
                spikesRoot.localPosition = current;
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
            // Visualize trap damage area in Scene view
            Gizmos.color = currentState == TrapState.Extended ? Color.red : (currentState == TrapState.Warning ? Color.yellow : Color.green);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.up * 0.25f, new Vector3(1.8f, 0.5f, 1.8f));
        }
    }
}
