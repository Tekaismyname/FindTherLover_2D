using UnityEngine;

namespace FindTheLover.Enemies.Data
{
    /// <summary>
    /// Blueprint configuring nighttime enemy spawning behavior and donut boundaries.
    /// Follows Data-Driven Architecture for easy balancing without recompiling code.
    /// </summary>
    [CreateAssetMenu(fileName = "NightSpawn_Config", menuName = "Game/Enemies/Night Spawn Config")]
    public class NightSpawnConfigSO : ScriptableObject
    {
        [Header("Spawn Rate & Capacity")]
        [Tooltip("Seconds between consecutive spawn attempts during nighttime")]
        [Range(1f, 20f)] public float spawnInterval = 4f;

        [Tooltip("Maximum active enemies allowed simultaneously to protect performance.")]
        [Range(1, 30)] public int maxSimultaneousEnemies = 8;

        [Header("Donut spawn distance")]
        [Tooltip("Minimum distance from player (inner donut ring - prevent popping directly on screen)")]
        [Range(5f, 30f)] public float minSpawnRadius = 15f;

        [Tooltip("Maximum distance from player (outer donut ring - keeps enemies within island bounds)")]
        [Range(15f, 50f)] public float maxSpawnRadius = 25f;

        [Header("NavMesh Validation")]
        [Tooltip("Maximun vertical tolerance when snapping calculated position onto NavMesh surface")]
        [Range(1f, 10f)] public float navMeshSnapDistance = 4f;
    }
}