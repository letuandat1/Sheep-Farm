using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn")]
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public Transform disappearPoint;
    public float firstSpawnDelay = 0f;
    public float spawnInterval = 3f;
    public int maxActiveEnemies = 0;

    [Header("Enemy Movement")]
    public Vector3 moveDirection = Vector3.left;
    public float moveSpeed = 2f;

    private float nextSpawnTime;

    private void Start()
    {
        nextSpawnTime = Time.time + firstSpawnDelay;
    }

    private void Update()
    {
        if (GameFlowManager.Instance != null && GameFlowManager.Instance.isGameEnded)
            return;

        if (Time.time < nextSpawnTime)
            return;

        SpawnEnemy();
        nextSpawnTime = Time.time + Mathf.Max(0.1f, spawnInterval);
    }

    public void SpawnEnemy()
    {
        if (enemyPrefab == null)
        {
            Debug.LogWarning("Enemy prefab is missing.");
            return;
        }

        if (maxActiveEnemies > 0 && CountActiveEnemies() >= maxActiveEnemies)
            return;

        Vector3 spawnPosition = spawnPoint != null ? spawnPoint.position : transform.position;
        GameObject enemyObject = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

        EnemyStraightMover mover = enemyObject.GetComponent<EnemyStraightMover>();
        if (mover != null)
        {
            mover.moveDirection = moveDirection;
            mover.moveSpeed = moveSpeed;
            if (disappearPoint != null)
            {
                mover.disappearPoint = disappearPoint;
            }
        }
    }

    private int CountActiveEnemies()
    {
        return FindObjectsByType<EnemyStraightMover>(FindObjectsInactive.Exclude, FindObjectsSortMode.None).Length;
    }
}