using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
using UnityEngine.SceneManagement;

public class HandS1MenuInput : MonoBehaviour
{
    [Tooltip("Reference to the Hand component on this object.")]
    public Hand hand;

    [Header("SteamVR Actions")]
    // DPAD North/South: Move Up/Down
    public SteamVR_Action_Boolean joyConUpAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("joyconup");
    public SteamVR_Action_Boolean joyConDownAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("joycondown");

    // X button: Confirm
    public SteamVR_Action_Boolean xButtonAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("XButton");

    // A button: Continue To Next Scene
    public SteamVR_Action_Boolean aButtonAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("AButton");

    [Header("S1 Tablet Menu")]
    public S1TabletMenu s1TabletMenu;

    private void Reset()
    {
        hand = GetComponent<Hand>();
        if (s1TabletMenu == null) s1TabletMenu = FindObjectOfType<S1TabletMenu>();
    }

    private void Awake()
    {
        if (hand == null)
            hand = GetComponent<Hand>();
        if (s1TabletMenu == null)
            s1TabletMenu = FindObjectOfType<S1TabletMenu>();
    }

    private void Update()
    {
        if (hand == null || s1TabletMenu == null) return;

        if (hand.handType != SteamVR_Input_Sources.RightHand)
            return;

        // DPAD Up: Move up
        if (joyConUpAction != null && joyConUpAction.GetStateDown(hand.handType))
        {
            s1TabletMenu.MoveUp();
            Debug.Log("[HandS1] joyconup: MoveUp()");
        }

        // DPAD Down: Move down
        if (joyConDownAction != null && joyConDownAction.GetStateDown(hand.handType))
        {
            s1TabletMenu.MoveDown();
            Debug.Log("[HandS1] joycondown: MoveDown()");
        }

        // X Button: Confirm
        if (xButtonAction != null)
        {
            bool xPressedLeft = xButtonAction.GetStateDown(SteamVR_Input_Sources.LeftHand);
            bool xPressedRight = xButtonAction.GetStateDown(SteamVR_Input_Sources.RightHand);

            if (xPressedLeft || xPressedRight)
            {
                Debug.Log("[HandS1] X pressed: ConfirmCurrent()");
                s1TabletMenu.ConfirmCurrent(); // Confirm Selection
            }
        }

        // A Button: Continue To Next Scene
        if (aButtonAction != null && s1TabletMenu.finishedGame == true)
        {
            bool aPressedLeft = aButtonAction.GetStateDown(SteamVR_Input_Sources.LeftHand);
            bool aPressedRight = aButtonAction.GetStateDown(SteamVR_Input_Sources.RightHand);
            if (aPressedLeft || aPressedRight)
            {
                Debug.Log("[HandS1] A pressed: ContinueToNextScene()");
                SceneManager.LoadScene("S2View");
            }
        }
    }
}
