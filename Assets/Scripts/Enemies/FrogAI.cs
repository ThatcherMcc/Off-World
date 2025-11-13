using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements.Experimental;

public class FrogAI : MonoBehaviour, IEnemy
{
    [Header("LOS")]
    [SerializeField] private float noticeRadius;
    private float runningTimer;

    public Transform player { get; set; }

    [Header("Enemy Idle")]
    private Vector3 mainIdlePos;
    [SerializeField] private float moveDuration;
    [SerializeField] private float maxWanderDist;
    [SerializeField] private float jumpForce;
    private float minWanderDist;

    private float idleSpeed;
    private float idleMovingTimer = 0;
    private float idleBreakTimer = 0;

    public float enemySpeed;

    private float runHopTimer = 0;

    private bool returningToStart;

    // Interface IEnemy
    public void EnableAI(bool enable)
    {
        aiEnabled = enable;
    }
    private bool aiEnabled = true;


    private Vector3 idleDirection;
    Rigidbody rb;

    // Start is called before the first frame update
    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody>();
        mainIdlePos = transform.position;
        idleSpeed = enemySpeed / 2;
        minWanderDist = maxWanderDist / 3;
    }

    // Update is called once per frame
    void Update()
    {
        if (aiEnabled)
        {
            if (InLineOfSight())
            {
                RunFromPlayer();
                // sets a timer to continue following for a second without LOS
                if (runningTimer > 0)
                {
                    runningTimer -= Time.deltaTime;
                }
            }
            else
            {
                Idle();
            }
        }
        else
        {
            Vector3 direction = (player.position - transform.position).normalized;
            rb.velocity = Vector3.zero;
            Quaternion rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = rotation;
        }
    }

    private bool InLineOfSight()
    {
        Vector3 direction = (player.position - transform.position).normalized;

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, noticeRadius))
        {
            if (hit.transform.gameObject.CompareTag("Player"))
            {
                runningTimer = 2;
                return true;
            }
        }
        return false;
    }

    private void RunFromPlayer()
    {
        Debug.Log("Running");
        Vector3 direction = (transform.position - player.position).normalized;

        if (runHopTimer <= 0)
        {
            Move(direction, enemySpeed);
            Hop(direction);
            runHopTimer = 1;
        }
        else
        {
            runHopTimer -= Time.deltaTime;
        }
        RotateMob(direction);
    }

    private void Idle()
    {
        isReturning();
        if (!returningToStart)
        {
            IdleWander();
        }
        else
        {
            ReturnToStart();
        }
    }

    private void isReturning()
    {
        if (Vector3.Distance(transform.position, mainIdlePos) > maxWanderDist)
        {
            returningToStart = true;
        }
        else if (Vector3.Distance(transform.position, mainIdlePos) < minWanderDist)
        {
            returningToStart = false;
        }
    }

    private void ReturnToStart()
    {
        Vector3 direction = (mainIdlePos - transform.position).normalized;
        if (idleBreakTimer <= 0)
        {
            Move(direction, idleSpeed);
            Hop(direction);
            idleBreakTimer = 2.5f;
        }
        else
        {
            idleBreakTimer -= Time.deltaTime;
        }
        RotateMob(direction);
    }

    private void IdleWander()
    {
        // Not resting or currently moving? Try moving!
        if (idleBreakTimer <= 0)
        {
            // Timer to make sure hes grounded before jumping
            if (idleMovingTimer <= 0)
            {
                GetIdleDirection();
                idleMovingTimer = moveDuration;
            }
            else
            {
                Move(idleDirection, idleSpeed);
                Hop(idleDirection);
                Debug.Log("IDLE");
                idleBreakTimer = 3;
                
            }
        } else
        {
            RotateMob(idleDirection);
            idleBreakTimer -= Time.deltaTime;
            idleMovingTimer -= Time.deltaTime;
        }
        
    }

    private void GetIdleDirection()
    {
        float angle = Random.Range(0f, 360);
        Vector3 randomDirection = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)).normalized;

        idleDirection = randomDirection;
    }

    private void Move(Vector3 direction, float newSpeed)
    {
        rb.MovePosition(transform.position + direction * newSpeed * Time.deltaTime);
    }

    private void RotateMob(Vector3 direction)
    {
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, enemySpeed * Time.deltaTime);
    }

    private void Hop(Vector3 direction)
    {
        rb.AddForce(new Vector3(direction.x * .5f, 1, direction.z * .5f) * jumpForce, ForceMode.Impulse);
        Debug.Log("JUMPING");
    }
}
