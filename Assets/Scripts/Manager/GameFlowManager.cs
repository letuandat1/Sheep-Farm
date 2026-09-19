using System;
using UnityEngine;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    [Header("Win / Lose")]
    public int sheepToWin = 10;
    public int maxRedLightXMarks = 10;

    [Header("Runtime")]
    public int sheepReachedEnd { get; private set; }
    public int redLightXMarks { get; private set; }
    public int winProgress { get; private set; }
    public int loseProgress { get; private set; }
    public bool isGameEnded { get; private set; }

    public event Action<int> OnSheepReachedEndChanged;
    public event Action<int> OnRedLightXChanged;
    public event Action<int> OnWinProgressChanged;
    public event Action<int> OnLoseProgressChanged;
    public event Action<string> OnGameEnded;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RegisterSheepReachedEnd()
    {
        if (isGameEnded)
            return;

        sheepReachedEnd++;
        winProgress = Mathf.Clamp(sheepReachedEnd, 0, sheepToWin);
        OnSheepReachedEndChanged?.Invoke(sheepReachedEnd);
        OnWinProgressChanged?.Invoke(winProgress);

        if (sheepReachedEnd >= sheepToWin)
        {
            EndGame("Win");
        }
    }

    public void RegisterHazardLoss(string reason)
    {
        loseProgress = Mathf.Clamp(loseProgress + 1, 0, maxRedLightXMarks);
        OnLoseProgressChanged?.Invoke(loseProgress);
        EndGame(reason);
    }

    public void RegisterRedLightDelay()
    {
        if (isGameEnded)
            return;

        redLightXMarks++;
        loseProgress = Mathf.Clamp(redLightXMarks, 0, maxRedLightXMarks);
        OnRedLightXChanged?.Invoke(redLightXMarks);
        OnLoseProgressChanged?.Invoke(loseProgress);

        if (redLightXMarks >= maxRedLightXMarks)
        {
            EndGame("Đèn đỏ quá lâu");
        }
    }

    public void ResetRedLightPenalty()
    {
        redLightXMarks = 0;
        loseProgress = 0;
        OnRedLightXChanged?.Invoke(redLightXMarks);
        OnLoseProgressChanged?.Invoke(loseProgress);
    }

    public void EndGame(string reason)
    {
        if (isGameEnded)
            return;

        isGameEnded = true;
        OnGameEnded?.Invoke(reason);
        Time.timeScale = 0f;
        Debug.Log("Game ended: " + reason);
    }
}
