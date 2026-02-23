using UnityEngine;
using BossFight.BehaviorTrees;
using BossFight.Strategies;

/// <summary>
/// Behavior-tree-driven wolf AI. Replaces the NavMesh-based WolfAI.
/// Uses Rigidbody physics for all movement.
///
/// Behavior:
///   Priority 2 - Chase: if the player is within notice radius, in FOV, and has LOS → chase via Rigidbody velocity.
///   Priority 1 - Idle:  wander randomly near spawn point.
///
/// Damage is still handled by WolfAttack (collision-based), so the wolf just needs to reach the player.
/// </summary>
public class WolfBT : MonoBehaviour, IEnemy
{
    [Header("Awareness")]
    [SerializeField] private float noticeRadius = 12f;
    [SerializeField] private float fieldOfView = 120f;
    [SerializeField] private LayerMask obstacleLayer;

    [Header("Movement")]
    [SerializeField] private float wanderSpeed = 2f;
    [SerializeField] private float chaseSpeed = 6f;
    [SerializeField] private float chaseStopDistance = 1.5f;
    [SerializeField] private float wanderRadius = 15f;
    [SerializeField] private float smoothing = 5f;

    [Header("Wander Timing")]
    [SerializeField] private float wanderStepDuration = 3f;

    // IEnemy
    public Transform player { get; set; }
    private bool aiEnabled = true;

    private Rigidbody rb;
    private BehaviorTree tree;
    private Vector3 spawnPoint;
    private Quaternion frozenRotation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        spawnPoint = transform.position;
    }

    private void Start()
    {
        BuildTree();
    }

    private void Update()
    {
        if (!aiEnabled)
        {
            // Stop all movement when AI disabled (incapacitated or dead)
            if (rb != null)
            {
                rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
                rb.angularVelocity = Vector3.zero;
            }
            // Lock rotation to what it was at the moment of incapacitation
            transform.rotation = frozenRotation;
            return;
        }

        tree?.Process();
    }

    private void BuildTree()
    {
        tree = new BehaviorTree("Wolf");
        var root = new PrioritySelector("WolfRoot");

        // --- Chase (priority 2) ---
        var chase = new Sequence("Chase", priority: 2);
        chase.AddChild(new Leaf("CanSeePlayer", new Condition(() => CanSeePlayer())));
        chase.AddChild(new Leaf("ChasePlayer",
            new ChasePlayerStrategy(rb, player, chaseSpeed, chaseStopDistance, smoothing)));
        root.AddChild(chase);

        // --- Idle Wander (priority 1) ---
        var idle = new Sequence("Idle", priority: 1);
        idle.AddChild(new Leaf("Wander",
            new WanderStrategy(rb, spawnPoint, wanderRadius, wanderSpeed, smoothing, wanderStepDuration)));
        root.AddChild(idle);

        tree.AddChild(root);
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > noticeRadius) return false;

        // FOV check
        if (Vector3.Angle(transform.forward, dirToPlayer) > fieldOfView * 0.5f) return false;

        // LOS check (make sure nothing blocks the view)
        if (Physics.Raycast(transform.position, dirToPlayer, dist, obstacleLayer)) return false;

        return true;
    }

    public void EnableAI(bool enable)
    {
        aiEnabled = enable;
        if (!enable)
        {
            frozenRotation = transform.rotation;
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
            rb.angularVelocity = Vector3.zero;
            tree?.Reset();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, noticeRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Application.isPlaying ? spawnPoint : transform.position, wanderRadius);
    }
}
