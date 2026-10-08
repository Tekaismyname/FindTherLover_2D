
using UnityEngine;

namespace FindTheLover.Combat.Data
{


    /// <summary>
    /// ScriptableObject defining all data, timings, and game-feel parameters for a single attack.
    /// </summary>
    [CreateAssetMenu(fileName = "New_AttackData", menuName = "FindTheLover/Combat/Attack Data")]
    public class AttackDataSO : ScriptableObject
    {
        [Header("Identity &  Animation")]
        [Tooltip("Name of the attack f or debugging")]
        public string attackName = "Punch";

        [Tooltip("Trigger name in ther Animator(e.g Attack1, Attack2, Attack3)")]
        public string animatorTrigger = "Attack";

        [Tooltip("Step index in the combo(1,2,3)")]
        public int comboStep = 1;

        [Header("Combat Stats")]
        [Tooltip("base damage dealt  to enemies or resouces nodes")]
        public float damage = 10f;

        [Tooltip("Knockback force applied to targets on hit")]
        public float knockbackForce = 3f;

        [Header("Timing &  Game feel")]
        [Tooltip("How long movement is l ocked while executing this attack")]
        public float lockMovementDuration = 0.3f;

        [Tooltip("Time window after this attack where the player can chain the next attack")]
        public float comboChainWindow = 0.8f;

        [Tooltip("Duration of the frame freeze (hit stop) int seconds")]
        [Range(0f, 0.2f)] public float hitStopDuration = 0.06f;

        [Header("Audio &  Juice")]
        [Tooltip("Sound played when swinging this attack")]
        public AudioClip swingSound;

        [Tooltip("Sound played when this attack  hits a target ")]
        public AudioClip hitImpactSound;

        [Tooltip("Screen shake intensity when landing this stike")]
        [Range(0f, 1f)] public float screenShakeIntensity = 0.1f;
    }
}