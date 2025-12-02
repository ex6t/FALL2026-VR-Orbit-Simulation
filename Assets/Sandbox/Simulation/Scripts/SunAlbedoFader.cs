using System.Collections;
using UnityEngine;

public class SunAlbedoFader : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Renderer that has the sun material. If empty, this will use the Renderer on this GameObject.")]
    public Renderer sunRenderer;

    [Header("Fade settings")]
    [Tooltip("Fade duration in seconds.")]
    public float duration = 5f;

    [Tooltip("Start automatically when the scene starts.")]
    public bool triggerOnStart = true;

    [Tooltip("Start when the Player (by tag) enters this object's trigger collider.")]
    public bool triggerOnPlayerEnter = false;

    [Tooltip("Tag used to detect the player for OnTriggerEnter.")]
    public string playerTag = "Player";

    // Target colors (255-based in UI; converted to Unity Color)
    private readonly Color startColor = new Color(1f, 1f, 1f, 1f); // 255,255,255
    private readonly Color endColor = new Color(1f, 0f, 0f, 1f);   // 255,0,0

    [Header("Emission (optional)")]
    [Tooltip("Animate the emission color as well as the albedo. Disable to avoid overriding the texture or creating strong bloom effects.")]
    public bool animateEmission = false;

    [Tooltip("Multiplier applied to the emission color when animating. Set to 0 to effectively disable emission output even if animateEmission is true.")]
    public float emissionIntensity = 1f;

    // Internal material instance we modify at runtime
    //private Material instancedMaterial;
    private bool isFading = false;

    // Cached property and emission capability
    private string colorProp = null;
    private bool hasEmission = false;
    private MaterialPropertyBlock mpb;

    void Start()
    {
        if (sunRenderer == null)
        {
            sunRenderer = GetComponent<Renderer>();
        }
        // Prepare MaterialPropertyBlock
        mpb = new MaterialPropertyBlock();
        sunRenderer.GetPropertyBlock(mpb);

        // Inspect the material (sharedMaterial is safe for read-only checks)
        var mat = sunRenderer.sharedMaterial;
        if (mat != null)
        {
            if (mat.HasProperty("_BaseColor")) colorProp = "_BaseColor";
            else if (mat.HasProperty("_Color")) colorProp = "_Color";

            hasEmission = mat.HasProperty("_EmissionColor");
            if (hasEmission)
            {
                // Only enable the emission keyword on the shared material if we intend to animate emission.
                // Enabling emission can cause the object to glow (especially with bloom/post-processing)
                // and may visually overwhelm or 'wash out' the texture. We avoid touching the material
                // unless the designer explicitly asked for emission animation via `animateEmission`.
                if (animateEmission)
                {
                    mat.EnableKeyword("_EMISSION");
                }
                else
                {
                    // Ensure the emission keyword isn't enabled accidentally
                    try { mat.DisableKeyword("_EMISSION"); } catch { }
                }
            }
        }
        else
        {
            // Fallback to _Color if no material info available
            colorProp = "_Color";
        }

        // Ensure the renderer starts with the expected color
        ApplyColorToBlock(startColor);

        if (triggerOnStart)
        {
            StartFade();
        }
    }

    /// <summary>
    /// Call this to start the fade from white to red.
    /// </summary>
    public void StartFade()
    {
        if (isFading) return;

        if (sunRenderer == null)
        {
            Debug.LogWarning("[SunAlbedoFader] No Renderer assigned or found to fade.");
            return;
        }

        StartCoroutine(FadeCoroutine(duration));
    }

    IEnumerator FadeCoroutine(float secs)
    {
        isFading = true;
        float elapsed = 0f;

        while (elapsed < secs)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / secs);
            Color c = Color.Lerp(startColor, endColor, t);

            ApplyColorToBlock(c);

            yield return null;
        }

        // Ensure final color is exact
        ApplyColorToBlock(endColor);
        isFading = false;
    }

    private void ApplyColorToBlock(Color c)
    {
        if (mpb == null) mpb = new MaterialPropertyBlock();
        // Clear previous values that could conflict (optional)
        // mpb.Clear();

        if (!string.IsNullOrEmpty(colorProp))
        {
            mpb.SetColor(colorProp, c);
        }
        else
        {
            mpb.SetColor("_Color", c);
        }

        // Only animate emission if the material supports it and the feature is enabled.
        // Setting emission can flood the look with a flat color (especially with bloom/HDR),
        // which is why we default emission animation off and let the designer enable it.
        if (hasEmission && animateEmission && emissionIntensity > 0f)
        {
            mpb.SetColor("_EmissionColor", c * emissionIntensity);
        }
        else if (hasEmission && !animateEmission)
        {
            // Ensure we don't leave previous emission values set in the block — clear to black.
            mpb.SetColor("_EmissionColor", Color.black);
        }

        sunRenderer.SetPropertyBlock(mpb);
    }

    // Optional trigger behavior: needs a Collider set as "isTrigger = true" on this GameObject
    void OnTriggerEnter(Collider other)
    {
        if (!triggerOnPlayerEnter) return;
        if (other.CompareTag(playerTag))
        {
            StartFade();
        }
    }

    void OnDestroy()
    {
        // If we created an instance via renderer.material we should allow it to be cleaned up.
        // (Do not call Destroy(instancedMaterial) unless you intentionally want to remove it at runtime.)
    }
}