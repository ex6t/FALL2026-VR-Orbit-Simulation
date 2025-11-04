using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.EventSystems;
using Valve.VR.InteractionSystem;

/// <summary>
/// PlanetNavigator controls entering per-planet camera views and the UI navigation.
/// - All planet child Cameras are initially disabled in Start().
/// - Press Space to start navigation (opens the UI and activates the first planet camera).
/// - Next/Prev UI buttons switch the active planet camera.
/// - ReturnToShip deactivates the planet camera, re-enables the player's HMD cameras, and returns the player rig to shipTransform.
/// 
/// This implementation does NOT disable the player's rig GameObject; instead it disables only the HMD Camera(s)
/// and AudioListener so the selected planet Camera can render while controllers/pointers remain active.
/// </summary>
public class PlanetNavigator : MonoBehaviour
{
    [Header("Planets (Transforms)")]
    [Tooltip("Assign the planet root transforms in order (Mercury = 0, Venus = 1, ...).")]
    public List<Transform> planetTransforms;

    [Header("Planet Data (ScriptableObjects, optional)")]
    [Tooltip("Optional PlanetInfo assets for UI display (must match planetTransforms length).")]
    public List<PlanetInfo> planetInfos;

    [Header("UI (Screen Space Canvas)")]
    public Canvas planetCanvas;               // Screen-space canvas (will be switched to ScreenSpaceCamera when planet camera active)
    public Text planetNameText;
    public Text planetDescriptionText;
    public Button nextButton;
    public Button prevButton;
    public Button returnToShipButton;

    [Header("Ship / Hub")]
    [Tooltip("Where the Player rig will be moved when returning to the ship.")]
    public Transform shipTransform;

    // internal state
    private int currentPlanetIndex = -1;
    private bool navigationActive = false;

    // store references so we can restore them when exiting planet view
    private Camera[] disabledHmdCameras = null;
    private AudioListener disabledHmdAudioListener = null;

    // if we add an AudioListener to the planet camera at runtime, keep a reference so we can remove it
    private AudioListener addedPlanetAudioListener = null;

    // the currently active planet camera
    private Camera activePlanetCamera = null;

    // If true, force the canvas to use ScreenSpaceCamera (useful when started by keyboard on desktop)
    private bool forceScreenSpaceMode = false;

    void Start()
    {
        // Hide UI on start
        if (planetCanvas != null)
            planetCanvas.gameObject.SetActive(false);

        // Hook up buttons (if assigned)
        if (nextButton != null) nextButton.onClick.AddListener(OnNextPlanet);
        if (prevButton != null) prevButton.onClick.AddListener(OnPrevPlanet);
        if (returnToShipButton != null) returnToShipButton.onClick.AddListener(ReturnToShip);

        // Ensure there's an EventSystem so UI buttons can be clicked in desktop play
        if (EventSystem.current == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();
            Debug.Log("[PlanetNavigator] Created missing EventSystem for UI interaction.");
        }

        // Ensure all planet cameras are turned off at start (defensive)
        if (planetTransforms != null)
        {
            Debug.Log("[PlanetNavigator] Start: scanning planetTransforms for child cameras and attaching parent watchers.");
            foreach (var t in planetTransforms)
            {
                if (t == null) continue;
                Camera cam = t.GetComponentInChildren<Camera>(true);
                if (cam != null)
                {
                    Debug.Log($"  Planet '{t.name}': child Camera '{cam.name}' at path '{GetHierarchyPath(cam.transform)}' (activeSelf={cam.gameObject.activeSelf}).");
                    // attach a parent-change watcher so we can detect reparenting at runtime
                    var watcher = cam.gameObject.GetComponent<PlanetCameraParentWatcher>();
                    if (watcher == null)
                        watcher = cam.gameObject.AddComponent<PlanetCameraParentWatcher>();
                    watcher.Initialize(GetHierarchyPath(cam.transform));
                    // attach an activation watcher so we can see who enables the camera GameObject at runtime
                    var actWatcher = cam.gameObject.GetComponent<PlanetCameraActivationWatcher>();
                    if (actWatcher == null)
                        actWatcher = cam.gameObject.AddComponent<PlanetCameraActivationWatcher>();
                    actWatcher.Initialize(GetHierarchyPath(cam.transform));
                    // Force-disable all planet cameras at Start to ensure a consistent initial state
                    cam.enabled = false;
                    if (cam.gameObject.activeSelf)
                        cam.gameObject.SetActive(false);
                }
                else
                {
                    Debug.LogWarning($"  Planet '{t.name}': no child Camera found at Start.");
                }
            }
        }
    }

