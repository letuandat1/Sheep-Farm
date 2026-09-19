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
        if (GameFlowManager.Instance == null || GameFlowManager.Instance.isGameEnded)
            return;

        if (other.GetComponentInParent<Sheep>() != null || other.GetComponentInParent<SheepSplineMover>() != null)
        {
            string reason = hazardType == HazardType.Cow ? "Bò cán" : "Sói mang đi mất";
            GameFlowManager.Instance.RegisterHazardLoss(reason);
        }
    }
}
