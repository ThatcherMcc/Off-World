using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;
using BossFight.BehaviorTrees;
using BossFight.Strategies;

public class CharacterInteraction : MonoBehaviour
{
    private GameObject player;
    private Animator animator;
    private Rigidbody rb;
    private RockBossHeadLook rockBossHeadLook;

    [Header("BossTree")]
    public float meleeRange;
    public float rangedRange;
    public float moveOffset;
    public float runSpeed = 50;
    BehaviorTree tree;

    public string playerNearParameter = "StartShake";
    public float proximityDistance = 10f;
    public float noticeDistance = 20f;
    public int layerIndex = 0; // Assuming it's on base layer

    private bool gettingUp = false;
    public bool gotUp = false;

    public bool isInteracting = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        rockBossHeadLook = GetComponent<RockBossHeadLook>();
        player = GameObject.FindGameObjectWithTag("Player");
        rockBossHeadLook.enabled = false;

        // start behavior tree
        tree = new BehaviorTree("RockBoss");
        //tree.AddChild(new Leaf("Chase", new ChasePlayerStrategy(rb, player.transform, runSpeed)));

        // Attack
        // Phys

        Sequence LeftPunch = new Sequence("LeftPunch");
        LeftPunch.AddChild(new Leaf("CanMeleePlayer", new Condition(() => PlayerWithinRange(meleeRange))));
        LeftPunch.AddChild(new Leaf("Punch", new AnimationWaitStrategy(animator, "PunchLeft", 1.667f)));

        Sequence RightPunch = new Sequence("RightPunch");
        RightPunch.AddChild(new Leaf("CanMeleePlayer", new Condition(() => PlayerWithinRange(meleeRange))));
        RightPunch.AddChild(new Leaf("Punch", new AnimationWaitStrategy(animator, "PunchRight", 1.667f)));

        RandomSelector PhysicalHits = new RandomSelector("PhysicalHits");
        PhysicalHits.AddChild(LeftPunch);
        PhysicalHits.AddChild(RightPunch);
        //
        // Ranged
        Sequence RangedAttack = new Sequence("RangedAttack");
        RangedAttack.AddChild(new Leaf("CanThrow", new Condition(() => PlayerWithinRange(rangedRange))));
        RangedAttack.AddChild(new Leaf("ThrowRock", new AnimationWaitStrategy(animator, "ThrowRock", 2f)));
        //

        Selector Attacks = new Selector("Attacks", 5);
        Attacks.AddChild(PhysicalHits);
        //Attacks.AddChild(RangedAttack);

        //

        // Move
        // Melee Distance
        Sequence LeapMelee = new Sequence("LeapMelee");
        LeapMelee.AddChild(new Leaf("inMeleeRange", new Condition(() => PlayerOutOfRange(meleeRange - 1f))));
        LeapMelee.AddChild(new Leaf("StartJumpingAnim", new AnimationWaitStrategy(animator, "JumpSlam", 0.74f)));
        LeapMelee.AddChild(new Leaf("NoTurn", new ActionStrategy(() => TurnOffProcedural())));
        LeapMelee.AddChild(new Leaf("MoveToMelee", new JumpOnPlayerStrategy(rb, player.transform, 2.91f)));
        LeapMelee.AddChild(new Leaf("WaitForJump", new WaitStrategy(3.02f)));
        LeapMelee.AddChild(new Leaf("Turn", new ActionStrategy(() => TurnONProcedural())));
        //
        // Ranged Distance
        Sequence RunRanged = new Sequence("RunRanged");
        RunRanged.AddChild(new Leaf("inRangedRange", new Condition(() => PlayerOutOfRange(rangedRange - 1f))));
        RunRanged.AddChild(new Leaf("StartWalkingAnim", new ActionStrategy(() => SetAnimTrigger("Walking"))));
        RunRanged.AddChild(new Leaf("MoveToRanged", new ChasePlayerStrategy(rb, player.transform, runSpeed, rangedRange - 1)));
        RunRanged.AddChild(new Leaf("StartWalkingAnim", new ActionStrategy(() => SetAnimTrigger("NotWalking"))));
        //

        RandomSelector Move = new RandomSelector("Move", 1);
        Move.AddChild(LeapMelee);
        //Move.AddChild(RunRanged);
        //

        // Main
        PrioritySelector MainNode = new PrioritySelector("MainNode", 100);

        MainNode.AddChild(Attacks);
        MainNode.AddChild(Move);
        //

        tree.AddChild(MainNode);

        // root - main - [Attacks, Move] - Attacks[PhysicalHits, RangedAttack] - PhysicalHits[LeftPunch, RightPunch]
        // Move[LeapMelee, RunRanged] 
    }

    void Update()
    {
        if (player != null && !isInteracting)
        {
            bool playerInRange = PlayerWithinRange(noticeDistance);
            bool alertedBoss = PlayerWithinRange(proximityDistance);

            if (!gettingUp && alertedBoss) // boss getting up 
            {
                StartCoroutine(GettingUpWait());
                gettingUp = true;
            }

            if (gotUp && playerInRange) // if boss is activated do boss things
            {
                tree.Process();
            }
        }
    }
    IEnumerator HandleInteraction()
    {
        isInteracting = true;
        rockBossHeadLook.enabled = false;
        yield return new WaitForSeconds(1f);
        rockBossHeadLook.enabled = true;
        isInteracting = false;
    }
    IEnumerator GettingUpWait()
    {
        isInteracting = true;
        TurnOffProcedural();
        animator.SetTrigger(playerNearParameter);
        yield return new WaitForSeconds(4.3f);
        TurnONProcedural();
        yield return new WaitForSeconds(3.3f);
        isInteracting = false;
        gotUp = true;
    }

    // turns on animation
    private void Attack(string trigger = "PunchLeft")
    {
        animator.SetTrigger(trigger);
    }

    // check if player within a range
    private bool PlayerWithinRange(float range)
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (distanceToPlayer <= range)
        {
            return true;
        }
        return false;
    }

    private bool PlayerOutOfRange(float range)
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (distanceToPlayer > range)
        {
            return true;
        }
        return false;
    }

    private void SetAnimTrigger(string trigger)
    {
        animator.SetTrigger(trigger);
    }
    private void SetAnimBoolTrue(string boolName)
    {
        animator.SetBool(boolName, true);
    }
    private void SetAnimBoolFalse(string boolName)
    {
        animator.SetBool(boolName, false);
    }

    private void TurnOffProcedural()
    {
        rockBossHeadLook.enabled = false;
    }
    private void TurnONProcedural()
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