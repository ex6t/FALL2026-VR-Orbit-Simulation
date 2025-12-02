using UnityEngine;

public class CanvasFollower : MonoBehaviour
{
    [Header("References")]
    public Transform playerCamera;   // VR camera (head)
    public Transform canvasRoot;     // The canvas/root object to move

    [Header("Placement Settings")]
    public float distanceInFront = 1.0f;   // How far in front of the player
    public float heightOffset = -0.2f;   // Offset up/down relative to camera

    [Header("Rotation")]
    public bool keepUpright = true;        // Ignore camera tilt so canvas stays level

    private void LateUpdate()
    {
        if (playerCamera == null || canvasRoot == null) return;

        // Direction in front of camera
        Vector3 forward = playerCamera.forward;

        if (keepUpright)
        {
            // Keep canvas level (no pitching up/down)
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f)
                return;
            forward.Normalize();
        }

        // Position in front of the player
        Vector3 targetPos =
            playerCamera.position +
            forward * distanceInFront +
            Vector3.up * heightOffset;

        canvasRoot.position = targetPos;

        // Make the canvas face the player
        if (keepUpright)
        {
            canvasRoot.rotation = Quaternion.LookRotation(forward);
        }
        else
        {
            // Fully face the camera, including tilt
            Vector3 lookDir = canvasRoot.position - playerCamera.position;
            canvasRoot.rotation = Quaternion.LookRotation(lookDir);
        }
    }
}
