using UnityEngine;
using System.Collections.Generic;

public class WolfAI : BaseAI, IEnemy
{
    [Header("Wolf Specifics")]
    [SerializeField] private float wanderSpeed = 2f;
    [SerializeField] private float runSpeed = 5f;

    private Vector3 idleTargetPosition;

    protected override void Idle()
    {
        Vector3 targetPosition;

        if (timeUntilIdleMoveChange <= 0f)
        {
            // DEBUG - Let's see what position it's choosing
            targetPosition = GetRandomRoamPosition();
            Debug.Log("New idle target: " + targetPosition);
            idleTargetPosition = targetPosition;
            ResetIdleMoveTimer();
        }
        else
        {
            targetPosition = idleTargetPosition;
        }

        // Calculate direction vector
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;

        // DEBUG - Visualize the direction
        Debug.DrawRay(transform.position, directionToTarget * 3, Color.yellow);

        // Use the direction for movement, not the full position
        Move(transform.position + directionToTarget, wanderSpeed);
        RotateMob(directionToTarget);
    }

    protected override void Chase()
    {
        // Calculate direction toward player
        Vector3 directionToPlayer = (player.position - transform.position).normalized;

        // DEBUG
        Debug.Log("Player position: " + player.position);
        Debug.Log("Wolf position: " + transform.position);
        Debug.Log("Direction to player: " + directionToPlayer);

        // Draw direction line
        Debug.DrawRay(transform.position, directionToPlayer * 5, Color.red, 0.1f);

        // Move along the direction vector, not directly to the player position
        Move(transform.position + directionToPlayer, runSpeed);
        RotateMob(directionToPlayer);
    }

    protected override void Chase(Vector3 position, float speedMultiplier)
    {
        // Calculate direction to last known position
        Vector3 directionToPosition = (position - transform.position).normalized;

        // DEBUG
        Debug.Log("Last known position: " + position);
        Debug.Log("Direction to last known: " + directionToPosition);

        // Draw direction line
        Debug.DrawRay(transform.position, directionToPosition * 5, Color.blue, 0.1f);

        // Move along the direction vector
        Move(transform.position + directionToPosition, runSpeed * speedMultiplier);
        RotateMob(directionToPosition);
    }

    private void RotateMob(Vector3 direction)
    {
        if (direction == Vector3.zero) return;

        // Create a rotation that looks in the direction of movement
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

        // Smoothly rotate toward the target direction
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }
}