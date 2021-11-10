using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawnController : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;

    [Header("Enemy objects")]
    [SerializeField] private GameObject basicEnemyObject;
    [SerializeField] private GameObject projectileEnemyObject;

    private Vector3 spawnPosition;


    private void Start()
    {
        spawnPosition = new Vector3(5, 5, 0);
        StartCoroutine(spawnEnemies());
    }

    // A coroutine that spawns enemies
    private IEnumerator spawnEnemies()
    {
        while (!Input.GetKeyDown(KeyCode.Escape))
        {
            yield return new WaitForSeconds(3f);
            GameObject newEnemy = Instantiate(projectileEnemyObject, spawnPosition, Quaternion.identity);
            newEnemy.GetComponent<ProjectileEnemyController>().SetPlayerTransform(playerTransform);
        }
    }
}
