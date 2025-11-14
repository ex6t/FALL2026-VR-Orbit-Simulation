using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// HubStationMover
/// Attach this to the Button (or a small manager on the podium) and wire the Button's OnClick to one of the public methods.
/// - Configure `hubStation` (the Transform to move). If left empty the script will try to find a GameObject named `hubStationName`.
/// - Add target marker Transforms to `targets` (empty GameObjects placed in-scene where you want the Hub Station to move).
/// - Use MoveToNext / MoveToPrevious for cycling, or call MoveToIndex(int index) from the Button's OnClick (you can pass an int parameter).
/// - Optionally enable `smoothMove` to animate the motion and rotation.
/// </summary>
public class HubStationMover : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The Hub Station Transform that will be moved. If null, the script will try to find a GameObject by name at Start.")]

    public Transform hubStation;
    [Tooltip("If hubStation is not set, try to find this name in scene.")]
    public string hubStationName = "Hub Station";

    [Header("Targets")]
    [Tooltip("Target Transforms to move the hub station to (create empty GameObjects as markers).")]
    public List<Transform> targets = new List<Transform>();

    [Header("Movement")]
    [Tooltip("If true, the hub station will smoothly move/rotate to the target over smoothDuration seconds.")]
    public bool smoothMove = true;
    [Tooltip("Duration (seconds) for smooth movement.")]
    public float smoothDuration = 1.0f;

    // internal index
    private int currentIndex = -1;
    private Coroutine movingCoroutine = null;

    void Start()
    {
        if (hubStation == null && !string.IsNullOrEmpty(hubStationName))
        {
            var go = GameObject.Find(hubStationName);
            if (go != null) hubStation = go.transform;
            else Debug.LogWarning($"[HubStationMover] hubStation not assigned and GameObject '{hubStationName}' not found in scene.");
        }

        if (targets == null || targets.Count == 0)
        {
            Debug.LogWarning("[HubStationMover] No targets assigned. Create empty GameObjects for each desired hub position and add them to the targets list.");
        }
    }

    /// <summary>
    /// Move the hub station to the next target (wraps around).
    /// </summary>
    public void MoveToNext()
    {
        if (targets == null || targets.Count == 0) return;
        int next = (currentIndex + 1) % targets.Count;
        MoveToIndex(next);
    }

    /// <summary>
    /// Move the hub station to the previous target (wraps around).
    /// </summary>
    public void MoveToPrevious()
    {
        if (targets == null || targets.Count == 0) return;
        int prev = (currentIndex - 1 + targets.Count) % targets.Count;
        MoveToIndex(prev);
    }

    /// <summary>
    /// Move the hub station to the given index in targets.
    /// This method is public so you can assign it to a Button and pass an int argument.
    /// </summary>
    public void MoveToIndex(int index)
    {
        if (targets == null || targets.Count == 0)
        {
            Debug.LogWarning("[HubStationMover] MoveToIndex called but no targets are configured.");
            return;
        }
        if (index < 0 || index >= targets.Count)
        {
            Debug.LogWarning($"[HubStationMover] MoveToIndex: index {index} out of range (0..{targets.Count-1}).");
            return;
        }
        if (hubStation == null)
        {
            Debug.LogWarning("[HubStationMover] MoveToIndex called but hubStation is null.");
            return;
        }

        currentIndex = index;
        Transform t = targets[index];
        if (t == null)
        {
            Debug.LogWarning($"[HubStationMover] target at index {index} is null.");
            return;
        }

        // stop any previous motion
        if (movingCoroutine != null) StopCoroutine(movingCoroutine);

        if (!smoothMove || smoothDuration <= 0f)
        {
            // instant teleport
            hubStation.position = t.position;
            hubStation.rotation = t.rotation;
        }
        else
        {
            movingCoroutine = StartCoroutine(SmoothMove(hubStation, t.position, t.rotation, smoothDuration));
        }
    }

    /// <summary>
    /// Teleport the hub station to a raw world position (no rotation change).
    /// </summary>
    public void TeleportToPosition(Vector3 worldPosition)
    {
        if (hubStation == null) return;
        if (movingCoroutine != null) StopCoroutine(movingCoroutine);
        hubStation.position = worldPosition;
    }

    private IEnumerator SmoothMove(Transform subject, Vector3 goalPos, Quaternion goalRot, float duration)
    {
        if (subject == null) yield break;
        float elapsed = 0f;
        Vector3 startPos = subject.position;
        Quaternion startRot = subject.rotation;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            subject.position = Vector3.Lerp(startPos, goalPos, t);
            subject.rotation = Quaternion.Slerp(startRot, goalRot, t);
            yield return null;
        }
        subject.position = goalPos;
        subject.rotation = goalRot;
        movingCoroutine = null;
    }

    // convenience: call this to move to a named target (useful from other scripts)
    public void MoveToTargetByName(string name)
    {
        if (targets == null) return;
        for (int i = 0; i < targets.Count; i++)
        {
            if (targets[i] != null && targets[i].name == name)
            {
                MoveToIndex(i);
                return;
            }
        }
        Debug.LogWarning($"[HubStationMover] MoveToTargetByName: no target named '{name}' found.");
    }

    // small debug helpers
    [ContextMenu("MoveToNext (Editor)")]
    void ContextMoveNext() { MoveToNext(); }
    [ContextMenu("MoveToPrevious (Editor)")]
    void ContextMovePrev() { MoveToPrevious(); }
}
