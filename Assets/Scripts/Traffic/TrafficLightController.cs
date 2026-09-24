using UnityEngine;

public class TrafficLightController : MonoBehaviour
{
    [Header("Stop Area")]
    public float stopRadius = 1.5f;

    public bool IsRed => !SheepTrafficState.IsMovementAllowed;

    public bool ShouldStopAt(Vector3 position)
    {
        return IsRed && Vector3.Distance(transform.position, position) <= stopRadius;
    }

    public void SetRed(bool value)
    {
        SheepTrafficState.SetMovementAllowed(!value);
    }

    public void Toggle()
    {
        SheepTrafficState.Toggle();
    }
}
