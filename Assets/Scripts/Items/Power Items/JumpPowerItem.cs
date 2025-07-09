using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpPowerItem : MonoBehaviour, PowerItemI
{
    private GameObject player;
    private PlayerMovement playerMovement;

    [Header("Powerup")]
    [SerializeField] private float jumpBoost = 2;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerMovement = player.GetComponent<PlayerMovement>();

    }

    public void Eat()
    {
        if (playerMovement != null)
        {
            playerMovement.jumpForce *= jumpBoost;

            Destroy(gameObject);
        }
    }
}
