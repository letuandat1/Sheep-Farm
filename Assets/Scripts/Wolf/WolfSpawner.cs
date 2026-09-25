using UnityEngine;

public class WolfSpawner : MonoBehaviour
{
    [Header("Spawn")]
    public GameObject wolfPrefab;
    public BoxCollider spawnArea;
    public BoxCollider exitArea;
    public float firstSpawnDelay = 5f;
    public float spawnInterval = 15f;
    public int maxActiveWolves = 1;
    public FarmerController farmer;
    public float minSpawnDistanceFromFarmer = 5f;
    public int maxSpawnPositionAttempts = 20;
    public float spawnEdgeInset = 0.05f;

    [Header("Wolf Movement")]
    public float moveSpeed = 1.2f;
    public float catchDistance = 1.2f;
    public float carrySpeed = 2.5f;

    private float nextSpawnTime;

    private void Start()
    {
        if (exitArea != null)
            exitArea.isTrigger = true;

        if (farmer == null)
            farmer = FindAnyObjectByType<FarmerController>();

        nextSpawnTime = Time.time + firstSpawnDelay;
    }

    private void Update()
    {
        if (GameFlowManager.Instance != null && GameFlowManager.Instance.isGameEnded)
            return;

        if (Time.time < nextSpawnTime)
            return;

        SpawnWolf();
        nextSpawnTime = Time.time + Mathf.Max(0.1f, spawnInterval);
    }

    public void SpawnWolf()
    {
        if (wolfPrefab == null || spawnArea == null || exitArea == null)
        {
            Debug.LogWarning("WolfSpawner needs a wolf prefab, spawn area, and exit area.");
            return;
        }

        if (maxActiveWolves > 0 && CountActiveWolves() >= maxActiveWolves)
            return;

        Bounds bounds = spawnArea.bounds;
        Vector3 spawnPosition;
        if (!TryGetSpawnPosition(bounds, out spawnPosition))
        {
            Debug.Log("Wolf spawn skipped because no position is far enough from the farmer.");
            return;
        }

        GameObject wolfObject = Instantiate(wolfPrefab, spawnPosition, Quaternion.identity);
        WolfController wolf = wolfObject.GetComponent<WolfController>();
        if (wolf == null)
        {
            Debug.LogWarning("Wolf prefab needs a WolfController component.", wolfObject);
            Destroy(wolfObject);
            return;
        }

        wolf.exitArea = exitArea;
        wolf.moveSpeed = moveSpeed;
        wolf.catchDistance = catchDistance;
        wolf.carrySpeed = carrySpeed;
    }

    private int CountActiveWolves()
    {
        return FindObjectsByType<WolfController>(FindObjectsInactive.Exclude).Length;
    }

    private bool TryGetSpawnPosition(Bounds bounds, out Vector3 spawnPosition)
    {
        int attempts = Mathf.Max(1, maxSpawnPositionAttempts);
        for (int attempt = 0; attempt < attempts; attempt++)
        {
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;

            int faceIndex = Random.Range(0, 6);
            switch (faceIndex)
            {
                case 0:
                    spawnPosition = new Vector3(min.x - spawnEdgeInset, Random.Range(min.y, max.y), Random.Range(min.z, max.z));
                    break;
                case 1:
                    spawnPosition = new Vector3(max.x + spawnEdgeInset, Random.Range(min.y, max.y), Random.Range(min.z, max.z));
                    break;
                case 2:
                    spawnPosition = new Vector3(Random.Range(min.x, max.x), min.y - spawnEdgeInset, Random.Range(min.z, max.z));
                    break;
                case 3:
                    spawnPosition = new Vector3(Random.Range(min.x, max.x), max.y + spawnEdgeInset, Random.Range(min.z, max.z));
                    break;
                case 4:
                    spawnPosition = new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y), min.z - spawnEdgeInset);
                    break;
                default:
                    spawnPosition = new Vector3(Random.Range(min.x, max.x), Random.Range(min.y, max.y), max.z + spawnEdgeInset);
                    break;
            }

            if (farmer == null || IsFarEnoughFromFarmer(spawnPosition))
                return true;
        }

        spawnPosition = default;
        return false;
    }

    private bool IsFarEnoughFromFarmer(Vector3 spawnPosition)
    {
        Vector3 farmerPosition = farmer.transform.position;
        farmerPosition.y = spawnPosition.y;
        float minimumDistance = Mathf.Max(0f, minSpawnDistanceFromFarmer);
        return (spawnPosition - farmerPosition).sqrMagnitude >= minimumDistance * minimumDistance;
    }
}