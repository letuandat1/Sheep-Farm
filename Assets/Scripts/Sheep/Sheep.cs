using UnityEngine;

public class Sheep : MonoBehaviour
{
    [Header("Movement")]
    public Vector3 moveDirection = Vector3.right;
    public float moveSpeed = 2f;

    [Header("Smooth Stop / Go")]
    public float acceleration = 2.5f;
    public float deceleration = 4f;
    public float safeDistance = 0.25f;
    public float followDistanceFactor = 1.6f;
    public float followDistanceFactor2 = 2.5f;

    private float currentSpeed;
    private SheepSpawner ownerSpawner;

    public void Initialize(Vector3 direction, float speed, SheepSpawner spawner)
    {
        moveDirection = direction.normalized;
        moveSpeed = speed;
        currentSpeed = 0f;
        ownerSpawner = spawner;
    }

    private void Update()
    {
        float desiredSpeed = GetDesiredSpeed();
        float targetChange = desiredSpeed >= currentSpeed ? acceleration : deceleration;
        currentSpeed = Mathf.MoveTowards(currentSpeed, desiredSpeed, targetChange * Time.deltaTime);

        transform.position += moveDirection * currentSpeed * Time.deltaTime;
    }

    private float GetDesiredSpeed()
    {
        float desired = moveSpeed;

        dog nearestDog = FindNearestDog();
        if (nearestDog != null && !nearestDog.allowSheepToMove)
        {
            Vector3 toDog = nearestDog.transform.position - transform.position;
            float alongMovement = Vector3.Dot(toDog, moveDirection.normalized);

            if (alongMovement > 0f)
            {
                float startSlowDistance = nearestDog.stopRadius;
                float fullStopDistance = nearestDog.stopDistanceFromDog;

                if (alongMovement <= startSlowDistance)
                {
                    float slowdownRatio = Mathf.InverseLerp(fullStopDistance, startSlowDistance, alongMovement);
                    desired = Mathf.Min(desired, moveSpeed * slowdownRatio);

                    if (alongMovement <= fullStopDistance)
                    {
                        desired = 0f;
                    }
                }
            }
        }

        Sheep nearestFrontSheep = null;
        float nearestDistance = float.MaxValue;

        foreach (Sheep sheep in FindObjectsOfType<Sheep>())
        {
            if (sheep == this)
                continue;

            Vector3 toOther = sheep.transform.position - transform.position;
            float forwardDistance = Vector3.Dot(toOther, moveDirection.normalized);

            if (forwardDistance > 0f && forwardDistance < nearestDistance)
            {
                nearestDistance = forwardDistance;
                nearestFrontSheep = sheep;
            }
        }

        if (nearestFrontSheep != null)
        {
            if (nearestDistance < safeDistance)
            {
                desired = Mathf.Min(desired, 0f);
            }
            else if (nearestDistance < safeDistance * followDistanceFactor)
            {
                desired = Mathf.Min(desired, nearestFrontSheep.currentSpeed * 0.6f);
            }
            else if (nearestDistance < safeDistance * followDistanceFactor2)
            {
                desired = Mathf.Min(desired, nearestFrontSheep.currentSpeed * 0.85f);
            }
        }

        return Mathf.Max(0f, desired);
    }

    private dog FindNearestDog()
    {
        dog nearest = null;
        float nearestDistance = float.MaxValue;

        foreach (dog dogObject in FindObjectsOfType<dog>())
        {
            float distance = Vector3.Distance(transform.position, dogObject.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = dogObject;
            }
        }

        return nearest;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("SheepEnd"))
            return;

        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.RegisterSheepReachedEnd();
        }

        Destroy(gameObject);
    }

    public Vector3 MoveDirection => moveDirection;
    public float MoveSpeed => moveSpeed;
    public float CurrentSpeed => currentSpeed;
}
