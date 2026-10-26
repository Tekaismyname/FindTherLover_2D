using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using FindTheLover.Player;

namespace FindTheLover.Environment.Puzzles
{
    /// <summary>
    /// Defines whether this brazier acts as a normal fire relay checkpoint or the mystical Soul Flame altar.
    /// </summary>
    public enum BrazierRole
    {
        NormalCheckpoint = 0, // Starts unlit; once lit by player's torch, stays lit forever and refills normal fuel
        SoulFlameAltar = 1    // Starts lit with Cyan Soul Flame; imbues player's torch with Soul Flame on interaction
    }

    /// <summary>
    /// Logic & Presentation Layer: Ancient stone brazier for Zone 2 Torch Chain Puzzle.
    /// Provides a permanent safe zone against poison gas once ignited and refills/upgrades the player's torch.
    /// </summary>
    public class AncientBrazier : MonoBehaviour
    {
        // Static registry of all active braziers for O(N) zero-allocation shelter queries by PoisonGasZone
        private static readonly HashSet<AncientBrazier> ActiveBraziers = new HashSet<AncientBrazier>();

        [Header("Brazier Configuration")]
        [Tooltip("NormalCheckpoint: relays normal fire; SoulFlameAltar: grants cyan Soul Flame")]
        [SerializeField] private BrazierRole brazierRole = BrazierRole.NormalCheckpoint;

        [Tooltip("Whether this brazier starts already lit when the scene begins")]
        [SerializeField] private bool startLit = false;

        [Tooltip("Horizontal XZ distance within which the player can press E to interact")]
        [SerializeField] private float interactRadius = 2.4f;

        [Tooltip("Safe zone radius that repels poison gas and auto-refills player torch once lit")]
        [SerializeField] private float safeRadius = 3.0f;

        [Header("Visual & Audio References")]
        [Tooltip("Visual flame object or particle system enabled when lit")]
        [SerializeField] private GameObject flameVisual;

        [Tooltip("Point light illuminating the brazier when lit")]
        [SerializeField] private Light brazierLight;

        [Tooltip("Sound played when igniting the brazier")]
        [SerializeField] private AudioClip igniteSFX;

        [Header("Puzzle Events")]
        [Tooltip("Fired once when this brazier is ignited")]
        public UnityEvent OnBrazierIgnited;

        private bool _isLit = false;
        private Transform _playerTransform;
        private PlayerTorchController _playerTorch;
        private AudioSource _audioSource;

        public bool IsLit => _isLit;
        public BrazierRole Role => brazierRole;
        public float SafeRadius => safeRadius;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            if (brazierLight == null) brazierLight = GetComponentInChildren<Light>(true);

            // Soul Flame Altar always starts lit with mystical cyan fire
            if (brazierRole == BrazierRole.SoulFlameAltar)
            {
                _isLit = true;
            }
            else
            {
                _isLit = startLit;
            }

            ApplyVisualState();
        }

        private void OnEnable()
        {
            ActiveBraziers.Add(this);
            ApplyVisualState();
        }

        private void OnDisable()
        {
            ActiveBraziers.Remove(this);
        }

        private void Update()
        {
            EnsurePlayerReference();
            if (_playerTransform == null || _playerTorch == null) return;

            // Calculate horizontal XZ distance to avoid vertical pivot offset issues
            Vector2 brazierXZ = new Vector2(transform.position.x, transform.position.z);
            Vector2 playerXZ = new Vector2(_playerTransform.position.x, _playerTransform.position.z);
            float distXZ = Vector2.Distance(brazierXZ, playerXZ);

            // 1. If already lit, automatically refill player's torch duration while inside safeRadius
            if (_isLit && distXZ <= safeRadius)
            {
                if (brazierRole == BrazierRole.NormalCheckpoint && _playerTorch.IsFlameActive &&
                    _playerTorch.RemainingBurnTime < _playerTorch.MaxBurnDuration - 0.5f)
                {
                    _playerTorch.IgniteNormalFlame();
                }
            }

            // 2. Check interaction key E within interactRadius
            if (distXZ <= interactRadius && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                TryInteract(_playerTorch);
            }

            // 3. Subtle flame light flicker when lit
            if (_isLit && brazierLight != null)
            {
                brazierLight.intensity = 2.0f + Mathf.PingPong(Time.time * 2.8f, 0.4f);
            }
        }

