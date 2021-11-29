using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealthController : MonoBehaviour
{
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private CircleCollider2D circleCollider;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject coinObject;
    [SerializeField] private int maxHealth = 1;

    private int health;

    private void Start()
    {
        health = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        health -= amount;

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Deactivate collider and sprite renderer
        if (boxCollider != null) { boxCollider.enabled = false; }
        else if (circleCollider != null) { circleCollider.enabled = false; }
        spriteRenderer.enabled = false;

        // Spawn in the coin
        Instantiate(coinObject, transform.position, transform.rotation);

        // Die
        Destroy(gameObject);
    }
}
