using UnityEngine;

public class CanvasFollower : MonoBehaviour
{
    [Header("Target to follow")]
    public Transform playerCamera;      // VR camera / head

    [Header("Placement Settings")]
    public float distanceInFront = 1.2f;
    public float heightOffset = 0.0f;

    [Header("Rotation")]
    public bool keepUpright = true;   // ignore camera tilt
    public bool faceCamera = true;   // turn to face the player

    void LateUpdate()
    {
        if (playerCamera == null) return;

        // Direction in front of the camera
        Vector3 forward = playerCamera.forward;

        if (keepUpright)
        {
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f)
                forward = playerCamera.forward;
            forward.Normalize();
        }

        // Position canvas in front of the player
        Vector3 targetPos =
            playerCamera.position +
            forward * distanceInFront +
            Vector3.up * heightOffset;

        transform.position = targetPos;

        // Rotate to face the player
        if (faceCamera)
        {
            if (keepUpright)
            {
                transform.rotation = Quaternion.LookRotation(forward);
            }
            else
            {
                Vector3 lookDir = transform.position - playerCamera.position;
                transform.rotation = Quaternion.LookRotation(lookDir);
            }
        }
    }
}
