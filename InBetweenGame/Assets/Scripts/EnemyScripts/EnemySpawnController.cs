using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnemySpawnController : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private PlayerCombat playerCombatScript;

    [Header("Enemy objects")]
    [SerializeField] private GameObject basicEnemyObject;
    [SerializeField] private GameObject projectileEnemyObject;
    [SerializeField] private GameObject stomperEnemyObject;
    [SerializeField] private GameObject rollerEnemyObject;

    [Header("Enemy Spawning Attributes")]
    [SerializeField] private float spawnYLevel; // Where on the y-axis will the enemies spawn
    [SerializeField] private float minXSpawn; // What is the minimum on the x-axis enemies can spawn
    [SerializeField] private float maxXSpawn; // What is the maximum on the x-axis enemies can spawn
    [SerializeField] private float beginningSpawningCooldown; // How fast does the spawner spawn enemies in the beginning of survival mode

    private int numOfEnemiesSpawned = 0;

    /// <summary>
    /// When the spawner speeds up, how much does it speed up
    /// </summary>
    [SerializeField] private float spawningSpeedUp; // When the spawner speeds up, how much does it speed up

    /// <summary>
    /// In seconds, how much time needed for the enemy spawner to speed up
    /// </summary>
    [SerializeField] private float speedUpCoolDownSec; // In seconds, how much time needed for the enemy spawner to speed up

    private float spawnCooldown;
    private Vector3 spawnPosition;
    
    private Coroutine spawnEnemiesCoroutine;
    private Coroutine decreaseSpawningCooldownCoroutine;


    private void Start()
    {
        spawnPosition = new Vector3(0, spawnYLevel, transform.position.z);
        spawnCooldown = beginningSpawningCooldown;
        spawnEnemiesCoroutine = StartCoroutine(spawnEnemies()); // Start spawning enemies!
        decreaseSpawningCooldownCoroutine = StartCoroutine(IncreaseSpawningCooldown()); // Start speeding up!
    }

    // A coroutine that spawns enemies
    private IEnumerator spawnEnemies()
    {
        yield return new WaitForSeconds(2f);
        int maxNumOfEnemies = GameManager.instance.numOfEnemiesLeft; // At this time, this variable will equal the actual variable which holds the max amount of enemies
        while (numOfEnemiesSpawned < maxNumOfEnemies)
        {
            numOfEnemiesSpawned++;
            int randomEnemyNumber = Random.Range(1, 5); // The number which will decide which enemy type gets spawned

            switch (randomEnemyNumber)
            {
                case 1:
                    SpawnEnemy(basicEnemyObject);
                    break;
                case 2:
                    SpawnEnemy(projectileEnemyObject);
                    break;
                case 3:
                    SpawnEnemy(stomperEnemyObject);
                    break;
                default: // When the value is 4
                    SpawnEnemy(rollerEnemyObject);
                    break;
            }
            yield return new WaitForSeconds(spawnCooldown);
        }

        // THE PLAYER HAS MANAGED TO DEFEAT THE LEVEL
        StopCoroutine(decreaseSpawningCooldownCoroutine);
    }

    private GameObject SpawnEnemy(GameObject newEnemyObject)
    {
        spawnPosition.x = Random.Range(minXSpawn, maxXSpawn);
        return Instantiate(newEnemyObject, spawnPosition, Quaternion.identity);
    }

    /// <summary>
    /// This coroutine decreases the spawning cooldown according to the speedUpCooldownSec variable
    /// </summary>
    /// <returns></returns>
    private IEnumerator IncreaseSpawningCooldown()
    {
        float divider = Mathf.Min(2, beginningSpawningCooldown);
        float maxDividerValue = beginningSpawningCooldown;
        while (true)
        {
            yield return new WaitForSeconds(speedUpCoolDownSec);
            spawnCooldown = beginningSpawningCooldown / divider; // Actually speed up the spawner
            divider = Mathf.Min(maxDividerValue, divider + 0.4f);
            print("SpawnCooldown:" + spawnCooldown.ToString());
        }
    }
}
