using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UI;

public class HandStartPlanetNavigator : MonoBehaviour
{
    [Header("Task Checkmarks")]
    public GameObject planetTaskCheckmark;

    [SerializeField] private PlanetNavigator planetNavigator;
    [SerializeField] private TabletSummoner tabletSummoner;
    [SerializeField] private TabletMenu tabletMenu;

    bool planetTaskStarted = false;
    private bool previousTabletToggle;
    private bool previousCloseTablet;
    private bool previousConfirm;
    private bool previousMoveUp;
    private bool previousMoveDown;
    private bool previousNextPlanet;
    private bool previousPreviousPlanet;

    void Reset()
    {
        if (planetNavigator == null) planetNavigator = FindObjectOfType<PlanetNavigator>();
        if (tabletSummoner == null) tabletSummoner = FindObjectOfType<TabletSummoner>();
        if (tabletMenu == null) tabletMenu = FindObjectOfType<TabletMenu>();

        if (planetTaskCheckmark != null)
            planetTaskCheckmark.SetActive(false);
    }

    void Update()
    {
        if (tabletSummoner != null &&
            XRInputButtons.GetButtonDown(XRNode.RightHand, XRMenuButton.PrimaryButton, KeyCode.T, ref previousTabletToggle))
        {
            tabletSummoner.OpenTablet();
        }

        if (XRInputButtons.GetButtonDown(XRNode.RightHand, XRMenuButton.SecondaryButton, KeyCode.Escape, ref previousCloseTablet))
        {
            if (planetTaskStarted && planetNavigator != null)
            {
                if (planetTaskCheckmark != null)
                    planetTaskCheckmark.SetActive(true);

                PlanetNavProgress.planetNavCompleted = true;
                planetNavigator.ReturnToShip();
                planetTaskStarted = false;
            }
            else if (tabletSummoner != null)
            {
                tabletSummoner.CloseTablet();
            }
        }

        if (tabletSummoner != null && tabletSummoner.IsTabletOpen && tabletMenu != null)
        {
            if (XRInputButtons.GetButtonDown(XRNode.LeftHand, XRMenuButton.TriggerButton, KeyCode.UpArrow, ref previousMoveUp))
            {
                tabletMenu.MoveUp();
            }

            if (XRInputButtons.GetButtonDown(XRNode.RightHand, XRMenuButton.TriggerButton, KeyCode.DownArrow, ref previousMoveDown))
            {
                tabletMenu.MoveDown();
            }

            if (XRInputButtons.GetButtonDown(XRNode.LeftHand, XRMenuButton.PrimaryButton, KeyCode.Return, ref previousConfirm))
            {
                Selectable current = tabletMenu.GetCurrentOption();
                tabletMenu.ConfirmCurrent();
                if (current != null && current.gameObject.name == "ViewEachPlanetBtn")
                    planetTaskStarted = true;
            }

            return;
        }

        if (planetNavigator == null)
            return;

        if (XRInputButtons.GetButtonDown(XRNode.RightHand, XRMenuButton.TriggerButton, KeyCode.RightArrow, ref previousNextPlanet))
        {
            planetNavigator.nextPlanetFromHand();
        }

        if (XRInputButtons.GetButtonDown(XRNode.LeftHand, XRMenuButton.TriggerButton, KeyCode.LeftArrow, ref previousPreviousPlanet))
        {
            planetNavigator.prevPlanetFromHand();
        }
    }
}
