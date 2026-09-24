using UnityEngine;

public class EnemyStraightMover : MonoBehaviour
{
    [Header("Movement")]
    public Vector3 moveDirection = Vector3.left;
    public float moveSpeed = 2f;

    [Header("Disappear")]
    public Transform disappearPoint;
    public float disappearDistance = 0.2f;

    [Header("Player Hit")]
    public string playerTag = "Player";
    public string loseReason = "Kẻ địch va vào người chơi";

    private bool hasHitPlayer;

    private void Update()
    {
        transform.position += moveDirection.normalized * moveSpeed * Time.deltaTime;

        if (disappearPoint != null && Vector3.Distance(transform.position, disappearPoint.position) <= disappearDistance)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TryHitPlayer(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        TryHitPlayer(collision.gameObject);
    }

    private void TryHitPlayer(GameObject other)
    {
        if (other.GetComponentInParent<SheepSplineMover>() != null)
            return;

        if (hasHitPlayer || GameFlowManager.Instance == null || GameFlowManager.Instance.isGameEnded)
            return;

        if (!other.CompareTag(playerTag) && !other.transform.root.CompareTag(playerTag))
            return;

        hasHitPlayer = true;
        GameFlowManager.Instance.RegisterHazardLoss(loseReason);
        Destroy(gameObject);
    }
}