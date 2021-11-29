using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinController : MonoBehaviour
{
    [SerializeField] private float secondsBeforeDespawning; // The number of seconds before despawning

    private void Start()
    {
        StartCoroutine(startDying());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            GameManager.instance.AddCoin();
            Destroy(gameObject);
        }
    }

    private IEnumerator startDying()
    {
        yield return new WaitForSeconds(secondsBeforeDespawning);
        Destroy(gameObject);
    }
}
