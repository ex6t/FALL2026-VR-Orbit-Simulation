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

    [Header("UI (World Space Canvas)")]
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
    private List<Camera> planetCameras = new List<Camera>();
    //private Camera activePlanetCamera = null;
    private Camera[] disabledHmdCameras = null;
    private AudioListener disabledHmdAudioListener = null;

    // if we add an AudioListener to the planet camera at runtime, keep a reference so we can remove it
    private AudioListener addedPlanetAudioListener = null;

    // the currently active planet camera
    private Camera activePlanetCamera = null;



    private bool _prevAPressed = false;

    void Start()
    {
        // Log the planets list so you can verify ordering / assigned objects (lightweight)
        if (planetTransforms == null || planetTransforms.Count == 0)
        {
            Debug.LogError("[PlanetNavigator] planetTransforms is empty. Assign planet root Transforms in the Inspector.");
            return;
        }

        Debug.Log("[PlanetNavigator] Planet list:");
        for (int i = 0; i < planetTransforms.Count; i++)
        {
            var t = planetTransforms[i];
            Debug.LogFormat("  Index {0}: {1} (obj={2})", i, (t != null ? t.name : "NULL"), t);
        }

        // collect planet cameras (one per planet) and disable them (no duplicate watchers)
        planetCameras.Clear();
        if (planetTransforms != null)
        {
            foreach (var t in planetTransforms)
            {
                if (t == null)
                {
                    planetCameras.Add(null);
                    continue;
                }

                // Primary lookup: any Camera component in children (includes inactive)
                Camera cam = t.GetComponentInChildren<Camera>(true);

                // Fallback #1: find a GameObject named "<PlanetName>Camera" anywhere in the scene
                if (cam == null)
                {
                    string fallbackName = t.name + "Camera";
                    var go = GameObject.Find(fallbackName);
                    if (go != null)
                    {
                        cam = go.GetComponent<Camera>();
                        if (cam != null)
                            Debug.LogFormat("[PlanetNavigator] Found camera by GameObject.Find(\"{0}\") for planet '{1}'", fallbackName, t.name);
                    }
                }

                // Fallback #2: search for any Camera in scene whose name contains the planet name (case-insensitive)
                if (cam == null)
                {
                    Camera[] allCams = GameObject.FindObjectsOfType<Camera>(true);
                    foreach (var c in allCams)
                    {
                        if (c == null || c.gameObject == null) continue;
                        if (c.name.ToLower().Contains(t.name.ToLower()))
                        {
                            cam = c;
                            Debug.LogFormat("[PlanetNavigator] Found camera by name-contains ('{0}') for planet '{1}' -> camera '{2}'", t.name, t.name, c.name);
                            break;
                        }
                    }
                }

                // store result and ensure it's initially disabled
                planetCameras.Add(cam);
                if (cam != null)
                {
                    // Ensure no duplicate Watcher: only add if not already present
                    var existingWatcher = cam.GetComponent<PlanetCameraActivationWatcher>();
                    if (existingWatcher == null)
                    {
                        // Add a lightweight watcher for debug purposes (optional)
                        cam.gameObject.AddComponent<PlanetCameraActivationWatcher>().parentName = t.name;
                    }

                    if (cam.gameObject.activeSelf)
                    {
                        // Start with cameras disabled
                        cam.gameObject.SetActive(false);
                    }
                }
                else
                {
                    Debug.LogWarningFormat("[PlanetNavigator] No Camera found under planet Transform '{0}'. Expected a child Camera or a GameObject named '{0}Camera'.", t.name);
                }
            }
        }

        // Hide UI canvas initially
        if (planetCanvas != null)
        {
            planetCanvas.gameObject.SetActive(false);
        }

        // Hook up button listeners if not already done in inspector
        if (nextButton != null) nextButton.onClick.AddListener(OnNextPlanet);
        if (prevButton != null) prevButton.onClick.AddListener(OnPrevPlanet);
        if (returnToShipButton != null) returnToShipButton.onClick.AddListener(ReturnToShip);

        // Ensure previous state cleaned
        activePlanetCamera = null;
        disabledHmdCameras = null;
        disabledHmdAudioListener = null;
        currentPlanetIndex = -1;
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
        if (navigationActive) return;

        bool aPressedThisFrame = false;

        try
        {
            var rightHand = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            if (rightHand.isValid)
            {
                bool pressedNow = false;
                if (rightHand.TryGetFeatureValue(CommonUsages.primaryButton, out pressedNow))
                {
                    aPressedThisFrame = pressedNow && !_prevAPressed; // edge
                    _prevAPressed = pressedNow;
                }
                else
                {
                    _prevAPressed = false;
                }
            }
            else
            {
                _prevAPressed = false;
            }
        }
        catch { }

        bool keyboardPressedThisFrame = Input.GetKeyDown(KeyCode.Space);

        if (aPressedThisFrame || keyboardPressedThisFrame)
        {
            Debug.Log("[PlanetNavigator] Start input detected. keyboard=" + keyboardPressedThisFrame);
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

            if (xrActive)
            {
                planetCanvas.renderMode = RenderMode.WorldSpace;
                var rt = planetCanvas.GetComponent<RectTransform>();
                planetCanvas.transform.SetParent(planetCam.transform, false);
                rt.localPosition = new Vector3(10f, 0f, 100f);
                rt.localRotation = Quaternion.identity;
                rt.localScale = Vector3.one * 0.25f;
                planetCanvas.worldCamera = planetCam;
                planetCanvas.gameObject.SetActive(true);
                Debug.Log("[PlanetNavigator] Using WorldSpace canvas for VR (placed 10m in front of planet camera).");

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
        if (planetInfos != null && planetInfos.Count == planetTransforms.Count) // Double Checks For Errors
        {
            PlanetInfo info = planetInfos[index]; // Grabs the PlanetInfo at the specified index

            // PLANET NAME
            if (planetNameText != null) // If Planet Name Exists
                planetNameText.text = (info != null && !string.IsNullOrEmpty(info.planetName))
                    ? info.planetName
                    : planetTransforms[index].name;

            // PLANET DESCRIPTION
            if (planetDescriptionText != null) // If Planet Description Exists
            {
                if (info != null && !string.IsNullOrEmpty(info.description))
                {
                    // Use description from PlanetInfo (the one in the Inspector)
                    planetDescriptionText.text = info.description;
                }
                else
                {
                    // Use Hardcoded descriptions based on planet name
                    string planetName = planetTransforms[index].name;

                    if (planetName == "Mercury")
                        planetDescriptionText.text = "-Orbit: 88 Earth Days\n-No Moons\n-No Rings\n-Second Densest Planet\n-Thinnest Atmosphere\n-Named After The Roman Messenger God";
                    else if (planetName == "Venus")
                        planetDescriptionText.text = "-Orbit: 225 Earth Days\n-Takes 117 Earth Days To Rotate\n-Rotates In Retrograde\n-No Moons\n-No Rings\n-Hottest Surface In The Solar System Apart From The Sun\n-Temperature Ranges: 86°F to 158°F\n-Named After Roman Goddess\n-Scientists Believe That Studying The History Of Venus' Creation Can Help Us Better Earth's Creation\n-Has An Induced Magnetic Field";
                    else if (planetName == "EarthModel" || planetName == "Earth")
                        planetDescriptionText.text = "-Line One Test\n-Line Two Test Earth";
                    else if (planetName == "Mars")
                        planetDescriptionText.text = "-Line One Test\n-Line Two Test Mars";
                    else if (planetName == "Jupiter")
                        planetDescriptionText.text = "-Line One Test\n-Line Two Test Jupiter";
                    else if (planetName == "Saturn")
                        planetDescriptionText.text = "-Line One Test" + Environment.NewLine + "-Line Two Test Saturn";
                    else if (planetName == "Uranus")
                        planetDescriptionText.text = "-Line One Test\n-Line Two Test Uranus";
                    else if (planetName == "Neptune")
                        planetDescriptionText.text = "-Line One Test\n-Line Two Test Neptune";
                    else
                        planetDescriptionText.text = "No description available.";
                }
            }
        }
        else
        {
            // If no planetInfos provided, use transform names as fallback
            if (planetNameText != null)
                planetNameText.text = planetTransforms[index].name;
            if (planetDescriptionText != null)
                planetDescriptionText.text = "";
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
        // optional: store parent name for clearer logs
        public string parentName;

        void OnEnable()
        {
            // Keep this extremely cheap to avoid lag. Only log a simple message (no stack traces).
            Debug.LogFormat("[PlanetNavigator][ActivationWatcher] Camera enabled: '{0}' parent='{1}'", gameObject.name, parentName ?? "(unknown)");
        }

        void OnDisable()
        {
            // Optional minimal log on disable
            Debug.LogFormat("[PlanetNavigator][ActivationWatcher] Camera disabled: '{0}' parent='{1}'", gameObject.name, parentName ?? "(unknown)");
        }
    }
}