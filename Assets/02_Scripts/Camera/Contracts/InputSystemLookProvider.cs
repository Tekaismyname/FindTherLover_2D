using UnityEngine;
using UnityEngine.InputSystem;

namespace FindTheLover.CameraSystem
{
    /// <summary>
    /// Minimal New Input System look provider.
    /// Reads <Pointer>/delta which automatically supports both PC Mouse and Android Touch.
    /// </summary>
    public class InputSystemLookProvider : MonoBehaviour, ILookInputProvider
    {
        [Tooltip("Reference to the look action from InputSystem_Actions (Vector2 Delta)")]
        [SerializeField] private InputActionReference lookAction;

        public Vector2 GetLookDelta() =>
            (lookAction != null && lookAction.action != null && lookAction.action.enabled)
                ? lookAction.action.ReadValue<Vector2>()
                : Vector2.zero;

        private void OnEnable() => lookAction?.action?.Enable();
        private void OnDisable() => lookAction?.action?.Disable();
    }
}