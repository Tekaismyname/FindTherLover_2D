using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using FindTheLover.Player;

namespace FindTheLover.Environment.Puzzles
{
    /// <summary>
    /// Logic & Presentation Layer: Ancient thorn barrier sealing the exit of Zone 2.
    /// Immune to normal fire; can only be burned down when the player interacts (Key E)
    /// while carrying the mystical cyan Soul Flame.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class ThornBarrierGate : MonoBehaviour
    {
        [Header("Barrier Interaction Settings")]
        [Tooltip("Horizontal XZ distance within which the player can press E to burn the thorns")]
        [SerializeField] private float interactRadius = 2.8f;

        [Tooltip("Duration in seconds for the Soul Flame to incinerate the barrier")]
        [SerializeField] private float burnDuration = 1.5f;

        [Header("Audio & Events")]
        [Tooltip("Sound played when the thorns are incinerated")]
        [SerializeField] private AudioClip burnSFX;

        [Tooltip("Fired once when the barrier has completely burned away")]
        public UnityEvent OnBarrierDestroyed;

        private BoxCollider _blockingCollider;
        private Transform _playerTransform;
        private PlayerTorchController _playerTorch;
        private bool _isBurning = false;
        private bool _isBurned = false;
        private Vector3 _initialScale;

        public bool IsBurned => _isBurned;

        private void Awake()
        {
            _blockingCollider = GetComponent<BoxCollider>();
            _initialScale = transform.localScale;
        }

        private void Update()
        {
            if (_isBurned || _isBurning) return;

            EnsurePlayerReference();
            if (_playerTransform == null || _playerTorch == null) return;

            Vector2 gateXZ = new Vector2(transform.position.x, transform.position.z);
            Vector2 playerXZ = new Vector2(_playerTransform.position.x, _playerTransform.position.z);
            float distXZ = Vector2.Distance(gateXZ, playerXZ);

            if (distXZ <= interactRadius && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
            {
                TryBurnBarrier(_playerTorch);
            }
        }

        /// <summary>
        /// Attempts to burn the thorn barrier using the player's current torch flame.
        /// Only succeeds if the player carries the Soul Flame.
        /// </summary>
        public bool TryBurnBarrier(PlayerTorchController playerTorch)
        {
            if (_isBurned || _isBurning || playerTorch == null) return false;

            if (!playerTorch.HasSoulFlame)
            {
                Debug.LogWarning("<color=orange>[ThornBarrierGate] Normal fire cannot burn these ancient cursed thorns! Find the Cyan SOUL FLAME!</color>");
                return false;
            }

            StartCoroutine(BurnDownRoutine());
            return true;
        }

        private IEnumerator BurnDownRoutine()
        {
            _isBurning = true;
            Debug.Log("<color=cyan>[ThornBarrierGate] SOUL FLAME ignited the Thorn Barrier! Incinerating...</color>");

            if (burnSFX != null)
            {
                AudioSource.PlayClipAtPoint(burnSFX, transform.position);
            }

            // Immediately disable physical blocking collider so player feels responsive unlocking
            if (_blockingCollider != null)
            {
                _blockingCollider.enabled = false;
            }

            float elapsed = 0f;
            Vector3 startPos = transform.localPosition;

            while (elapsed < burnDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / burnDuration);

                // Shrink vertically into ashes while shaking slightly
                float currentScaleY = Mathf.Lerp(_initialScale.y, 0.0f, t);
                transform.localScale = new Vector3(_initialScale.x, currentScaleY, _initialScale.z);
                transform.localPosition = startPos + new Vector3(Mathf.Sin(elapsed * 35f) * 0.04f, 0f, 0f);

                yield return null;
            }

            _isBurned = true;
            _isBurning = false;
            OnBarrierDestroyed?.Invoke();
            Debug.Log("<color=cyan>[ThornBarrierGate] Thorn Barrier completely incinerated! Path to Zone 3 is open!</color>");
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Forces the barrier burned/intact state when restoring a saved GameData snapshot.
        /// </summary>
        public void ForceSetBurnedState(bool isBurned)
        {
            _isBurned = isBurned;
            _isBurning = false;
            if (_blockingCollider != null) _blockingCollider.enabled = !isBurned;
            gameObject.SetActive(!isBurned);
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

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, interactRadius);
        }
    }
}