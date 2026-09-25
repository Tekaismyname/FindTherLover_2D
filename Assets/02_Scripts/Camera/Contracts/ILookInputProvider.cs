using UnityEngine;

namespace FindTheLover.CameraSystem
{
    /// <summary>
    /// Contract defining a look input provider for camera rotation systems.
    /// Follows SOLID Dependency Inversion Principle (DIP).
    /// </summary>
    public interface ILookInputProvider
    {
        Vector2 GetLookDelta();
    }
}