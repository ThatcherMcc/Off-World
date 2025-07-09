using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class Villager : MonoBehaviour
{
    [Header("Player Detection")]
    public GameObject player;
    public float noticeRadius = 5f;
    private bool playerInRange = false;

    [Header("Dialogue")]
    public GameObject speechBubble;
    public List<string> dialogue;
    private int currentDialogueIndex = 0;
    public float talkDuration = 3f;
    private float talkTimer;

    [Header("Looking Behavior")]
    public float minLookWaitTime = 2f;
    public float maxLookWaitTime = 5f;
    public float rotationSpeed = 2f;
    public float maxLookAngle = 80f;
    private float lookTimer;
    private Quaternion targetRotation;
    private bool isRotating = false;

    private void Start()
    {
        // Hide speech bubble at start
        if (speechBubble != null)
        {
            speechBubble.SetActive(false);
        }

        // Find player if not assigned
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
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
                ShowSpeechBubble();
            }
            else
            {
                // Hide speech bubble if it was shown because of player proximity
                if (speechBubble != null && speechBubble.activeSelf && talkTimer <= 0)
                {
                    speechBubble.SetActive(false);
                }
                // Random looking when player not in range
                RandomLooking();
            }

            // Update talk timer
            if (talkTimer > 0)
            {
                talkTimer -= Time.deltaTime;
                if (talkTimer <= 0)
                {
                    // Hide speech bubble when timer expires
                    if (speechBubble != null)
                    {
                        speechBubble.SetActive(false);
                    }
                }
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

    private void ShowSpeechBubble()
    {
        if (speechBubble != null)
        {
            speechBubble.SetActive(true);

            // Update text component if it exists
            TextMesh textMesh = speechBubble.GetComponentInChildren<TextMesh>();
            if (textMesh != null)
            {
                // Pick a random dialogue line
                int randomIndex = Random.Range(0, dialogue.Count);
                textMesh.text = dialogue[randomIndex];
            }

            // Set talk timer
            talkTimer = talkDuration;
        }
    }

    public void Talk()
    {
        if (dialogue.Count > 0 && speechBubble != null)
        {
            speechBubble.SetActive(true);

            // Update text component if it exists
            TextMesh textMesh = speechBubble.GetComponentInChildren<TextMesh>();
            if (textMesh != null)
            {
                textMesh.text = dialogue[currentDialogueIndex];
            }

            // Cycle to next dialogue line
            currentDialogueIndex = (currentDialogueIndex + 1) % dialogue.Count;

            // Reset talk timer
            talkTimer = talkDuration;
        }
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