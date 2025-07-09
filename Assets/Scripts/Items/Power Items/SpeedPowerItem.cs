using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedPowerItem : MonoBehaviour, PowerItemI
{
    private GameObject player;
    private PlayerMovement playerMovement;

    [Header("Powerup")]
    [SerializeField] private float speedBoost = 2;

    void Awake()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerMovement = player.GetComponent<PlayerMovement>();
        
    }

    public void Eat()
    {
        if (playerMovement != null)
        {
            playerMovement.walkSpeed = playerMovement.walkSpeed * speedBoost;
            playerMovement.sprintSpeed = playerMovement.sprintSpeed * speedBoost;

            Destroy(gameObject);
        }
    }
}
