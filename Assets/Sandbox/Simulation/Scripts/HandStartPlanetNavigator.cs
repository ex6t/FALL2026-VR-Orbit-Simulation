using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class HandStartPlanetNavigator : MonoBehaviour
{
    [Tooltip("Reference to the Hand component on this object (optional)")]
    public Hand hand;

    // A button (start navigation)
    public SteamVR_Action_Boolean aButtonAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("AButton");

    // NEW: triggers for next/prev
    public SteamVR_Action_Boolean nextPlanetAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("NextPlanet");
    public SteamVR_Action_Boolean prevPlanetAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PrevPlanet");

    [SerializeField] private PlanetNavigator planetNavigator; // drag PlanetManager here (has PlanetNavigator)

    void Reset()
    {
        hand = GetComponent<Hand>();
        if (planetNavigator == null) planetNavigator = FindObjectOfType<PlanetNavigator>();
    }

    void Update()
    {
        if (planetNavigator == null) planetNavigator = FindObjectOfType<PlanetNavigator>();

        // --- Start navigation on A (RIGHT hand) ---
        // A lives on the RIGHT Touch controller
        if (hand.handType == SteamVR_Input_Sources.RightHand && aButtonAction.GetStateDown(hand.handType)) 
        { 
            if (planetNavigator == null) planetNavigator = FindObjectOfType<PlanetNavigator>(); 
            if (planetNavigator != null) 
            {
                planetNavigator.StartPlanetTask(); 
                Debug.Log("[Hand] A pressed → PlanetNavigator.StartPlanetTask()"); 
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
