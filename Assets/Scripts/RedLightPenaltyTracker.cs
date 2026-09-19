using UnityEngine;
using UnityEngine.UI;

public class RedLightPenaltyTracker : MonoBehaviour
{
    [Header("References")]
    public TrafficLightController trafficLight;

    [Header("Detection")]
    public float stopSpeedThreshold = 0.05f;
    public float firstWarningDuration = 10f;
    public float xInterval = 10f;

    [Header("UI")]
    public Image clockImage;
    public Sprite clockSprite;
    public Sprite inactiveClockSprite;
    public Image[] xMarks;
    public Sprite xMarkSprite;
    public Sprite emptyXMarkSprite;

    private float stopTimer;
    private float penaltyTimer;

    private void Update()
    {
        if (GameFlowManager.Instance == null || GameFlowManager.Instance.isGameEnded)
            return;

        if (trafficLight == null)
        {
            trafficLight = FindAnyObjectByType<TrafficLightController>();
        }

        bool redLightIsOn = trafficLight != null && trafficLight.IsRed;
        bool anySheepStopped = IsAnySheepStopped();

        if (!redLightIsOn || !anySheepStopped)
        {
            ResetStopState();
            UpdateClockUi(0f, GameFlowManager.Instance != null ? GameFlowManager.Instance.redLightXMarks : 0);
            return;
        }

        stopTimer += Time.deltaTime;

        if (stopTimer >= firstWarningDuration)
        {
            penaltyTimer += Time.deltaTime;
            while (penaltyTimer >= xInterval)
            {
                penaltyTimer -= xInterval;
                GameFlowManager.Instance.RegisterRedLightDelay();
            }
        }

        UpdateClockUi(stopTimer, GameFlowManager.Instance.redLightXMarks);
    }

    private void ResetStopState()
    {
        stopTimer = 0f;
        penaltyTimer = 0f;
    }

    private bool IsAnySheepStopped()
    {
        Sheep[] sheepList = FindObjectsByType<Sheep>(FindObjectsInactive.Include);
        foreach (Sheep sheep in sheepList)
        {
            if (sheep == null || !sheep.gameObject.activeInHierarchy)
                continue;

            if (sheep.CurrentSpeed <= stopSpeedThreshold)
                return true;
        }

        SheepSplineMover[] splineSheep = FindObjectsByType<SheepSplineMover>(FindObjectsInactive.Include);
        foreach (SheepSplineMover sheep in splineSheep)
        {
            if (sheep == null || !sheep.gameObject.activeInHierarchy)
                continue;

            if (sheep.currentSpeed <= stopSpeedThreshold)
                return true;
        }

        return false;
    }

    private void UpdateClockUi(float timerValue, int xCount)
    {
        if (clockImage != null)
        {
            clockImage.sprite = timerValue > 0f ? clockSprite : inactiveClockSprite;
            clockImage.gameObject.SetActive(timerValue > 0f || xCount > 0);
        }

        if (xMarks == null)
            return;

        for (int i = 0; i < xMarks.Length; i++)
        {
            if (xMarks[i] == null)
                continue;

            bool isActive = i < xCount;
            xMarks[i].gameObject.SetActive(isActive || timerValue > 0f);
            xMarks[i].sprite = isActive ? xMarkSprite : emptyXMarkSprite;
        }
    }
}
