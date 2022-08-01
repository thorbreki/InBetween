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

    private Vector3 spawnPosition;
    
    private Coroutine spawnEnemiesCoroutine;


    private void Start()
    {
        spawnPosition = new Vector3(0, spawnYLevel, transform.position.z);
        spawnEnemiesCoroutine = StartCoroutine(spawnEnemies()); // Start spawning enemies!
    }

    // A coroutine that spawns enemies
    private IEnumerator spawnEnemies()
    {
        yield return new WaitForSeconds(2f);
        int maxNumOfEnemies = GameManager.instance.numOfEnemiesLeft; // At this time, this variable will equal the actual variable which holds the max amount of enemies
        float secondsToSpawn = ApplicationManager.instance.currLevelData.secondsToSpawn;
        while (numOfEnemiesSpawned < maxNumOfEnemies)
        {
            if (GameManager.instance.activeEnemies < 2)
            {
                //print("Decreased spawning rate to: " + secondsToSpawn.ToString());
                secondsToSpawn = Mathf.Max(0.2f, secondsToSpawn - 0.2f);
            }
            if (GameManager.instance.activeEnemies < ApplicationManager.instance.currLevelData.numOfMaxActiveEnemies)
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

                GameManager.instance.activeEnemies++;
            }
            yield return new WaitForSeconds(secondsToSpawn);
        }
    }

    private GameObject SpawnEnemy(GameObject newEnemyObject)
    {
        spawnPosition.x = Random.Range(minXSpawn, maxXSpawn);
        return Instantiate(newEnemyObject, spawnPosition, Quaternion.identity);
    }
}
