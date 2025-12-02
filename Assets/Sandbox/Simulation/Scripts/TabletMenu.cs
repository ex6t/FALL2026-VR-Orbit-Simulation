using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class TabletMenu : MonoBehaviour
{
    [Header("Buttons")]
    public List<Selectable> options = new List<Selectable>();

    [Header("Highlight Boxes")]
    public List<GameObject> highlightBoxes = new List<GameObject>();

    [Header("Checkmarks")]
    public GameObject planetNavCheckmark; // Checkmark For Planetary Navigation Completion
    public GameObject redSunCheckmark; // Checkmark For Red Sun Completion
    public GameObject seasonsCheckmark; // Checkmark For Seasons Completion

    private int currentIndex = 0;

    private void Start()
    {
        // PLANET NAV CHECKMARK
        if (planetNavCheckmark != null)
        {
            planetNavCheckmark.SetActive(PlanetNavProgress.planetNavCompleted); // Sets Planet Nav Checkmark Based On PlanetNavProgress
        }

        // RED SUN CHECKMARK
        if (redSunCheckmark != null)
        {
            redSunCheckmark.SetActive(RedSunProgress.redSunCompleted); // Sets Red Sun Checkmark Based On RedSunProgress
        }

        // SEASONS CHECKMARK
        if (seasonsCheckmark != null)
        {
            seasonsCheckmark.SetActive(SeasonsProgress.seasonsCompleted); // Sets Seasons Checkmark Based On SeasonsProgress
        }

        // Ensure only one highlight starts active
        UpdateHighlight();

        // Select first option
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

        SelectCurrent();
        UpdateHighlight();
    }

    public void MoveUp()
    {
        if (options.Count == 0) return;

        currentIndex--;
        if (currentIndex < 0)
            currentIndex = 0; // clamp

        SelectCurrent();
        UpdateHighlight();
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
            EventSystem.current.SetSelectedGameObject(option.gameObject);
            Debug.Log("[TabletMenu] Selected: " + option.name);
        }
    }

    // Turns RedBoxes on/off based on the currentIndex
    private void UpdateHighlight()
    {
        for (int i = 0; i < highlightBoxes.Count; i++)
        {
            if (highlightBoxes[i] == null) continue;

            bool shouldBeActive = (i == currentIndex);
            highlightBoxes[i].SetActive(shouldBeActive);
        }
    }
}