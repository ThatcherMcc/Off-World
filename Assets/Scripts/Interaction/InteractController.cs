using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OffWorld.Anatomy;

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

    /// <summary>Combined mask so both items and enemy corpses are interactable via E key.</summary>
    private LayerMask CombinedInteractMask => InteractLayerMask | enemyInteractLayerMask;

    /// <summary>What the player is currently looking at for harvest prompt rendering.</summary>
    public CreatureCorpse LookedAtCorpse { get; private set; }
    /// <summary>Downed creature the player is looking at (for unified harvest menu).</summary>
    public EnemyHealth LookedAtDownedCreature { get; private set; }

    /// <summary>Base station the player is looking at (ExtractionChamber or SurgeryTable).</summary>
    public MonoBehaviour LookedAtBaseStation { get; private set; }

    private void UICheck()
    {
        LookedAtCorpse = null;
        LookedAtDownedCreature = null;
        LookedAtBaseStation = null;

        if (Physics.SphereCast(fpsCam.position, InteractRadius, fpsCam.forward, out RaycastHit raycastHit, InteractRange, CombinedInteractMask))
        {
            // Check for IInteractable (items, corpses, base stations)
            if (raycastHit.transform.TryGetComponent(out IInteractable _))
            {
                // Track corpse for custom prompt
                var corpse = raycastHit.transform.GetComponent<CreatureCorpse>();
                if (corpse != null)
                    LookedAtCorpse = corpse;

                // Track base stations for custom prompt
                var chamber = raycastHit.transform.GetComponent<ExtractionChamber>();
                var table = raycastHit.transform.GetComponent<SurgeryTable>();
                var terminal = raycastHit.transform.GetComponent<StorageTerminal>();
                if (chamber != null)
                    LookedAtBaseStation = chamber;
                else if (table != null)
                    LookedAtBaseStation = table;
                else if (terminal != null)
                    LookedAtBaseStation = terminal;

                // Hide default prompt for corpses and base stations (custom prompts handle them)
                bool hasCustomPrompt = corpse != null || LookedAtBaseStation != null;
                if (!chatting)
                    cg.alpha = hasCustomPrompt ? 0 : 1;
                else
                    cg.alpha = 0;
                return;
            }

            // Check for downed creature (unified harvest menu)
            var enemyHealth = raycastHit.transform.GetComponent<EnemyHealth>();
            if (enemyHealth == null)
                enemyHealth = raycastHit.transform.GetComponentInParent<EnemyHealth>();

            if (enemyHealth != null && enemyHealth.IsIncapacitated && !enemyHealth.HasBeenExtracted)
            {
                LookedAtDownedCreature = enemyHealth;
                cg.alpha = 0; // Hide default prompt, custom prompt handles it
                return;
            }

            cg.alpha = 0;
        }
        else
        {
            cg.alpha = 0;
        }
    }

    private void Interact()
    {
        if (Physics.SphereCast(fpsCam.position, InteractRadius, fpsCam.forward, out RaycastHit raycastHit, InteractRange, CombinedInteractMask))
        {
            // Standard IInteractable (items, corpses, NPCs)
            if (raycastHit.transform.TryGetComponent(out IInteractable newInteractable))
            {
                newInteractable.Interact(this);
                return;
            }

            // Incapacitated creature (no IInteractable, but alive and downed -> harvest choice)
            var enemyHealth = raycastHit.transform.GetComponent<EnemyHealth>();
            if (enemyHealth == null)
                enemyHealth = raycastHit.transform.GetComponentInParent<EnemyHealth>();

            if (enemyHealth != null && enemyHealth.IsIncapacitated && !enemyHealth.HasBeenExtracted)
            {
                var loot = enemyHealth.GetComponent<CreatureLootTable>();
                if (loot != null)
                {
                    var harvestUI = HarvestChoiceMenuUI.Instance;
                    if (harvestUI != null && !harvestUI.IsShowing)
                        harvestUI.Show(enemyHealth, loot);
                }
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
        IPowerItem powerItem = heldObject.GetComponent<IPowerItem>() as IPowerItem;
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
