using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class HandStartPlanetNavigator : MonoBehaviour
{
    [Tooltip("Reference to the Hand component on this object (optional)")]
    public Hand hand;

    // A button (start navigation)
    public SteamVR_Action_Boolean aButtonAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("AButton");

    // B button (return to ship)
    public SteamVR_Action_Boolean bButtonAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("returntoshipbutton");

    // NEW: triggers for next/prev
    public SteamVR_Action_Boolean nextPlanetAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("NextPlanet");
    public SteamVR_Action_Boolean prevPlanetAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PrevPlanet");

    [SerializeField] private PlanetNavigator planetNavigator; // drag PlanetManager here (has PlanetNavigator)

    // 🔹 NEW: reference to TabletSummoner
    [SerializeField] private TabletSummoner tabletSummoner;

    void Reset()
    {
        hand = GetComponent<Hand>();
        if (planetNavigator == null) planetNavigator = FindObjectOfType<PlanetNavigator>();
        if (tabletSummoner == null) tabletSummoner = FindObjectOfType<TabletSummoner>();
    }

    void Update()
    {
        if (hand == null)
        {
            hand = GetComponent<Hand>();
            if (hand == null) return; // no hand, nothing to do
        }

        if (planetNavigator == null) planetNavigator = FindObjectOfType<PlanetNavigator>();
        if (tabletSummoner == null) tabletSummoner = FindObjectOfType<TabletSummoner>();

        // --- Start navigation on A (RIGHT hand) ---
        if (hand.handType == SteamVR_Input_Sources.RightHand && aButtonAction.GetStateDown(hand.handType))
        {
            if (tabletSummoner != null)
            {
                tabletSummoner.ToggleAndPlaceTablet();
                Debug.Log("[Hand] A pressed: TabletSummoner.ToggleAndPlaceTablet()");
            }
            else
            {
                Debug.LogWarning("[Hand] No TabletSummoner found in scene.");
            }
        }

        // --- Start navigation on B (RIGHT hand) ---
        if (hand.handType == SteamVR_Input_Sources.RightHand && bButtonAction.GetStateDown(hand.handType))
        {
            if (planetNavigator == null) planetNavigator = FindObjectOfType<PlanetNavigator>();
            if (planetNavigator != null)
            {
                planetNavigator.DeactivatePlanetCamera();
                Debug.Log("[Hand] B pressed → PlanetNavigator.DeactivatePlanetCamera()");
            }
            else
            {
                Debug.LogWarning("[Hand] PlanetNavigator not found in scene.");
            }
        }

        if (planetNavigator == null) return; // nothing else to do

        // --- Next planet on RIGHT trigger ---
        if (nextPlanetAction != null && nextPlanetAction.GetStateDown(SteamVR_Input_Sources.RightHand))
        {
            if (planetNavigator.nextButton != null)
            {
                planetNavigator.nextButton.onClick.Invoke();
                Debug.Log("[HandNav] Right Trigger → NextPlanet");
            }
            else
            {
                Debug.LogWarning("[HandNav] nextButton is not assigned on PlanetNavigator.");
            }
        }

        // --- Previous planet on LEFT trigger ---
        if (prevPlanetAction != null && prevPlanetAction.GetStateDown(SteamVR_Input_Sources.LeftHand))
        {
            if (planetNavigator.prevButton != null)
            {
                planetNavigator.prevButton.onClick.Invoke();
                Debug.Log("[HandNav] Left Trigger → PrevPlanet");
            }
            else
            {
                Debug.LogWarning("[HandNav] prevButton is not assigned on PlanetNavigator.");
            }
        }
    }
}
