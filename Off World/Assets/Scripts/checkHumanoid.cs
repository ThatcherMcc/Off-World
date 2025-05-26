using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkHumanoid : MonoBehaviour
{
    void Start()
    {
        CheckIfHumanoid();
    }

    void CheckIfHumanoid()
    {
        Animator animator = GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogWarning(gameObject.name + ": No Animator component found. Not a humanoid (or no animation setup).", this);
            return;
        }

        if (animator.avatar == null)
        {
            Debug.LogWarning(gameObject.name + ": Animator has no Avatar assigned. Not a humanoid (or misconfigured).", this);
            return;
        }

        if (animator.avatar.isHuman)
        {
            Debug.Log(gameObject.name + ": **This GameObject is rigged as a Humanoid!**", this);
        }
        else
        {
            Debug.Log(gameObject.name + ": This GameObject is **NOT** rigged as a Humanoid (it's either Generic or Legacy).", this);
        }
    }
}

