using UnityEngine;
using System.Collections.Generic; //  For storing visited positions

public abstract class BaseAI : MonoBehaviour, IEnemy
{
    [Header("Awareness")]
    [SerializeField] protected float noticeRadius = 10f;
    [SerializeField] protected float loseInterestRadius = 15f;
    [SerializeField] protected float fieldOfView = 90f;
    [SerializeField] protected LayerMask obstacleLayer;
    [SerializeField] protected float LOSMultiplier = 1.5f;

    [Header("Movement")]
    [SerializeField] protected float baseSpeed = 3f;
    [SerializeField] protected float chaseSpeedMultiplier = 2f;
    [SerializeField] protected float moveSmoothness = 5f; // Smoothing factor
    [SerializeField] protected float idleMoveIntervalMin = 2f; // Min time between direction changes
    [SerializeField] protected float idleMoveIntervalMax = 5f; // Max time between direction changes

    [Header("Rotation")]
    [SerializeField] protected float rotationSpeed = 5f; //  Rotation speed

    public Transform player { get; set; } // From IEnemy
    protected bool aiEnabled = true; // From IEnemy - now protected
    private bool isChasing = false;
    protected Vector3 roamCenter;
    protected float roamDistance = 20f;

    private Vector3 lastKnownPlayerPos;
    private bool hasSeenPlayerBefore = false;
    private float timeSinceLostSight = 0f;
    private bool isSearching = false;
    private Vector3 currentSearchTarget; 
    private Stack<Vector3> visitedPositions = new Stack<Vector3>();
    private float stackTotal = 20f;
    [SerializeField] protected float positionMemoryInterval = 1f;
    private float timeSinceLastPositionMemory = 0f;
    protected float timeUntilIdleMoveChange = 0f; // Timer for next direction change

    // Declare these variables at the class level
    [SerializeField] protected float searchTime = 5f;
    [SerializeField] protected float searchMoveSpeedMultiplier = 0.75f;
    [SerializeField] protected float investigateRadius = 5f;

    protected Rigidbody rb;
    protected Vector3 moveDirection; // Store the intended movement direction


    protected virtual void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform; // get player transform
        roamCenter = transform.position; // giving the mob a specific distance where they can roam/idle
        rb = GetComponent<Rigidbody>();

        // Initialize all position-related variables to current position
        lastKnownPlayerPos = transform.position; 
        currentSearchTarget = transform.position;

        // Make sure we start in idle state
        isChasing = false;
        isSearching = false;

