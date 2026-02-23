using System.Collections;
using UnityEngine;
using BossFight.BehaviorTrees;
using BossFight.Strategies;
/// <summary>
/// RockBoss controller: wake-up, healthbar, and behavior tree.
/// Tree: PunishBack (6) → Attacks (5) → Movement (1). Five attacks: BACKSPIN, THROWROCK, SLAM, PUNCHLEFT, PUNCHRIGHT.
/// </summary>
public class CharacterInteraction : MonoBehaviour
{
    [SerializeField] private GameObject player;
    private Animator animator;
    private Rigidbody rb;
    private RockBossHeadLook rockBossHeadLook;
    private EnemyHealth enemyHealth;
    [SerializeField] private CanvasGroup bossHealthUI;

    [Header("BossTree")]
    public float meleeRange; // the distance the player must be within for melee attacks
    public float rangedRange; // the distance the player must be within for ranged attacks
    [Tooltip("Max range for stomp/kick back (punish backside). Often same as meleeRange.")]
    public float backStompRange = 5f;
    public float runSpeed = 50; // speed at which the boss chases the player
    [Header("Anim triggers & durations (UPPERCASE names; durations match clip length)")]
    private string animTriggerBackspin = "BACKSPIN";
    private string animTriggerSlam = "SLAM";
    private float backspinAnimDuration = 5.87f;
    private float slamAnimDuration = 6.667f;
    private string playerNearParameter = "STARTSHAKE";
    BehaviorTree tree; // The behavior tree instance
    public float proximityDistance = 10f; // distance at which the boss becomes alive
    public float noticeDistance = 20f; // distance at which the boss notices the player when up
    public int layerIndex = 0; // Assuming it's on base layer

    public bool gotUp = false; // whether the boss has fully gotten up
    public bool isBusy = false; // is the boss busy performing an action
    private bool gettingUpStarted = false; // prevent starting wake-up coroutine every frame

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rockBossHeadLook = GetComponent<RockBossHeadLook>();
        enemyHealth = GetComponent<EnemyHealth>();
        if (player == null) {
            player = GameObject.FindGameObjectWithTag("Player");
        }
        if (bossHealthUI == null)
        {
            bossHealthUI = GameObject.Find("BossHealthbarUI").GetComponent<CanvasGroup>();
        }

        // Animator uses UPPERCASE trigger names; fix if prefab/scene still has old mixed-case values
        if (string.Equals(playerNearParameter, "StartShake", System.StringComparison.OrdinalIgnoreCase))
            playerNearParameter = "STARTSHAKE";
        if (string.Equals(animTriggerSlam, "Slam", System.StringComparison.OrdinalIgnoreCase))
            animTriggerSlam = "SLAM";
        if (string.Equals(animTriggerBackspin, "Backspin", System.StringComparison.OrdinalIgnoreCase))
            animTriggerBackspin = "BACKSPIN";

        rockBossHeadLook.enabled = false;
        DeactivateHealthbar();

        // Behavior tree: PunishBack (6) > Attacks (5) > Movement (1). 5 attacks = BACKSPIN, THROWROCK, SLAM, PUNCHLEFT, PUNCHRIGHT.
        tree = new BehaviorTree("RockBoss");
        PrioritySelector root = new PrioritySelector("RootPrioritySelector");
        tree.AddChild(root);

        // —— 1. Punish back (priority 6): BACKSPIN when player is behind; disable head look and flip forward for anim, then restore ——
        GuardedSequence PunishBack = new GuardedSequence(
            "PunishBack",
            () => StartBackspinSetup(),
            () => EndBackspinSetup(),
            6
        );
        PunishBack.AddChild(new Leaf("PlayerBehind", new Condition(() => PlayerIsBehind())));
        PunishBack.AddChild(new Leaf("PlayerInBackStompRange", new Condition(() => PlayerWithinRange(backStompRange))));
        PunishBack.AddChild(new Leaf("BackspinAttack", new AnimationWaitStrategy(animator, animTriggerBackspin, backspinAnimDuration)));

