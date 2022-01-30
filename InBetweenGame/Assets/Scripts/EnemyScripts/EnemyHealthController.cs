using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnemyHealthController : MonoBehaviour
{
    [Header("Colliders")]
    [SerializeField] private BoxCollider2D boxCollider;
    [SerializeField] private CircleCollider2D circleCollider;
    
    [Header("Combonents and Objects")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private GameObject coinObject;
    [SerializeField] private GameObject damageIndicatorPrefab;
                     private TextMeshPro damageIndicatorText;
                     private GameObject damageIndicatorObject;

    [Header("Health Attributes")]
    [SerializeField] private int maxHealth = 1;

    private Vector3 damageIndicatorPosition;

    private int health;
    private Coroutine showDamageCoroutine;

    private void Start()
    {
        health = maxHealth;
        damageIndicatorPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z - 0.1f);

        // Get the text component of the damage indicator object
        damageIndicatorObject = Instantiate(damageIndicatorPrefab);
        damageIndicatorObject.transform.position = damageIndicatorPosition;
        damageIndicatorText = damageIndicatorObject.GetComponent<TextMeshPro>();
    }

    private void Update()
    {
        // Position damage indicator respectively
        damageIndicatorPosition.x = transform.position.x; damageIndicatorPosition.y = transform.position.y + 1;
        damageIndicatorObject.transform.position = damageIndicatorPosition;
    }
    public void TakeDamage(int amount)
    {
        health -= amount;

        if (showDamageCoroutine != null) { StopCoroutine(showDamageCoroutine); }
        showDamageCoroutine = StartCoroutine(ShowDamageCor(amount));

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Deactivate collider and sprite renderer
        if (boxCollider != null)
        {
            boxCollider.enabled = false;
        }
        else if (circleCollider != null)
        {
            circleCollider.enabled = false;
        }
        spriteRenderer.enabled = false;

        // Remove the Damage Indicator
        Destroy(damageIndicatorObject);

        // Spawn in the coin
        Instantiate(coinObject, transform.position, transform.rotation);

        // Die
        Destroy(gameObject);
    }

    private IEnumerator ShowDamageCor(int amount)
    {
        damageIndicatorText.text = "-" + amount.ToString();
        damageIndicatorObject.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        damageIndicatorObject.SetActive(false);
    }


    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
