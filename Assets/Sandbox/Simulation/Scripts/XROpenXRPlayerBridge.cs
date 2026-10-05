using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class XROpenXRPlayerBridge : MonoBehaviour
{
    public Camera playerCamera;
    public bool driveHeadPose = true;
    public bool disableOtherCamerasOnAwake = true;
    public bool useFloorTrackingOrigin = true;
    public float fallbackHeadHeight = 1.6f;

    private Transform playerRoot;

    void Awake()
    {
        if (playerCamera == null)
            playerCamera = GetComponent<Camera>();

        playerRoot = FindPlayerRoot();
        DisableLegacyCameraPoseDrivers();
        ConfigureTrackingOrigin();

        if (disableOtherCamerasOnAwake)
            DisableOtherCameras();

        UpdateHeadPose();
    }

    void OnEnable()
    {
        Application.onBeforeRender += UpdateHeadPose;
    }

    void OnDisable()
    {
        Application.onBeforeRender -= UpdateHeadPose;
    }

    void LateUpdate()
    {
        UpdateHeadPose();
    }

    private Transform FindPlayerRoot()
    {
        Transform current = transform;
        while (current != null)
        {
            if (current.name == "Player")
                return current;

            current = current.parent;
        }

        return transform.root;
    }

    private void DisableLegacyCameraPoseDrivers()
    {
        MonoBehaviour[] behaviours = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour == null || behaviour == this)
                continue;

            System.Type type = behaviour.GetType();
            string typeName = type != null ? type.FullName : string.Empty;
            if (typeName.Contains("SteamVR") || typeName.Contains("Valve.VR"))
                behaviour.enabled = false;
        }
    }

    private void ConfigureTrackingOrigin()
    {
        List<XRInputSubsystem> subsystems = new List<XRInputSubsystem>();
        SubsystemManager.GetInstances(subsystems);

        foreach (XRInputSubsystem subsystem in subsystems)
        {
            if (subsystem == null || !subsystem.running)
                continue;

            if (useFloorTrackingOrigin)
                subsystem.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor);

            subsystem.TryRecenter();
        }
    }

    private void DisableOtherCameras()
    {
        if (playerCamera == null)
            return;

        Camera[] cameras = FindObjectsOfType<Camera>(true);
        foreach (Camera camera in cameras)
        {
            if (camera == null || camera == playerCamera)
                continue;

            bool belongsToPlayer = playerRoot != null && camera.transform.IsChildOf(playerRoot);
            if (!belongsToPlayer || camera.CompareTag("MainCamera"))
                camera.enabled = false;
        }

        playerCamera.enabled = true;
        playerCamera.tag = "MainCamera";
    }

    private void UpdateHeadPose()
    {
        if (!driveHeadPose)
            return;

        InputDevice head = InputDevices.GetDeviceAtXRNode(XRNode.Head);
        Vector3 localPosition = Vector3.zero;
        Quaternion localRotation = Quaternion.identity;

        bool hasPosition = head.isValid && head.TryGetFeatureValue(CommonUsages.devicePosition, out localPosition);
        bool hasRotation = head.isValid && head.TryGetFeatureValue(CommonUsages.deviceRotation, out localRotation);

        if (!hasPosition)
            localPosition = Vector3.zero;

        if (!hasRotation)
            localRotation = Quaternion.identity;

        if (localPosition.sqrMagnitude < 0.0001f)
            localPosition = new Vector3(0f, fallbackHeadHeight, 0f);

        if (localRotation == Quaternion.identity && !hasRotation)
            localRotation = Quaternion.identity;

        // Preserve the authored Player/HubStation transforms; only the camera local pose is driven by headset tracking.
        transform.localPosition = localPosition;
        transform.localRotation = localRotation;
    }
}