    // small helper to get readable path used by logs (we declare it here to keep file self-contained)
    private string GetHierarchyPath(Transform t)
    {
        if (t == null) return "<null>";
        string path = t.name;
        var p = t.parent;
        while (p != null)
        {
            path = p.name + "/" + path;
            p = p.parent;
        }
        return path;
    }

    void Update()
    {
        // Trigger navigation with XR primary button (right hand) - Oculus 'A' maps to primaryButton -
        // fall back to Space key for desktop testing. If started by keyboard we force ScreenSpace canvas mode
        bool startPressed = false;
        bool keyboardPressed = Input.GetKeyDown(KeyCode.Space);

        try
        {
            var rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            if (rightHand.isValid)
            {
                bool primaryPressed = false;
                if (rightHand.TryGetFeatureValue(CommonUsages.primaryButton, out primaryPressed) && primaryPressed)
                {
                    startPressed = true;
                }
            }
        }
        catch (Exception)
        {
            // ignore XR errors
        }

        // keyboard fallback
        if (!startPressed && keyboardPressed) startPressed = true;

        if (!navigationActive && startPressed)
        {
            // if started from keyboard, force ScreenSpaceCamera mode so the HUD is visible in desktop tests
            forceScreenSpaceMode = keyboardPressed;
            Debug.Log("[PlanetNavigator] Start input detected. keyboard=" + keyboardPressed + ", forcingScreenSpace=" + forceScreenSpaceMode);
            StartPlanetTask();
        }
    }

    public void StartPlanetTask()
    {
        if (!ListsValid())
        {
            Debug.LogError("[PlanetNavigator] planetTransforms and planetInfos (if provided) must be assigned and planetTransforms must be non-empty.");
            return;
        }

        currentPlanetIndex = 0;
        navigationActive = true;

        if (planetCanvas != null)
            planetCanvas.gameObject.SetActive(true);

        ActivatePlanetCamera(currentPlanetIndex);
        UpdateUIForIndex(currentPlanetIndex);
    }

    private bool ListsValid()
    {
        if (planetTransforms == null || planetTransforms.Count == 0) return false;
        if (planetInfos != null && planetInfos.Count > 0 && planetInfos.Count != planetTransforms.Count) return false;
        return true;
    }

    private void ActivatePlanetCamera(int index)
    {
        if (!ListsValid())
        {
            Debug.LogError("[PlanetNavigator] ActivatePlanetCamera: lists invalid.");
            return;
        }
        if (index < 0 || index >= planetTransforms.Count)
        {
            Debug.LogError($"[PlanetNavigator] ActivatePlanetCamera: index {index} out of range.");
            return;
        }

        // Deactivate previous planet camera if any
        if (activePlanetCamera != null)
        {
            activePlanetCamera.gameObject.SetActive(false);
            activePlanetCamera = null;
        }

        Transform planet = planetTransforms[index];
        if (planet == null)
        {
            Debug.LogError($"[PlanetNavigator] planetTransforms[{index}] is null.");
            return;
        }

        // Find Camera under this planet (including inactive)
        Camera planetCam = planet.GetComponentInChildren<Camera>(true);
        if (planetCam == null)
        {
            Debug.LogError($"[PlanetNavigator] No Camera child found under planet '{planet.name}'. Add a Camera GameObject as a child and position it for the close-up view.");
            return;
        }

        // Disable HMD camera(s) so planet camera can render, but keep Player rig and controllers active
        disabledHmdCameras = null;
        disabledHmdAudioListener = null;
        if (Player.instance != null)
        {
            Transform hmd = Player.instance.hmdTransform;
            if (hmd != null)
            {
                var cams = hmd.GetComponentsInChildren<Camera>(true);
                if (cams != null && cams.Length > 0)
                {
                    disabledHmdCameras = new Camera[cams.Length];
                    for (int i = 0; i < cams.Length; i++)
                    {
                        disabledHmdCameras[i] = cams[i];
                        cams[i].enabled = false;
                    }
                }

                var audio = hmd.GetComponentInChildren<AudioListener>(true);
                if (audio != null)
                {
                    disabledHmdAudioListener = audio;
                    audio.enabled = false;
                }
            }
        }
        else
        {
            // fallback: disable main camera
            if (Camera.main != null) Camera.main.enabled = false;
        }

        // Start activation in coroutine to avoid conflicts with other scripts that run in Start/Awake
        StartCoroutine(ActivatePlanetCameraCoroutine(planetCam));

        // Update UI and button states
        UpdateUIForIndex(index);
        if (prevButton != null) prevButton.interactable = index > 0;
        if (nextButton != null) nextButton.interactable = index < planetTransforms.Count - 1;
    }

