using UnityEngine;
using System.Diagnostics;
using System.Text;

public class HierarchyWatch : MonoBehaviour
{
    [SerializeField] bool logChildrenChanges = true;

    void OnTransformParentChanged()
    {
        UnityEngine.Debug.LogError($"[HierarchyWatch] Parent changed: {FullPath(transform)} -> new parent: {(transform.parent ? transform.parent.name : "<null>")}\n{new StackTrace(true)}", this);
    }
    void OnTransformChildrenChanged()
    {
        if (!logChildrenChanges) return;
        UnityEngine.Debug.LogError($"[HierarchyWatch] Children changed under: {FullPath(transform)} (count={transform.childCount})\n{new StackTrace(true)}", this);
    }
    string FullPath(Transform t)
    {
        var sb = new StringBuilder(t.name);
        var cur = t.parent;
        while (cur != null)
        {
            sb.Insert(0, cur.name + "/");
            cur = cur.parent;
        }
        return sb.ToString();
    }
}