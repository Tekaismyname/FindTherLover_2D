using UnityEngine;
using UnityEngine.InputSystem;

namespace FindTheLover.Combat
{
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
            // Safely enable the Attack action when the player awakens
            if (attackAction != null && attackAction.action != null)
            {
                attackAction.action.Enable();
            }
        }

        private void OnDisable()
        {
            // Safely disable the Attack action to prevent memory leaks and ghost inputs
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

            // Pure New Input System: Triggered on the frame the button/click is pressed
            if (attackAction.action.WasPressedThisFrame())
            {
                TryExecuteAttack();
            }
        }

        /// <summary>
        /// Validates timing and combo flow before executing the next strike.
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
        /// Triggers animation, sets combo step, plays swing audio, and advances combo.
        /// </summary>
        private void ExecuteAttack()
        {
            AttackDataSO attackData = currentCombo.GetAttack(currentComboIndex);
            if (attackData == null) return;

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

            // 3. Update combat state tracking
            lastAttackTime = Time.time;
            lastExecutedAttack = attackData;
            isAttacking = true;

            // 4. Advance to next attack or loop back to 0
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

            // Use the specific comboChainWindow defined in the attack itself
            if (Time.time > lastAttackTime + lastExecutedAttack.comboChainWindow)
            {
                currentComboIndex = 0;
                isAttacking = false;
                lastExecutedAttack = null;
            }
        }

        /// <summary>
        /// Helper property to check if the player is currently locked in an attack animation.
        /// Useful for PlayerLocomotion to temporarily stop movement.
        /// </summary>
        public bool IsMovementLocked()
        {
            if (!isAttacking || lastExecutedAttack == null) return false;
            return Time.time < lastAttackTime + lastExecutedAttack.lockMovementDuration;
        }
    }
}