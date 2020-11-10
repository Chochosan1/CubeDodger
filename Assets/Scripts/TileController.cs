using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
///  Each tile controls its own enemy spawners together with a pooling system and randomized enemies.
/// </summary>
public class TileController : MonoBehaviour
{
    [Header("Spawners")]
    [SerializeField] private Transform[] enemySpawns;

    [Header("Enemy prefabs")]
    [SerializeField] private GameObject[] enemies;

    [Header("Properties")]
    [SerializeField] private float minSpawnCooldown = 1f;
    [SerializeField] private float maxSpawnCooldown = 3f;
    [SerializeField] private int maxPoolSize = 5;
    [Tooltip("If true the system will spawn each enemy from the prefab array at least once. If false the chosen enemies will be random so some of them might not make it into the pool.")]
    [SerializeField] private bool isSpawnEachEnemyFromPrefabArrayAtLeastOnce = true;
    [Tooltip("If true the pooling system will reuse enemies randomly. If false it will cycle through the pool from the beginning to the end then reset.")]
    [SerializeField] private bool isPoolSystemChooseRandomEnemy = false;
    private float spawnCooldown;
    private float spawnTimestamp;
    private bool isStillSpawning = true;
    private int currentPoolItem;
    private int currentSpawner, currentEnemyToSpawn;

    //used to spawn all prefabs from the enemies array at least once
    private int initialEnemySpawnIndex = 0;

    private List<GameObject> enemyPool;
    private List<Renderer> enemyRenderersPool;

    private void Start()
    {
        enemyPool = new List<GameObject>();
        enemyRenderersPool = new List<Renderer>();

        spawnCooldown = Random.Range(minSpawnCooldown, maxSpawnCooldown);
        spawnTimestamp = Time.time + spawnCooldown;

        //pool size should be the size at least of the enemies array to make sure that all prefabs in the array exist at least once in the game
        if (maxPoolSize < enemies.Length)
            maxPoolSize = enemies.Length;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time >= spawnTimestamp)
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

                currentSpawner = Random.Range(0, enemySpawns.Length);

                GameObject enemyCopy = Instantiate(enemies[currentEnemyToSpawn].gameObject, enemySpawns[currentSpawner].position, enemySpawns[currentSpawner].rotation);
                enemyPool.Add(enemyCopy);
                enemyRenderersPool.Add(enemyCopy.GetComponent<Renderer>());
                if (enemyPool.Count >= maxPoolSize)
                {
                    isStillSpawning = false;
                }
            }
            else
            {
                if(isPoolSystemChooseRandomEnemy)
                {
                    currentPoolItem = Random.Range(0, enemyPool.Count);
                }

                if (!enemyRenderersPool[currentPoolItem].isVisible)
                {
                    enemyPool[currentPoolItem].SetActive(true);
                    currentSpawner = Random.Range(0, enemySpawns.Length);
                    enemyPool[currentPoolItem].transform.position = enemySpawns[currentSpawner].position;
                    enemyPool[currentPoolItem].transform.rotation = enemySpawns[currentSpawner].rotation;                  
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
    }
}