        // —— 2. Attacks (priority 5): ranged or melee (slam, left hook, right hook) ——
        RandomSelector Attacks = new RandomSelector("AttackSelector", 5);
        // Ranged: rock throw
        GuardedSequence RangedAttack = new GuardedSequence(
            "RangedAttackSequence",
            () => SetIsBusy(true),
            () => SetIsBusy(false)
        );
        RangedAttack.AddChild(new Leaf("CanRangedAttackPlayer", new Condition(() => PlayerWithinRange(rangedRange) && !PlayerWithinRange(meleeRange))));
        RangedAttack.AddChild(new Leaf("FacingPlayer", new Condition(() => rockBossHeadLook.IsLookingAtPlayer())));
        RangedAttack.AddChild(new Leaf("RangedAttackAnimation", new AnimationWaitStrategy(animator, "THROWROCK", 2.2f)));
        // Melee: random among Slam (AOE), Left hook, Right hook
        RandomSelector PhysicalAttacks = new RandomSelector("PhysicalAttackSelector");
        GuardedSequence Slam = new GuardedSequence("SlamSequence", () => SetIsBusy(true), () => SetIsBusy(false));
        Slam.AddChild(new Leaf("CanMeleePlayer", new Condition(() => PlayerWithinRange(meleeRange))));
        Slam.AddChild(new Leaf("FacingPlayer", new Condition(() => rockBossHeadLook.IsLookingAtPlayer())));
        Slam.AddChild(new Leaf("SlamAttack", new AnimationWaitStrategy(animator, animTriggerSlam, slamAnimDuration)));

        GuardedSequence LeftHook = new GuardedSequence("LeftHookSequence", () => SetIsBusy(true), () => SetIsBusy(false));
        LeftHook.AddChild(new Leaf("CanMeleePlayer", new Condition(() => PlayerWithinRange(meleeRange))));
        LeftHook.AddChild(new Leaf("FacingPlayer", new Condition(() => rockBossHeadLook.IsLookingAtPlayer())));
        LeftHook.AddChild(new Leaf("LeftHookAttack", new AnimationWaitStrategy(animator, "PUNCHLEFT", 1.7f)));

        GuardedSequence RightHook = new GuardedSequence("RightHookSequence", () => SetIsBusy(true), () => SetIsBusy(false));
        RightHook.AddChild(new Leaf("CanMeleePlayer", new Condition(() => PlayerWithinRange(meleeRange))));
        RightHook.AddChild(new Leaf("FacingPlayer", new Condition(() => rockBossHeadLook.IsLookingAtPlayer())));
        RightHook.AddChild(new Leaf("RightHookAttack", new AnimationWaitStrategy(animator, "PUNCHRIGHT", 1.7f)));

        PhysicalAttacks.AddChild(Slam);
        PhysicalAttacks.AddChild(LeftHook);
        PhysicalAttacks.AddChild(RightHook);
        Attacks.AddChild(RangedAttack);
        Attacks.AddChild(PhysicalAttacks);

        // —— 3. Movement (priority 1): close distance when out of melee. Head look stays ON until we pass FacingPlayer so boss can turn. ——
        RandomSelector Movement = new RandomSelector("MovementSelector", 1);
        GuardedSequence JumpSlam = new GuardedSequence("JumpSlam", null, () => SetIsBusy(false));
        JumpSlam.AddChild(new Leaf("PlayerOutOfRange", new Condition(() => PlayerOutOfRange(meleeRange))));
        JumpSlam.AddChild(new Leaf("FacingPlayer", new Condition(() => rockBossHeadLook.IsLookingAtPlayer())));
        JumpSlam.AddChild(new Leaf("SetBusyForJump", new ActionStrategy(() => SetIsBusy(true))));
        JumpSlam.AddChild(new Leaf("StartJumpAnimation", new AnimationWaitStrategy(animator, "JUMPSLAM", 0.85f)));
        JumpSlam.AddChild(new Leaf("JumpSlamAttack", new JumpOnPlayerStrategy(rb, player.transform, 2.3f)));
        JumpSlam.AddChild(new Leaf("WaitForJumpToFinish", new WaitStrategy(6f)));

