using System.Collections;
using UnityEngine;
using BossFight.BehaviorTrees;
using BossFight.Strategies;
using UnityEngine.Rendering.VirtualTexturing;

public class CharacterInteraction : MonoBehaviour
{
    private GameObject player;
    private Animator animator;
    private Rigidbody rb;
    private RockBossHeadLook rockBossHeadLook;

    [Header("BossTree")]
    public float meleeRange; // the distance the player must be within for melee attacks
    public float rangedRange; // the distance the player must be within for ranged attacks
    public float runSpeed = 50; // speed at which the boss chases the player
    BehaviorTree tree; // The behavior tree instance

    public string playerNearParameter = "StartShake"; // animator parameter to trigger when player is near
    public float proximityDistance = 10f; // distance at which the boss becomes alive
    public float noticeDistance = 20f; // distance at which the boss notices the player when up
    public int layerIndex = 0; // Assuming it's on base layer

    public bool gotUp = false; // whether the boss has fully gotten up
    public bool isBusy = false; // is the boss busy performing an action

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rockBossHeadLook = GetComponent<RockBossHeadLook>();
        player = GameObject.FindGameObjectWithTag("Player");
        rockBossHeadLook.enabled = false;

        // start behavior tree
        tree = new BehaviorTree("RockBoss"); // create new behavior tree
        PrioritySelector root = new PrioritySelector("RootPrioritySelector"); // create root node that everything branches from
        tree.AddChild(root);
        //

        ////// Attacks
        RandomSelector Attacks = new RandomSelector("AttackSelector", 5);
        //// Ranged Attacks
        GuardedSequence RangedAttack = new GuardedSequence(
            "RangedAttackSequence",
            () => SetIsBusy(true),
            () => SetIsBusy(false)
        );
        RangedAttack.AddChild(new Leaf("CanRangedAttackPlayer", new Condition(() => PlayerWithinRange(rangedRange) && !PlayerWithinRange(meleeRange))));
        RangedAttack.AddChild(new Leaf("FacingPlayer", new Condition(() => rockBossHeadLook.IsLookingAtPlayer())));
        RangedAttack.AddChild(new Leaf("RangedAttackAnimation", new AnimationWaitStrategy(animator, "ThrowRock", 2.2f)));
        ////
        //// Physical Attacks
        RandomSelector PhysicalAttacks = new RandomSelector("PhysicalAttackSelector");
        // A single left punch attack sequence
        GuardedSequence LeftPunch = new GuardedSequence("LeftPunchSequence",
            () => SetIsBusy(true),
            () => SetIsBusy(false)
        );
        LeftPunch.AddChild(new Leaf("CanMeleePlayer", new Condition(() => PlayerWithinRange(meleeRange))));
        LeftPunch.AddChild(new Leaf("FacingPlayer", new Condition(() => rockBossHeadLook.IsLookingAtPlayer())));
        LeftPunch.AddChild(new Leaf("LeftPunchAttack", new AnimationWaitStrategy(animator, "PunchLeft", 1.7f)));
        // a single right punch attack sequence
        GuardedSequence RightPunch = new GuardedSequence("LeftPunchSequence",
            () => SetIsBusy(true),
            () => SetIsBusy(false)
        );
        RightPunch.AddChild(new Leaf("CanMeleePlayer", new Condition(() => PlayerWithinRange(meleeRange))));
        RightPunch.AddChild(new Leaf("FacingPlayer", new Condition(() => rockBossHeadLook.IsLookingAtPlayer())));
        RightPunch.AddChild(new Leaf("RightPunchAttack", new AnimationWaitStrategy(animator, "PunchRight", 1.7f)));

        PhysicalAttacks.AddChild(LeftPunch);
        PhysicalAttacks.AddChild(RightPunch);
        ////
        Attacks.AddChild(RangedAttack);
        Attacks.AddChild(PhysicalAttacks);
        //////

        //// Movement
        RandomSelector Movement = new RandomSelector("MovementSelector", 1);
        // Jump slam attack sequence
        GuardedSequence JumpSlam = new GuardedSequence(
            "JumpSlam",
            () => SetIsBusy(true),
            () => SetIsBusy(false)
        );
        JumpSlam.AddChild(new Leaf("PlayerOutOfRange", new Condition(() => PlayerOutOfRange(meleeRange))));
        JumpSlam.AddChild(new Leaf("FacingPlayer", new Condition(() => rockBossHeadLook.IsLookingAtPlayer())));
        JumpSlam.AddChild(new Leaf("TurnOffHeadLook", new ActionStrategy(() => TurnOFFHeadLook())));
        JumpSlam.AddChild(new Leaf("StartJumpAnimation", new AnimationWaitStrategy(animator, "JumpSlam", 0.85f)));
        JumpSlam.AddChild(new Leaf("JumpSlamAttack", new JumpOnPlayerStrategy(rb, player.transform, 2.3f)));
        JumpSlam.AddChild(new Leaf("WaitForJumpToFinish", new WaitStrategy(6f)));
        JumpSlam.AddChild(new Leaf("TurnOffHeadLook", new ActionStrategy(() => TurnONHeadLook())));
        // Chase player sequence
        GuardedSequence ChasePlayer = new GuardedSequence(
            "ChasePlayer",
            () => SetIsBusy(true),
            () => SetIsBusy(false)
        );
        ChasePlayer.AddChild(new Leaf("PlayerOutOfRange", new Condition(() => PlayerOutOfRange(meleeRange))));
        ChasePlayer.AddChild(new Leaf("FacingPlayer", new Condition(() => rockBossHeadLook.IsLookingAtPlayer())));
        ChasePlayer.AddChild(new Leaf("StartRunningAnimation", new ActionStrategy(() => SetAnimTrigger("StartWalking"))));
        ChasePlayer.AddChild(new Leaf("ChasePlayer", new ChasePlayerStrategy(rb, player.transform, runSpeed, 2.5f)));
        ChasePlayer.AddChild(new Leaf("StopRunningAnimation", new ActionStrategy(() => SetAnimTrigger("StopWalking"))));

        Movement.AddChild(JumpSlam);
        Movement.AddChild(ChasePlayer);
        ////

        root.AddChild(Movement);
        root.AddChild(Attacks);

    }

    void Update()
    {
        if (player != null)
        {
            bool alertedBoss = PlayerWithinRange(proximityDistance); // check if player within proximity distance

            if (!gotUp && alertedBoss) // boss getting up 
            {
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
        yield return new WaitForSeconds(4.3f);
        TurnONHeadLook();
        yield return new WaitForSeconds(3.3f);
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

    // Sets isBusy to true or false
    private void SetIsBusy(bool state)
    {
        isBusy = state;
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