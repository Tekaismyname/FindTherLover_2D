using System;
using FindTheLover.Combat;
using FindTheLover.Equipment.Data;
using FindTheLover.Player;
using FindTheLover.PlayerLocomotion;
using UnityEngine;

namespace FindTherLover.Player
{
    /// <summary>
    /// Logic & Presentation Layer: Modular Hand Socket controller managing held weapons & tools.
    /// Renders standalone weapon sprites tinted by 5 material tiers, executes juicy procedural swing arcs
    /// synchronized with PlayerCombat combos, dynamically layers SortingOrder (in front/behind Quene),
    /// and eliminates the need for dozens of baked weapon character spritesheets.
    /// </summary>
    public class PlayerEquipment : MonoBehaviour
    {
        [Header("Modular Hand Socket Configuration")]
        [Tooltip("Pivot transform anchored near the character's hand/hip.")]
        [SerializeField] private Transform beltSocket;

        [Tooltip("Hand anchor offset relative to character center when facing Front (South).")]
        [SerializeField] private Vector2 handOffsetRight = new Vector2(0.74f, -0.78f);

        [Tooltip("Hand anchor offset when in Side profile (Right; mirrored automatically for Left).")]
        [SerializeField] private Vector2 handOffsetSide = new Vector2(0.46f, -0.76f);

        [Tooltip("Hand anchor offset when facing away (Back / North).")]
        [SerializeField] private Vector2 handOffsetBack = new Vector2(0.64f, -0.68f);

        [Tooltip("Small local Z separation (meters) to prevent 3D Z-fighting with the character sprite when orbiting horizontally.")]
        [SerializeField] private float depthLayerZOffset = 0.08f;

        [Tooltip("Idle resting Z rotation angle (degrees) for modular sprites when facing right.")]
        [SerializeField] private float idleTiltAngle = -14f;

        [Header("Starting Equipment")]
        [Tooltip("Item automatically equipped upon scene initialization if Hotbar does not override.")]
        [SerializeField] private EquipmentItemDataSO initialEquipment;

        [Header("Procedural Slash Juice")]
        [Tooltip("Forward thrust push distance during a melee swing.")]
        [SerializeField] private float swingForwardReach = 0.65f;

        [Tooltip("Temporary scale punch peak during weapon swing.")]
        [SerializeField] private float swingScalePunch = 1.18f;

        // Sibling component references
        private SpriteRenderer _playerSpriteRenderer;
        private PlayerLocomotion _playerLocomotion;
        private PlayerCombat _playerCombat;
        private PlayerHarvestInteraction _harvestInteraction;
        private SpriteRenderer _itemSpriteRenderer;

        // Runtime equipment state
        private EquipmentItemDataSO currentData;
        private GameObject currentInstance;
        private Vector3 initialSocketScale = Vector3.one;

        // Procedural swing state
        private bool _isSwinging;
        private float _swingElapsed;
        private float _activeSwingDuration = 0.22f;
        private float _activeStartAngle = 55f;
        private float _activeEndAngle = -105f;
        private int _activeComboStep = 1;

        /// <summary>Event broadcast whenever the player equips or unequips gear (consumed by GameDataManager & HotbarUI).</summary>
        public event Action<EquipmentItemDataSO> OnEquipmentChanged;

        public EquipmentItemDataSO CurrentEquippedData => currentData;
        public EquipmentType CurrentEquipmentType => currentData != null ? currentData.equipmentType : EquipmentType.None;
        public float CurrentWeaponDamageBonus => currentData != null ? currentData.weaponDamageBonus : 0f;
        public float CurrentHarvestPowerMultiplier => currentData != null ? Mathf.Max(0.5f, currentData.harvestPowerMultiplier) : 1f;
        public HarvestToolType CurrentHarvestTool
        {
            get
            {
                if (currentData == null) return HarvestToolType.None;
                return currentData.equipmentType switch
                {
                    EquipmentType.Axe => HarvestToolType.Axe,
                    EquipmentType.Pickaxe => HarvestToolType.Pickaxe,
                    _ => HarvestToolType.None
                };
            }
        }
        public bool IsSwinging => _isSwinging;

