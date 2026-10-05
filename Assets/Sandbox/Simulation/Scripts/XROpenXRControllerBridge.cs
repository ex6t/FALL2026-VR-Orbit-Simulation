using UnityEngine;
using UnityEngine.XR;

public class XROpenXRControllerBridge : MonoBehaviour
{
    public XRNode controllerNode = XRNode.RightHand;
    public bool driveControllerPose = true;
    public bool createFallbackVisual = true;
    public bool createFallbackRay = true;
    public float fallbackRayLength = 4f;

    private GameObject fallbackVisual;
    private LineRenderer fallbackRay;

    void Awake()
    {
        DisableLegacyPoseDrivers();

        if (createFallbackVisual)
            EnsureFallbackVisual();

        if (createFallbackRay)
            EnsureFallbackRay();

        UpdateControllerPose();
    }

    void OnEnable()
    {
        Application.onBeforeRender += UpdateControllerPose;
    }

    void OnDisable()
    {
        Application.onBeforeRender -= UpdateControllerPose;
    }

    void LateUpdate()
    {
        UpdateControllerPose();
        UpdateFallbackRay();
    }

    private void DisableLegacyPoseDrivers()
    {
        MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour == null || behaviour == this)
                continue;

            System.Type type = behaviour.GetType();
            string typeName = type != null ? type.FullName : string.Empty;
            if (typeName.Contains("SteamVR_Behaviour_Pose"))
                behaviour.enabled = false;
        }
    }

    private void EnsureFallbackVisual()
    {
        if (fallbackVisual != null)
            return;

        fallbackVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fallbackVisual.name = "OpenXR Controller Visual";
        fallbackVisual.transform.SetParent(transform, false);
        fallbackVisual.transform.localPosition = new Vector3(0f, 0f, 0.08f);
        fallbackVisual.transform.localRotation = Quaternion.identity;
        fallbackVisual.transform.localScale = new Vector3(0.08f, 0.08f, 0.18f);

        Collider collider = fallbackVisual.GetComponent<Collider>();
        if (collider != null)
            Destroy(collider);

        Renderer renderer = fallbackVisual.GetComponent<Renderer>();
        if (renderer != null)
            renderer.material.color = controllerNode == XRNode.LeftHand ? new Color(0.15f, 0.45f, 1f, 1f) : new Color(1f, 0.25f, 0.2f, 1f);
    }

    private void EnsureFallbackRay()
    {
        if (fallbackRay != null)
            return;

        GameObject rayObject = new GameObject("OpenXR Controller Ray");
        rayObject.transform.SetParent(transform, false);
        fallbackRay = rayObject.AddComponent<LineRenderer>();
        fallbackRay.useWorldSpace = false;
        fallbackRay.positionCount = 2;
        fallbackRay.startWidth = 0.01f;
        fallbackRay.endWidth = 0.003f;
        fallbackRay.material = new Material(Shader.Find("Sprites/Default"));
        fallbackRay.startColor = new Color(0.2f, 0.9f, 1f, 0.85f);
        fallbackRay.endColor = new Color(0.2f, 0.9f, 1f, 0.05f);
    }

    private void UpdateControllerPose()
    {
        if (!driveControllerPose)
            return;

        InputDevice controller = InputDevices.GetDeviceAtXRNode(controllerNode);
        Vector3 localPosition = DefaultLocalPosition();
        Quaternion localRotation = Quaternion.identity;

        if (controller.isValid)
        {
            controller.TryGetFeatureValue(CommonUsages.devicePosition, out localPosition);
            controller.TryGetFeatureValue(CommonUsages.deviceRotation, out localRotation);
        }

        // Preserve the authored Player/HubStation/root transforms; only each hand's local pose is driven by controller tracking.
        transform.localPosition = localPosition;
        transform.localRotation = localRotation;
    }

    private Vector3 DefaultLocalPosition()
    {
        float x = controllerNode == XRNode.LeftHand ? -0.25f : 0.25f;
        return new Vector3(x, 1.15f, 0.35f);
    }

    private void UpdateFallbackRay()
    {
        if (fallbackRay == null)
            return;

        fallbackRay.SetPosition(0, Vector3.forward * 0.08f);
        fallbackRay.SetPosition(1, Vector3.forward * fallbackRayLength);
    }
}
