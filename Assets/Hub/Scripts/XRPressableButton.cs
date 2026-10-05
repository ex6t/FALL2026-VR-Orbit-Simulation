using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

[RequireComponent(typeof(Collider))]
public class XRPressableButton : XRSimpleInteractable
{
    [Header("Button Events")]
    public UnityEvent onPressed;

    [Header("Motion")]
    public Transform movingPart;
    public Vector3 localPressOffset = new Vector3(0f, -0.1f, 0f);
    public float pressReturnSpeed = 12f;

    [Header("Input")]
    public float cooldown = 0.35f;
    public bool allowTriggerPress = true;
    public bool invokeLegacySteamVREvents = true;

    private Vector3 startPosition;
    private float lastPressedTime = -999f;
    private int triggerPressCount;

    protected override void Awake()
    {
        base.Awake();

        if (movingPart == null && transform.childCount > 0)
            movingPart = transform.GetChild(0);
    }

    protected override void Start()
    {
        base.Start();

        if (movingPart != null)
            startPosition = movingPart.localPosition;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        Press();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!allowTriggerPress || !IsLikelyInteractor(other))
            return;

        triggerPressCount++;
        Press();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!allowTriggerPress || !IsLikelyInteractor(other))
            return;

        triggerPressCount = Mathf.Max(0, triggerPressCount - 1);
    }

    private void Update()
    {
        if (movingPart == null)
            return;

        Vector3 targetPosition = triggerPressCount > 0 ? startPosition + localPressOffset : startPosition;
        movingPart.localPosition = Vector3.Lerp(movingPart.localPosition, targetPosition, Time.deltaTime * pressReturnSpeed);
    }

    public void Press()
    {
        if (Time.time < lastPressedTime + cooldown)
            return;

        lastPressedTime = Time.time;

        if (movingPart != null)
            movingPart.localPosition = startPosition + localPressOffset;

        onPressed?.Invoke();

        if (invokeLegacySteamVREvents)
            InvokeLegacySteamVRButtonEvents();
    }

    private void InvokeLegacySteamVRButtonEvents()
    {
        Component legacyButton = GetComponent("Valve.VR.InteractionSystem.HoverButton");
        if (legacyButton == null)
            return;

        InvokeLegacyHandEvent(legacyButton, "onButtonDown");
        InvokeLegacyHandEvent(legacyButton, "onButtonIsPressed");
    }

    private void InvokeLegacyHandEvent(Component legacyButton, string fieldName)
    {
        FieldInfo eventField = legacyButton.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public);
        if (eventField == null)
            return;

        object handEvent = eventField.GetValue(legacyButton);
        if (handEvent == null)
            return;

        MethodInfo invokeMethod = handEvent.GetType().GetMethod("Invoke");
        if (invokeMethod == null)
            return;

        invokeMethod.Invoke(handEvent, new object[] { null });
    }

    private bool IsLikelyInteractor(Collider other)
    {
        if (other == null)
            return false;

        if (other.GetComponentInParent<XRBaseInteractor>() != null)
            return true;

        Transform current = other.transform;
        while (current != null)
        {
            string objectName = current.name.ToLowerInvariant();
            if (objectName.Contains("hand") || objectName.Contains("controller") || objectName.Contains("interactor"))
                return true;

            current = current.parent;
        }

        return false;
    }
}
