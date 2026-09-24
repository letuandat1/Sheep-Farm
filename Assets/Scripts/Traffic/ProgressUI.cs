using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressUI : MonoBehaviour
{
    [Header("Win Progress")]
    public Image winFillBar;
    public TMP_Text winText;
    public int winGoal = 10;

    private void Start()
    {
        SubscribeToGameFlow();
        RefreshFromGameFlow();
    }

    private void OnDestroy()
    {
        UnsubscribeFromGameFlow();
    }

    private void SubscribeToGameFlow()
    {
        if (GameFlowManager.Instance == null)
            return;

        GameFlowManager.Instance.OnWinProgressChanged += UpdateWinProgress;
    }

    private void UnsubscribeFromGameFlow()
    {
        if (GameFlowManager.Instance == null)
            return;

        GameFlowManager.Instance.OnWinProgressChanged -= UpdateWinProgress;
    }

    private void RefreshFromGameFlow()
    {
        if (GameFlowManager.Instance == null)
            return;

        UpdateWinProgress(GameFlowManager.Instance.winProgress);
    }

    public void UpdateWinProgress(int value)
    {
        int current = Mathf.Clamp(value, 0, winGoal);
        UpdateFillBar(winFillBar, current, winGoal);
        UpdateText(winText, current, winGoal);
    }

    private void UpdateFillBar(Image fillBar, int current, int goal)
    {
        if (fillBar == null)
            return;

        fillBar.fillAmount = goal > 0 ? (float)current / goal : 0f;
    }

    private void UpdateText(TMP_Text text, int current, int goal)
    {
        if (text == null)
            return;

        text.text = current + "/" + goal;
    }
}