        /// <summary>
        /// Executes interaction between the player's torch and this brazier.
        /// </summary>
        public bool TryInteract(PlayerTorchController playerTorch)
        {
            if (playerTorch == null) return false;

            if (brazierRole == BrazierRole.SoulFlameAltar)
            {
                // Grant Soul Flame to the player!
                playerTorch.IgniteSoulFlame();
                PlayIgniteAudio();
                OnBrazierIgnited?.Invoke();
                Debug.Log($"<color=cyan>[AncientBrazier] {gameObject.name} bestowed the SOUL FLAME upon Player!</color>");
                return true;
            }

            // Normal Checkpoint Brazier logic
            if (!_isLit)
            {
                // Player must have an active flame to light an unlit brazier
                if (playerTorch.IsFlameActive)
                {
                    _isLit = true;
                    ApplyVisualState();
                    playerTorch.IgniteNormalFlame(); // Also top up player's torch to full 40s
                    PlayIgniteAudio();
                    OnBrazierIgnited?.Invoke();
                    Debug.Log($"<color=yellow>[AncientBrazier] {gameObject.name} IGNITED permanently! Safe zone active.</color>");
                    return true;
                }
                else
                {
                    Debug.LogWarning($"[AncientBrazier] Cannot light {gameObject.name} because your torch is extinguished!");
                    return false;
                }
            }
            else
            {
                // Brazier is already lit -> re-ignite or refill player's extinguished torch!
                playerTorch.IgniteNormalFlame();
                PlayIgniteAudio();
                Debug.Log($"<color=yellow>[AncientBrazier] Re-ignited player's torch from {gameObject.name}!</color>");
                return true;
            }
        }

        /// <summary>
        /// Checks if a world position is sheltered within this brazier's safe light radius.
        /// </summary>
        public bool IsPositionSheltered(Vector3 worldPos)
        {
            if (!_isLit) return false;
            Vector2 a = new Vector2(transform.position.x, transform.position.z);
            Vector2 b = new Vector2(worldPos.x, worldPos.z);
            return Vector2.Distance(a, b) <= safeRadius;
        }

        /// <summary>
        /// Static query used by PoisonGasZone to check if the player is near any lit AncientBrazier.
        /// </summary>
        public static bool IsPositionShelteredByAnyBrazier(Vector3 worldPos)
        {
            foreach (var brazier in ActiveBraziers)
            {
                if (brazier != null && brazier.IsPositionSheltered(worldPos))
                {
                    return true;
                }
            }
            return false;
        }

        private void EnsurePlayerReference()
        {
            if (_playerTransform != null && _playerTorch != null) return;

            var locomotion = FindAnyObjectByType<PlayerLocomotion.PlayerLocomotion>();
            if (locomotion != null)
            {
                _playerTransform = locomotion.transform;
                _playerTorch = locomotion.GetComponentInChildren<PlayerTorchController>() ??
                               locomotion.GetComponentInParent<PlayerTorchController>();
            }
        }

        private void ApplyVisualState()
        {
            Color flameColor = (brazierRole == BrazierRole.SoulFlameAltar)
                ? new Color(0.15f, 0.95f, 1.0f, 1.0f)
                : new Color(1.0f, 0.65f, 0.2f, 1.0f);

            if (flameVisual != null)
            {
                flameVisual.SetActive(_isLit);
            }

            if (brazierLight != null)
            {
                brazierLight.enabled = _isLit;
                brazierLight.color = flameColor;
                brazierLight.range = safeRadius * 1.4f;
            }
        }

        private void PlayIgniteAudio()
        {
            if (_audioSource != null && igniteSFX != null)
            {
                _audioSource.PlayOneShot(igniteSFX);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = (brazierRole == BrazierRole.SoulFlameAltar) ? Color.cyan : new Color(1f, 0.6f, 0.1f, 0.7f);
            Gizmos.DrawWireSphere(transform.position, safeRadius);
        }
    }
}