        ResetIdleMoveTimer(); // Initialize the timer
    }

    protected virtual void Update()
    {
        if (!aiEnabled) return; // turns off movement for captured animals

        timeSinceLastPositionMemory += Time.deltaTime;
        timeUntilIdleMoveChange -= Time.deltaTime; // Decrement the timer

        if (CanSeePlayer()) // In LOS of mob
        {
            isChasing = true; // we activate chasing
            timeSinceLostSight = 0f; // our time since lost sight is 0, since we see him
            Chase(); // chase him
            lastKnownPlayerPos = player.position; // keep updating the player position until we lose him
            hasSeenPlayerBefore = true; // Mark that we've seen the player at least once 
            RememberPosition(); // keep track of our route to character
        }
        else if (isChasing) // we've seen the player, now we chase
        {
            timeSinceLostSight += Time.deltaTime; // add to lost sight time for when we cant see him
            Chase(lastKnownPlayerPos, chaseSpeedMultiplier); // chase the player

            if (timeSinceLostSight >= searchTime) // if the time since we lost sight is too long we start searching
            {
                isChasing = false; // no longer chasing
                // Only enter search mode if we've actually seen the player before
                isSearching = hasSeenPlayerBefore; // we search if we have actually seen the player
                timeSinceLostSight = 0f; // reset lost sight timer
                if (hasSeenPlayerBefore)
                {
                    StartSearch(); // start our search
                }
            }
        }
        else if (isSearching)
        {
            Search(); // searching mode activated
        }
        else
        {
            Idle();
        }

        // Draw the movement direction line
        DrawMoveDirectionLine();
    }

    protected bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= noticeRadius)
        {
            if (Vector3.Angle(transform.forward, directionToPlayer) < fieldOfView * 0.5f)
            {
                if (!Physics.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleLayer))
                {
                    
                    return true;
                }
            }
        }
        return false;
    }

    public virtual void EnableAI(bool enable)
    {
        aiEnabled = enable;
    }

    protected abstract void Idle();
    protected abstract void Chase();
    protected abstract void Chase(Vector3 position, float speedMultiplier);

    protected virtual void Move(Vector3 targetPosition, float speed)
    {
        // Calculate move direction
        Vector3 direction = (targetPosition - transform.position).normalized;
        moveDirection = direction; // Store for visualization

        // Apply movement
        if (rb != null)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, direction * speed, Time.deltaTime * moveSmoothness);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, transform.position + direction * speed * Time.deltaTime, speed * Time.deltaTime);
        }
    }

    protected Vector3 GetRandomRoamPosition()
    {
        Vector2 randomCircle = Random.insideUnitCircle * roamDistance;
        Vector3 randomDirection = new Vector3(randomCircle.x, 0f, randomCircle.y);
        randomDirection += roamCenter;
        //  Add a small random offset to prevent getting stuck
        randomDirection += Random.insideUnitSphere * 0.5f;
        return randomDirection;
    }

    // stores path more traveled so it can go back the way it came
    private void RememberPosition()
    {
        // If the time since weve seen any player is longer than the time we want to store positions. so we dont store every frame.
        if (timeSinceLastPositionMemory >= positionMemoryInterval) 
        {
            // Only store position if we're chasing (to create a path to retrace)
            if (isChasing)
            {
                visitedPositions.Push(transform.position); // adds current position to the stack
                timeSinceLastPositionMemory = 0f; // reset the attention span

                // Limit stack size to prevent memory issues
                if (visitedPositions.Count > stackTotal)
                {
                    // Remove oldest positions if we have too many
                    // This is trickier with Stack, but we can create a temporary stack
                    Stack<Vector3> tempStack = new Stack<Vector3>();
                    for (int i = 0; i < stackTotal/2; i++)
                    {
                        if (visitedPositions.Count > 0)
                            tempStack.Push(visitedPositions.Pop());
                    }
                    visitedPositions.Clear();
                    while (tempStack.Count > 0)
                    {
                        visitedPositions.Push(tempStack.Pop());
                    }
                }
            }
        }
    }

    private void StartSearch()
    {
        currentSearchTarget = GetRandomPointAround(lastKnownPlayerPos); // set the search postion to the last spot we saw the player
    }

    // Modify the Search method to properly retrace steps
    private void Search()
    {
        // Move toward the search target
        Move(currentSearchTarget, baseSpeed * searchMoveSpeedMultiplier);
        // if we are at our search target or close, search
        if (Vector3.Distance(transform.position, currentSearchTarget) < 1f)
        {
            if (visitedPositions.Count > 0) // if theres still positions we haven't returned, go there and search
            {
                // Pop the most recent position to retrace steps in reverse
                currentSearchTarget = visitedPositions.Pop();
            }
            else
            {
                // If we've retraced all steps, go back to idle
                isSearching = false;
            }
        }
    }

    // Function to reset the idle movement timer
    protected void ResetIdleMoveTimer()
    {
        timeUntilIdleMoveChange = Random.Range(idleMoveIntervalMin, idleMoveIntervalMax);
    }

    private Vector3 GetRandomPointAround(Vector3 center)
    {
        Vector2 randomCircle = Random.insideUnitCircle * investigateRadius; // gives random angle in a 360 degree radius around mob.
        Vector3 randomDirection = new Vector3(randomCircle.x, 0f, randomCircle.y); // turns that angle into a direction
        randomDirection += center; // add that random direction to the center or position given.
        return randomDirection;
    }

    protected virtual void RotateMob(Vector3 direction)
    {
        if (direction == Vector3.zero) return; //  Don't rotate if no direction

        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, noticeRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(roamCenter, roamDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(lastKnownPlayerPos, investigateRadius);
    }

    // Function to draw the movement direction line
    private void DrawMoveDirectionLine()
    {
        if (moveDirection != Vector3.zero)
        {
            Debug.DrawLine(transform.position, transform.position + moveDirection * 2f, Color.cyan);
        }
    }
}