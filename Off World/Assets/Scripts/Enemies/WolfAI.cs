using UnityEngine;
using System.Collections.Generic;

public class WolfAI : BaseAI, IEnemy
{
    [Header("Wolf Specifics")]
    [SerializeField] private float wanderSpeed = 2f;
    [SerializeField] private float runSpeed = 5f;

    private Vector3 moveDirection;

    protected override void Awake()
    {
        base.Awake();
        ResetIdleMoveTimer(); // Ensure timer is initialized in Awake
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void Idle()
    {
        if (timeUntilIdleMoveChange <= 0f) // Check the timer
        {
            moveDirection = GetRandomRoamPosition(); // Get a new direction
            ResetIdleMoveTimer(); // Reset the timer
        }
        Move(moveDirection, wanderSpeed);
        RotateMob(moveDirection);
    }

    protected override void Chase()
    {
        moveDirection = (player.position - transform.position).normalized;
        Move(moveDirection, runSpeed);
        RotateMob(moveDirection);
    }

    protected override void Chase(Vector3 position, float speedMultiplier)
    {
        moveDirection = (position - transform.position).normalized;
        Move(moveDirection, runSpeed * speedMultiplier);
        RotateMob(moveDirection);
    }

    private void RotateMob(Vector3 direction)
    {
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = lookRotation;
    }
}