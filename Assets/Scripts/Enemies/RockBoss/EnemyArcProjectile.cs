using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyArcProjectile : MonoBehaviour
{
    private Transform player;
    private Vector3 target;
    private Rigidbody rb;
    private float arcAngle = 50f;
    private float lifeDuration = 7f;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody>();
        target = player.position;

        Vector3 initialVelocity = CalculateLaunchVelocity(transform.position, target, arcAngle);
        rb.velocity = initialVelocity;
        Destroy(gameObject, lifeDuration);
    }

    Vector3 CalculateLaunchVelocity(Vector3 startPoint, Vector3 targetPoint, float launchAngleDeg)
    {
        float angleRad = launchAngleDeg * Mathf.Deg2Rad; // Convert angle to radians

        // Flatten the vectors to calculate horizontal distance
        Vector3 planarStart = new Vector3(startPoint.x, 0, startPoint.z);
        Vector3 planarTarget = new Vector3(targetPoint.x, 0, targetPoint.z);

        float R = Vector3.Distance(planarStart, planarTarget); // Horizontal distance
        float H = targetPoint.y - startPoint.y;               // Vertical distance (target Y - start Y)

        float g = Physics.gravity.y; // Gravity, typically negative (-9.81)

        // The core formula derived above
        float denominator = 2 * Mathf.Cos(angleRad) * Mathf.Cos(angleRad) * (H - R * Mathf.Tan(angleRad));

        // Check for impossibility: If denominator is zero or positive, or the term inside sqrt is negative
        if (Mathf.Abs(denominator) < 0.001f || // Avoid division by very small numbers close to zero
            (g * R * R) / denominator < 0)    // If the term inside sqrt is negative
        {
            Debug.LogWarning("Target unreachable with angle " + launchAngleDeg + " degrees. Launching straight instead.");
            // Fallback: Launch straight towards the target with a default speed
            return (targetPoint - startPoint).normalized * 15f; // You can adjust this fallback speed
        }

        float v0Squared = (g * R * R) / denominator;
        float v0 = Mathf.Sqrt(v0Squared);

        // Calculate time of flight
        // We calculate time to ensure we get the correct velocity components.
        // t = R / (v0 * cos(theta))
        float timeToTarget = R / (v0 * Mathf.Cos(angleRad));

        // Calculate initial velocity components
        float velocityX = v0 * Mathf.Cos(angleRad);
        float velocityY = v0 * Mathf.Sin(angleRad);

        // Get the horizontal direction vector
        Vector3 horizontalDirection = (planarTarget - planarStart).normalized;

        // Combine to form the initial velocity vector
        Vector3 initialVelocity = horizontalDirection * velocityX;
        initialVelocity.y = velocityY;

        return initialVelocity;
    }
}
