using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private GameObject shootEffectObject; // The object which shows where the player shot
    [SerializeField] private LayerMask shootLayerMask; // The Layer Mask for player shooting
    [SerializeField] private float shootDistance; // The distance the player can shoot
    [SerializeField] private float maxShootEnergy = 10f; // The max energy the player's gun can have
    [SerializeField] private float ShootEnergyPenalty = 2f; // The amount of energy the gun loses when shooting
    [SerializeField] private float ShootenergyRechargeRate = 0.1f; // The rate of recharging for the gun's energy

    private float shootEnergy; // The energy the player's gun has

    // Useful stuff
    private Vector2 groundVector; // Normalized vector representing the ground, which goes straight to the right

    private void Start()
    {
        shootEnergy = maxShootEnergy;
        groundVector = new Vector2(1, 0);
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
                print(hit.transform.name);
            }

            // Spawn in the shot effect line

            if (mousePosition.y < transform.position.y)
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
    /// This function keeps updating and increasing the gun's shooting energy and makes sure it never goes over maxEnergy and always increases at same speed
    /// </summary>
    private void HandleShootEnergy()
    {
        if (shootEnergy == maxShootEnergy)
            return;

        shootEnergy = Mathf.Min(shootEnergy + (ShootenergyRechargeRate * Time.deltaTime), maxShootEnergy);
    }
}
