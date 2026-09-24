using UnityEngine;
using UnityEngine.UI;

public class SheepWaitingUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject warningRoot;
    public Image clockImage;
    public GameObject penaltyRoot;
    public Image penaltyImage;

    [Header("Position")]
    public Vector3 localOffset = new Vector3(0f, 1.2f, 0f);
    public float worldScale = 0.01f;

    private Camera mainCamera;
    private Transform sheepTransform;
    private Canvas parentCanvas;

    private void Awake()
    {
        mainCamera = Camera.main;
        SheepSplineMover mover = GetComponentInParent<SheepSplineMover>();
        sheepTransform = mover != null ? mover.transform : transform.root;

        if (warningRoot == null && clockImage != null)
            warningRoot = clockImage.gameObject;

        if (penaltyRoot == null && penaltyImage != null)
            penaltyRoot = penaltyImage.gameObject;

        if (warningRoot != null)
        {
            parentCanvas = warningRoot.GetComponentInParent<Canvas>(true);
            if (parentCanvas != null)
            {
                parentCanvas.renderMode = RenderMode.WorldSpace;
                parentCanvas.transform.localPosition = Vector3.zero;
                parentCanvas.transform.localRotation = Quaternion.identity;
                parentCanvas.transform.localScale = Vector3.one * worldScale;
            }

            RectTransform rootRect = warningRoot.GetComponent<RectTransform>();
            if (rootRect != null)
                rootRect.anchoredPosition = Vector2.zero;

            if (clockImage != null)
                clockImage.rectTransform.anchoredPosition = Vector2.zero;
        }

        if (penaltyRoot != null)
        {
            RectTransform penaltyRect = penaltyRoot.GetComponent<RectTransform>();
            if (penaltyRect != null)
                penaltyRect.anchoredPosition = Vector2.zero;

            penaltyRoot.SetActive(false);
        }

        SetWaitingTime(0f, 10f, false);
    }

    private void LateUpdate()
    {
        if (parentCanvas == null || sheepTransform == null)
            return;

        Transform uiTransform = parentCanvas.transform;
        uiTransform.position = sheepTransform.TransformPoint(localOffset);

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera != null)
            uiTransform.rotation = Quaternion.LookRotation(mainCamera.transform.position - uiTransform.position);
    }

    public void SetWaitingTime(float stoppedSeconds, float warningDuration, bool penaltyReached)
    {
        bool shouldShowWarning = stoppedSeconds >= warningDuration && !penaltyReached;
        if (parentCanvas != null)
            parentCanvas.gameObject.SetActive(true);

        if (clockImage != null)
            clockImage.gameObject.SetActive(shouldShowWarning);

        if (penaltyRoot != null)
            penaltyRoot.SetActive(penaltyReached);

        if (penaltyReached && penaltyRoot == null && penaltyImage != null)
            penaltyImage.gameObject.SetActive(true);
    }
}
