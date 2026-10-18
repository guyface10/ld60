using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    [SerializeField]
    private ZombieSpawn[] zombieSpawns;

    [Space]
    [SerializeField]
    private float timeBtwWaves;
    [SerializeField]
    private float btwTimeDecreaseAmount;
    [SerializeField]
    private float minTimeBtwWaves;
    [SerializeField]
    private float numZombiesPerWave;
    [SerializeField]
    private float numZombiesIncreaseAmount; 
    [SerializeField]
    private float maxZombiesPerWave;
    [SerializeField]
    private float wonTimeBtwWaves = 1.5f;

    private float waveSpawnTimer;

    private Transform[] spawnPoints;
    private Player player;

    void Awake()
    {
        spawnPoints = GetComponentsInChildren<Transform>();
        player = FindAnyObjectByType<Player>();
    }

    void Update()
    {
        if (player != null && player.Won)
            timeBtwWaves = wonTimeBtwWaves;

        waveSpawnTimer += Time.deltaTime;
        if (waveSpawnTimer >= timeBtwWaves)
            SpawnNextWave();
    }

    private void SpawnNextWave()
    {
        waveSpawnTimer = 0;
        timeBtwWaves -= btwTimeDecreaseAmount;
        if (timeBtwWaves < minTimeBtwWaves)
            timeBtwWaves = minTimeBtwWaves;

        ShuffleArray(spawnPoints);
        for (int i = 0; i < (int)numZombiesPerWave; i++)
        {
            int index = ChooseWeightedSpawnIndex();

            for (int j = 0; j < zombieSpawns[index].Amount; j++)
            {
                GameObject newZombie = Instantiate(zombieSpawns[index].ZombiePrefab);

                Vector3 position = spawnPoints[i].position;
                position.x -= 0.1f * i;
                newZombie.transform.position = position;
            }
        }

        numZombiesPerWave += numZombiesIncreaseAmount;
        if (numZombiesPerWave >= maxZombiesPerWave)
            numZombiesPerWave = maxZombiesPerWave;

        if (numZombiesPerWave >= spawnPoints.Length)
            numZombiesPerWave = spawnPoints.Length - 1;
    }

    private int ChooseWeightedSpawnIndex()
    {
        float totalLikelihood = 0;
        for (int i = 0; i < zombieSpawns.Length; i++)
            totalLikelihood += zombieSpawns[i].Likelihood;

        float rand = Random.Range(0, totalLikelihood);
        for (int i = 0; i < zombieSpawns.Length; i++)
        {
            rand -= zombieSpawns[i].Likelihood;
            if (rand < 0)
                return i;
        }

        return zombieSpawns.Length - 1;
    }

    private void ShuffleArray<T>(T[] array)
    {
        int n = array.Length;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            (array[k], array[n]) = (array[n], array[k]);
        }
    }   

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        spawnPoints = GetComponentsInChildren<Transform>();
        for (int i = 0; i < spawnPoints.Length; i++)
        {
            Gizmos.DrawSphere(spawnPoints[i].position, 0.2f);
        }
    }
}

[System.Serializable]
public class ZombieSpawn
{
    [field: SerializeField]
    public GameObject ZombiePrefab { get; private set; }

    [field: SerializeField]
    public int Amount { get; private set; }

    [field: SerializeField]
    public float Likelihood { get; private set; }
}

