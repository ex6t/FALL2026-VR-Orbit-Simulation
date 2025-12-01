using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class S1TabletMenu : MonoBehaviour
{

    [Tooltip("Options in order (top to bottom).")]
    public List<Selectable> options = new List<Selectable>();

    [Tooltip("Highlight boxes (RedBox1, RedBox2, ...) in the SAME order as options.")]
    public List<GameObject> highlightBoxes = new List<GameObject>();

    [Header("Canvases")]
    [Tooltip("Game Canvas")]
    public GameObject gameCanvas; // Canvas That Has The Question And Options

    [Tooltip("Correct Canvas")]
    public GameObject correctCanvas; // Canvas Shown On Correct Answer

    [Tooltip("Incorrect Canvas")]
    public GameObject incorrectCanvas; // Canvas Shown On Incorrect Answer

    [Tooltip("Current and Correct Indexes")]
    private int currentIndex = 0; // Starts At The First Option
    public int correctIndex = 2;

    public bool finishedGame = false; // To Prevent Multiple Confirmations

    private void Start()
    {
        // Makes sure index is valid
        if (options.Count > 0)
        {
            currentIndex = Mathf.Clamp(currentIndex, 0, options.Count - 1);
        }

        // Makes sure result canvases start OFF
        if (correctCanvas != null) correctCanvas.SetActive(false);
        if (incorrectCanvas != null) incorrectCanvas.SetActive(false);

        // Turn on only the correct highlight
        UpdateHighlight();

        // Select the first option for UI focus
        HoverCurrent();
    }

    // FUNCTION FOR MOVING DOWN THE MENU
    public void MoveDown()
    {
        if (options.Count == 0) return;

        currentIndex++;
        if (currentIndex >= options.Count)
            currentIndex = options.Count - 1; // clamp at bottom

        HoverCurrent(); // Hovers The Current Option For UI Focus
        UpdateHighlight(); // Update highlights
    }

    // FUNCTION FOR MOVING UP THE MENU
    public void MoveUp()
    {
        if (options.Count == 0) return;

        currentIndex--;
        if (currentIndex < 0)
            currentIndex = 0; 

        HoverCurrent(); // Hovers The Current Option For UI Focus
        UpdateHighlight(); // Update highlights
    }

    // FUNCTION FOR GETTING THE CURRENT OPTION
    public Selectable GetCurrentOption()
    {
        if (currentIndex >= 0 && currentIndex < options.Count)
            return options[currentIndex];
        return null;
    }

    // FUNCTION FOR CONFIRMING THE CURRENT SELECTION
    public void ConfirmCurrent()
    {
        var option = GetCurrentOption();
        if (option == null)
        {
            Debug.LogWarning("[S1TabletMenu] ConfirmCurrent called but no current option.");
            return;
        }

        //  Checks If The Selected Option Is Correct
        bool isCorrect = (currentIndex == correctIndex);

        if (isCorrect) // Correct Answer
        {
            Debug.Log("[S1TabletMenu] CORRECT ANSWER selected!");

            if (gameCanvas != null)
                gameCanvas.SetActive(false); // Turns Off The Game Canvas

            if (correctCanvas != null)
            {
                correctCanvas.SetActive(true); // Turns On The Correct Canvas
                finishedGame = true; // Marks The Game As Finished
            }

            if (incorrectCanvas != null)
                incorrectCanvas.SetActive(false); // Turns Off The Incorrect Canvas In Case It Was On
        }
        else // Incorrect Answer
        {
            Debug.Log("[S1TabletMenu] INCORRECT ANSWER selected!");

            if (gameCanvas != null)
                gameCanvas.SetActive(false); // Turns Off The Game Canvas 

            if (correctCanvas != null)
                correctCanvas.SetActive(false); // Turns Off The Correct Canvas In Case In Case It Was On

            if (incorrectCanvas != null)
            {
                incorrectCanvas.SetActive(true); // Turns On The Incorrect Canvas
                finishedGame = true; // Marks The Game As Finished
            }
        }

        // Still trigger button functionality if needed
        if (option.TryGetComponent<Button>(out Button btn))
        {
            btn.onClick.Invoke();
        }
    }

    // FUNCTION FOR HOVERING THE CURRENT OPTION FOR UI FOCUS
    private void HoverCurrent()
    {
        var option = GetCurrentOption();
        if (option != null)
        {
            EventSystem.current.SetSelectedGameObject(option.gameObject);
            Debug.Log("[S1TabletMenu] Hovering: " + option.name);
        }
    }

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
