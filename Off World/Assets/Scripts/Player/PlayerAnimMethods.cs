using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimMethods : MonoBehaviour
{
    public Collider rightArmCollider;
    public Collider leftArmCollider;

    private void Start()
    {
        if (rightArmCollider != null)
        {
            rightArmCollider.enabled = false;
        }
        if (leftArmCollider != null)
        {
            leftArmCollider.enabled = false;
        }
    }
    public void TurnOnRightArmHurtBox()
    {
        rightArmCollider.enabled = true;
    }
    public void TurnOffRightArmHurtBox()
    {
        rightArmCollider.enabled = false;
    }
    public void TurnOnLeftArmHurtBox()
    {
        leftArmCollider.enabled = true;
    }
    public void TurnOffLeftArmHurtBox()
    {
        leftArmCollider.enabled = false;
    }
}
