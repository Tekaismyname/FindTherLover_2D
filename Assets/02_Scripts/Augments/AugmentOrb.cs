using UnityEngine;
using FindTheLover.Augments.Data;

namespace FindTheLover.Augments
{
    /// <summary>
    /// Logic & Presentation Layer: Collectible glowing Augment Orb dropped by Elite raiders and Bosses.
    /// Bobbing crystalline sphere that opens the TFT 3-card Augment Selection Modal upon player contact.
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class AugmentOrb : MonoBehaviour
    {
        [Header("Orb Rarity Configuration")]
        [Tooltip("Minimum guaranteed tier when this orb is picked up (Bronze, Silver, or Gold).")]
        [SerializeField] private AugmentTier _guaranteedMinTier = AugmentTier.Bronze;

        [Header("Floating & Visual Feedback")]
        [SerializeField] private float _hoverAmplitude = 0.25f;
        [SerializeField] private float _hoverFrequency = 2.8f;
        [SerializeField] private float _spinDegreesPerSecond = 75f;
        [SerializeField] private float _pickupMagnetRadius = 3.2f;
        [SerializeField] private float _pickupCollectRadius = 1.3f;
        [SerializeField] private float _magnetPullSpeed = 7.5f;

        [Header("Optional Visual References")]
        [SerializeField] private Renderer _orbRenderer;
        [SerializeField] private Light _orbPointLight;

        private Vector3 _baseAnchorPosition;
        private Transform _playerTransform;
        private bool _isCollected;

        public AugmentTier GuaranteedMinTier => _guaranteedMinTier;

        private void Awake()
        {
            SphereCollider col = GetComponent<SphereCollider>();
            col.isTrigger = true;
            col.radius = Mathf.Max(col.radius, 1.2f);

            if (_orbRenderer == null) _orbRenderer = GetComponentInChildren<Renderer>();
            if (_orbPointLight == null) _orbPointLight = GetComponentInChildren<Light>();
        }

        private void Start()
        {
            _baseAnchorPosition = transform.position;
            ApplyTierVisualTint(_guaranteedMinTier);
            LocatePlayer();
        }

        private void Update()
        {
            if (_isCollected) return;

            // 1. Gentle vertical hover and rotation
            float bobOffset = Mathf.Sin(Time.time * _hoverFrequency) * _hoverAmplitude;
            transform.Rotate(Vector3.up, _spinDegreesPerSecond * Time.deltaTime, Space.World);

            // 2. Magnetic pull toward nearby player
            if (_playerTransform == null) LocatePlayer();
            if (_playerTransform != null)
            {
                Vector3 toPlayer = _playerTransform.position + Vector3.up * 0.9f - _baseAnchorPosition;
                float dist = toPlayer.magnitude;

                if (dist <= _pickupCollectRadius)
                {
                    CollectOrb();
                    return;
                }

                if (dist <= _pickupMagnetRadius)
                {
                    _baseAnchorPosition = Vector3.MoveTowards(_baseAnchorPosition, _playerTransform.position + Vector3.up * 0.9f, _magnetPullSpeed * Time.deltaTime);
                }
            }

            transform.position = _baseAnchorPosition + Vector3.up * bobOffset;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isCollected) return;
            if (other.CompareTag("Player"))
            {
                CollectOrb();
            }
        }

        /// <summary>
        /// Configures this orb's guaranteed tier and updates its glowing tint dynamically upon spawn.
        /// </summary>
        public void InitializeTier(AugmentTier minTier)
        {
            _guaranteedMinTier = minTier;
            _baseAnchorPosition = transform.position;
            ApplyTierVisualTint(minTier);
        }

        private void CollectOrb()
        {
            if (_isCollected) return;
            if (AugmentManager.Instance == null || AugmentManager.Instance.IsSelectionPending) return;

            bool offered = AugmentManager.Instance.OfferAugmentSelection(_guaranteedMinTier);
            if (offered)
            {
                _isCollected = true;
                Debug.Log($"<color=#FFD700>[AugmentOrb]</color> Collected [{_guaranteedMinTier}] Augment Orb!");
                Destroy(gameObject);
            }
        }

        private void ApplyTierVisualTint(AugmentTier tier)
        {
            Color tint = GetTierColor(tier);

            if (_orbRenderer != null)
            {
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                _orbRenderer.GetPropertyBlock(block);
                block.SetColor("_BaseColor", tint);
                block.SetColor("_Color", tint);
                block.SetColor("_EmissionColor", tint * 2.2f);
                _orbRenderer.SetPropertyBlock(block);
            }

            if (_orbPointLight != null)
            {
                _orbPointLight.color = tint;
            }
        }

        private static Color GetTierColor(AugmentTier tier)
        {
            switch (tier)
            {
                case AugmentTier.Gold: return new Color(1.0f, 0.84f, 0.18f, 1f);
                case AugmentTier.Silver: return new Color(0.75f, 0.88f, 1.0f, 1f);
                default: return new Color(0.84f, 0.52f, 0.24f, 1f);
            }
        }

        private void LocatePlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                _playerTransform = player.transform;
            }
        }
    }
}
