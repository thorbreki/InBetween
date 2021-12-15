using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    private void Start()
    {
        spawnPosition = new Vector3(0, spawnYLevel, transform.position.z);
        spawnCooldown = beginningSpawningCooldown;
        //StartCoroutine(spawnEnemies()); // Start spawning enemies!
        //StartCoroutine(IncreaseSpawningCooldown()); // Start speeding up!
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightShift))
        {
            if (spawnEnemiesCoroutine != null) { return; }
            spawnEnemiesCoroutine = StartCoroutine(spawnEnemies());
            StartCoroutine(IncreaseSpawningCooldown()); // Start speeding up!
        }
    }

    // A coroutine that spawns enemies
    private IEnumerator spawnEnemies()
    {
        while (!Input.GetKey(KeyCode.Escape))
        {
            int randomEnemyNumber = Random.Range(1, 5); // The number which will decide which enemy type gets spawned
            GameObject newEnemy;

            switch (randomEnemyNumber)
            {
                case 1:
                    newEnemy = SpawnEnemy(basicEnemyObject);
                    newEnemy.GetComponent<BasicEnemyController>().playerTransform = playerTransform;
                    break;
                case 2:
                    newEnemy = SpawnEnemy(projectileEnemyObject);
                    newEnemy.GetComponent<ProjectileEnemyController>().SetPlayerTransform(playerTransform);
                    break;
                case 3:
                    newEnemy = SpawnEnemy(stomperEnemyObject);
                    newEnemy.GetComponent<StomperController>().playerTransform = playerTransform;
                    newEnemy.GetComponent<StomperController>().playerCombatScript = playerCombatScript;
                    break;
                default: // When the value is 4
                    newEnemy = SpawnEnemy(rollerEnemyObject);
                    newEnemy.GetComponent<RollerEnemyController>().playerTransform = playerTransform;
                    break;
            }
            yield return new WaitForSeconds(spawnCooldown);
        }
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
        while (!Input.GetKey(KeyCode.Escape))
        {
            yield return new WaitForSeconds(speedUpCoolDownSec);
            spawnCooldown -= spawningSpeedUp; // Actually speed up the spawner
        }
    }
}