        public enum FacingDirection2D
        {
            Front,
            Back,
            Right,
            Left
        }

        /// <summary>
        /// Pure deterministic 4-way facing classifier based on camera-relative 2D movement input.
        /// Eliminates flipX desync bugs when transitioning between side attacks and front/back walks.
        /// </summary>
        public static FacingDirection2D ResolveFacingDirection(Vector2 facing)
        {
            if (facing.sqrMagnitude < 0.01f) return FacingDirection2D.Front;

            if (Mathf.Abs(facing.x) >= Mathf.Abs(facing.y) && Mathf.Abs(facing.x) > 0.2f)
            {
                return facing.x < 0f ? FacingDirection2D.Left : FacingDirection2D.Right;
            }

            return facing.y > 0.15f ? FacingDirection2D.Back : FacingDirection2D.Front;
        }

        private void Awake()
        {
            _playerSpriteRenderer = GetComponent<SpriteRenderer>();
            _playerLocomotion = GetComponent<PlayerLocomotion>();
            _playerCombat = GetComponent<PlayerCombat>();
            _harvestInteraction = GetComponent<PlayerHarvestInteraction>();

            if (beltSocket != null)
            {
                initialSocketScale = beltSocket.localScale;
                if (Mathf.Abs(initialSocketScale.x) < 0.01f) initialSocketScale = Vector3.one;
            }
            else
            {
                GameObject fallback = new GameObject("Hand_Socket");
                fallback.transform.SetParent(transform, false);
                fallback.transform.localPosition = new Vector3(handOffsetRight.x, handOffsetRight.y, 0f);
                beltSocket = fallback.transform;
                initialSocketScale = Vector3.one;
            }
        }

        private void OnEnable()
        {
            if (_playerCombat != null)
            {
                _playerCombat.OnAttackSwingStarted += HandleAttackSwingStarted;
            }
        }

        private void OnDisable()
        {
            if (_playerCombat != null)
            {
                _playerCombat.OnAttackSwingStarted -= HandleAttackSwingStarted;
            }
        }

        private void Start()
        {
            if (currentData == null && initialEquipment != null)
            {
                EquipItem(initialEquipment);
            }
        }

        private void LateUpdate()
        {
            UpdateSwingTimer();
            UpdateSortingAndFlipping();
        }

        /// <summary>
        /// Equips a new item blueprint into the Modular Hand Socket, supporting both custom prefabs
        /// (e.g., Torch/Lantern) and standalone tiered weapon sprites (Sword, Axe, Pickaxe).
        /// </summary>
        public void EquipItem(EquipmentItemDataSO newItem)
        {
            if (newItem == null)
            {
                UnequipCurrentItem();
                return;
            }

            UnequipCurrentItem(silent: true);
            currentData = newItem;

            if (beltSocket != null)
            {
                if (newItem.heldPrefab != null)
                {
                    currentInstance = Instantiate(newItem.heldPrefab, beltSocket);
                    currentInstance.transform.localPosition = newItem.localOffset;
                    currentInstance.transform.localRotation = Quaternion.Euler(newItem.localRotation);
                    _itemSpriteRenderer = currentInstance.GetComponentInChildren<SpriteRenderer>();
                }
                else if (newItem.weaponSprite != null)
                {
                    // Build clean Modular Weapon visual directly from Sprite + Tier Tint Color!
                    currentInstance = new GameObject($"ModularWeapon_{newItem.displayName}");
                    currentInstance.transform.SetParent(beltSocket, false);
                    currentInstance.transform.localPosition = newItem.localOffset;
                    currentInstance.transform.localRotation = Quaternion.Euler(newItem.localRotation);
                    currentInstance.transform.localScale = Vector3.one * Mathf.Max(0.1f, newItem.visualScale);

                    _itemSpriteRenderer = currentInstance.AddComponent<SpriteRenderer>();
                    _itemSpriteRenderer.sprite = newItem.weaponSprite;
                    _itemSpriteRenderer.color = newItem.tierTintColor;
                    if (_playerSpriteRenderer != null)
                    {
                        _itemSpriteRenderer.sortingLayerID = _playerSpriteRenderer.sortingLayerID;
                    }
                }
            }

            SyncToolWithHarvestSystem(newItem.equipmentType);
            Debug.Log($"[PlayerEquipment] Equipped Modular Gear: {newItem.displayName} [{newItem.materialTier} {newItem.equipmentType}] (+{newItem.weaponDamageBonus} DMG)");
            OnEquipmentChanged?.Invoke(currentData);
        }

