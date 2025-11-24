using UnityEngine;
using UnityEngine.InputSystem; // only if you're using the new Input System

public class TabletSummoner : MonoBehaviour
{
    [Header("References")]
    public Transform playerCamera;      // VR camera
    public Transform tabletCanvas;      // The root transform of the 2D tablet canvas

    [Header("Placement Settings")]
    public float distanceInFront = 1.0f; // How far in front of the player
    public float heightOffset = -0.2f;   // Slightly lower than eye level

    void Update()
    {
        // OPTIONAL: Only if you also want Gamepad A to trigger this
        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            ToggleAndPlaceTablet();
        }
    }

    public void ToggleAndPlaceTablet()
    {
        // Toggle active
        bool newActive = !tabletCanvas.gameObject.activeSelf;
        tabletCanvas.gameObject.SetActive(newActive);

        if (!newActive) return; // if we just turned it off, don't reposition

        // Place it in front of player
        Vector3 forward = playerCamera.forward;
        forward.y = 0f;                 // keep it horizontal so it doesn’t tilt up/down weirdly
        forward.Normalize();

        Vector3 targetPos = playerCamera.position
                            + forward * distanceInFront
                            + Vector3.up * heightOffset;

        tabletCanvas.position = targetPos;

        // Make it face the player
        tabletCanvas.rotation = Quaternion.LookRotation(forward);
    }
}
