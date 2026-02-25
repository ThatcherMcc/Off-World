using UnityEngine;

namespace OffWorld.Anatomy
{
    /// <summary>
    /// Configuration asset for the drone pickup system.
    /// Holds the drone prefab reference and flight settings.
    /// </summary>
    [CreateAssetMenu(fileName = "DronePickupConfig", menuName = "Off-World/Drone Pickup Config")]
    public class DronePickupConfig : ScriptableObject
    {
        [Tooltip("Prefab to instantiate for the drone. Can be a placeholder capsule/cube.")]
        public GameObject dronePrefab;

        [Tooltip("Speed at which the drone slowly hovers down toward the item.")]
        public float approachSpeed = 4f;

        [Tooltip("Speed at which the drone arcs away after grabbing the item.")]
        public float departSpeed = 18f;

        [Tooltip("How close the drone must get to the item before 'grabbing' it.")]
        public float arrivalThreshold = 0.5f;

        [Tooltip("Distance from camera at which the departing drone is considered gone.")]
        public float despawnDistance = 80f;
    }
}
