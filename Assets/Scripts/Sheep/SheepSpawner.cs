using UnityEngine;

public class SheepSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject sheepPrefab;
    public Transform spawnPoint;
    public Vector3 moveDirection = Vector3.right;
    public float spawnInterval = 5f;
    public float minSpeed = 1.5f;
    public float maxSpeed = 2.8f;

    private float nextSpawnTime;

    private void Start()
    {
        SpawnFromStartPoint();
        ScheduleNextSpawn();
    }

    private void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnFromStartPoint();
            ScheduleNextSpawn();
        }
    }

    public void SpawnFromStartPoint()
    {
        if (sheepPrefab == null)
        {
            Debug.LogWarning("Sheep prefab is not assigned.");
            return;
        }

        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : transform.position;
        GameObject sheepObject = Instantiate(sheepPrefab, spawnPosition, Quaternion.identity);

        Sheep sheep = sheepObject.GetComponent<Sheep>();
        if (sheep != null)
        {
            float randomSpeed = Random.Range(minSpeed, maxSpeed);
            sheep.Initialize(moveDirection, randomSpeed, this);
        }
    }

    private void ScheduleNextSpawn()
    {
        nextSpawnTime = Time.time + spawnInterval;
    }
}
