using UnityEngine;
using UnityEngine.UI;

public class ProgressUI : MonoBehaviour
{
    [Header("Win Progress")]
    public int winGoal = 10;
    public Image winFillBar;
    public Text winText;

    [Header("Lose Progress")]
    public int loseGoal = 10;
    public Image loseFillBar;
    public Text loseText;

    private void OnEnable()
    {
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.OnWinProgressChanged += UpdateWinProgress;
            GameFlowManager.Instance.OnLoseProgressChanged += UpdateLoseProgress;

            UpdateWinProgress(GameFlowManager.Instance.winProgress);
            UpdateLoseProgress(GameFlowManager.Instance.loseProgress);
        }
    }

    private void OnDisable()
    {
        if (GameFlowManager.Instance != null)
        {
            GameFlowManager.Instance.OnWinProgressChanged -= UpdateWinProgress;
            GameFlowManager.Instance.OnLoseProgressChanged -= UpdateLoseProgress;
        }
    }

    private void Start()
    {
        if (GameFlowManager.Instance != null)
        {
            UpdateWinProgress(GameFlowManager.Instance.winProgress);
            UpdateLoseProgress(GameFlowManager.Instance.loseProgress);
        }
    }

    public void UpdateWinProgress(int value)
    {
        int clamped = Mathf.Clamp(value, 0, winGoal);
        if (winFillBar != null)
        {
            winFillBar.fillAmount = winGoal <= 0 ? 0f : (float)clamped / winGoal;
        }

        if (winText != null)
        {
            winText.text = clamped + "/" + winGoal;
        }

        bool shouldShow = clamped > 0;
        if (gameObject.activeSelf != shouldShow)
        {
            gameObject.SetActive(shouldShow);
        }
    }

    public void UpdateLoseProgress(int value)
    {
        int clamped = Mathf.Clamp(value, 0, loseGoal);
        if (loseFillBar != null)
        {
            loseFillBar.fillAmount = loseGoal <= 0 ? 0f : (float)clamped / loseGoal;
        }

        if (loseText != null)
        {
            loseText.text = clamped + "/" + loseGoal;
        }

        bool shouldShow = clamped > 0;
        if (gameObject.activeSelf != shouldShow)
        {
            gameObject.SetActive(shouldShow);
        }
    }
}
