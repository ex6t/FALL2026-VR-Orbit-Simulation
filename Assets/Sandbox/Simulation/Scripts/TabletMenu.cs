using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TabletMenu : MonoBehaviour
{
    [Tooltip("Options in order (top to bottom). E.g. 3 buttons.")]
    public List<Selectable> options = new List<Selectable>();

    private int currentIndex = 0;

    private void Start()
    {
        if (options.Count > 0 && options[0] != null)
        {
            options[0].Select();
        }
    }

    public void MoveDown()
    {
        if (options.Count == 0) return;

        currentIndex++;
        if (currentIndex >= options.Count)
            currentIndex = options.Count - 1; // clamp

    }

    public void MoveUp()
    {
        if (options.Count == 0) return;

        currentIndex--;
        if (currentIndex < 0)
            currentIndex = 0; // clamp

    }

    public Selectable GetCurrentOption()
    {
        if (currentIndex >= 0 && currentIndex < options.Count)
            return options[currentIndex];
        return null;
    }

    private void SelectCurrent()
    {
        var option = GetCurrentOption();
        if (option != null)
        {
            option.Select();
            Debug.Log("[TabletMenu] Selected: " + option.name);
        }
    }
}
