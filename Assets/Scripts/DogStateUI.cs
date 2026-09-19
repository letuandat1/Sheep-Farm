using UnityEngine;
using UnityEngine.UI;

public class DogStateUI : MonoBehaviour
{
    [Header("Traffic Light")]
    public Image statusImage;
    public Sprite allowSprite;
    public Sprite blockSprite;

    private void OnEnable()
    {
        SheepTrafficState.OnStateChanged += UpdateState;
        UpdateState(SheepTrafficState.IsMovementAllowed);
    }

    private void OnDisable()
    {
        SheepTrafficState.OnStateChanged -= UpdateState;
    }

    private void UpdateState(bool isAllowed)
    {
        if (statusImage == null)
            return;

        statusImage.sprite = isAllowed ? allowSprite : blockSprite;
    }
}
