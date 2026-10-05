using UnityEngine;
using UnityEngine.UI;

public class TabletSummoner : MonoBehaviour
{
    [Header("References")]
    public Transform playerCamera; // VR camera
    public Transform tabletCanvas; // tablet canvas root

    [Header("Placement Settings")]
    public float distanceInFront = 1.0f;
    public float heightOffset = -0.2f;
    public bool openOnStart = false;

    public bool IsTabletOpen => tabletCanvas != null && tabletCanvas.gameObject.activeSelf;

    void Start()
    {
        ResolvePlayerCamera();

        if (openOnStart)
            OpenTablet();
        else
            CloseTablet();
    }

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
        if (tabletCanvas == null)
        {
            Debug.LogWarning("[TabletSummoner] Missing tablet canvas reference.");
            return;
        }

        ResolvePlayerCamera();
        if (playerCamera == null)
        {
            Debug.LogWarning("[TabletSummoner] Missing player camera reference.");
            return;
        }

        Vector3 forward = playerCamera.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.001f)
            forward = playerCamera.forward;
        forward.Normalize();

        Vector3 targetPos = playerCamera.position
                            + forward * distanceInFront
                            + Vector3.up * heightOffset;

        Canvas canvas = tabletCanvas.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = playerCamera.GetComponent<Camera>();
        }

        GraphicRaycaster raycaster = tabletCanvas.GetComponent<GraphicRaycaster>();
        if (raycaster != null)
        {
            raycaster.enabled = true;
            raycaster.ignoreReversedGraphics = false;
        }

        tabletCanvas.position = targetPos;
        tabletCanvas.rotation = Quaternion.LookRotation(-forward);
        tabletCanvas.gameObject.SetActive(true);
    }

    // FUNCTION TO CLOSE TABLET
    public void CloseTablet()
    {
        if (tabletCanvas != null)
        {
            tabletCanvas.gameObject.SetActive(false);
        }
    }

    private void ResolvePlayerCamera()
    {
        if (IsPlayerCamera(playerCamera))
            return;

        playerCamera = null;

        GameObject hubStation = GameObject.Find("HubStation");
        if (hubStation != null)
        {
            Transform player = hubStation.transform.Find("Player");
            if (player != null)
            {
                Camera playerRigCamera = player.GetComponentInChildren<Camera>(true);
                if (IsPlayerCamera(playerRigCamera != null ? playerRigCamera.transform : null))
                {
                    playerCamera = playerRigCamera.transform;
                    return;
                }
            }
        }

        Camera mainCamera = Camera.main;
        if (IsPlayerCamera(mainCamera != null ? mainCamera.transform : null))
        {
            playerCamera = mainCamera.transform;
            return;
        }

        Camera[] cameras = FindObjectsOfType<Camera>(true);
        foreach (Camera camera in cameras)
        {
            if (IsPlayerCamera(camera.transform))
            {
                playerCamera = camera.transform;
                return;
            }
        }
    }

    private bool IsPlayerCamera(Transform cameraTransform)
    {
        if (cameraTransform == null)
            return false;

        Camera camera = cameraTransform.GetComponent<Camera>();
        if (camera == null || camera.targetTexture != null)
            return false;

        if (camera.CompareTag("MainCamera"))
            return true;

        Transform current = cameraTransform;
        while (current != null)
        {
            if (current.name == "Player")
                return true;

            current = current.parent;
        }

        return cameraTransform.name == "Main Camera";
    }
}