    private System.Collections.IEnumerator ActivatePlanetCameraCoroutine(Camera planetCam)
    {
        // wait a frame so other Start/Awake logic can finish (avoids other code reparenting/enabling cameras after we act)
        yield return null;

        if (planetCam == null)
            yield break;

        Debug.Log($"[PlanetNavigator] Activating planet camera '{planetCam.name}' at path '{GetHierarchyPath(planetCam.transform)}'.");

        // Activate planet camera
        planetCam.enabled = true;
        planetCam.gameObject.SetActive(true);
        activePlanetCamera = planetCam;

        // Ensure at least one AudioListener exists/enabled so Unity doesn't spam warnings
        EnsureAudioListenerForActiveCamera();

        // Configure the canvas to use the planet camera so UI is rendered in the camera view and hit-testable
        if (planetCanvas != null)
        {
            bool xrActive = false;
            try { xrActive = UnityEngine.XR.XRSettings.isDeviceActive; } catch (Exception) { xrActive = false; }

            if (xrActive && !forceScreenSpaceMode)
            {
                planetCanvas.renderMode = RenderMode.WorldSpace;
                var rt = planetCanvas.GetComponent<RectTransform>();
                planetCanvas.transform.SetParent(planetCam.transform, false);
                rt.localPosition = new Vector3(0f, 0f, 2.0f);
                rt.localRotation = Quaternion.identity;
                rt.localScale = Vector3.one * 0.0025f;
                planetCanvas.worldCamera = planetCam;
                planetCanvas.gameObject.SetActive(true);
                Debug.Log("[PlanetNavigator] Using WorldSpace canvas for VR (placed 2m in front of planet camera).");

                try
                {
                    planetCam.stereoTargetEye = StereoTargetEyeMask.Both;
                    planetCam.rect = new Rect(0f, 0f, 1f, 1f);
                    planetCam.targetTexture = null;
                }
                catch (Exception) { }
            }
            else
            {
                planetCanvas.renderMode = RenderMode.ScreenSpaceCamera;
                planetCanvas.worldCamera = planetCam;
                if (planetCanvas.planeDistance < 0.01f) planetCanvas.planeDistance = 1f;
                planetCanvas.gameObject.SetActive(true);
            }
        }

        yield break;
    }

    private void DeactivatePlanetCamera()
    {
        // Deactivate the active planet camera if present
        if (activePlanetCamera != null)
        {
            activePlanetCamera.gameObject.SetActive(false);
            activePlanetCamera = null;
        }

        // Restore HMD cameras
        if (disabledHmdCameras != null)
        {
            foreach (var cam in disabledHmdCameras)
            {
                if (cam != null) cam.enabled = true;
            }
            disabledHmdCameras = null;
        }
        if (disabledHmdAudioListener != null)
        {
            disabledHmdAudioListener.enabled = true;
            disabledHmdAudioListener = null;
        }

        // If we added a runtime AudioListener to the planet camera, remove it when exiting
        if (addedPlanetAudioListener != null)
        {
            Destroy(addedPlanetAudioListener);
            addedPlanetAudioListener = null;
            Debug.Log("[PlanetNavigator] Removed runtime-added AudioListener from planet camera.");
        }

        // Hide UI canvas after leaving planet view
        if (planetCanvas != null)
        {
            planetCanvas.gameObject.SetActive(false);
            // Optionally reset to overlay if desired:
            // planetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            // planetCanvas.worldCamera = null;
        }
    }

    private void UpdateUIForIndex(int index)
    {
        if (planetInfos != null && planetInfos.Count == planetTransforms.Count)
        {
            PlanetInfo info = planetInfos[index];
            if (planetNameText != null) planetNameText.text = (info != null && !string.IsNullOrEmpty(info.planetName)) ? info.planetName : planetTransforms[index].name;
            if (planetDescriptionText != null) planetDescriptionText.text = (info != null && !string.IsNullOrEmpty(info.description)) ? info.description : "No description set.";
        }
        else
        {
            // If no planetInfos provided, use transform names as fallback
            if (planetNameText != null) planetNameText.text = planetTransforms[index].name;
            if (planetDescriptionText != null) planetDescriptionText.text = "";
        }
    }

    private void OnNextPlanet()
    {
        if (!navigationActive) return;
        if (currentPlanetIndex < planetTransforms.Count - 1)
        {
            currentPlanetIndex++;
            ActivatePlanetCamera(currentPlanetIndex);
        }
    }

