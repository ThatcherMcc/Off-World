using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationMethodsRockBoss : MonoBehaviour
{
    [Header("BACKSPIN facing sync")]
    [Tooltip("Seconds to wait after the animation event before flipping 180°. If your event is placed slightly before the last frame, set this so the flip lands when the anim ends (e.g. 0.08).")]
    [SerializeField] private float backspinFlipDelay = 0f;
    public GameObject rock;
    public Collider rpCollider;
    public Collider lpCollider;
    [Header("BACKSPIN (punish backside)")]
    [Tooltip("Hitbox behind boss; enable from BACKSPIN animation event.")]
    public Collider backspinCollider;
    [Header("SLAM (AOE)")]
    [Tooltip("AOE hitbox under boss; enable from SLAM animation event.")]
    public Collider slamCollider;

    private void Start()
    {
        if (lpCollider != null) lpCollider.enabled = false;
        if (rpCollider != null) rpCollider.enabled = false;
        if (backspinCollider != null) backspinCollider.enabled = false;
        if (slamCollider != null) slamCollider.enabled = false;
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

    public void ActivateBackspin() { if (backspinCollider != null) backspinCollider.enabled = true; }
    public void DeactivateBackspin() { if (backspinCollider != null) backspinCollider.enabled = false; }
    public void ActivateSlam() { if (slamCollider != null) slamCollider.enabled = true; }
    public void DeactivateSlam() { if (slamCollider != null) slamCollider.enabled = false; }

    /// <summary>Call from BACKSPIN animation event. Rotates root 180° so GameObject facing matches the model's end pose.
    /// If the boss lerps during the anim then snaps back: your BACKSPIN clip has root rotation. Remove root rotation from the clip (rotate only body bones) so the transform stays put during the anim; then this call flips it once at the end.</summary>
    public void SyncFacingAfterBackspin()
    {
        if (backspinFlipDelay <= 0f)
        {
            transform.Rotate(0f, 180f, 0f, Space.Self);
            return;
        }
        StartCoroutine(SyncFacingAfterBackspinDelayed());
    }

    private IEnumerator SyncFacingAfterBackspinDelayed()
    {
        yield return new WaitForSeconds(backspinFlipDelay);
        transform.Rotate(0f, 180f, 0f, Space.Self);
    }
}
