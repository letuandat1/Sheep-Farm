using UnityEngine;

public class dog : MonoBehaviour
{
    [Header("Dog Toggle")]
    public bool allowSheepToMove = true;

    [Header("Stop Area")]
    public float stopRadius = 3f;
    public float stopDistanceFromDog = 1.2f;

    private void Start()
    {
        SheepTrafficState.SetMovementAllowed(allowSheepToMove);
    }

    private void OnMouseDown()
    {
        if (FindAnyObjectByType<FarmerController>() != null)
            return;

        ToggleMovement();
    }

    public void ToggleMovement()
    {
        allowSheepToMove = !allowSheepToMove;
        SheepTrafficState.SetMovementAllowed(allowSheepToMove);
    }
}

