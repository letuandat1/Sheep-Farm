using UnityEngine;
using UnityEngine.Splines;

public class SheepSplineSpawner : MonoBehaviour
{
    [Header("Spawner")]
    public GameObject sheepPrefab;
    public SplineContainer levelSpline;
    public Transform spawnPoint;

    [Header("Spawn Timing")]
    public float spawnInterval = 5f;
    public float spawnOffset = 0f;

    private float nextSpawnTime;

    private void Start()
    {
        nextSpawnTime = Time.time + spawnOffset;
    }

    private void Update()
    {
        if (Time.time >= nextSpawnTime)
        {
            SpawnSheep();
            nextSpawnTime = Time.time + spawnInterval;
        }
    }

    public void SpawnSheep()
    {
        if (sheepPrefab == null)
        {
            Debug.LogWarning("Sheep prefab is missing.");
            return;
        }

        if (levelSpline == null)
        {
            Debug.LogWarning("Level spline is missing.");
            return;
        }

        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : transform.position;
        GameObject sheep = Instantiate(sheepPrefab, spawnPosition, Quaternion.identity);

        SheepSplineMover mover = sheep.GetComponentInChildren<SheepSplineMover>();
        if (mover == null)
        {
            Debug.LogWarning("Sheep prefab needs a SheepSplineMover component.", sheep);
            Destroy(sheep);
            return;
        }

        mover.spline = levelSpline;
        mover.startProgress = 0f;
    }
}
