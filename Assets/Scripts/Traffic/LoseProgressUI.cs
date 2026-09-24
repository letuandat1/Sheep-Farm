using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoseProgressUI : MonoBehaviour
{
    [Header("Lose Progress")]
    public Image loseFillBar;
    public TMP_Text loseText;
    public int loseGoal = 10;

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

        GameFlowManager.Instance.OnLoseProgressChanged += UpdateLoseProgress;
    }

    private void UnsubscribeFromGameFlow()
    {
        if (GameFlowManager.Instance == null)
            return;

        GameFlowManager.Instance.OnLoseProgressChanged -= UpdateLoseProgress;
    }

    private void RefreshFromGameFlow()
    {
        if (GameFlowManager.Instance == null)
            return;

        UpdateLoseProgress(GameFlowManager.Instance.loseProgress);
    }

    private void UpdateLoseProgress(int value)
    {
        int current = Mathf.Clamp(value, 0, loseGoal);

        if (loseFillBar != null)
            loseFillBar.fillAmount = loseGoal > 0 ? (float)current / loseGoal : 0f;

        if (loseText != null)
            loseText.text = current + "/" + loseGoal;
    }
}