        /// <summary>
        /// Removes and destroys the currently equipped visual in the hand socket.
        /// </summary>
        public void UnequipCurrentItem(bool silent = false)
        {
            bool hadItem = currentData != null || currentInstance != null;
            if (currentInstance != null)
            {
                Destroy(currentInstance);
                currentInstance = null;
                _itemSpriteRenderer = null;
            }

            currentData = null;
            _isSwinging = false;
            SyncToolWithHarvestSystem(EquipmentType.None);

            if (!silent && hadItem)
            {
                OnEquipmentChanged?.Invoke(null);
            }
        }

        /// <summary>
        /// Starts a procedural weapon swing arc on the Modular Hand Socket matching the active combo step.
        /// </summary>
        public void TriggerWeaponSwing(int comboStep = 1)
        {
            if (currentData == null || currentInstance == null) return;

            _activeComboStep = Mathf.Clamp(comboStep, 1, 3);
            _activeSwingDuration = Mathf.Max(0.08f, currentData.swingDuration);
            _swingElapsed = 0f;
            _isSwinging = true;

            // Alternate slash arc direction per combo step for juicy 3-hit visual variety!
            if (_activeComboStep == 2 && currentData.equipmentType == EquipmentType.Sword)
            {
                // Combo Step 2: Reverse upward backhand slash
                _activeStartAngle = currentData.swingEndAngle;
                _activeEndAngle = currentData.swingStartAngle + 15f;
            }
            else if (_activeComboStep == 3)
            {
                // Combo Step 3: Wide heavy finisher arc
                _activeStartAngle = currentData.swingStartAngle + 25f;
                _activeEndAngle = currentData.swingEndAngle - 25f;
            }
            else
            {
                // Combo Step 1 / Axe Chop / Pickaxe Mine: Crisp downward cleave
                _activeStartAngle = currentData.swingStartAngle;
                _activeEndAngle = currentData.swingEndAngle;
            }
        }

        /// <summary>
        /// Pure deterministic interpolation helper computing Z rotation angle across a swing (0..1).
        /// </summary>
        public static float EvaluateSwingAngle(float startAngle, float endAngle, float normalizedProgress)
        {
            float t = Mathf.Clamp01(normalizedProgress);
            // Fast explosive acceleration into smooth follow-through easing (EaseOutBack feel)
            float eased = 1f - Mathf.Pow(1f - t, 3f);
            return Mathf.Lerp(startAngle, endAngle, eased);
        }

        private void HandleAttackSwingStarted(int comboStep, Vector2 facing2D)
        {
            TriggerWeaponSwing(comboStep);
        }

        private void UpdateSwingTimer()
        {
            if (!_isSwinging) return;

            _swingElapsed += Time.deltaTime;
            if (_swingElapsed >= _activeSwingDuration)
            {
                _isSwinging = false;
                _swingElapsed = 0f;
            }
        }

