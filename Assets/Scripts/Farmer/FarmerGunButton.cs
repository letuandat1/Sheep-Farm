using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class FarmerGunButton : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public FarmerController farmer;
    public RectTransform aimImage;

    private Button button;
    private bool isAiming;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.interactable = true;

        if (aimImage != null)
        {
            aimImage.gameObject.SetActive(false);
            Image image = aimImage.GetComponent<Image>();
            if (image != null)
                image.raycastTarget = false;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (farmer == null)
            return;

        isAiming = true;
        SetAimImagePosition(eventData.position);
        if (aimImage != null)
            aimImage.gameObject.SetActive(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isAiming)
            return;

        SetAimImagePosition(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isAiming)
            return;

        isAiming = false;

        if (farmer != null)
        {
            if (farmer.ShootNearestWolfIfInRange())
            {
                if (aimImage != null)
                    aimImage.gameObject.SetActive(false);
                return;
            }

            if (!farmer.ShootWolfAtScreenPosition(eventData.position))
                farmer.MoveToScreenPosition(eventData.position);
        }

        if (aimImage != null)
            aimImage.gameObject.SetActive(false);
    }

    private void SetAimImagePosition(Vector2 screenPosition)
    {
        if (aimImage != null)
            aimImage.position = screenPosition;
    }
}