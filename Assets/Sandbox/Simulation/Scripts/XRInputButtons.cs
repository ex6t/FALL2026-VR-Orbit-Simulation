using UnityEngine;
using UnityEngine.XR;

public enum XRMenuButton
{
    PrimaryButton,
    SecondaryButton,
    TriggerButton
}

public static class XRInputButtons
{
    private const float AxisThreshold = 0.65f;
    private const float TriggerThreshold = 0.65f;

    public static bool GetButtonDown(XRNode node, XRMenuButton button, KeyCode fallbackKey, ref bool previousState)
    {
        bool pressed = GetButton(node, button) || Input.GetKey(fallbackKey);
        bool pressedThisFrame = pressed && !previousState;
        previousState = pressed;
        return pressedThisFrame;
    }

    public static bool GetVerticalDown(XRNode node, bool up, KeyCode fallbackKey, ref bool previousState)
    {
        bool pressed = Input.GetKey(fallbackKey) || GetPrimary2DAxisVertical(node, up);
        bool pressedThisFrame = pressed && !previousState;
        previousState = pressed;
        return pressedThisFrame;
    }

    private static bool GetButton(XRNode node, XRMenuButton button)
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(node);
        if (!device.isValid)
            return false;

        switch (button)
        {
            case XRMenuButton.PrimaryButton:
                return TryGetBool(device, CommonUsages.primaryButton);
            case XRMenuButton.SecondaryButton:
                return TryGetBool(device, CommonUsages.secondaryButton);
            case XRMenuButton.TriggerButton:
                return TryGetBool(device, CommonUsages.triggerButton) || TryGetAxis(device, CommonUsages.trigger, TriggerThreshold);
            default:
                return false;
        }
    }

    private static bool GetPrimary2DAxisVertical(XRNode node, bool up)
    {
        InputDevice device = InputDevices.GetDeviceAtXRNode(node);
        if (!device.isValid)
            return false;

        if (!device.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 axis))
            return false;

        return up ? axis.y >= AxisThreshold : axis.y <= -AxisThreshold;
    }

    private static bool TryGetBool(InputDevice device, InputFeatureUsage<bool> usage)
    {
        return device.TryGetFeatureValue(usage, out bool value) && value;
    }

    private static bool TryGetAxis(InputDevice device, InputFeatureUsage<float> usage, float threshold)
    {
        return device.TryGetFeatureValue(usage, out float value) && value >= threshold;
    }
}
