using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SceneManagement;

public class HandS2MenuInput : MonoBehaviour
{
    private bool previousContinue;

    private void Update()
    {
        if (XRInputButtons.GetButtonDown(XRNode.RightHand, XRMenuButton.SecondaryButton, KeyCode.Return, ref previousContinue))
        {
            SceneManager.LoadScene("S3View"); // Continue To The S3 View Scene
        }
    }
}
