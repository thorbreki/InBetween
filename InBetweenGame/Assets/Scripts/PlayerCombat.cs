using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private GameObject projectileObject;

    private Quaternion projectileStartingRotation; // The vector for the rotation of the projectiles when spawned

    private void Start()
    {
        projectileStartingRotation = Quaternion.identity;
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
            Instantiate(projectileObject, transform.position, projectileStartingRotation);
        }
    }
}
