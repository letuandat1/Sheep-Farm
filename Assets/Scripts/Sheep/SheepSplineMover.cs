using UnityEngine;
using UnityEngine.Splines;

public class SheepSplineMover : MonoBehaviour
{
    [Header("Spline")]
    public SplineContainer spline;
    public float startProgress = 0f;
    public bool loop = false;

    [Header("Movement")]
    public float normalSpeed = 1.5f;
    public float currentSpeed;
    public float acceleration = 2.5f;
    public float deceleration = 4f;

    [Header("Stop Before Dog")]
    public dog dogController;
    public float stopRadius = 2.5f;
    public float stopDistance = 0.7f;

    [Header("Queue Follow")]
    public float safeDistance = 1.5f;
    public float followDistanceFactor = 1.5f;
    public float followDistanceFactor2 = 2.2f;

    public bool IsAngry { get; private set; }
    public bool IsBeingCarried { get; private set; }
    public float Progress => progress;

    private float totalLength;
    private float progress;
    private float targetSpeed;
    private TrafficLightController trafficLight;
    private bool brakingAtRedLight;
    private bool lostToEnemy;

    private void Start()
    {
        trafficLight = FindAnyObjectByType<TrafficLightController>();

        if (dogController == null)
        {
            dogController = FindAnyObjectByType<dog>();
        }

        if (spline != null)
        {
            totalLength = spline.Spline.GetLength();
        }

        progress = startProgress;
        currentSpeed = 0f;
        targetSpeed = normalSpeed;
    }

    private void Update()
    {
        if (IsBeingCarried)
            return;

        if (spline == null)
            return;

        if (trafficLight == null)
        {
            trafficLight = FindAnyObjectByType<TrafficLightController>();
        }

        bool redLightIsNear = IsTrafficLightRedNearSheep();
        if (redLightIsNear)
        {
            brakingAtRedLight = true;
        }
        else if (SheepTrafficState.IsMovementAllowed)
        {
            brakingAtRedLight = false;
        }

        if (brakingAtRedLight)
        {
            targetSpeed = 0f;
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, deceleration * Time.deltaTime);

            if (currentSpeed > 0.01f)
            {
                progress += currentSpeed * Time.deltaTime;
            }

            return;
        }

        UpdateTargetSpeed();
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, (targetSpeed >= currentSpeed ? acceleration : deceleration) * Time.deltaTime);

        if (currentSpeed > 0.01f)
        {
            progress += currentSpeed * Time.deltaTime;
        }

        if (progress >= totalLength)
        {
            if (loop)
            {
                progress %= totalLength;
            }
            else
            {
                if (GameFlowManager.Instance != null)
                {
                    if (!IsAngry)
                        GameFlowManager.Instance.RegisterSheepReachedEnd();
                }

                Destroy(gameObject);
                return;
            }
        }

        if (progress < 0f)
        {
            progress = 0f;
        }

        Vector3 pos = spline.EvaluatePosition(progress / totalLength);
        transform.position = pos;
    }

    private bool IsTrafficLightRedNearSheep()
    {
        if (trafficLight != null && trafficLight.ShouldStopAt(transform.position))
            return true;

        TrafficLightController[] trafficLights = FindObjectsByType<TrafficLightController>(FindObjectsInactive.Include);
        foreach (TrafficLightController light in trafficLights)
        {
            if (light != null && light.ShouldStopAt(transform.position))
                return true;
        }

        return false;
    }

    private void UpdateTargetSpeed()
    {
        float desired = normalSpeed;

        SheepSplineMover frontSheep = FindNearestFrontSheep();
        if (frontSheep != null)
        {
            float progressGap = frontSheep.progress - progress;

            if (progressGap < safeDistance)
            {
                desired = Mathf.Min(desired, 0f);
            }
            else if (progressGap < safeDistance * followDistanceFactor)
            {
                desired = Mathf.Min(desired, frontSheep.currentSpeed * 0.6f);
            }
            else if (progressGap < safeDistance * followDistanceFactor2)
            {
                desired = Mathf.Min(desired, frontSheep.currentSpeed * 0.85f);
            }
        }

        if (dogController != null && dogController.gameObject.activeInHierarchy && dogController.enabled && !dogController.allowSheepToMove)
        {
            float distanceToDog = Vector3.Distance(transform.position, dogController.transform.position);
            if (distanceToDog < stopRadius)
            {
                float brakeT = Mathf.InverseLerp(stopRadius, stopDistance, distanceToDog);
                float easedBrake = Mathf.SmoothStep(0f, 1f, brakeT);
                desired = Mathf.Min(desired, normalSpeed * (1f - easedBrake));

                if (distanceToDog <= stopDistance)
                {
                    desired = Mathf.Min(desired, 0.05f);
                }
            }
        }

        targetSpeed = Mathf.Max(0f, desired);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<EnemyStraightMover>() != null)
        {
            LoseToEnemy();
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponentInParent<EnemyStraightMover>() != null)
        {
            LoseToEnemy();
        }
    }

    private void LoseToEnemy()
    {
        if (lostToEnemy)
            return;

        lostToEnemy = true;
        GameFlowManager.Instance?.RegisterSheepLost(gameObject, "Cừu bị enemy bắt");
        Destroy(gameObject);
    }

    public bool TryStartBeingCarried(Transform carrier)
    {
        if (IsBeingCarried || lostToEnemy || carrier == null)
            return false;

        IsBeingCarried = true;
        lostToEnemy = true;
        enabled = false;
        return true;
    }

    public void ReleaseFromCarrier(Vector3 releasePosition)
    {
        if (!IsBeingCarried)
            return;

        IsBeingCarried = false;
        lostToEnemy = false;

        if (spline != null && totalLength > 0f)
        {
            float closestT = 0f;
            float closestDistanceSq = float.MaxValue;
            int sampleCount = 200;

            for (int i = 0; i <= sampleCount; i++)
            {
                float t = i / (float)sampleCount;
                Vector3 samplePosition = spline.EvaluatePosition(t);
                float distanceSq = (samplePosition - releasePosition).sqrMagnitude;
                if (distanceSq < closestDistanceSq)
                {
                    closestDistanceSq = distanceSq;
                    closestT = t;
                }
            }

            progress = closestT * totalLength;
        }

        transform.position = releasePosition;
        enabled = true;
    }

    public void MarkAngry()
    {
        IsAngry = true;
    }

    private SheepSplineMover FindNearestFrontSheep()
    {
        SheepSplineMover nearest = null;
        float smallestProgressGap = float.MaxValue;

        SheepSplineMover[] allSheep = FindObjectsByType<SheepSplineMover>(FindObjectsInactive.Include);
        foreach (var sheep in allSheep)
        {
            if (sheep == null || sheep == this || !sheep.isActiveAndEnabled)
                continue;

            if (spline != null && sheep.spline != spline)
                continue;

            float progressGap = sheep.progress - progress;
            if (progressGap <= 0f)
                continue;

            if (progressGap < smallestProgressGap)
            {
                smallestProgressGap = progressGap;
                nearest = sheep;
            }
        }

        return nearest;
    }
}
