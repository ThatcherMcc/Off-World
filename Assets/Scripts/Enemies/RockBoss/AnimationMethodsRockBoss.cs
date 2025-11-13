using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationMethodsRockBoss : MonoBehaviour
{
    public GameObject rock;
    public Collider rpCollider;
    public Collider lpCollider;

    private void Start()
    {
        if (lpCollider != null)
        {
            lpCollider.enabled = false;
        }
        if (rpCollider != null)
        {
            rpCollider.enabled = false;
        }
    }
    public void ActivateRightPunch()
    {
        rpCollider.enabled = true;
    }
    public void DeactivateRightPunch()
    {
        rpCollider.enabled = false;
    }
    public void ActivateLeftPunch()
    {
        lpCollider.enabled = true;
    }
    public void DeactivateLeftPunch()
    {
        lpCollider.enabled = false;
    }   

    public void LaunchRock()
    {
        Instantiate(rock, new Vector3(transform.position.x, transform.position.y + 5f, transform.position.z), transform.rotation);
    }
}
