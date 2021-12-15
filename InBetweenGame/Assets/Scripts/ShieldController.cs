using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldController : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    public PlayerCombat playerCombatScript;
    [SerializeField] private Rigidbody2D rigidBody;

    [SerializeField] private float diameter; // the diameter of the circle shield

    [SerializeField] private float movementSpeed; // How fast the shield is to catch up to the mouse pointer

    public float enemyKnockbackForce; // How much force the enemies are thrown back when colliding with the shield
    public float enemyParalyzationSeconds; // How long the enemies are paralyzed when being knocked back by the shield

    

    Vector3 mousePosition; // The vector which keeps track of the mouse's position
    Vector3 lookAtPlayerVector;

    private void Start()
    {
        SetScale();

        lookAtPlayerVector = Vector3.zero;
        mousePosition = Vector3.zero;
    }

    private void Update()
    {
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = transform.position.z;
        transform.position = mousePosition;
    }

    /// <summary>
    /// Sets the scale of the shield (its width and height)
    /// </summary>
    private void SetScale()
    {
        transform.localScale = new Vector3(diameter, diameter, 1);
    }

    //private void OnTriggerEnter2D(Collider2D collision)
    //{
    //    // If the shield collides with an enemy or projectile
    //    if (collision.transform.CompareTag("Enemy") || collision.transform.CompareTag("Projectile"))
    //    {
    //        playerCombatScript.OnShieldProtect(); // Alert the player that you were protecting them, you're very welcome player!
    //    }
    //}
}
