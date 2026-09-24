using UnityEngine;

public class HazardTrigger : MonoBehaviour
{
    public enum HazardType
    {
        Cow,
        Wolf
    }

    public HazardType hazardType = HazardType.Cow;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<SheepSplineMover>() != null)
        {
            if (GetComponentInParent<EnemyStraightMover>() != null)
                return;

            if (GameFlowManager.Instance == null || GameFlowManager.Instance.isGameEnded)
                return;

            string reason = hazardType == HazardType.Cow ? "Bò cán" : "Sói mang đi mất";
            GameFlowManager.Instance.RegisterHazardLoss(reason);
        }
    }
}
