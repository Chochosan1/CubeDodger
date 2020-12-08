using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnerManager : MonoBehaviour
{
    public static SpawnerManager Instance;

    [Header("Spawners")]
    [SerializeField] private Transform[] enemySpawnsUp;
    [SerializeField] private Transform[] enemySpawnsDown;

    [Header("Enemy prefabs")]
    [SerializeField] private GameObject[] enemies;

    [Header("Properties")]
    [Tooltip("Set to true if the same pattern should never appear twice consequently.")]
    [SerializeField] private bool isAvoidTheSamePatternTwice = true;
    [Tooltip("Each pattern will choose a random values from this array and will spawn that many enemies. The best way is to keep all values divisible by 3. E.g. 3 6 9 12")]
    [SerializeField] private int[] enemiesToSpawnValues;
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
    [SerializeField] private float absoluteMaximumSpeedBoost = 3f;
    private float spawnCooldown;
    private float spawnTimestamp;
    private bool isStillSpawning = true;
    private int currentPoolItem;
    private int currentSpawner, currentEnemyToSpawn;
    private bool isPatternChosen = false;
    private int lastPatternChosen = 0;
    private float enemySpeedBooster = 0f;

    //used to spawn all prefabs from the enemies array at least once
    private int initialEnemySpawnIndex = 0;

    private List<EnemyController> enemyPool;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

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
        else if (isUsePatterns)
        {
            if (!isPatternChosen)
            {
                int patternIndex = Random.Range(0, 7);
             //   patternIndex = 6;

                //avoid the same pattern running twice consequently
                if (isAvoidTheSamePatternTwice && patternIndex == lastPatternChosen)
                {
                    patternIndex++;
                    if (patternIndex >= 6)
                        patternIndex = 0;
                }

                //forbid these patterns if there is currently a missing tile or about to lose a tile
                if((patternIndex == 2 || patternIndex == 5 || patternIndex == 6) && TileDestroyerManager.Instance.IsAnyTileDestroyed())
                {
                    patternIndex++;
                    if (patternIndex >= 7)
                        patternIndex = 0;
                }

           
                int enemiesToSpawnIndex = Random.Range(0, enemiesToSpawnValues.Length);
                SpawnPattern(patternIndex, enemiesToSpawnValues[enemiesToSpawnIndex]);
                isPatternChosen = true;
                lastPatternChosen = patternIndex;                      
            }
        }
    }

    /// <summary>Will load all enemies from the prefab list into the pool.</summary>
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

    /// <summary>Determines a bonus to the movement speed of the enemies. Scales with the current score and the current score multiplier up to a certain maximum. </summary>
    private void DetermineSpeedBoost()
    {
        enemySpeedBooster = PlayerController.Instance.ScoreMultiplier / 10f + PlayerController.Instance.CurrentScore * 0.01f;
        if (enemySpeedBooster >= absoluteMaximumSpeedBoost)
            enemySpeedBooster = absoluteMaximumSpeedBoost;
    }

    public float GetCurrentSpeedBoost()
    {
        return enemySpeedBooster;
    }

    private void SpawnPattern(int patternIndex, int enemiesToSpawn)
    {
        switch (patternIndex)
        {
            case 0:
                StartCoroutine(StartPattern0(enemiesToSpawn));
                break;
            case 1:
                StartCoroutine(StartPattern1(enemiesToSpawn));
                break;
            case 2:
                StartCoroutine(StartPattern2(enemiesToSpawn));
                break;
            case 3:
                StartCoroutine(StartPattern3(enemiesToSpawn));
                break;
            case 4:
                StartCoroutine(StartPattern4(enemiesToSpawn));
                break;
            case 5:
                StartCoroutine(StartPattern5(enemiesToSpawn));
                break;
            case 6:
                StartCoroutine(StartPattern6(enemiesToSpawn));
                break;
        }
    }

    /// <summary>
    /// Uses all spawners from the top side consequently (spawner array traverse: ASCENDING). Enemies spawned are recycled from the pool and are also chosen consequently.
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartPattern0(int enemiesToSpawn)
    {
        float timeBetweenEnemies = 0.8f;
        bool isPatternExecuting = true;
        int enemiesSpawned = 0;
        currentSpawner = 0;
        while (isPatternExecuting)
        {
            timeBetweenEnemies = 0.8f;
            timeBetweenEnemies -= enemySpeedBooster * 0.2f;
            if (timeBetweenEnemies <= 0.5f)
                timeBetweenEnemies = 0.5f;
            if (currentSpawner >= enemySpawnsUp.Length)
                currentSpawner = 0;
            if (currentPoolItem >= enemyPool.Count)
                currentPoolItem = 0;

            enemyPool[currentPoolItem].gameObject.SetActive(true);
            enemyPool[currentPoolItem].Reset(enemySpawnsUp[currentSpawner].position, enemySpawnsUp[currentSpawner].rotation);
            currentSpawner++;
            currentPoolItem++;
            enemiesSpawned++;

            //wait a bit more time between waves
            if (enemiesSpawned % 3 == 0)
            {
                timeBetweenEnemies *= 2f;
            }

            //determine speed every 4th, 7th, 10th etc. enemy so that it is calculated at the start of the wave 
            //(this way waves and enemies sync very well and the game remains fair)
            if (enemiesSpawned % 3 == 1)
            {
                DetermineSpeedBoost();
            }

            if (enemiesSpawned >= enemiesToSpawn)
            {
                isPatternExecuting = false;
            }
            yield return new WaitForSeconds(timeBetweenEnemies);
        }

        DetermineSpeedBoost();
        isPatternChosen = false;
    }

    /// <summary>
    /// Uses all spawners from the bot side consequently (spawner array traverse: ASCENDING). Enemies spawned are recycled from the pool and are also chosen consequently.
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartPattern1(int enemiesToSpawn)
    {
        float timeBetweenEnemies = 0.8f;
        bool isPatternExecuting = true;
        int enemiesSpawned = 0;
        currentSpawner = 0;
        while (isPatternExecuting)
        {
            timeBetweenEnemies = 0.8f;
            timeBetweenEnemies -= enemySpeedBooster * 0.2f;
            if (timeBetweenEnemies <= 0.5f)
                timeBetweenEnemies = 0.5f;
            if (currentSpawner >= enemySpawnsUp.Length)
                currentSpawner = 0;
            if (currentPoolItem >= enemyPool.Count)
                currentPoolItem = 0;

            enemyPool[currentPoolItem].gameObject.SetActive(true);
            enemyPool[currentPoolItem].Reset(enemySpawnsDown[currentSpawner].position, enemySpawnsDown[currentSpawner].rotation);
            currentSpawner++;
            currentPoolItem++;
            enemiesSpawned++;

            //wait a bit more time between waves
            if (enemiesSpawned % 3 == 0)
            {
                timeBetweenEnemies *= 2f;
            }


            //determine speed every 4th, 7th, 10th etc. enemy so that it is calculated at the start of the wave 
            //(this way waves and enemies sync very well and the game remains fair)
            if (enemiesSpawned % 3 == 1)
            {
                DetermineSpeedBoost();
            }

            if (enemiesSpawned >= enemiesToSpawn)
            {
                isPatternExecuting = false;
            }
            yield return new WaitForSeconds(timeBetweenEnemies);
        }

        DetermineSpeedBoost();
        isPatternChosen = false;
    }

    /// <summary>
    /// Uses all spawners from both sides consequently. Enemies spawned are recycled from the pool and are also chosen consequently.
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartPattern2(int enemiesToSpawn)
    {
        TileDestroyerManager.Instance.SetPauseTileDestruction(true);
        float timeBetweenEnemies = 1f;
        bool isPatternExecuting = true;
        int enemiesSpawned = 0;
        int isTopOrBot = 0; //0 for top; 1 for bot
        currentSpawner = 0;
        while (isPatternExecuting)
        {
            timeBetweenEnemies = 1f;
            timeBetweenEnemies -= enemySpeedBooster * 0.2f;
            if (timeBetweenEnemies <= 0.5f)
                timeBetweenEnemies = 0.5f;
            if (currentSpawner >= enemySpawnsUp.Length)
                currentSpawner = 0;
            if (currentPoolItem >= enemyPool.Count)
                currentPoolItem = 0;
            if (isTopOrBot >= 2)
                isTopOrBot = 0;

            enemyPool[currentPoolItem].gameObject.SetActive(true);
            if (isTopOrBot == 0)
            {
                enemyPool[currentPoolItem].Reset(enemySpawnsUp[currentSpawner].position, enemySpawnsUp[currentSpawner].rotation);
            }
            else if (isTopOrBot == 1)
            {
                enemyPool[currentPoolItem].Reset(enemySpawnsDown[currentSpawner].position, enemySpawnsDown[currentSpawner].rotation);
            }

            currentSpawner++;
            currentPoolItem++;
            enemiesSpawned++;
            isTopOrBot++;

            //wait a bit more time between waves
            if (enemiesSpawned % 3 == 0)
            {
                timeBetweenEnemies *= 1.5f;
            }


            //determine speed every 4th, 7th, 10th etc. enemy so that it is calculated at the start of the wave 
            //(this way waves and enemies sync very well and the game remains fair)
            if (enemiesSpawned % 3 == 1)
            {
                DetermineSpeedBoost();
            }


            if (enemiesSpawned >= enemiesToSpawn)
            {
                isPatternExecuting = false;
            }
            yield return new WaitForSeconds(timeBetweenEnemies);
        }

        yield return new WaitForSeconds(0.1f);
        TileDestroyerManager.Instance.SetPauseTileDestruction(true);
        DetermineSpeedBoost();
        isPatternChosen = false;
    }

    /// <summary>
    /// Uses all spawners from the bot side consequently (spawner array traverse: DESCENDING). Enemies spawned are recycled from the pool and are also chosen consequently.
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartPattern3(int enemiesToSpawn)
    {
        float timeBetweenEnemies = 0.8f;
        bool isPatternExecuting = true;
        int enemiesSpawned = 0;
        currentSpawner = 2;

        while (isPatternExecuting)
        {
            timeBetweenEnemies = 0.8f;
            timeBetweenEnemies -= enemySpeedBooster * 0.2f;
            if (timeBetweenEnemies <= 0.5f)
                timeBetweenEnemies = 0.5f;
            if (currentSpawner < 0)
                currentSpawner = enemySpawnsDown.Length - 1;
            if (currentPoolItem >= enemyPool.Count)
                currentPoolItem = 0;

            enemyPool[currentPoolItem].gameObject.SetActive(true);
            enemyPool[currentPoolItem].Reset(enemySpawnsDown[currentSpawner].position, enemySpawnsDown[currentSpawner].rotation);
            currentSpawner--;
            currentPoolItem++;
            enemiesSpawned++;

            //wait a bit more time between waves
            if (enemiesSpawned % 3 == 0)
            {
                timeBetweenEnemies *= 2f;
            }


            //determine speed every 4th, 7th, 10th etc. enemy so that it is calculated at the start of the wave 
            //(this way waves and enemies sync very well and the game remains fair)
            if (enemiesSpawned % 3 == 1)
            {
                DetermineSpeedBoost();
            }

            if (enemiesSpawned >= enemiesToSpawn)
            {
                isPatternExecuting = false;
            }
            yield return new WaitForSeconds(timeBetweenEnemies);
        }

        DetermineSpeedBoost();
        isPatternChosen = false;
    }


    /// <summary>
    /// Uses all spawners from the top side consequently (spawner array traverse: DESCENDING). Enemies spawned are recycled from the pool and are also chosen consequently.
    /// </summary>
    /// <returns></returns>
    private IEnumerator StartPattern4(int enemiesToSpawn)
    {
        float timeBetweenEnemies = 0.8f;
        bool isPatternExecuting = true;
        int enemiesSpawned = 0;
        currentSpawner = 2;

        while (isPatternExecuting)
        {
            timeBetweenEnemies = 0.8f;
            timeBetweenEnemies -= enemySpeedBooster * 0.2f;
            if (timeBetweenEnemies <= 0.5f)
                timeBetweenEnemies = 0.5f;
            if (currentSpawner < 0)
                currentSpawner = enemySpawnsUp.Length - 1;
            if (currentPoolItem >= enemyPool.Count)
                currentPoolItem = 0;

            enemyPool[currentPoolItem].gameObject.SetActive(true);
            enemyPool[currentPoolItem].Reset(enemySpawnsUp[currentSpawner].position, enemySpawnsUp[currentSpawner].rotation);
            currentSpawner--;
            currentPoolItem++;
            enemiesSpawned++;

            //wait a bit more time between waves
            if (enemiesSpawned % 3 == 0)
            {
                timeBetweenEnemies *= 2f;
            }


            //determine speed every 4th, 7th, 10th etc. enemy so that it is calculated at the start of the wave 
            //(this way waves and enemies sync very well and the game remains fair)
            if (enemiesSpawned % 3 == 1)
            {
                DetermineSpeedBoost();
            }

            if (enemiesSpawned >= enemiesToSpawn)
            {
                isPatternExecuting = false;
            }
            yield return new WaitForSeconds(timeBetweenEnemies);
        }

        DetermineSpeedBoost();
        isPatternChosen = false;
    }

    /// <summary>
    /// Uses 2 spawners simultaneously to spawn 2 balls at the same time from the top side. Pauses tile destruction so that the game remains fair.
    /// </summary>
    /// <param name="enemiesToSpawn"></param>
    /// <returns></returns>
    private IEnumerator StartPattern5(int enemiesToSpawn)
    {
        TileDestroyerManager.Instance.SetPauseTileDestruction(true);
        float timeBetweenEnemies = 0.8f;
        bool isPatternExecuting = true;
        int enemiesSpawned = 0;
        int firstSpawner = 0;

        //override the enemiesToSpawn because this pattern works with divisible by 2 nums
        int[] enemiesSpawnValues = { 2, 4, 6, 8 };
        enemiesToSpawn = enemiesSpawnValues[Random.Range(0, enemiesSpawnValues.Length)];

        while (isPatternExecuting)
        {
            timeBetweenEnemies = 0.8f;
            timeBetweenEnemies -= enemySpeedBooster * 0.2f;
            if (timeBetweenEnemies <= 0.5f)
                timeBetweenEnemies = 0.5f;
            if (currentPoolItem >= enemyPool.Count)
                currentPoolItem = 0;

            currentSpawner = Random.Range(0, enemySpawnsUp.Length);
            enemyPool[currentPoolItem].gameObject.SetActive(true);
            enemyPool[currentPoolItem].Reset(enemySpawnsUp[currentSpawner].position, enemySpawnsUp[currentSpawner].rotation);
            firstSpawner = currentSpawner;
            currentPoolItem++;
            enemiesSpawned++;

            currentSpawner = Random.Range(0, enemySpawnsUp.Length);
            if(currentSpawner == firstSpawner)
            {
                currentSpawner++;
                if (currentSpawner >= enemySpawnsUp.Length)
                    currentSpawner = 0;
            }

            if (currentPoolItem >= enemyPool.Count)
                currentPoolItem = 0;

            enemyPool[currentPoolItem].gameObject.SetActive(true);
            enemyPool[currentPoolItem].Reset(enemySpawnsUp[currentSpawner].position, enemySpawnsUp[currentSpawner].rotation);
            currentPoolItem++;
            enemiesSpawned++;

            //wait a bit more time between waves
            if (enemiesSpawned % 2 == 0)
            {
                timeBetweenEnemies *= 2f;
            }

            if (enemiesSpawned >= enemiesToSpawn)
            {
                isPatternExecuting = false;
            }
            yield return new WaitForSeconds(timeBetweenEnemies);
        }

        yield return new WaitForSeconds(0.1f);
        TileDestroyerManager.Instance.SetPauseTileDestruction(false);
        isPatternChosen = false;
    }

    /// <summary>
    /// Uses 2 spawners simultaneously to spawn 2 balls at the same time from the bot side. Pauses tile destruction so that the game remains fair.
    /// </summary>
    /// <param name="enemiesToSpawn"></param>
    /// <returns></returns>
    private IEnumerator StartPattern6(int enemiesToSpawn)
    {
        TileDestroyerManager.Instance.SetPauseTileDestruction(true);
        float timeBetweenEnemies = 0.8f;
        bool isPatternExecuting = true;
        int enemiesSpawned = 0;
        int firstSpawner = 0;

        //override the enemiesToSpawn because this pattern works with divisible by 2 nums
        int[] enemiesSpawnValues = { 2, 4, 6, 8 };
        enemiesToSpawn = enemiesSpawnValues[Random.Range(0, enemiesSpawnValues.Length)];

        while (isPatternExecuting)
        {
            timeBetweenEnemies = 0.8f;
            timeBetweenEnemies -= enemySpeedBooster * 0.2f;
            if (timeBetweenEnemies <= 0.5f)
                timeBetweenEnemies = 0.5f;
            if (currentPoolItem >= enemyPool.Count)
                currentPoolItem = 0;

            currentSpawner = Random.Range(0, enemySpawnsDown.Length);
            enemyPool[currentPoolItem].gameObject.SetActive(true);
            enemyPool[currentPoolItem].Reset(enemySpawnsDown[currentSpawner].position, enemySpawnsDown[currentSpawner].rotation);
            firstSpawner = currentSpawner;
            currentPoolItem++;
            enemiesSpawned++;

            currentSpawner = Random.Range(0, enemySpawnsDown.Length);
            if (currentSpawner == firstSpawner)
            {
                currentSpawner++;
                if (currentSpawner >= enemySpawnsDown.Length)
                    currentSpawner = 0;
            }

            if (currentPoolItem >= enemyPool.Count)
                currentPoolItem = 0;

            enemyPool[currentPoolItem].gameObject.SetActive(true);
            enemyPool[currentPoolItem].Reset(enemySpawnsDown[currentSpawner].position, enemySpawnsDown[currentSpawner].rotation);
            currentPoolItem++;
            enemiesSpawned++;

            //wait a bit more time between waves
            if (enemiesSpawned % 2 == 0)
            {
                timeBetweenEnemies *= 2f;
            }

            if (enemiesSpawned >= enemiesToSpawn)
            {
                isPatternExecuting = false;
            }
            yield return new WaitForSeconds(timeBetweenEnemies);
        }

        yield return new WaitForSeconds(0.1f);
        TileDestroyerManager.Instance.SetPauseTileDestruction(false);
        isPatternChosen = false;
    }
}
