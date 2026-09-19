using UnityEngine;

public class TrafficLightController : MonoBehaviour
{
    [Header("Traffic Light State")]
    public bool isRed = true;

    [Header("Visuals")]
    public SpriteRenderer redLightRenderer;
    public SpriteRenderer greenLightRenderer;

    public bool IsRed => isRed;

    private void Start()
    {
        ApplyVisualState();
    }

    public void SetRed(bool value)
    {
        isRed = value;
        ApplyVisualState();
    }

    public void Toggle()
    {
        SetRed(!isRed);
    }

    private void ApplyVisualState()
    {
        if (redLightRenderer != null)
            redLightRenderer.enabled = isRed;

        if (greenLightRenderer != null)
            greenLightRenderer.enabled = !isRed;
    }
}
