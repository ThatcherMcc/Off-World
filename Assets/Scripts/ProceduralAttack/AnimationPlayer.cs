using UnityEditor.Animations;
using UnityEngine;

public class AnimationPlayer : MonoBehaviour
{
    [Header("Animation Settings")]
    public AnimationClip animationClip;
    public bool loopAnimation = true;

    [Header("HandIK")]
    public Transform targetObj;


    [Header("Debug Controls")]
    public bool enableDebugSpeedControl = false;
    [Range(0f, 2f)]
    public float animationSpeed = 1.0f; // Default speed is 1 (normal)

    private Animator _animator;
    private bool _isInitialized = false;

    void Awake()
    {
        _animator = GetComponent<Animator>();

        if (_animator == null)
        {
            Debug.LogError("AnimationPlayer: No Animator component found on this GameObject. Please add an Animator.", this);
            enabled = false; // Disable this script if no Animator
            return;
        }
        if (animationClip == null)
        {
            Debug.LogWarning("AnimationPlayer: No Animation Clip assigned. Please assign an AnimationClip in the Inspector.", this);
            return; // Don't try to play if no clip is assigned
        }

        AnimatorOverrideController overrideController = new AnimatorOverrideController(_animator.runtimeAnimatorController);

        if (_animator.runtimeAnimatorController == null)
        {
            Debug.Log("AnimationPlayer: No Animator Controller found. Creating a simple one for direct clip playback.", this);
            _animator.runtimeAnimatorController = CreateSimpleAnimatorController();
        }

        if (animationClip != null)
        {
            if (_animator.runtimeAnimatorController != null)
            {
                _animator.Play(animationClip.name);
                _isInitialized = true;
            }
        }
    }

    void Update()
    {
        if (!_isInitialized || _animator == null) return;

        if (enableDebugSpeedControl)
        {
            _animator.speed = animationSpeed;
        }
        else
        {
            if (_animator.speed != 1.0f)
            {
                _animator.speed = 1.0f;
            }
        }
    }

    // Helper to create a very basic Animator Controller if one isn't assigned
    private RuntimeAnimatorController CreateSimpleAnimatorController()
    {
        AnimatorController controller = new AnimatorController();
        controller.AddLayer("Base Layer");
        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        AnimatorState state = stateMachine.AddState("DefaultAnimation"); // Add a state to play the clip in
        state.motion = animationClip; // Assign the clip to the state
        state.name = animationClip.name; // Use clip name as state name for easy reference
        state.cycleOffset = loopAnimation ? 0 : 1; // Basic loop control if set directly on state
        state.writeDefaultValues = false; // Important for procedural animation to not overwrite

        return controller;
    }
}