    private void OnPrevPlanet()
    {
        if (!navigationActive) return;
        if (currentPlanetIndex > 0)
        {
            currentPlanetIndex--;
            ActivatePlanetCamera(currentPlanetIndex);
        }
    }

    public void ReturnToShip()
    {
        if (!navigationActive) return;

        // Deactivate current planet camera and restore HMD cameras & audio
        DeactivatePlanetCamera();

        // Move the Player rig back to ship location (if available)
        if (Player.instance != null && shipTransform != null)
        {
            Player.instance.transform.position = shipTransform.position;
            Player.instance.transform.rotation = shipTransform.rotation;
        }

        navigationActive = false;
        currentPlanetIndex = -1;
    }

    // Ensure at least one AudioListener is enabled when we switch to the planet camera.
    private void EnsureAudioListenerForActiveCamera()
    {
        if (activePlanetCamera == null)
        {
            Debug.LogWarning("[PlanetNavigator] EnsureAudioListenerForActiveCamera called but activePlanetCamera is null.");
            return;
        }

        var allListeners = FindObjectsOfType<AudioListener>(true);
        int enabledCount = 0;
        AudioListener firstDisabled = null;
        foreach (var l in allListeners)
        {
            if (l == null) continue;
            if (l.enabled) enabledCount++;
            else if (firstDisabled == null) firstDisabled = l;
        }

        if (enabledCount > 0)
        {
            Debug.Log($"[PlanetNavigator] AudioListeners present: total={allListeners.Length}, enabled={enabledCount}.");
            return; // someone else is listening
        }

        // enable an AudioListener on the active camera if it already has one
        var camListener = activePlanetCamera.GetComponent<AudioListener>();
        if (camListener != null)
        {
            camListener.enabled = true;
            Debug.Log($"[PlanetNavigator] Enabled existing AudioListener on planet camera '{activePlanetCamera.name}'.");
            return;
        }

        // enable an existing disabled listener if present
        if (firstDisabled != null)
        {
            firstDisabled.enabled = true;
            Debug.Log($"[PlanetNavigator] Enabled existing disabled AudioListener on '{firstDisabled.gameObject.name}'.");
            return;
        }

        // otherwise add one to the planet camera
        addedPlanetAudioListener = activePlanetCamera.gameObject.AddComponent<AudioListener>();
        addedPlanetAudioListener.enabled = true;
        Debug.Log($"[PlanetNavigator] No AudioListener found; added AudioListener to planet camera '{activePlanetCamera.name}'.");
    }

    // A lightweight component attached to planet camera GameObjects to detect re-parenting at runtime
    private class PlanetCameraParentWatcher : MonoBehaviour
    {
        private Transform lastParent = null;
        private string initialPath = null;

        public void Initialize(string currentPath)
        {
            lastParent = transform.parent;
            initialPath = currentPath;
        }

        void Awake()
        {
            if (lastParent == null) lastParent = transform.parent;
        }

        void OnTransformParentChanged()
        {
            var newParent = transform.parent;
            string oldPath = lastParent == null ? "<null>" : GetPath(lastParent);
            string newPath = newParent == null ? "<null>" : GetPath(newParent);
            Debug.LogError($"[PlanetNavigator][ParentWatcher] Camera '{gameObject.name}' parent changed. Old='{oldPath}' New='{newPath}'. Initial='{initialPath}'. Stack:\n{Environment.StackTrace}");
            lastParent = newParent;
        }

        private string GetPath(Transform t)
        {
            if (t == null) return "<null>";
            string path = t.name;
            var p = t.parent;
            while (p != null)
            {
                path = p.name + "/" + path;
                p = p.parent;
            }
            return path;
        }
    }

    // Watcher that logs when a camera GameObject becomes enabled/disabled so we can trace who activated it
    private class PlanetCameraActivationWatcher : MonoBehaviour
    {
        private string initialPath;
        public void Initialize(string path)
        {
            initialPath = path;
        }

        void OnEnable()
        {
            UnityEngine.Debug.LogError($"[PlanetNavigator][ActivationWatcher] GameObject enabled: '{gameObject.name}' path='{initialPath}' currentParent='{(transform.parent? GetPath(transform.parent): "<null>")}'. Stack:\n{Environment.StackTrace}", this);
        }

        void OnDisable()
        {
            UnityEngine.Debug.Log($"[PlanetNavigator][ActivationWatcher] GameObject disabled: '{gameObject.name}' path='{initialPath}'", this);
        }

        private string GetPath(Transform t)
        {
            if (t == null) return "<null>";
            string path = t.name;
            var p = t.parent;
            while (p != null)
            {
                path = p.name + "/" + path;
                p = p.parent;
            }
            return path;
        }
    }
}