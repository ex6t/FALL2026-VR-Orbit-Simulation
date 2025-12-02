using UnityEngine;
using UnityEngine.InputSystem; 

public class TabletSummoner : MonoBehaviour
{
    [Header("References")]
    public Transform playerCamera; // VR camera
    public Transform tabletCanvas; // tablet canvas root

    [Header("Placement Settings")]
    public float distanceInFront = 1.0f;
    public float heightOffset = -0.2f;

    public bool IsTabletOpen => tabletCanvas != null && tabletCanvas.gameObject.activeSelf;

    void Update()
    {

    }

    // FUNCTION TO TOGGLE TABLET
    public void ToggleAndPlaceTablet()
    {
        if (!IsTabletOpen) OpenTablet(); // Open if closed
        else CloseTablet(); // Close if already open
    }

    // FUNCTION TO OPEN TABLET
    public void OpenTablet()
    {
        if (tabletCanvas == null || playerCamera == null)
        {
            Debug.LogWarning("[TabletSummoner] Missing references.");
            return;
        }

        tabletCanvas.gameObject.SetActive(true);

        Vector3 forward = playerCamera.forward;
        forward.y = 0f;
        forward.Normalize();

        Vector3 targetPos = playerCamera.position
                            + forward * distanceInFront
                            + Vector3.up * heightOffset;

        tabletCanvas.position = targetPos;
        tabletCanvas.rotation = Quaternion.LookRotation(forward);
    }

    // FUNCTION TO CLOSE TABLET
    public void CloseTablet()
    {
        if (tabletCanvas != null)
        {
            tabletCanvas.gameObject.SetActive(false);
        }
    }
}
