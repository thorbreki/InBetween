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

    [HideInInspector] public float gunEnergy; // The energy the player's gun has

    private bool gunCanShoot = true;


    private int health; // The player's health

    // Coroutines
    private Coroutine shieldCooldownCoroutine;

    // Useful stuff
    private Vector2 groundVector; // Normalized vector representing the ground, which goes straight to the right
    private Vector3 mousePosition; // Position of the mouse
    private Vector3 attackDirection; // direction of the player's attack

    private void Start()
    {
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
        HandleGunMode(); // Allow the player to switch between gun modes
    }

    /// <summary>
    /// This function handles the pistol attack mechanism of the player
    /// </summary>
    private void HandlePlayerPistolAttack()
    {
        // When player presses left mouse button, instantiate projectile
        if (Input.GetMouseButton(0))
        {
            if (!gunCanShoot)
                return; // You can't shoot if the gun hasn't reloaded

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

            StartCoroutine(GunCooldown());

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

    private IEnumerator GunCooldown()
    {
        gunCanShoot = false;
        yield return new WaitForSeconds(0.1f);
        gunCanShoot = true;
    }

    /// <summary>
    /// This function handles the player bomb attack functionality
    /// </summary>
    private void HandlePlayerBombAttack()
    {
        if (Input.GetMouseButtonDown(0))
        {
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
        hit.transform.GetComponent<EnemyHealthController>().TakeDamage(1f);
    }

    /// <summary>
    /// This function runs when an enemy hurts a player
    /// </summary>
    /// <param name="damage">The damage the player will endure</param>
    public void TakeDamage(int damage)
    {
        health = Mathf.Max(health - damage, 0);

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

    private void HandleShield()
    {
        // The player can only use the shield if:
        //     player is holding down the right mouse button
        //     the shield is not cooling down after using it last time
        //     the gun has energy left to spare on the shield
        if (Input.GetMouseButton(1))
        {
            // When the shield object becomes active again, teleport it to the mouse's position first
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = transform.position.z;
            shieldObject.transform.position = mousePosition;

            shieldObject.SetActive(true);
        }
        else
        {
            shieldObject.SetActive(false);
        }
    }
}
