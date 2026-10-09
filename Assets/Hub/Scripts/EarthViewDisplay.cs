using UnityEngine;

public class EarthViewDisplay : MonoBehaviour
{
    public RenderTexture earthCentricTexture;
    public GameObject tvScreen;

    void Start()
    {
        ShowEarthView();
    }

    public void ShowEarthView()
    {
        if (tvScreen == null || earthCentricTexture == null)
        {
            Debug.LogError("[EarthViewDisplay] Assign the monitor and Earth camera texture.", this);
            return;
        }

        Renderer screenRenderer = tvScreen.GetComponent<Renderer>();
        if (screenRenderer == null)
        {
            Debug.LogError("[EarthViewDisplay] The assigned monitor needs a Renderer.", this);
            return;
        }

        MaterialPropertyBlock properties = new MaterialPropertyBlock();
        screenRenderer.GetPropertyBlock(properties);
        properties.SetTexture("_MainTex", earthCentricTexture);
        screenRenderer.SetPropertyBlock(properties);
    }
}
