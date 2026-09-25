using UnityEngine;
using System.Collections.Generic;

public class WolfController : MonoBehaviour
{
    [Header("Targeting")]
    public float moveSpeed = 1.2f;
    public float catchDistance = 1.2f;

    [Header("Escape")]
    public BoxCollider exitArea;
    public float carrySpeed = 2.5f;
    public string loseReason = "Sói bắt mất cừu";

    private SheepSplineMover targetSheep;
    private GameObject carriedSheep;
    private bool isCarrying;
    private bool lossRegistered;
    private Vector3 exitDirection;
    private bool isFleeing;
    private Vector3 fleeDirection;
    private float fleeEndTime;
    private Rigidbody carriedSheepRigidbody;
    private readonly List<Collider> carriedSheepColliders = new List<Collider>();

    public bool IsFleeing => isFleeing;

    private void Update()
    {
        if (GameFlowManager.Instance != null && GameFlowManager.Instance.isGameEnded)
            return;

        if (isFleeing)
        {
            MoveTowards(transform.position + fleeDirection, carrySpeed);
            if (Time.time >= fleeEndTime)
                Destroy(gameObject);
            return;
        }

        if (isCarrying)
        {
            MoveToExit();
            return;
        }

        if (targetSheep == null || !targetSheep.isActiveAndEnabled || targetSheep.IsBeingCarried)
            targetSheep = FindNearestSheep();

        if (targetSheep == null)
            return;

        Vector3 sheepPosition = targetSheep.transform.position;
        sheepPosition.y = transform.position.y;
        MoveTowards(sheepPosition, moveSpeed);
        if (Vector3.Distance(transform.position, targetSheep.transform.position) <= catchDistance)
            CaptureSheep();
    }

    private void CaptureSheep()
    {
        if (targetSheep == null || !targetSheep.TryStartBeingCarried(transform))
            return;

        carriedSheep = targetSheep.transform.root.gameObject;
        carriedSheepRigidbody = carriedSheep.GetComponent<Rigidbody>();
        if (carriedSheepRigidbody != null)
        {
            carriedSheepRigidbody.isKinematic = true;
            carriedSheepRigidbody.useGravity = false;
            carriedSheepRigidbody.linearVelocity = Vector3.zero;
            carriedSheepRigidbody.angularVelocity = Vector3.zero;
        }

        carriedSheepColliders.AddRange(carriedSheep.GetComponentsInChildren<Collider>());
        foreach (Collider collider in carriedSheepColliders)
            collider.enabled = false;

        carriedSheep.transform.SetParent(transform);
        carriedSheep.transform.localPosition = Vector3.zero;
        CalculateExitDirection();
        isCarrying = true;
    }

    private void MoveToExit()
    {
        if (exitArea == null)
        {
            DropCarriedSheep();
            return;
        }

        MoveTowards(transform.position + exitDirection, carrySpeed);
        if (!exitArea.bounds.Contains(transform.position))
            DropCarriedSheep();
    }

    private void DropCarriedSheep()
    {
        if (!lossRegistered && carriedSheep != null)
        {
            lossRegistered = true;
            GameFlowManager.Instance?.RegisterSheepLost(carriedSheep, loseReason);
            Destroy(carriedSheep);
        }

        Destroy(gameObject);
    }

    public void FleeFrom(Vector3 threatPosition, float duration, float speed)
    {
        if (isFleeing)
            return;

        if (isCarrying)
            ReleaseCarriedSheep();

        Vector3 direction = transform.position - threatPosition;
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.001f)
            direction = -transform.forward;

        fleeDirection = direction.normalized;
        carrySpeed = Mathf.Max(carrySpeed, speed);
        fleeEndTime = Time.time + Mathf.Max(0.1f, duration);
        isFleeing = true;
        targetSheep = null;
    }

    private void ReleaseCarriedSheep()
    {
        if (carriedSheep == null)
        {
            isCarrying = false;
            return;
        }

        Vector3 releasePosition = carriedSheep.transform.position;
        SheepSplineMover sheepMover = carriedSheep.GetComponentInChildren<SheepSplineMover>();

        carriedSheep.transform.SetParent(null);

        if (sheepMover != null)
            sheepMover.ReleaseFromCarrier(releasePosition);

        if (carriedSheepRigidbody != null)
        {
            carriedSheepRigidbody.isKinematic = false;
            carriedSheepRigidbody.useGravity = true;
            carriedSheepRigidbody.linearVelocity = Vector3.zero;
            carriedSheepRigidbody.angularVelocity = Vector3.zero;
        }

        foreach (Collider collider in carriedSheepColliders)
        {
            if (collider != null)
                collider.enabled = true;
        }

        carriedSheepColliders.Clear();
        carriedSheep = null;
        carriedSheepRigidbody = null;
        isCarrying = false;
    }

    private void MoveTowards(Vector3 destination, float speed)
    {
        Vector3 direction = destination - transform.position;
        if (direction.sqrMagnitude <= 0.001f)
            return;

        transform.position = Vector3.MoveTowards(transform.position, destination, speed * Time.deltaTime);
        transform.forward = Vector3.Lerp(transform.forward, direction.normalized, 8f * Time.deltaTime);
    }

    private void CalculateExitDirection()
    {
        Bounds bounds = exitArea.bounds;
        Vector3 position = transform.position;
        float distanceToLeft = Mathf.Abs(position.x - bounds.min.x);
        float distanceToRight = Mathf.Abs(bounds.max.x - position.x);
        float distanceToBack = Mathf.Abs(position.z - bounds.min.z);
        float distanceToFront = Mathf.Abs(bounds.max.z - position.z);

        float nearestDistance = distanceToLeft;
        exitDirection = Vector3.left;

        if (distanceToRight < nearestDistance)
        {
            nearestDistance = distanceToRight;
            exitDirection = Vector3.right;
        }

        if (distanceToBack < nearestDistance)
        {
            nearestDistance = distanceToBack;
            exitDirection = Vector3.back;
        }

        if (distanceToFront < nearestDistance)
            exitDirection = Vector3.forward;
    }

    private SheepSplineMover FindNearestSheep()
    {
        SheepSplineMover nearest = null;
        float nearestDistance = float.MaxValue;
        SheepSplineMover[] allSheep = FindObjectsByType<SheepSplineMover>(FindObjectsInactive.Exclude);

        foreach (SheepSplineMover sheep in allSheep)
        {
            if (sheep == null || !sheep.isActiveAndEnabled || sheep.IsBeingCarried)
                continue;

            float distance = (sheep.transform.position - transform.position).sqrMagnitude;
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = sheep;
            }
        }

        return nearest;
    }
}