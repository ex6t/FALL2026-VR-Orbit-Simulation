using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
using UnityEngine.SceneManagement;

public class HandSeasonsMenuInput : MonoBehaviour
{
    public Hand hand;

    [Header("SteamVR Actions")]
    // A button: Switch Tablets / Load Next Scene
    public SteamVR_Action_Boolean aButtonAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("AButton");

    [Header("Tablets")]
    public GameObject tablet1; // First Tablet GameObject
    public GameObject tablet2; // Second Tablet GameObject

    // How Many Times A Has Been Pressed
    private int aPressCount = 0;

    private void Awake()
    {
        if (hand == null)
            hand = GetComponent<Hand>();
    }

    private void Start()
    {
        // Check If Tablets Are Null
        if (tablet1 == null || tablet2 == null)
            Debug.LogError("[HandSeasonsMenuInput] One of the tablet GameObjects is not assigned!");

        // Start With Tablet1 Active, Tablet2 Inactive
        if (tablet1 != null)
            tablet1.SetActive(true); // Sets Tablet1 On

        if (tablet2 != null)
            tablet2.SetActive(false); // Sets Tablet2 Off
    }

    private void Update()
    {
        if (hand == null) return;
        if (hand.handType != SteamVR_Input_Sources.RightHand)
            return;

        // A Button: Switch Tablets / Load Next Scene
        if (aButtonAction != null && aButtonAction.GetStateDown(hand.handType))
        {
            aPressCount++; // Increment A Press Count

            switch (aPressCount)
            {
                // First Time Pressing A: Switch To Tablet2
                case 1:
                    if (tablet1 != null) tablet1.SetActive(false); // Sets Tablet1 Off
                    if (tablet2 != null) tablet2.SetActive(true); // Sets Tablet2 On
                    break;

                // Second Time Pressing A: Load Next Scene
                case 2:
                    SeasonsProgress.seasonsCompleted = true; // Marks Seasons Task As Completed
                    SceneManager.LoadScene("OrbitalModel"); // Swaps To The Main Scene
                    break;

                default:
                    Debug.Log("Didn't Swap Scenes Properly, Counted An Extra Press!"); // Error Message 
                    break;
            }
        }
    }
}
