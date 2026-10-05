using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SceneManagement;

public class HandS1MenuInput : MonoBehaviour
{
    [Header("S1 Tablet Menu")]
    public S1TabletMenu s1TabletMenu;

    private bool previousMoveUp;
    private bool previousMoveDown;
    private bool previousConfirm;
    private bool previousContinue;

    private void Reset()
    {
        if (s1TabletMenu == null) s1TabletMenu = FindObjectOfType<S1TabletMenu>();
    }

    private void Awake()
    {
        if (s1TabletMenu == null)
            s1TabletMenu = FindObjectOfType<S1TabletMenu>();
    }

    private void Update()
    {
        if (s1TabletMenu == null)
            return;

        if (XRInputButtons.GetVerticalDown(XRNode.RightHand, true, KeyCode.UpArrow, ref previousMoveUp))
        {
            s1TabletMenu.MoveUp();
            Debug.Log("[HandS1] MoveUp()");
        }

        if (XRInputButtons.GetVerticalDown(XRNode.RightHand, false, KeyCode.DownArrow, ref previousMoveDown))
        {
            s1TabletMenu.MoveDown();
            Debug.Log("[HandS1] MoveDown()");
        }

        if (XRInputButtons.GetButtonDown(XRNode.LeftHand, XRMenuButton.PrimaryButton, KeyCode.X, ref previousConfirm))
        {
            Debug.Log("[HandS1] ConfirmCurrent()");
            s1TabletMenu.ConfirmCurrent();
        }

        if (s1TabletMenu.finishedGame &&
            XRInputButtons.GetButtonDown(XRNode.RightHand, XRMenuButton.SecondaryButton, KeyCode.Return, ref previousContinue))
        {
            Debug.Log("[HandS1] ContinueToNextScene()");
            SceneManager.LoadScene("S2View");
        }
    }
}
