using UnityEngine;
using UnityEngine.SceneManagement;

public class HubStationSceneController : MonoBehaviour
{
    private GameObject hub;
    private bool wasActive = false;

    void Start()
    {
        // Look for the HubStation in DontDestroyOnLoad
        hub = GameObject.Find("HubStation");

        if (hub != null)
        {
            wasActive = hub.activeSelf;
            hub.SetActive(false);
            Debug.Log("[S1View] HubStation detected and DISABLED.");
        }

        // Listen for scene changes
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void OnSceneChanged(Scene oldScene, Scene newScene)
    {
        // If we leave S1View and HubStation exists → re-enable it
        if (hub != null && wasActive && newScene.name != "S1View")
        {
            hub.SetActive(true);
            Debug.Log("[S1View] HubStation RE-ENABLED (left S1View).");

            // Stop listening — avoids multiple calls
            SceneManager.activeSceneChanged -= OnSceneChanged;
        }
    }

    private void OnDestroy()
    {
        // Clean up listener if script is destroyed
        SceneManager.activeSceneChanged -= OnSceneChanged;
    }
}
