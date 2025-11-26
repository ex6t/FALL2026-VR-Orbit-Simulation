using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HandStartPlanetNavigator : MonoBehaviour
{
    [Tooltip("Reference to the Hand component on this object (optional)")]
    public Hand hand;

    // A button: open tablet
    public SteamVR_Action_Boolean aButtonAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("AButton");

    // B button: close tablet
    public SteamVR_Action_Boolean bButtonAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("returntoshipbutton");

    // X button: confirm selection
    public SteamVR_Action_Boolean xButtonAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("XButton");

    // Existing planet actions
    public SteamVR_Action_Boolean nextPlanetAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("NextPlanet");
    public SteamVR_Action_Boolean prevPlanetAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("PrevPlanet");

    // NEW: Right joystick (vector2)
    public SteamVR_Action_Vector2 rightStickAction = SteamVR_Input.GetAction<SteamVR_Action_Vector2>("RightStick");

    // JoyCon up/down as *buttons* (DPAD)
    public SteamVR_Action_Boolean joyConUpAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("joyconup");
    public SteamVR_Action_Boolean joyConDownAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("joycondown");

    [SerializeField] private PlanetNavigator planetNavigator;
    [SerializeField] private TabletSummoner tabletSummoner;
    [SerializeField] private TabletMenu tabletMenu;

    void Reset()
    {
        hand = GetComponent<Hand>();
        if (planetNavigator == null) planetNavigator = FindObjectOfType<PlanetNavigator>();
        if (tabletSummoner == null) tabletSummoner = FindObjectOfType<TabletSummoner>();
        if (tabletMenu == null) tabletMenu = FindObjectOfType<TabletMenu>();
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
            // A → open tablet
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

            // B → close tablet
            if (bButtonAction != null && bButtonAction.GetStateDown(hand.handType))
            {
                if (tabletSummoner != null)
                {
                    tabletSummoner.CloseTablet();
                    Debug.Log("[Hand] B pressed → close tablet");
                }
                else
                {
                    Debug.LogWarning("[Hand] B pressed but tabletSummoner is NULL");
                }
            }

            // Only handle menu navigation when tablet is open
            if (tabletSummoner != null && tabletSummoner.IsTabletOpen && tabletMenu != null)
            {
                // DPAD North → Move up
                if (joyConUpAction != null && joyConUpAction.GetStateDown(hand.handType))
                {
                    tabletMenu.MoveUp();
                    Debug.Log("[Hand] joyconup → TabletMenu.MoveUp()");
                }

                // DPAD South → Move down
                if (joyConDownAction != null && joyConDownAction.GetStateDown(hand.handType))
                {
                    tabletMenu.MoveDown();
                    Debug.Log("[Hand] joycondown → TabletMenu.MoveDown()");
                    // For testing just trying selecting the current option
                    tabletMenu.GetCurrentOption()?.Select();
                }
            }
        }

        // X → confirm current option (check either controller and guard nulls)
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

                    // If it's a Button, invoke its onClick (recommended)
                    if (current.TryGetComponent<Button>(out Button btn))
                    {
                        Debug.Log("[Hand] Invoking Button.onClick() for: " + btn.name);
                        btn.onClick.Invoke();
                    }
                    else
                    {
                        string optionName = current.gameObject.name;
                        switch (optionName)
                        {
                            case "ViewEachPlanetBtn":
                                if (planetNavigator != null)
                                {
                                    Debug.Log("[Hand] X → StartPlanetTask()");
                                    planetNavigator.StartPlanetTask();
                                }
                                else
                                {
                                    Debug.LogWarning("[Hand] planetNavigator is NULL, cannot StartPlanetTask()");
                                }
                                break;

                            case "RedSunBtn":
                                Debug.Log("[Hand] X → RedSunBtn selected (TODO: implement action)");
                                break;

                            case "SeasonsBtn":
                                Debug.Log("[Hand] X → SeasonsBtn selected (TODO: implement action)");
                                break;

                            default:
                                Debug.LogWarning("[Hand] X pressed on unknown option: " + optionName);
                                break;
                        }
                    }
                }
            }
        }

        // ------ EXISTING PLANET NAV STUFF BELOW ------
        if (planetNavigator == null) return;

        // Next planet on RIGHT trigger
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

        // Previous planet on LEFT trigger
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