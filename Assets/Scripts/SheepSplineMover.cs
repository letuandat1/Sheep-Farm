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
    public float safeDistance = 0.3f;
    public float followDistanceFactor = 1.5f;
    public float followDistanceFactor2 = 2.2f;

    private float totalLength;
    private float progress;
    private float targetSpeed;

    private void Start()
    {
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
        if (spline == null)
            return;

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
