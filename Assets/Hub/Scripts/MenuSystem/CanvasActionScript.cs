using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class CanvasActionScript : MonoBehaviour
{
    public GameObject Menu;
    private bool previousMenuButton;

    void Start()
    {
        if (Menu != null)
            Menu.SetActive(false);
    }

    public void ButtonUp()
    {
        Debug.Log("Button is up");
        if (Menu != null)
            Menu.SetActive(false);
    }

    public void ButtonDown()
    {
        Debug.Log("Button is down");
        if (Menu != null)
            Menu.SetActive(true);
    }

    void Update()
    {
        if (XRInputButtons.GetButtonDown(XRNode.RightHand, XRMenuButton.SecondaryButton, KeyCode.M, ref previousMenuButton))
        {
            if (Menu != null)
                Menu.SetActive(!Menu.activeSelf);
        }
    }
}
