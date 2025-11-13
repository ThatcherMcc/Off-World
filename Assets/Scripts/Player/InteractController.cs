using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractController : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Transform fpsCam;
    public Transform objectGrabPointTransform;
    public Transform objectGrabPointOffHandTransform;

    [Header("Type2Grab")]
    [SerializeField] private LayerMask InteractLayerMask;
    [SerializeField] private LayerMask enemyInteractLayerMask;

    [Header("Properties")]
    public float InteractRange = 3f;
    public float InteractRadius = .5f;
    public float dropForwardForce, dropUpwardForce;

    [Header("Equip Status")]
    public bool isEquipped = false;
    public bool isEquippedOffHand = false;

    [Header("KeyBinds")]
    public KeyCode interact;
    public KeyCode drop;
    public KeyCode eat;

    public GameObject heldObject;
    private GameObject heldObjectOffHand;

    private EnemyGrabbable enemyGrabbable;

    private GameObject UIInteract;
    private CanvasGroup cg;
    public bool chatting;

    private void Start()
    {
        UIInteract = GameObject.FindGameObjectWithTag("InteractText");
        if (UIInteract != null)
        {
            UIInteract.SetActive(true);
        }
        cg = UIInteract.GetComponent<CanvasGroup>();
    }

    void Update()
    {
        UICheck();
        // Interact and Drop
        if (Input.GetKeyDown(interact))
        {
            Interact();
        }
        if (Input.GetKeyDown(drop) && isEquipped)
        {
            Drop();
        }
        if (Input.GetKeyDown(eat) && isEquipped && heldObject != null)
        {
            Eat();
        }
        if (Input.GetMouseButtonDown(0))
        {
            UseAction();
        }
        if (Input.GetMouseButtonDown(1))
        {
            UseAltAction();
        }

    }

    private void UICheck()
    {
        if (Physics.SphereCast(fpsCam.position, InteractRadius, fpsCam.forward, out RaycastHit raycastHit, InteractRange, InteractLayerMask))
        {
            if (raycastHit.transform.TryGetComponent(out InteractableI newInteractable))
            {
                if (!chatting)
                {
                    cg.alpha = 1;
                } else
                {
                    cg.alpha = 0;  
                }
            }
            else
            {
                cg.alpha = 0;
            }
        }
        else
        {
            cg.alpha = 0;
        }
    }

    private void Interact()
    {
        if (Physics.SphereCast(fpsCam.position, InteractRadius, fpsCam.forward, out RaycastHit raycastHit, InteractRange, InteractLayerMask))
        {
            if (raycastHit.transform.TryGetComponent(out InteractableI newInteractable))
            {
                newInteractable.Interact(this);
            }
        }
    }

    private void Drop()
    {
        Rigidbody rb = heldObject.GetComponent<Rigidbody>();

        if (heldObject.GetComponent<ObjectGrabbable>())
        {
            isEquipped = false;
            heldObject.GetComponent<ObjectGrabbable>().Drop();
        }

        rb.velocity = fpsCam.gameObject.GetComponentInParent<Rigidbody>().velocity;
        rb.AddForce(fpsCam.forward * dropForwardForce, ForceMode.Impulse);
        rb.AddForce(fpsCam.up * dropUpwardForce, ForceMode.Impulse);

        heldObject = null;
    }

    private void Eat()
    {
        PowerItemI powerItem = heldObject.GetComponent<PowerItemI>() as PowerItemI;
        if (powerItem != null)
        {
            powerItem.Eat();
            isEquipped = false;
            heldObject = null;
        }
    }

    private void UseAction()
    {
        if (isEquipped && heldObject.GetComponent<NetScript>())
        {
            if (Physics.SphereCast(fpsCam.position, InteractRadius, fpsCam.forward, out RaycastHit raycastHit, InteractRange, enemyInteractLayerMask))
            {
                if (raycastHit.transform.TryGetComponent(out EnemyGrabbable newEnemyGrabbable))
                {
                    isEquippedOffHand = true;
                    enemyGrabbable = newEnemyGrabbable;
                    heldObjectOffHand = enemyGrabbable.gameObject;
                    enemyGrabbable.Capture(objectGrabPointOffHandTransform);
                }
            }
        }
    }

    private void UseAltAction()
    {
        if (isEquippedOffHand)
        {
            isEquippedOffHand = false;
            enemyGrabbable.Release();
            enemyGrabbable = null;
            heldObjectOffHand = null;
        }
    }

    private void HandleDialogueStateChanged(bool isDialogueActive)
    {
        cg.alpha = isDialogueActive ? 0 : 1;
    }
}
