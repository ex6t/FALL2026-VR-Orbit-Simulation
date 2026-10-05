using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SceneManagement;

public class HandS3MenuInput : MonoBehaviour
{
    private bool previousContinue;

    private void Update()
    {
        if (XRInputButtons.GetButtonDown(XRNode.RightHand, XRMenuButton.SecondaryButton, KeyCode.Return, ref previousContinue))
        {
            RedSunProgress.redSunCompleted = true; // Mark Red Sun Simulation As Completed
            SceneManager.LoadScene("OrbitalModel"); // Continue The Main Scene
        }
    }
}
