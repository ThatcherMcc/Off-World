using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterInteraction : MonoBehaviour
{
    private GameObject player;
    private Animator animator;
    private RockBossController rockBossController;

    public string playerNearParameter = "StartShake";
    public float proximityDistance = 10f;
    public string animationStateName = "Shake"; // Name of the animation clip
    public int layerIndex = 0; // Assuming it's on base layer
    private bool gotUp = false;

    public bool isInteracting = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        rockBossController = GetComponent<RockBossController>();
        player = GameObject.FindGameObjectWithTag("Player");
    }

    void Update()
    {
        if (player != null && !isInteracting)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);

            if (!gotUp && distanceToPlayer <= proximityDistance) // boss getting up 
            {
                StartCoroutine(GettingUpWait());
                gotUp = true;
            }
        }
    }

    IEnumerator HandleInteraction()
    {
        isInteracting = true;
        rockBossController.enabled = false;
        // animator.SetTrigger(playerNearParameter);

        // Wait until the animation actually starts
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(layerIndex).IsName(animationStateName));
        // Wait until the animation finishes
        yield return new WaitUntil(() =>
            animator.GetCurrentAnimatorStateInfo(layerIndex).IsName(animationStateName) &&
            animator.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime >= 2f);

        rockBossController.enabled = true;
        isInteracting = false;
    }

    IEnumerator GettingUpWait()
    {
        isInteracting = true;
        rockBossController.enabled = false;
        animator.SetTrigger(playerNearParameter);
        yield return new WaitForSeconds(4f);
        rockBossController.enabled = true;
        isInteracting = false;
    }
}