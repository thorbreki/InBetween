using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovementScript; // The player movement script attached to the player
    [SerializeField] private GameObject shootEffectObject; // The object which shows where the player shot
    [SerializeField] private LayerMask shootLayerMask; // The Layer Mask for player shooting
    [SerializeField] private float shootDistance; // The distance the player can shoot
    [SerializeField] public float maxShootEnergy = 10f; // The max energy the player's gun can have
    [SerializeField] private float ShootEnergyPenalty = 2f; // The amount of energy the gun loses when shooting
    [SerializeField] private float ShootenergyRechargeRate = 0.1f; // The rate of recharging for the gun's energy
    [SerializeField] private int maxHealth = 1; // The player's possible max health
    [SerializeField] private HealthBarController healthBarController; // The script for the health bar

    public float shootEnergy; // The energy the player's gun has

    private int health; // The player's health

    // Useful stuff
    private Vector2 groundVector; // Normalized vector representing the ground, which goes straight to the right

    private void Start()
    {
        shootEnergy = maxShootEnergy;
        groundVector = new Vector2(1, 0);
        health = maxHealth;
    }

    private void Update()
    {
        HandlePlayerAttack();
        HandleShootEnergy();
    }

    /// <summary>
    /// This function handles the attack mechanism of the player
    /// </summary>
    private void HandlePlayerAttack()
    {
        // When player presses left mouse button, instantiate projectile
        if (Input.GetMouseButtonDown(0))
        {
            if (shootEnergy < ShootEnergyPenalty)
                return; // You don't want to do anything if the player doesn't have enough energy to shoot

            shootEnergy -= ShootEnergyPenalty; // When player shoots, always loses energy

            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 attackDirection = Quaternion.Euler(0f, 0f, Random.Range(-20, 21)) * (mousePosition - transform.position);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, attackDirection, shootDistance, shootLayerMask);

            if (hit.collider) {

                // If shot hits enemy, then damage that enemy
                if (hit.transform.tag == "Enemy")
                {
                    DamageEnemy(hit.transform);
                }
            }

            // Spawn in the shot effect line

            if (attackDirection.y < transform.position.y)
            {
                Instantiate(shootEffectObject, transform.position, Quaternion.Euler(0, 0, -Vector2.Angle(attackDirection, groundVector)));
            }
            else
            {
                Instantiate(shootEffectObject, transform.position, Quaternion.Euler(0, 0, Vector2.Angle(attackDirection, groundVector)));
            }
        }
    }


    /// <summary>
    /// Damage enemy with player's damage
    /// </summary>
    private void DamageEnemy(Transform hit)
    {
        hit.transform.GetComponent<EnemyHealthController>().TakeDamage(1);
    }

    /// <summary>
    /// This function keeps updating and increasing the gun's shooting energy and makes sure it never goes over maxEnergy and always increases at same speed
    /// </summary>
    private void HandleShootEnergy()
    {
        if (shootEnergy == maxShootEnergy)
            return;

        shootEnergy = Mathf.Min(shootEnergy + (ShootenergyRechargeRate * Time.deltaTime), maxShootEnergy);
    }

    /// <summary>
    /// This function runs when an enemy hurts a player
    /// </summary>
    /// <param name="damage">The damage the player will endure</param>
    private void TakeDamage(int damage)
    {
        health = Mathf.Max(health - damage, 0);
        healthBarController.SetHealthBarRatio((float)health / (float)maxHealth);

        if (health == 0)
        {
            GameManager.instance.PlayerDied();
        }
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Enemy"))
        {
            TakeDamage(1);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Projectile"))
        {
            TakeDamage(1);
        }
    }
}
