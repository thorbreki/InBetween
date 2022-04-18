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
    [SerializeField] private TextMeshPro numberOfEnemiesLeftText;
                     private TextMeshPro damageIndicatorText;
                     private GameObject damageIndicatorObject;

    [Header("Health Attributes")]
    [SerializeField] private int maxHealth = 1; // This is the base level of health the enemy can have

    private Vector3 damageIndicatorPosition;

    private float health;
    private Coroutine showDamageCoroutine;

    private void Start()
    {
        health = maxHealth + ApplicationManager.instance.currLevelData.enemyHealthBoost;
        damageIndicatorPosition = new Vector3(transform.position.x, transform.position.y, transform.position.z - 0.1f);

        // Get the text component of the damage indicator object
        damageIndicatorObject = Instantiate(damageIndicatorPrefab);
        damageIndicatorObject.transform.position = damageIndicatorPosition;
        damageIndicatorText = damageIndicatorObject.GetComponent<TextMeshPro>();
    }

    private void Update()
    {
        // Position damage indicator respectively
        if (damageIndicatorObject == null) { return; }
        damageIndicatorPosition.x = transform.position.x; damageIndicatorPosition.y = transform.position.y + 1;
        damageIndicatorObject.transform.position = damageIndicatorPosition;
    }
    public void TakeDamage(float amount)
    {
        health -= amount;

        if (showDamageCoroutine != null) { StopCoroutine(showDamageCoroutine); }
        showDamageCoroutine = StartCoroutine(ShowDamageCor(amount));

        if (health <= 0)
        {
            Die();
        }
    }

    public void Die()
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

        // Let Game Manager know I have died
        GameManager.instance.OnEnemyDeath();

        // Die
        Destroy(gameObject);
    }

    private IEnumerator ShowDamageCor(float amount)
    {
        damageIndicatorText.text = "-" + amount.ToString();
        if (damageIndicatorObject != null) { damageIndicatorObject.SetActive(true); }
        yield return new WaitForSeconds(0.3f);

        if (damageIndicatorObject != null) { damageIndicatorObject.SetActive(false); }
    }


    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
