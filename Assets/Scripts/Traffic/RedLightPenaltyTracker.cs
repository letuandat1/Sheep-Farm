using System.Collections.Generic;
using UnityEngine;

public class RedLightPenaltyTracker : MonoBehaviour
{
    [Header("Detection")]
    public float stopSpeedThreshold = 0.05f;
    public float firstWarningDuration = 10f;
    public float xInterval = 10f;

    private readonly Dictionary<int, SheepStopState> stoppedSheep = new Dictionary<int, SheepStopState>();

    private class SheepStopState
    {
        public SheepWaitingUI waitingUI;
        public float stopTimer;
        public float penaltyTimer;
        public bool penaltyReached;
    }

    private void Update()
    {
        if (GameFlowManager.Instance == null || GameFlowManager.Instance.isGameEnded)
            return;

        SheepSplineMover[] allSheep = FindObjectsByType<SheepSplineMover>(FindObjectsInactive.Include);
        HashSet<int> activeStoppedIds = new HashSet<int>();

        foreach (SheepSplineMover sheep in allSheep)
        {
            if (sheep == null || !sheep.gameObject.activeInHierarchy)
                continue;

            int sheepId = sheep.GetInstanceID();
            SheepWaitingUI waitingUI = sheep.GetComponentInChildren<SheepWaitingUI>(true);

            stoppedSheep.TryGetValue(sheepId, out SheepStopState existingState);

            if (sheep.currentSpeed > stopSpeedThreshold)
            {
                if (existingState != null && existingState.penaltyReached)
                {
                    activeStoppedIds.Add(sheepId);
                    waitingUI?.SetWaitingTime(existingState.stopTimer, firstWarningDuration, true);
                }
                else
                {
                    waitingUI?.SetWaitingTime(0f, firstWarningDuration, false);
                    stoppedSheep.Remove(sheepId);
                }

                continue;
            }

            activeStoppedIds.Add(sheepId);

            if (existingState == null)
            {
                existingState = new SheepStopState { waitingUI = waitingUI };
                stoppedSheep.Add(sheepId, existingState);
            }

            SheepStopState state = existingState;
            if (state.penaltyReached)
            {
                state.waitingUI?.SetWaitingTime(state.stopTimer, firstWarningDuration, true);
                continue;
            }

            if (state.waitingUI == null)
            {
                state.waitingUI = waitingUI;
            }

            state.stopTimer += Time.deltaTime;
            if (state.stopTimer >= firstWarningDuration)
            {
                state.penaltyTimer += Time.deltaTime;
                while (state.penaltyTimer >= xInterval)
                {
                    state.penaltyTimer -= xInterval;
                    GameFlowManager.Instance.RegisterSheepStandingPenalty(sheep.gameObject);
                    sheep.MarkAngry();
                    state.penaltyReached = true;
                }
            }

            state.waitingUI?.SetWaitingTime(state.stopTimer, firstWarningDuration, state.penaltyReached);
        }

        List<int> stoppedIds = new List<int>(stoppedSheep.Keys);
        foreach (int sheepId in stoppedIds)
        {
            if (!activeStoppedIds.Contains(sheepId))
                stoppedSheep.Remove(sheepId);
        }
    }
}
