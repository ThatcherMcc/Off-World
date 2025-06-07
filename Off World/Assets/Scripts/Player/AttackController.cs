using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackController : MonoBehaviour
{
    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (animator != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                animator.SetTrigger("SwipeRight");
            }
            if (Input.GetMouseButtonDown(1))
            {
                animator.SetTrigger("SwipeLeft");
            }
        }
    }
}
