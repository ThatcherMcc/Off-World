using UnityEngine;
using BossFight.BehaviorTrees;
using BossFight.Strategies;

/// <summary>
/// Behavior-tree-driven frog AI. Replaces the original FrogAI.
/// Uses Rigidbody hop-based physics for all movement. No NavMesh.
///
/// Behavior:
///   Priority 2 - Flee: if the player is within notice radius and has LOS → burst of hops away.
///   Priority 1 - Idle: random hop-wander near spawn point with rest periods.
///
/// The frog is prey -- it never attacks, only flees.
/// </summary>
public class FrogBT : MonoBehaviour, IEnemy
{
    [Header("Awareness")]
    [SerializeField] private float noticeRadius = 8f;

    [Header("Movement")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float maxWanderDist = 10f;

    [Header("Flee")]
    [SerializeField] private float fleeHopCooldown = 0.8f;
    [SerializeField] private int fleeHopsPerBurst = 3;

    [Header("Idle Wander")]
    [SerializeField] private float restMin = 2f;
    [SerializeField] private float restMax = 4f;

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
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            // Lock rotation to what it was at the moment of incapacitation
            // This overrides Animator, physics, and everything else
            transform.rotation = frozenRotation;
            return;
        }

        tree?.Process();
    }

    private void BuildTree()
    {
        tree = new BehaviorTree("Frog");
        var root = new PrioritySelector("FrogRoot");

        // --- Flee (priority 2) ---
        var flee = new Sequence("Flee", priority: 2);
        flee.AddChild(new Leaf("PlayerNearby", new Condition(() => CanSeePlayer())));
        flee.AddChild(new Leaf("HopAway",
            new HopFleeStrategy(rb, player, jumpForce, fleeHopCooldown, fleeHopsPerBurst)));
        root.AddChild(flee);

        // --- Idle Hop Wander (priority 1) ---
        var idle = new Sequence("Idle", priority: 1);
        idle.AddChild(new Leaf("HopWander",
            new HopWanderStrategy(rb, spawnPoint, maxWanderDist, jumpForce, restMin, restMax)));
        root.AddChild(idle);

        tree.AddChild(root);
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist > noticeRadius) return false;

        // Simple LOS raycast (frogs have no FOV restriction -- 360 degree awareness)
        Vector3 dir = (player.position - transform.position).normalized;
        if (Physics.Raycast(transform.position, dir, out RaycastHit hit, noticeRadius))
        {
            if (hit.transform.CompareTag("Player"))
                return true;
        }

        return false;
    }

    public void EnableAI(bool enable)
    {
        aiEnabled = enable;
        if (!enable)
        {
            frozenRotation = transform.rotation;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            tree?.Reset();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, noticeRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(Application.isPlaying ? spawnPoint : transform.position, maxWanderDist);
    }
}
