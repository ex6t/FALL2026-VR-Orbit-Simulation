using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SceneManagement;

public class HandSeasonsMenuInput : MonoBehaviour
{
    [Header("Tablets")]
    public GameObject tablet1; // First Tablet GameObject
    public GameObject tablet2; // Second Tablet GameObject

    // How Many Times Continue Has Been Pressed
    private int aPressCount = 0;
    private bool previousContinue;

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
        if (XRInputButtons.GetButtonDown(XRNode.RightHand, XRMenuButton.SecondaryButton, KeyCode.Return, ref previousContinue))
        {
            aPressCount++; // Increment Continue Press Count

            switch (aPressCount)
            {
                // First Time Pressing Continue: Switch To Tablet2
                case 1:
                    if (tablet1 != null) tablet1.SetActive(false); // Sets Tablet1 Off
                    if (tablet2 != null) tablet2.SetActive(true); // Sets Tablet2 On
                    break;

                // Second Time Pressing Continue: Load Next Scene
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
