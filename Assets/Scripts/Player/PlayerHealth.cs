using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public Healthbar healthbar;
    [SerializeField] private CanvasGroup cg;
    public int maxHealth = 100;
    private int health;
    private Vector3 respawnPosition;
    private bool immune;
    private readonly float immunityDuration = 1f;


    private void Start()
    {
        health = maxHealth;
        if (healthbar == null)
        {
            healthbar = GameObject.FindGameObjectWithTag("Healthbar").GetComponent<Healthbar>();
        }
        if (cg == null)
        {
            cg = GameObject.FindGameObjectWithTag("DeathScreen").GetComponent<CanvasGroup>();
        }
        cg.alpha = 0;
        respawnPosition = transform.position;
        immune = false;
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

    public void StartImmunity()
    {
        immune = true;
    }
    public void EndImmunity()
    {
        immune = false;
    }

    private void Die()
    {
        cg.alpha = 1;
        transform.position = respawnPosition;
        Invoke("ResetDead", 3);
    }

    private void ResetDead()
    {
        cg.alpha = 0;
        PlayerHeal(1000);
    }
}
