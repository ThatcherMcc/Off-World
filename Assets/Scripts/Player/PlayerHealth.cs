using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public Healthbar healthbar;
    public int maxHealth = 100;
    [SerializeField] private GameObject deathScreen;
    private CanvasGroup cg;
    private int health;
    private bool dead;
    private Vector3 respawnPosition;
    private bool immune;
    private float immunityDuration = 1f;


    private void Start()
    {
        health = maxHealth;
        healthbar = GameObject.FindGameObjectWithTag("Healthbar").GetComponent<Healthbar>();
        cg = GameObject.FindGameObjectWithTag("DeathScreen").GetComponent<CanvasGroup>();
        cg.alpha = 0;
        respawnPosition = transform.position;
        immune = false;
    }

    private void Update()
    {
        if (dead)
        {
            transform.position = respawnPosition;
        }
        if (immune == true)
        {

        }
    }

    public void PlayerHeal(int healing)
    {
        health += healing;
        healthbar.SetHealth(health);

        if (health > 100)
        {
            health = maxHealth;
        }
    }

    public void PlayerTakeDMG(int damage)
    {
        if ( immune == false )
        {
            immune = true; 
            health -= damage;
            healthbar.SetHealth(health);

            Invoke("EndImmunity", immunityDuration);

            if (health <= 0)
            {
                health = 0;
                Die();
            }
        }
    }

    private void EndImmunity()
    {
        immune = false;
    }

    private void Die()
    {
        cg.alpha = 1;
        dead = true;
        transform.position = respawnPosition;
        Invoke("ResetDead", 3);
    }

    private void ResetDead()
    {
        dead = false;
        cg.alpha = 0;
        PlayerHeal(1000);
    }
}
