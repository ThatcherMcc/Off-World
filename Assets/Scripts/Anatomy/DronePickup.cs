using UnityEngine;
using OffWorld.Anatomy;

/// <summary>
/// Manages the drone pickup animation sequence:
/// 1. Drone spawns slightly above the item
/// 2. Slowly hovers down over the target (gentle descent)
/// 3. Hides the item (cosmetic grab)
/// 4. Arcs upward and toward the base in a long sweeping curve
/// 5. Destroys drone and target item once far enough away
///
/// Items are added to BaseStorage immediately on interaction (before the drone spawns)
/// so the drone is purely cosmetic — no data loss if the scene changes mid-flight.
/// </summary>
public class DronePickup : MonoBehaviour
{
    private enum DroneState { Hovering, Arcing }
    private DroneState state = DroneState.Hovering;

    private Vector3 targetPosition;
    private GameObject targetItem;

    // Config values
    private float hoverSpeed;
    private float arcSpeed;
    private float arrivalThreshold;
    private float despawnDistance;

    // Arc flight
    private Vector3 arcDirection;     // Flattened direction toward base (or away from player)
    private float arcTimer;
    private float arcUpStrength = 18f;  // How strongly the drone pulls upward during the arc
    private Vector3 velocity;

    /// <summary>
    /// Dispatch a drone to visually collect an item.
    /// The item should already be added to BaseStorage before calling this.
    /// </summary>
    public static void Dispatch(GameObject item, DronePickupConfig config)
    {
        if (item == null || config == null || config.dronePrefab == null) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        // Spawn just above the item with slight random offset
        Vector3 spawnPos = item.transform.position
            + Vector3.up * 6f
            + new Vector3(Random.Range(-1.5f, 1.5f), 0f, Random.Range(-1.5f, 1.5f));

        var droneObj = Instantiate(config.dronePrefab, spawnPos, Quaternion.identity);
        var drone = droneObj.GetComponent<DronePickup>();
        if (drone == null)
            drone = droneObj.AddComponent<DronePickup>();

        drone.targetPosition = item.transform.position + Vector3.up * 0.5f; // Hover just above ground
        drone.targetItem = item;
        drone.hoverSpeed = config.approachSpeed * 0.35f; // Slow hover — about 1/3 approach speed
        drone.arcSpeed = config.departSpeed;
        drone.arrivalThreshold = config.arrivalThreshold;
        drone.despawnDistance = config.despawnDistance;

        // Arc direction: away from the player, slightly randomized
        Vector3 awayFromPlayer = (item.transform.position - cam.transform.position);
        awayFromPlayer.y = 0f;
        if (awayFromPlayer.sqrMagnitude < 0.01f)
            awayFromPlayer = cam.transform.forward;
        awayFromPlayer.Normalize();

        // Add slight random rotation (±30°) so multiple drones don't fly the exact same path
        float randomAngle = Random.Range(-30f, 30f);
        drone.arcDirection = Quaternion.Euler(0f, randomAngle, 0f) * awayFromPlayer;
    }

    private void Update()
    {
        switch (state)
        {
            case DroneState.Hovering:
                UpdateHover();
                break;
            case DroneState.Arcing:
                UpdateArc();
                break;
        }
    }

    private void UpdateHover()
    {
        // Slowly descend toward the item
        transform.position = Vector3.MoveTowards(
            transform.position, targetPosition,
            hoverSpeed * Time.deltaTime);

        // Face the target
        Vector3 lookDir = targetPosition - transform.position;
        if (lookDir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(lookDir), 5f * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < arrivalThreshold)
            OnArrived();
    }

    private void UpdateArc()
    {
        arcTimer += Time.deltaTime;

        // Upward force is strong initially, then eases as the drone gains altitude
        // This creates the "sweeping up" part of the arc
        float upFactor = Mathf.Lerp(1f, 0.3f, Mathf.Clamp01(arcTimer / 3f));
        Vector3 upForce = Vector3.up * arcUpStrength * upFactor;

        // Forward force increases over time — starts slow, accelerates into the distance
        float forwardFactor = Mathf.Lerp(0.4f, 1.5f, Mathf.Clamp01(arcTimer / 2.5f));
        Vector3 forwardForce = arcDirection * arcSpeed * forwardFactor;

        // Combine into velocity and move
        velocity = Vector3.Lerp(velocity, upForce + forwardForce, 3f * Time.deltaTime);
        transform.position += velocity * Time.deltaTime;

        // Face movement direction
        if (velocity.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(velocity), 4f * Time.deltaTime);

        // Despawn check
        Camera cam = Camera.main;
        if (cam != null)
        {
            float dist = Vector3.Distance(transform.position, cam.transform.position);
            if (dist > despawnDistance)
                Complete();
        }
        else
        {
            Complete();
        }
    }

    private void OnArrived()
    {
        if (targetItem != null)
            targetItem.SetActive(false);

        velocity = Vector3.up * 2f; // Start with a gentle upward drift
        arcTimer = 0f;
        state = DroneState.Arcing;
    }

    private void Complete()
    {
        if (targetItem != null)
            Destroy(targetItem);

        Destroy(gameObject);
    }
}
