using UnityEngine;
using UnityEngine.InputSystem;
using FindTheLover.Combat.Data;
using FindTheLover.CameraSystem;

namespace FindTheLover.Combat
{
    /// <summary>
    /// Logic Layer: Executes multi-step melee attack combos, detects hitboxes,
    /// applies damage & knockback, and triggers CameraShake + HitStop feedback.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class PlayerCombat : MonoBehaviour
    {
        [Header("Input Settings (New Input System)")]
        [Tooltip("Reference to the Attack Input Action from your .inputactions asset")]
        [SerializeField] private InputActionReference attackAction;

        [Header("Combo Configuration")]
        [Tooltip("Active weapon combo ScriptableObject")]
        [SerializeField] private WeaponComboSO currentCombo;

        [Header("References")]
        [SerializeField] private Animator animator;

        [Header("Hitbox Detection")]
        [Tooltip("Distance in front of player where hitbox center is placed")]
        [SerializeField] private float attackRange = 1.5f;

        [Tooltip("Radius of the sphere hitbox")]
        [SerializeField] private float attackRadius = 1.2f;

        [Tooltip("Layer that can be hit (Environment, Enemies)")]
        [SerializeField] private LayerMask hitLayers = ~0;

        [Header("Juice & VFX")]
        [Tooltip("Particle system prefab spawned at contact points upon impact")]
        [SerializeField] private GameObject hitSparkPrefab;

        [SerializeField] private SpriteRenderer spriteRenderer;
        private PlayerLocomotion.PlayerLocomotion _locomotion;
        private static readonly int PosXHash = Animator.StringToHash("PosX");
        private static readonly int PosYHash = Animator.StringToHash("PosY");

        // Internal combat state tracking
        private int currentComboIndex = 0;
        private float lastAttackTime = 0f;
        private bool isAttacking = false;
        private AttackDataSO lastExecutedAttack = null;

        private void Awake()
        {
            if (animator == null)
            {
                animator = GetComponent<Animator>();
            }
        }

        private void OnEnable()
        {
            if (attackAction != null && attackAction.action != null)
            {
                attackAction.action.Enable();
            }
        }

        private void OnDisable()
        {
            if (attackAction != null && attackAction.action != null)
            {
                attackAction.action.Disable();
            }
        }

        private void Update()
        {
            HandleComboReset();
            HandleInput();
        }

        /// <summary>
        /// Reads attack input strictly through the New Input System Action.
        /// </summary>
        private void HandleInput()
        {
            if (attackAction == null || attackAction.action == null) return;

            if (attackAction.action.WasPressedThisFrame())
            {
                TryExecuteAttack();
            }
        }

        /// <summary>
        /// Validates timing and lock duration before executing the next strike.
        /// </summary>
        private void TryExecuteAttack()
        {
            if (currentCombo == null || currentCombo.TotalAttacks == 0)
            {
                Debug.LogWarning("[PlayerCombat] No WeaponComboSO assigned or combo chain is empty!");
                return;
            }

            // Prevent spamming attacks faster than the current attack's lock duration
            if (isAttacking && lastExecutedAttack != null)
            {
                if (Time.time < lastAttackTime + lastExecutedAttack.lockMovementDuration)
                {
                    return;
                }
            }

            ExecuteAttack();
        }

        /// <summary>
        /// Detects all IDamageable entities, applies damage, knockback, and triggers Combat Juice.
        /// </summary>
        private void ExecuteAttack()
        {
            AttackDataSO attackData = currentCombo.GetAttack(currentComboIndex);
            if (attackData == null) return;

            Vector2 facing2D = _locomotion != null ? _locomotion.LastFacingDirection : Vector2.down;
            Vector3 worldFacing = _locomotion != null ? _locomotion.LastWorldFacingDirection : transform.forward;
            worldFacing.y = 0f;
            if (worldFacing.sqrMagnitude < 0.01f) worldFacing = Vector3.back;
            worldFacing.Normalize();
            // Gửi tọa độ vào BlendTree của đòn đánh
            animator.SetFloat(PosXHash, facing2D.x);
            animator.SetFloat(PosYHash, facing2D.y);
            // Lật hình nhân vật khi đánh sang trái
            if (spriteRenderer != null)
            {
                if (facing2D.x > 0.05f) spriteRenderer.flipX = false;
                else if (facing2D.x < -0.05f) spriteRenderer.flipX = true;
            }

            // 1. Send ComboStep (1, 2, 3) and Trigger to Animator
            animator.SetInteger("ComboStep", attackData.comboStep);

            if (!string.IsNullOrEmpty(attackData.animatorTrigger))
            {
                animator.SetTrigger(attackData.animatorTrigger);
            }

            // 2. Play swing sound effect if assigned
            if (attackData.swingSound != null)
            {
                AudioSource.PlayClipAtPoint(attackData.swingSound, transform.position);
            }

            // 3. Evaluate sphere hitbox in front of player
            Vector3 hitboxCenter = transform.position + worldFacing * attackRange + Vector3.up * 1.0f;
            Collider[] colliders = Physics.OverlapSphere(hitboxCenter, attackRadius, hitLayers);

            bool hasHitAnyTarget = false;

            foreach (var col in colliders)
            {
                if (col.transform.root == transform.root) continue;

                // A. Apply Damage via IDamageable
                var damageable = col.GetComponentInParent<IDamageable>() ?? col.GetComponentInChildren<IDamageable>();
                if (damageable != null && !damageable.IsDead)
                {
                    Vector3 hitPoint = col.ClosestPoint(hitboxCenter);
                    damageable.TakeDamage(attackData.damage, hitPoint, -worldFacing);

                    // B. Apply Knockback via IKnockbackable
                    var knockbackable = col.GetComponentInParent<Iknockbackable>() ?? col.GetComponentInChildren<Iknockbackable>();
                    if (knockbackable != null)
                    {
                        Vector3 knockbackDir = (col.transform.position - transform.position).normalized;
                        knockbackDir.y = 0f;
                        knockbackable.ApplyKnockBack(knockbackDir, attackData.knockbackForce);
                    }

                    // C. Spawn Hit Spark VFX
                    if (hitSparkPrefab != null)
                    {
                        Instantiate(hitSparkPrefab, hitPoint, Quaternion.LookRotation(worldFacing));
                    }

                    hasHitAnyTarget = true;
                }
            }

            // 4. Trigger synchronized Combat Juice if at least one target was struck
            if (hasHitAnyTarget)
            {
                // Screen Shake (Presentation Layer)
                if (CameraShake.Instance != null && attackData.screenShakeIntensity > 0f)
                {
                    CameraShake.Instance.Shake(attackData.screenShakeIntensity, 0.15f);
                }

                // Freeze Frame Hit Stop (Logic Layer)
                if (HitStopManager.Instance != null && attackData.hitStopDuration > 0f)
                {
                    HitStopManager.Instance.TriggerHitStop(attackData.hitStopDuration);
                }

                // Audio Impact
                if (attackData.hitImpactSound != null)
                {
                    AudioSource.PlayClipAtPoint(attackData.hitImpactSound, hitboxCenter);
                }
            }

            // 5. Update combat state tracking
            lastAttackTime = Time.time;
            lastExecutedAttack = attackData;
            isAttacking = true;

            // Advance combo index (loops back to 0 at end of combo chain)
            currentComboIndex++;
            if (currentComboIndex >= currentCombo.TotalAttacks)
            {
                currentComboIndex = 0;
            }
        }

        /// <summary>
        /// Resets the combo back to Attack 1 if the player pauses longer than the combo window.
        /// </summary>
        private void HandleComboReset()
        {
            if (currentComboIndex == 0 || lastExecutedAttack == null) return;

            if (Time.time > lastAttackTime + lastExecutedAttack.comboChainWindow)
            {
                currentComboIndex = 0;
                isAttacking = false;
                lastExecutedAttack = null;
            }
        }

        /// <summary>
        /// Helper property to check if the player is currently locked in an attack animation.
        /// </summary>
        public bool IsMovementLocked()
        {
            if (!isAttacking || lastExecutedAttack == null) return false;
            return Time.time < lastAttackTime + lastExecutedAttack.lockMovementDuration;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Vector3 worldFacing = _locomotion != null ? _locomotion.LastWorldFacingDirection : transform.forward;
            worldFacing.y = 0f;
            if (worldFacing.sqrMagnitude < 0.01f) worldFacing = Vector3.back;
            worldFacing.Normalize();
            Vector3 hitboxCenter = transform.position + worldFacing * attackRange + Vector3.up * 1.0f;
            Gizmos.DrawWireSphere(hitboxCenter, attackRadius);
        }
    }
}