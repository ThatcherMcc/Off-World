using UnityEngine;
using System.Collections.Generic; //  For storing visited positions

public abstract class BaseAI : MonoBehaviour, IEnemy
{
    [Header("Awareness")]
    [SerializeField] protected float noticeRadius = 10f;
    [SerializeField] protected float loseInterestRadius = 15f;
    [SerializeField] protected float fieldOfView = 90f;
    [SerializeField] protected LayerMask obstacleLayer;

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
    private float timeSinceLostSight = 0f;
    private bool isSearching = false;
    private Vector3 currentSearchTarget;
    private Queue<Vector3> visitedPositions = new Queue<Vector3>();
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
        player = GameObject.FindGameObjectWithTag("Player").transform;
        roamCenter = transform.position;
        rb = GetComponent<Rigidbody>();
        ResetIdleMoveTimer(); // Initialize the timer
    }

    protected virtual void Update()
    {
        if (!aiEnabled) return;

        timeSinceLastPositionMemory += Time.deltaTime;
        timeUntilIdleMoveChange -= Time.deltaTime; // Decrement the timer

        if (CanSeePlayer())
        {
            isChasing = true;
            timeSinceLostSight = 0f;
            Chase();
            lastKnownPlayerPos = player.position;
            RememberPosition();
        }
        else if (isChasing)
        {
            timeSinceLostSight += Time.deltaTime;
            Chase(lastKnownPlayerPos, chaseSpeedMultiplier);

            if (timeSinceLostSight >= searchTime)
            {
                isChasing = false;
                isSearching = true;
                timeSinceLostSight = 0f;
                StartSearch();
            }
        }
        else if (isSearching)
        {
            Search();
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
        moveDirection = (targetPosition - transform.position).normalized;
        if (rb != null)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, moveDirection * speed, Time.deltaTime * moveSmoothness);
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
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

    private void RememberPosition()
    {
        if (timeSinceLastPositionMemory >= positionMemoryInterval)
        {
            visitedPositions.Enqueue(transform.position);
            timeSinceLastPositionMemory = 0f;
            if (visitedPositions.Count > 10)
            {
                visitedPositions.Dequeue();
            }
        }
    }

    private void StartSearch()
    {
        currentSearchTarget = GetRandomPointAround(lastKnownPlayerPos);
    }

    private void Search()
    {
        Move(currentSearchTarget, baseSpeed * searchMoveSpeedMultiplier);

        if (Vector3.Distance(transform.position, currentSearchTarget) < 1f)
        {
            if (visitedPositions.Count > 0)
            {
                currentSearchTarget = visitedPositions.Dequeue();
            }
            else
            {
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
        Vector2 randomCircle = Random.insideUnitCircle * investigateRadius;
        Vector3 randomDirection = new Vector3(randomCircle.x, 0f, randomCircle.y);
        randomDirection += center;
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