        /// <summary>
        /// Dynamically positions the Modular Hand Socket for all 4 character facing views (Front, Back, Left, Right),
        /// applies Z-depth separation to eliminate 3D billboard Z-fighting during horizontal camera orbits,
        /// drives procedural Ease-Out Cubic swing arcs, and adjusts 2.5D SortingOrder.
        /// </summary>
        public void UpdateSortingAndFlipping()
        {
            if (_playerLocomotion == null || _playerSpriteRenderer == null || beltSocket == null) return;

            Vector2 facing = _playerLocomotion.LastFacingDirection;
            FacingDirection2D facingDir = ResolveFacingDirection(facing);

            // Prevent stale flipX left over from side attacks when walking Front/Back
            if ((facingDir == FacingDirection2D.Front || facingDir == FacingDirection2D.Back) && _playerSpriteRenderer.flipX)
            {
                _playerSpriteRenderer.flipX = false;
            }

            bool isFacingLeft = facingDir == FacingDirection2D.Left;
            bool isFacingBack = facingDir == FacingDirection2D.Back;
            bool isSideProfile = facingDir == FacingDirection2D.Left || facingDir == FacingDirection2D.Right;

            // 1. Dynamic 2.5D Sorting Order & Physical Z-depth separation (fixes Z-fighting during camera yaw!)
            if (_itemSpriteRenderer != null)
            {
                _itemSpriteRenderer.sortingOrder = isFacingBack
                    ? _playerSpriteRenderer.sortingOrder - 1
                    : _playerSpriteRenderer.sortingOrder + 2;
            }

            // 2. Compute current swing progress, angle, forward thrust, and scale punch
            float zAngle = isFacingBack ? idleTiltAngle * 0.6f : idleTiltAngle;
            float reachX = 0f;
            float reachY = 0f;
            float scaleMult = 1f;

            if (_isSwinging && _activeSwingDuration > 0f)
            {
                float progress = Mathf.Clamp01(_swingElapsed / _activeSwingDuration);
                zAngle = EvaluateSwingAngle(_activeStartAngle, _activeEndAngle, progress);
                float punchCurve = Mathf.Sin(progress * Mathf.PI); // Peaks cleanly at mid-swing (0.5)
                reachX = punchCurve * swingForwardReach;
                reachY = punchCurve * (isFacingBack ? 0.25f : 0.10f);
                scaleMult = Mathf.Lerp(1f, swingScalePunch, punchCurve);
            }

            // 3. Select exact 4-way hand anchor offset + local Z separation
            float dirSign = isFacingLeft ? -1f : 1f;
            Vector2 baseAnchor = facingDir switch
            {
                FacingDirection2D.Back => handOffsetBack,
                FacingDirection2D.Left => handOffsetSide,
                FacingDirection2D.Right => handOffsetSide,
                _ => handOffsetRight
            };

            // DynamicBillboard faces away from camera along +Z (LookRotation(camForward)),
            // so negative local Z is closer to the camera (in front of Quene), positive local Z is behind Quene.
            float localZ = isFacingBack ? Mathf.Abs(depthLayerZOffset) : -Mathf.Abs(depthLayerZOffset);

            // Mirror X cleanly when facing Left; keep Back hand tucked behind Quene's right side
            float anchoredX = (baseAnchor.x + (isSideProfile ? reachX : reachX * 0.55f)) * dirSign;
            float anchoredY = baseAnchor.y + reachY;

            beltSocket.localPosition = new Vector3(anchoredX, anchoredY, localZ);
            beltSocket.localRotation = Quaternion.Euler(0f, 0f, zAngle * dirSign);
            beltSocket.localScale = new Vector3(
                Mathf.Abs(initialSocketScale.x) * dirSign * scaleMult,
                Mathf.Abs(initialSocketScale.y) * scaleMult,
                Mathf.Abs(initialSocketScale.z)
            );
        }

        private void SyncToolWithHarvestSystem(EquipmentType eqType)
        {
            if (_harvestInteraction == null) return;

            switch (eqType)
            {
                case EquipmentType.Axe:
                    _harvestInteraction.SetEquippedTool(HarvestToolType.Axe);
                    break;
                case EquipmentType.Pickaxe:
                    _harvestInteraction.SetEquippedTool(HarvestToolType.Pickaxe);
                    break;
                default:
                    _harvestInteraction.SetEquippedTool(HarvestToolType.None);
                    break;
            }
        }
    }
}
