using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
    [Header("Spawners")]
    [SerializeField] private Transform[] enemySpawnsUp;
    [SerializeField] private Transform[] enemySpawnsDown;

    [Header("Enemy prefabs")]
    [SerializeField] private GameObject[] enemies;

    [Header("Properties")]
    [Tooltip("How much later after starting the level should the enemy spawn (after that the enemies will use the min/max cooldowns)?")]
    [SerializeField] private float initialSpawnCooldown = 1f;
    [SerializeField] private float minSpawnCooldown = 1f;
    [SerializeField] private float maxSpawnCooldown = 3f;
    [SerializeField] private int maxPoolSize = 5;
    [Tooltip("If true the system will spawn each enemy from the prefab array at least once. If false the chosen enemies will be random so some of them might not make it into the pool.")]
    [SerializeField] private bool isSpawnEachEnemyFromPrefabArrayAtLeastOnce = true;
    [Tooltip("If true the pooling system will reuse enemies randomly. If false it will cycle through the pool from the beginning to the end then reset.")]
    [SerializeField] private bool isPoolSystemChooseRandomEnemy = false;
    [SerializeField] private bool isUsePatterns = true;
    private float spawnCooldown;
    private float spawnTimestamp;
    private bool isStillSpawning = true;
    private int currentPoolItem;
    private int currentSpawner, currentEnemyToSpawn;
    private bool isPatternChosen = false;

    //used to spawn all prefabs from the enemies array at least once
    private int initialEnemySpawnIndex = 0;

    private List<EnemyController> enemyPool;

    private void Start()
    {
        enemyPool = new List<EnemyController>();

        spawnTimestamp = Time.time + initialSpawnCooldown;

        //pool size should be the size at least of the enemies array to make sure that all prefabs in the array exist at least once in the game
        if (maxPoolSize < enemies.Length)
            maxPoolSize = enemies.Length;

        SpawnAllEnemiesInitially();
    }

    void Update()
    {
        if (Time.time >= spawnTimestamp && !isUsePatterns)
        {
            spawnCooldown = Random.Range(minSpawnCooldown, maxSpawnCooldown);
            spawnTimestamp = Time.time + spawnCooldown;

            if (isStillSpawning)
            {
                //make sure that at least one of each enemy in the array is spawned before spawning more random enemies if the pool size allows it
                if (isSpawnEachEnemyFromPrefabArrayAtLeastOnce && initialEnemySpawnIndex < enemies.Length)
                {
                    currentEnemyToSpawn = initialEnemySpawnIndex;
                    initialEnemySpawnIndex++;
                }
                else
                {
                    currentEnemyToSpawn = Random.Range(0, enemies.Length);
                }

                currentSpawner = Random.Range(0, enemySpawnsUp.Length);

                GameObject enemyCopy = Instantiate(enemies[currentEnemyToSpawn].gameObject, enemySpawnsUp[currentSpawner].position, enemySpawnsUp[currentSpawner].rotation);
                enemyPool.Add(enemyCopy.GetComponent<EnemyController>());
                if (enemyPool.Count >= maxPoolSize)
                {
                    isStillSpawning = false;
                }
            }
            else
            {
                if (isPoolSystemChooseRandomEnemy)
                {
                    currentPoolItem = Random.Range(0, enemyPool.Count);
                }

                if (!enemyPool[currentPoolItem].isVisible)
                {
                    enemyPool[currentPoolItem].gameObject.SetActive(true);
                    currentSpawner = Random.Range(0, enemySpawnsUp.Length);
                    enemyPool[currentPoolItem].Reset(enemySpawnsUp[currentSpawner].position, enemySpawnsUp[currentSpawner].rotation);
                }
                else
                {
                    spawnTimestamp = Time.time; //reset cooldown so it can immediately try to spawn another one
                }

                currentPoolItem++;

                if (currentPoolItem >= enemyPool.Count)
                {
                    currentPoolItem = 0;
                }
            }
        }
        else if(isUsePatterns)
        {
            if(!isPatternChosen)
            {
                SpawnPattern1();
                isPatternChosen = true;
            }
           
        }
    }

    private void SpawnAllEnemiesInitially()
    {
        for (int i = 0; i < maxPoolSize; i++)
        {
            if (isSpawnEachEnemyFromPrefabArrayAtLeastOnce && initialEnemySpawnIndex < enemies.Length)
            {
                currentEnemyToSpawn = initialEnemySpawnIndex;
                initialEnemySpawnIndex++;
            }
            else
            {
                currentEnemyToSpawn = Random.Range(0, enemies.Length);
            }
            currentSpawner = Random.Range(0, enemySpawnsUp.Length);

            GameObject enemyCopy = Instantiate(enemies[currentEnemyToSpawn].gameObject, enemySpawnsUp[currentSpawner].position, enemySpawnsUp[currentSpawner].rotation);
            enemyPool.Add(enemyCopy.GetComponent<EnemyController>());
            enemyCopy.SetActive(false);
            if (enemyPool.Count >= maxPoolSize)
            {
                isStillSpawning = false;
            }
        }
    }

    private void SpawnPattern1()
    {
        StartCoroutine(StartPattern1());
    }

    /// <summary>
    /// Uses all spawners from one of the sides consquentially. Enemies spawned are recycled from the pool and are also chosen consequentially.
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartPattern1()
    {
        float timeBetweenEnemies = 1f;
        bool isPatternExecuting = true;
        int enemiesSpawned = 0;
        currentSpawner = 0;
        currentPoolItem = 0;
        while (isPatternExecuting)
        {
            if (currentSpawner >= enemySpawnsUp.Length)
                currentSpawner = 0;
            if (currentPoolItem >= enemyPool.Count)
                currentPoolItem = 0;
            Debug.Log("SPAWN AT: " + enemySpawnsUp[currentSpawner].name);
            enemyPool[currentPoolItem].gameObject.SetActive(true);
            enemyPool[currentPoolItem].Reset(enemySpawnsUp[currentSpawner].position, enemySpawnsUp[currentSpawner].rotation);
            currentSpawner++;          
            currentPoolItem++;
            enemiesSpawned++;
            if (enemiesSpawned >= 18)
            {
                isPatternExecuting = false;
            }
            yield return new WaitForSeconds(timeBetweenEnemies);
        }

        isPatternChosen = false;
    }

}
