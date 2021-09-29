using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private GameObject shootEffectObject; // The object which shows where the player shot
    [SerializeField] private LayerMask shootLayerMask; // The Layer Mask for player shooting
    [SerializeField] private float shootDistance; // The distance the player can shoot

    private Vector2 groundVector; // Normalized vector representing the ground, which goes straight to the right

    private void Start()
    {
        groundVector = new Vector2(1, 0);
    }

    private void Update()
    {
        HandlePlayerAttack();
    }

    /// <summary>
    /// This function handles the attack mechanism of the player
    /// </summary>
    private void HandlePlayerAttack()
    {
        // When player presses left mouse button, instantiate projectile
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector3 attackDirection = Quaternion.Euler(0f, 0f, Random.Range(-20, 21)) * (mousePosition - transform.position);
            RaycastHit2D hit = Physics2D.Raycast(transform.position, attackDirection, shootDistance, shootLayerMask);



            if (hit.collider) {
                print(hit.transform.name);
            }

            // Spawn in the shot effect line

            if (transform.position.y > attackDirection.y)
            {
                Instantiate(shootEffectObject, transform.position, Quaternion.Euler(0, 0, -Vector2.Angle(attackDirection, groundVector)));
            }
            else
            {
                Instantiate(shootEffectObject, transform.position, Quaternion.Euler(0, 0, Vector2.Angle(attackDirection, groundVector)));
            }
            
        }
    }
}