        GuardedSequence ChasePlayer = new GuardedSequence("ChasePlayer", null, () => SetIsBusy(false));
        ChasePlayer.AddChild(new Leaf("PlayerOutOfRange", new Condition(() => PlayerOutOfRange(meleeRange))));
        ChasePlayer.AddChild(new Leaf("FacingPlayer", new Condition(() => rockBossHeadLook.IsLookingAtPlayer())));
        ChasePlayer.AddChild(new Leaf("SetBusyForChase", new ActionStrategy(() => SetIsBusy(true))));
        ChasePlayer.AddChild(new Leaf("StartRunningAnimation", new ActionStrategy(() => SetAnimTrigger("STARTWALKING"))));
        ChasePlayer.AddChild(new Leaf("ChasePlayer", new ChasePlayerStrategy(rb, player.transform, runSpeed, 2.5f)));
        ChasePlayer.AddChild(new Leaf("StopRunningAnimation", new ActionStrategy(() => SetAnimTrigger("STOPWALKING"))));

        Movement.AddChild(JumpSlam);
        Movement.AddChild(ChasePlayer);

        root.AddChild(PunishBack);
        root.AddChild(Attacks);
        root.AddChild(Movement);
    }

    void Update()
    {
        if (player != null)
        {
            bool alertedBoss = PlayerWithinRange(proximityDistance); // check if player within proximity distance

            if (!gotUp && !gettingUpStarted && alertedBoss) // start wake-up once when player is near
            {
                gettingUpStarted = true;
                StartCoroutine(GettingUpWait());
            }

            bool playerInRange = PlayerWithinRange(noticeDistance); // check if player within notice distance

            if (gotUp && playerInRange) // if boss is activated do boss things
            {
                tree.Process();
            }
        }
    }

    // Begins the process of the boss getting up
    IEnumerator GettingUpWait() 
    {
        TurnOFFHeadLook();
        animator.SetTrigger(playerNearParameter);
        yield return new WaitForSeconds(4.6f);
        TurnONHeadLook();
        ActivateHealthBar();
        yield return new WaitForSeconds(3f);
        gotUp = true;
    }

    // Checks if player within a range
    private bool PlayerWithinRange(float range)
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (distanceToPlayer <= range)
        {
            return true;
        }
        return false;
    }

    // Checks if player out of a range
    private bool PlayerOutOfRange(float range)
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (distanceToPlayer > range)
        {
            return true;
        }
        return false;
    }

    /// <summary> True when player is in the rear hemisphere (behind boss). Used for stomp/kick back. </summary>
    private bool PlayerIsBehind()
    {
        Vector3 toPlayer = (player.transform.position - transform.position).normalized;
        toPlayer.y = 0f;
        toPlayer.Normalize();
        float dot = Vector3.Dot(transform.forward, toPlayer);
        return dot < -0.3f; // behind = dot < ~0 (e.g. -0.3 gives ~100° cone behind)
    }

    // Debugging function to print comments to console
    private void Debugging(string comment)
    {
               Debug.Log(comment);  
    }

    // Activates animation trigger to start an animation
    private void SetAnimTrigger(string trigger)
    {
        animator.SetTrigger(trigger);
    }

    // Sets isBusy to true or false; when true, stop rotating toward player so the chosen animation plays without head look fighting it.
    private void SetIsBusy(bool state)
    {
        isBusy = state;
        if (rockBossHeadLook != null)
            rockBossHeadLook.enabled = !state;
    }

    // Turn off procedural head look script, so the bosses head doesn't rotate in awkward ways during certain animations
    private void TurnOFFHeadLook()
    {
        rockBossHeadLook.enabled = false;
    }
    // Turn on procedural head look script
    private void TurnONHeadLook()
    {
        rockBossHeadLook.enabled = true;
    }

    /// <summary>Called when BACKSPIN starts. (Head look off via SetIsBusy.)</summary>
    private void StartBackspinSetup()
    {
        SetIsBusy(true);
    }
    /// <summary>Called when BACKSPIN ends. Facing sync is done by animation event calling AnimationMethodsRockBoss.SyncFacingAfterBackspin().</summary>
    private void EndBackspinSetup()
    {
        SetIsBusy(false);
    }

    private void ActivateHealthBar()
    {
        enemyHealth.SetActivated(true);
        bossHealthUI.alpha = 1f;
    }
    public void DeactivateHealthbar()
    {
        bossHealthUI.alpha = 0f;
        enemyHealth.SetActivated(false);
    }


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangedRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, noticeDistance);
    }
}