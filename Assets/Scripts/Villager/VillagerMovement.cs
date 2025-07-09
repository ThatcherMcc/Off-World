using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillagerMovement : MonoBehaviour
{
    [Header("Player Detection")]
    public GameObject player;
    public float noticeRadius = 5f;
    private bool playerInRange = false;

    [Header("Looking Behavior")]
    public float minLookWaitTime = 2f;
    public float maxLookWaitTime = 5f;
    public float rotationSpeed = 2f;
    public float maxLookAngle = 80f;
    private float lookTimer;
    private Quaternion targetRotation;
    private bool isRotating = false;

    [Header("References")]
    [SerializeField] private VillagerChatting chatComponent;

    private void Start()
    {
        // Find player if not assigned
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
        }

        // Get dialogue component if not assigned
        if (chatComponent == null)
        {
            chatComponent = GetComponent<VillagerChatting>();
        }

        // Initialize look timer
        ResetLookTimer();
    }

    private void Update()
    {
        if (player != null)
        {
            // Check if player is in range
            CheckPlayerDistance();

            // Determine looking behavior
            if (playerInRange)
            {
                // Look at player when in range
                Vector3 direction = (player.transform.position - transform.position).normalized;
                LookAtPlayer(direction);

                // Notify dialogue component that player is in range
                if (chatComponent != null)
                {
                    chatComponent.PlayerInRange(true);
                }
            }
            else
            {
                // Notify dialogue component that player is not in range
                if (chatComponent != null)
                {
                    chatComponent.PlayerInRange(false);
                }

                // Random looking when player not in range
                RandomLooking();
            }
        }
    }

    private void CheckPlayerDistance()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);

        // Check if player is within notice radius
        if (distance <= noticeRadius)
        {
            // Check if player is visible (not behind obstacles)
            Vector3 direction = (player.transform.position - transform.position).normalized;
            if (Physics.Raycast(transform.position, direction, out RaycastHit hit, noticeRadius))
            {
                if (hit.transform.gameObject.CompareTag("Player"))
                {
                    // Player just entered range
                    if (!playerInRange)
                    {
                        playerInRange = true;
                    }
                    return;
                }
            }
        }

        // Player not in range or not visible
        playerInRange = false;
    }

    private void RandomLooking()
    {
        if (isRotating)
        {
            // Continue rotation towards target
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

            // Check if we've approximately reached the target rotation
            if (Quaternion.Angle(transform.rotation, targetRotation) < 1f)
            {
                isRotating = false;
                ResetLookTimer();
            }
        }
        else
        {
            // Countdown until next random look
            lookTimer -= Time.deltaTime;
            if (lookTimer <= 0)
            {
                // Pick a new random rotation
                float randomAngle = Random.Range(-maxLookAngle, maxLookAngle);
                Vector3 currentEuler = transform.rotation.eulerAngles;
                targetRotation = Quaternion.Euler(currentEuler.x, currentEuler.y + randomAngle, currentEuler.z);
                isRotating = true;
            }
        }
    }

    private void ResetLookTimer()
    {
        lookTimer = Random.Range(minLookWaitTime, maxLookWaitTime);
    }

    private void LookAtPlayer(Vector3 direction)
    {
        Quaternion rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotationSpeed * 2);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, noticeRadius);
    }
}