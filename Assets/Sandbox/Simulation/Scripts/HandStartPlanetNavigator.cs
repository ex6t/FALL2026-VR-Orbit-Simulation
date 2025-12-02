using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HandStartPlanetNavigator : MonoBehaviour
{
    [Tooltip("Reference to the Hand component on this object (optional)")]
    public Hand hand;

    [Header("Task Checkmarks")]
    public GameObject planetTaskCheckmark;

    [Header("SteamVR Actions")]
    // A button: Open Tablet
    public SteamVR_Action_Boolean aButtonAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("AButton");

    // B button: Closes Tablet & Ends Close Up Cameras
    public SteamVR_Action_Boolean bButtonAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("returntoshipbutton");

    // X button: Confirm Selection
    public SteamVR_Action_Boolean xButtonAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("XButton");

    // Triggers: Moves Between Each Planet (Triggers)
    public SteamVR_Action_Boolean nextPlanetAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("NextPlanet");
    public SteamVR_Action_Boolean prevPlanetAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PrevPlanet");

    // Right joystick (vector2)
    public SteamVR_Action_Vector2 rightStickAction = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("RightStick");

    // JoyCon up/down as *buttons* (DPAD)
    public SteamVR_Action_Boolean joyConUpAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("joyconup");
    public SteamVR_Action_Boolean joyConDownAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("joycondown");

    [SerializeField] private PlanetNavigator planetNavigator;
    [SerializeField] private TabletSummoner tabletSummoner;
    [SerializeField] private TabletMenu tabletMenu;

    bool planetTaskStarted = false; // Track if the planet task has started

    void Reset()
    {
        hand = GetComponent<Hand>();
        if (planetNavigator == null) planetNavigator = FindObjectOfType<PlanetNavigator>();
        if (tabletSummoner == null) tabletSummoner = FindObjectOfType<TabletSummoner>();
        if (tabletMenu == null) tabletMenu = FindObjectOfType<TabletMenu>();

        // Hide Check At Start
        if (planetTaskCheckmark != null)
            planetTaskCheckmark.SetActive(false);
    }

    void Update()
    {
        if (xButtonAction == null)
        {
            Debug.LogWarning("[Hand] xButtonAction is NULL – check the 'XButton' action in SteamVR Input.");
        } else
        {
            Debug.Log("[Hand] xButtonAction is assigned.");
        }

        if (hand == null)
        {
            hand = GetComponent<Hand>();
            if (hand == null) return;
        }

        // Make sure the tablet/menu refs are present before using them
        if (tabletSummoner == null) tabletSummoner = FindObjectOfType<TabletSummoner>();
        if (tabletMenu == null) tabletMenu = FindObjectOfType<TabletMenu>();

        Debug.Log("[Hand] tabletSummoner assigned: " + (tabletSummoner != null));
        Debug.Log("[Hand] tabletMenu assigned: " + (tabletMenu != null));

        if (joyConUpAction == null || joyConDownAction == null)
        {
            Debug.LogWarning("[Hand] JoyCon actions are NULL – check action names in SteamVR_Input!");
        }

        // ---------- TABLET CONTROLS (RIGHT HAND) ----------
        if (hand.handType == SteamVR_Input_Sources.RightHand)
        {
            // A: OPENS TABLET
            if (aButtonAction != null && aButtonAction.GetStateDown(hand.handType))
            {
                if (tabletSummoner != null)
                {
                    tabletSummoner.OpenTablet();
                    Debug.Log("[Hand] A pressed → open tablet");
                }
                else
                {
                    Debug.LogWarning("[Hand] A pressed but tabletSummoner is NULL");
                }
            }

            // B: CLOSES TABLET OR ENDS PLANET TASK
            if (bButtonAction != null && bButtonAction.GetStateDown(hand.handType))
            {
                if (planetTaskStarted == true) // Checks If The Planet Task Has Started
                {
                    if (planetNavigator != null)
                    {
                        if (planetTaskCheckmark != null)
                            planetTaskCheckmark.SetActive(true); // Show The Checkmark For Completing The Planet Task

                        PlanetNavProgress.planetNavCompleted = true; // Marks The Planet Navigation Task As Completed
                        planetNavigator.ReturnToShip(); // Ends The Planet Task

                        Debug.Log("[Hand] B pressed: EndPlanetTask()"); // Debug Log
                    }
                    else
                    {
                        Debug.LogWarning("[Hand] planetNavigator is NULL, cannot EndPlanetTask()"); // Debug Log Warning
                    }
                    planetTaskStarted = false; // Reset The Planet Task Started Bool
                }

                else if (tabletSummoner != null) // If The Planet Task Has Not Started, Just Close The Tablet
                {
                    tabletSummoner.CloseTablet();
                    Debug.Log("[Hand] B pressed → close tablet"); // Debug Log
                }
                else
                {
                    Debug.LogWarning("[Hand] B pressed but tabletSummoner is NULL"); // Debug Log Warning
                }
            }

            // DPAD Up/Down: NAVIGATE TABLET MENU
            // Only handle menu navigation when tablet is open
            if (tabletSummoner != null && tabletSummoner.IsTabletOpen && tabletMenu != null)
            {
                // DPAD North: Move up
                if (joyConUpAction != null && joyConUpAction.GetStateDown(hand.handType))
                {
                    tabletMenu.MoveUp();
                    Debug.Log("[Hand] joyconup → TabletMenu.MoveUp()");
                }

                // DPAD South: Move down
                if (joyConDownAction != null && joyConDownAction.GetStateDown(hand.handType))
                {
                    tabletMenu.MoveDown();
                    Debug.Log("[Hand] joycondown → TabletMenu.MoveDown()");
                    // For testing just trying selecting the current option
                    tabletMenu.GetCurrentOption()?.Select();
                }
            }
        }

        // X: CONFIRM SELECTION
        if (tabletSummoner != null && tabletMenu != null && xButtonAction != null)
        {
            bool isTabletOpen = tabletSummoner.IsTabletOpen;
            bool xPressedOnLeft = xButtonAction.GetStateDown(SteamVR_Input_Sources.LeftHand);
            bool xPressedOnRight = xButtonAction.GetStateDown(SteamVR_Input_Sources.RightHand);

            if (isTabletOpen && (xPressedOnLeft || xPressedOnRight))
            {
                Debug.Log("[Hand] X pressed (detected on " + (xPressedOnLeft ? "Left" : "Right") + " hand)");

                var current = tabletMenu.GetCurrentOption();
                if (current == null)
                {
                    Debug.LogWarning("[Hand] X pressed but current option is NULL");
                }
                else
                {
                    Debug.Log("[Hand] X current option: " + current.gameObject.name);

                    // If it's a Button, invoke its onClick
                    if (current.TryGetComponent<Button>(out Button btn))
                    {
                        Debug.Log("[Hand] Invoking Button.onClick() for: " + btn.name);
                        btn.onClick.Invoke();

                        // Special case: if it's the ViewEachPlanetBtn, mark planetTaskStarted to be true
                        if (current.gameObject.name == "ViewEachPlanetBtn")
                        {
                            planetTaskStarted = true; // Marks that the planet task has started
                            Debug.Log("[Hand] planetTaskStarted = true (via Button.onClick)"); // Debug Log
                        }
                    }

                    // If it's not a Button, handle specific cases
                    else
                    {
                        string optionName = current.gameObject.name;
                        switch (optionName)
                        {
                            // CASE 1: VIEW EACH PLANET
                            case "ViewEachPlanetBtn":
                                if (planetNavigator != null)
                                {
                                    Debug.Log("[Hand] X: StartPlanetTask()");
                                    planetTaskStarted = true; // Marks that the planet task has started
                                    planetNavigator.StartPlanetTask();
                                    
                                }
                                else
                                {
                                    Debug.LogWarning("[Hand] planetNavigator is NULL, cannot StartPlanetTask()");
                                }
                                break;

                            // CASE 2: RED SUN
                            case "RedSunBtn":
                                Debug.Log("[Hand] X: RedSunBtn selected (TODO: implement action)");
                                
                                break;

                            // CASE 3: SEASONS
                            case "SeasonsBtn":
                                Debug.Log("[Hand] X: SeasonsBtn selected (TODO: implement action)");
                                
                                break;

                            default:
                                Debug.LogWarning("[Hand] X pressed on unknown option: " + optionName);
                                break;
                        }
                    }
                }
            }
        }

        // TRIGGERS: NAVIGATE PLANETS
        if (planetNavigator == null) return;

        // RIGHT TRIGGER: NEXT PLANET
        if (nextPlanetAction != null && nextPlanetAction.GetStateDown(SteamVR_Input_Sources.RightHand))
        {
            if (planetNavigator.nextButton != null)
            {
                //planetNavigator.nextButton.onClick.Invoke();
                planetNavigator.nextPlanetFromHand();
                Debug.Log("[HandNav] Right Trigger: NextPlanet");
            }
            else
            {
                Debug.LogWarning("[HandNav] nextButton is not assigned on PlanetNavigator.");
            }
        }

        // LEFT TRIGGER: PREV PLANET
        if (prevPlanetAction != null && prevPlanetAction.GetStateDown(SteamVR_Input_Sources.LeftHand))
        {
            if (planetNavigator.prevButton != null)
            {
                //planetNavigator.prevButton.onClick.Invoke();
                planetNavigator.prevPlanetFromHand();
                Debug.Log("[HandNav] Left Trigger: PrevPlanet");
            }
            else
            {
                Debug.LogWarning("[HandNav] prevButton is not assigned on PlanetNavigator.");
            }
        }
    }
}