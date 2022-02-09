using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Components and Objects")]
    [SerializeField] private PlayerMovement playerMovementScript; // The player movement script attached to the player
    [SerializeField] private GameObject shootEffectObject; // The object which shows where the player shot
    [SerializeField] private GameObject shieldObject; // The shield object that protects the player
    [SerializeField] private CanvasFunctions canvasFunctionsScript; // The script for the canvas functions
    [SerializeField] private GameObject bombPrefab; // The bomb prefab the player can shoot from the gun

    [Header("Shooting Attributes")]
    [SerializeField] private LayerMask shootLayerMask; // The Layer Mask for player shooting
    [SerializeField] private float shootDistance; // The distance the player can shoot
    [SerializeField] public float maxGunEnergy = 10f; // The max energy the player's gun can have
    [SerializeField] private float PistolEnergyPenalty = 2f; // The amount of energy the gun loses when shooting in pistol mode
    [SerializeField] private float BombEnergyPenalty = 0f; // The amount of energy the gun loses when shooting in bomb mode
    [SerializeField] private float gunEnergyRechargeRate = 0.1f; // The rate of recharging for the gun's energy
    [SerializeField] private float shootingInaccuracy = 0.0f; // How inaccurate the player's shooting is

    [Header("Bomb Attributes")]
    public float explosionRadius;
    public float explosionTimeToLive;
    public float explosionParalyzationSeconds; // How long the bomb paralyzes the enemies
    public float explosionMaxForce;

    private enum GunMode
    {
        Pistol,
        Bomb
    }
    private GunMode gunMode; // The mode the gun is in 

    [Header("Health")]
    [SerializeField] private int maxHealth = 1; // The player's possible max health
    [SerializeField] private HealthBarController healthBarController; // The script for the health bar

    [Header("Shields Attributes")]
    [SerializeField] private float coolDownSeconds; // How much time (in seconds) does the shield need to be used again
    [SerializeField] private float shieldEnergyDepletionRate; // How much energy the shield uses while being active
    [SerializeField] private float shieldProtectionEnergyPenalty; // How much energy the shield uses when pushing back enemies and destroying projectiles


    private bool shieldCoolDownActive = false; // cooldown for the shield

    [HideInInspector] public float gunEnergy; // The energy the player's gun has


    private int health; // The player's health

    // Coroutines
    private Coroutine shieldCooldownCoroutine;

    // Useful stuff
    private Vector2 groundVector; // Normalized vector representing the ground, which goes straight to the right
    private Vector3 mousePosition; // Position of the mouse
    private Vector3 attackDirection; // direction of the player's attack

    private void Start()
    {
        gunEnergy = maxGunEnergy;
        health = maxHealth;
        shieldObject.SetActive(false); // Shield is always inactive in the beginning

        // Initialize vectors
        groundVector = new Vector2(1, 0);
        mousePosition = Vector3.zero;
        attackDirection = Vector3.zero;
    }

    private void Update()
    {
        switch (gunMode)
        {
            case GunMode.Pistol:
                HandlePlayerPistolAttack();
                break;
            case GunMode.Bomb:
                HandlePlayerBombAttack();
                break;
            default:
                break;
        }
        HandleShield();
        HandleGunEnergy(); // Handle the regeneration of the gun's energy if and only if no active abilities are turned on (for instance, using the shield)
        HandleGunMode(); // Allow the player to switch between gun modes
    }

    /// <summary>
    /// This function handles the pistol attack mechanism of the player
    /// </summary>
    private void HandlePlayerPistolAttack()
    {
        // When player presses left mouse button, instantiate projectile
        if (Input.GetMouseButtonDown(0))
        {
            if (gunEnergy < PistolEnergyPenalty)
                return; // You don't want to do anything if the player doesn't have enough energy to shoot

            gunEnergy -= PistolEnergyPenalty; // When player shoots, always loses energy

            mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            attackDirection = Quaternion.Euler(0f, 0f, Random.Range(-shootingInaccuracy, shootingInaccuracy)) * (mousePosition - transform.position);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, attackDirection, shootDistance, shootLayerMask);

            if (hit.collider) {
                //print(hit.collider.transform.name);

                // If shot hits enemy, then damage that enemy
                if (hit.transform.tag == "Enemy" || hit.transform.tag == "PlayerShield" || hit.transform.tag == "PlayerShield2")
                {
                    DamageEnemy(hit.transform);
                }
            }

            // Spawn in the shot effect line
            //print(attackDirection);
            if (attackDirection.y < 0f)
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
    /// This function handles the player bomb attack functionality
    /// </summary>
    private void HandlePlayerBombAttack()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (gunEnergy < BombEnergyPenalty) { return; } // You don't want to do anything if the player doesn't have enough energy to shoot bombs

            gunEnergy -= BombEnergyPenalty; // When player shoots, always lose energy

            mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            attackDirection = Quaternion.Euler(0f, 0f, Random.Range(-shootingInaccuracy, shootingInaccuracy)) * (mousePosition - transform.position);

            // Spawn in Bomb object
            GameObject bomb = Instantiate(bombPrefab, transform.position, transform.rotation);
            bomb.GetComponent<BombController>().StartBomb(transform.position, mousePosition);
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
    /// This function keeps updating and increasing the gun's energy and makes sure it never goes over maxEnergy and always increases at same speed
    /// </summary>
    private void HandleGunEnergy()
    {
        // While the shield is active, the gun can't recharge it's energy levels and loses energy
        if (shieldObject.activeSelf)
        {
            gunEnergy = Mathf.Max(gunEnergy - (shieldEnergyDepletionRate * Time.deltaTime), 0f);
            return;
        } 
        if (gunEnergy == maxGunEnergy) { return; } // If the gun's energy is already at maximum, no need to deal with anything in this function

        gunEnergy = Mathf.Min(gunEnergy + (gunEnergyRechargeRate * Time.deltaTime), maxGunEnergy); // Increase the gun's energy and cap at max energy
    }

    /// <summary>
    /// This function runs when an enemy hurts a player
    /// </summary>
    /// <param name="damage">The damage the player will endure</param>
    public void TakeDamage(int damage)
    {
        health = Mathf.Max(health - damage, 0);
        healthBarController.SetHealthBarRatio((float)health / (float)maxHealth);

        if (health == 0)
        {
            GameManager.instance.PlayerDied();
        }
    }

    /// <summary>
    /// Handle the player wanting to change between gun modes
    /// </summary>
    private void HandleGunMode()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) { ChangeGunMode(GunMode.Pistol); }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) { ChangeGunMode(GunMode.Bomb); }
    }

    private void ChangeGunMode(GunMode newGunMode)
    {
        gunMode = newGunMode;
        canvasFunctionsScript.ChangeGunModeText(newGunMode.ToString());

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Projectile"))
        {
            TakeDamage(1);
        }
    }


    // TODO: ADD COOLDOWN FUNCTIONALITY FOR THE GUN

    // ---------- SHIELD

    private void HandleShield()
    {
        // The player can only use the shield if:
        //     player is holding down the right mouse button
        //     the shield is not cooling down after using it last time
        //     the gun has energy left to spare on the shield
        if (Input.GetMouseButtonDown(1) && !shieldCoolDownActive && gunEnergy > 0f)
        {
            // When the shield object becomes active again, teleport it to the mouse's position first
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = transform.position.z;
            shieldObject.transform.position = mousePosition;

            shieldObject.SetActive(true);
        }
        else if (Input.GetMouseButtonUp(1))
        {
            if (shieldObject.activeSelf == false) { return; } // If the shield was never activated, do nothing
            shieldObject.SetActive(false);
            if (shieldCooldownCoroutine != null) { return; } // If the coroutine is already running, we do nothing
            shieldCooldownCoroutine = StartCoroutine(CoolDownShieldCor()); // Start shield cooldown!
        }
        else if (shieldObject.activeSelf && gunEnergy == 0) // Player is trying to use the shield but there is no energy left
        {
            shieldObject.SetActive(false); // Turn off the shield!
            if (shieldCooldownCoroutine != null) { return; } // If the coroutine is already running, we do nothing
            shieldCooldownCoroutine = StartCoroutine(CoolDownShieldCor()); // Start shield cooldown!
        }
    }

    /// <summary>
    /// This function is called when the shield protects player from enemy or projectile, thus suffering the protection penalty
    /// </summary>
    public void OnShieldProtect()
    {
        gunEnergy = Mathf.Max(gunEnergy - shieldProtectionEnergyPenalty, 0f);
    }

    /// <summary>
    /// Controls the cool down of the shield, blocks the shield from being fired up again for a short while
    /// </summary>
    /// <returns></returns>
    private IEnumerator CoolDownShieldCor()
    {
        shieldCoolDownActive = true;
        yield return new WaitForSeconds(coolDownSeconds);
        shieldCoolDownActive = false;
        shieldCooldownCoroutine = null; // Just in case
    }
}
