using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class FarmerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float movementPlaneY = 0f;
    public Camera movementCamera;
    public float avoidanceRadius = 0.35f;
    public float avoidanceDistance = 0.15f;
    public float avoidanceStepAngle = 15f;
    public float maxAvoidanceAngle = 75f;
    public LayerMask obstacleMask = -1;
    public float idleAvoidanceRadius = 1.1f;
    public float idleAvoidanceSpeed = 2f;

    [Header("Gun")]
    public Transform gun;
    public Transform shootOrigin;
    public float gunRange = 4f;
    public float shootInterval = 1f;
    public float wolfFleeDuration = 3f;
    public float wolfFleeSpeed = 3.5f;

    private Vector3 destination;
    private Vector3 smoothedAvoidanceDirection;
    private bool hasDestination;
    private float nextShotTime;

    private void Start()
    {
        if (movementCamera == null)
            movementCamera = Camera.main;

        destination = transform.position;
        movementPlaneY = transform.position.y;
    }

    private void Update()
    {
        if (GameFlowManager.Instance != null && GameFlowManager.Instance.isGameEnded)
            return;

        if (TryGetMoveInput(out Vector2 screenPosition))
            SetDestination(screenPosition);

        if (hasDestination)
            MoveToDestination();
        else
            AvoidNearbyAnimalsWhileIdle();
    }

    private bool TryGetMoveInput(out Vector2 screenPosition)
    {
        screenPosition = default;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            screenPosition = Mouse.current.position.ReadValue();
            if (TryToggleDog(screenPosition))
                return false;

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return false;

            return true;
        }

        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            int touchId = Touchscreen.current.primaryTouch.touchId.ReadValue();
            screenPosition = Touchscreen.current.primaryTouch.position.ReadValue();
            if (TryToggleDog(screenPosition))
                return false;

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(touchId))
                return false;

            return true;
        }

        return false;
    }

    private void SetDestination(Vector2 screenPosition)
    {
        if (movementCamera == null)
            return;

        Ray ray = movementCamera.ScreenPointToRay(screenPosition);
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.GetComponentInParent<dog>() != null)
            return;

        Plane movementPlane = new Plane(Vector3.up, new Vector3(0f, movementPlaneY, 0f));
        if (movementPlane.Raycast(ray, out float distance))
        {
            destination = ray.GetPoint(distance);
            destination.y = movementPlaneY;
            hasDestination = true;
        }
    }

    private bool TryToggleDog(Vector2 screenPosition)
    {
        if (movementCamera == null)
            return false;

        Ray ray = movementCamera.ScreenPointToRay(screenPosition);
        RaycastHit[] hits = Physics.RaycastAll(ray);
        foreach (RaycastHit hit in hits)
        {
            dog dogController = hit.collider.GetComponentInParent<dog>();
            if (dogController == null)
                continue;

            dogController.ToggleMovement();
            return true;
        }

        return false;
    }

    private void MoveToDestination()
    {
        if (!hasDestination)
            return;

        Vector3 direction = destination - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude <= 0.01f)
        {
            hasDestination = false;
            return;
        }

        Vector3 desiredDirection = direction.normalized;
        Vector3 calculatedAvoidanceDirection = CalculateAvoidanceDirection();
        if (calculatedAvoidanceDirection.sqrMagnitude > 0.001f)
        {
            smoothedAvoidanceDirection = Vector3.Slerp(
                smoothedAvoidanceDirection,
                calculatedAvoidanceDirection,
                Mathf.Clamp01(8f * Time.deltaTime));
        }
        else
        {
            smoothedAvoidanceDirection = Vector3.Slerp(
                smoothedAvoidanceDirection,
                Vector3.zero,
                Mathf.Clamp01(2f * Time.deltaTime));
        }

        Vector3 avoidanceDirection = smoothedAvoidanceDirection;
        Vector3 moveDirection = desiredDirection;

        if (avoidanceDirection.sqrMagnitude > 0.001f)
        {
            float combinedWeight = 0.55f;
            moveDirection = (desiredDirection * (1f - combinedWeight) + avoidanceDirection * combinedWeight).normalized;
        }

        Vector3 nextPosition = transform.position + moveDirection * moveSpeed * Time.deltaTime;
        nextPosition.y = movementPlaneY;

        transform.position = nextPosition;
        transform.forward = Vector3.Lerp(transform.forward, moveDirection.normalized, 10f * Time.deltaTime);
    }

    private Vector3 CalculateAvoidanceDirection()
    {
        float detectionRadius = avoidanceRadius + avoidanceDistance;
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, detectionRadius, obstacleMask, QueryTriggerInteraction.Collide);
        Vector3 combinedPush = Vector3.zero;
        bool hasObstacle = false;

        foreach (Collider collider in nearbyColliders)
        {
            if (collider == null || collider.transform == transform || collider.gameObject == gameObject)
                continue;

            bool isSheep = collider.GetComponentInParent<SheepSplineMover>() != null;
            bool isWolf = collider.GetComponentInParent<WolfController>() != null;
            bool isCow = collider.GetComponentInParent<EnemyStraightMover>() != null;
            if (collider.isTrigger && !isSheep && !isWolf && !isCow)
                continue;

            if (collider.CompareTag("Ground") || collider.CompareTag("Terrain") || collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                continue;

            Bounds bounds = collider.bounds;
            float colliderHeight = bounds.size.y;
            float heightDifference = Mathf.Abs(bounds.center.y - transform.position.y);
            if (colliderHeight > 1.5f && heightDifference < 0.8f)
                continue;

            Vector3 closestPoint = collider.ClosestPoint(transform.position);
            Vector3 toCollider = transform.position - closestPoint;
            toCollider.y = 0f;
            float distance = toCollider.magnitude;
            if (distance < 0.0001f)
            {
                toCollider = transform.position - bounds.center;
                toCollider.y = 0f;
            }

            if (toCollider.sqrMagnitude < 0.0001f)
                continue;

            float clearance = Mathf.Max(0f, avoidanceDistance - distance);
            float weight = clearance > 0f ? 1f + clearance / Mathf.Max(0.01f, avoidanceDistance) : 0.15f;
            combinedPush += toCollider.normalized * weight;
            hasObstacle = true;
        }

        if (!hasObstacle)
            return Vector3.zero;

        combinedPush.y = 0f;
        return combinedPush.normalized;
    }

    private void AvoidNearbyAnimalsWhileIdle()
    {
        Collider[] nearbyColliders = Physics.OverlapSphere(
            transform.position,
            idleAvoidanceRadius,
            obstacleMask,
            QueryTriggerInteraction.Collide);

        Collider nearestAnimal = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider collider in nearbyColliders)
        {
            if (!IsAnimalCollider(collider))
                continue;

            Vector3 closestPoint = collider.ClosestPoint(transform.position);
            float distance = (closestPoint - transform.position).sqrMagnitude;
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestAnimal = collider;
            }
        }

        if (nearestAnimal == null)
            return;

        Vector3 awayFromAnimal = transform.position - nearestAnimal.bounds.center;
        awayFromAnimal.y = 0f;
        if (awayFromAnimal.sqrMagnitude < 0.001f)
            awayFromAnimal = -transform.forward;

        Vector3 sideDirection = Vector3.Cross(Vector3.up, awayFromAnimal.normalized);
        if (Vector3.Dot(sideDirection, transform.right) < 0f)
            sideDirection = -sideDirection;

        Vector3 nextPosition = transform.position + sideDirection * idleAvoidanceSpeed * Time.deltaTime;
        nextPosition.y = movementPlaneY;
        transform.position = nextPosition;
        transform.forward = Vector3.Lerp(transform.forward, sideDirection, 8f * Time.deltaTime);
    }

    private bool IsAnimalCollider(Collider collider)
    {
        if (collider == null)
            return false;

        return collider.GetComponentInParent<SheepSplineMover>() != null
            || collider.GetComponentInParent<WolfController>() != null
            || collider.GetComponentInParent<EnemyStraightMover>() != null;
    }

    public bool CanShootWolf()
    {
        return Time.time >= nextShotTime && FindNearestWolf() != null;
    }

    public void ShootWolf()
    {
        if (Time.time < nextShotTime)
            return;

        WolfController nearestWolf = FindNearestWolf();
        ShootWolf(nearestWolf);
    }

    public bool ShootWolfAtScreenPosition(Vector2 screenPosition)
    {
        if (movementCamera == null || Time.time < nextShotTime)
            return false;

        Ray ray = movementCamera.ScreenPointToRay(screenPosition);
        WolfController wolf = FindWolfUnderScreenPosition(ray);
        if (wolf == null || wolf.IsFleeing || !IsWolfInRange(wolf))
            return false;

        ShootWolf(wolf);
        return true;
    }

    public bool ShootNearestWolfIfInRange()
    {
        if (Time.time < nextShotTime)
            return false;

        WolfController nearestWolf = FindNearestWolf();
        if (nearestWolf == null || nearestWolf.IsFleeing || !IsWolfInRange(nearestWolf))
            return false;

        ShootWolf(nearestWolf);
        return true;
    }

    public void MoveToScreenPosition(Vector2 screenPosition)
    {
        SetDestination(screenPosition);
    }

    private WolfController FindWolfUnderScreenPosition(Ray ray)
    {
        RaycastHit[] hits = Physics.RaycastAll(ray);
        foreach (RaycastHit hit in hits)
        {
            WolfController wolf = hit.collider.GetComponentInParent<WolfController>();
            if (wolf != null)
                return wolf;
        }

        return null;
    }

    private void ShootWolf(WolfController wolf)
    {
        if (wolf == null || Time.time < nextShotTime)
            return;

        Vector3 direction = wolf.transform.position - transform.position;
        direction.y = 0f;
        if (direction.sqrMagnitude > 0.001f)
        {
            transform.forward = Vector3.Lerp(transform.forward, direction.normalized, 12f * Time.deltaTime);
            if (gun != null)
                gun.forward = direction.normalized;
        }

        wolf.FleeFrom(transform.position, wolfFleeDuration, wolfFleeSpeed);
        nextShotTime = Time.time + Mathf.Max(0.1f, shootInterval);
    }

    private bool IsWolfInRange(WolfController wolf)
    {
        return wolf != null && (wolf.transform.position - transform.position).sqrMagnitude <= gunRange * gunRange;
    }

    private WolfController FindNearestWolf()
    {
        WolfController nearest = null;
        float nearestDistance = gunRange * gunRange;
        WolfController[] wolves = FindObjectsByType<WolfController>(FindObjectsInactive.Exclude);

        foreach (WolfController wolf in wolves)
        {
            if (wolf == null || !wolf.isActiveAndEnabled || wolf.IsFleeing)
                continue;

            float distance = (wolf.transform.position - transform.position).sqrMagnitude;
            if (distance <= nearestDistance)
            {
                nearestDistance = distance;
                nearest = wolf;
            }
        }

        return nearest;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, gunRange);
    }
}