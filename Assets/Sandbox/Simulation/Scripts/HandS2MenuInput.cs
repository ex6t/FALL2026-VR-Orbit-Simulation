using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
using UnityEngine.SceneManagement;

public class HandS2MenuInput : MonoBehaviour
{
    public Hand hand;

    [Header("SteamVR Actions")]
    // A button: Continue To Next Scene
    public SteamVR_Action_Boolean aButtonAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("AButton");

    private void Awake()
    {
        if (hand == null)
            hand = GetComponent<Hand>();
    }

    private void Update()
    {
        if (hand == null) return;

        if (hand.handType != SteamVR_Input_Sources.RightHand)
            return;

        // A Button: Continue To Next Scene
        if (aButtonAction != null && aButtonAction.GetStateDown(hand.handType))
        {
            SceneManager.LoadScene("S3View"); // Continue To The S3 View Scene
        }
    }
}
