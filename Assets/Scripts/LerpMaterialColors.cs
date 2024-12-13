using UnityEngine;

public class LerpMaterialColors : MonoBehaviour
{
    [Tooltip("Assign the parent GameObject with children containing MeshRenderers.")]
    public GameObject parentObject;

    [Tooltip("Target color for the first material (if it exists).")]
    public Color targetColor1 = Color.red;

    [Tooltip("Target color for the second material (if it exists).")]
    public Color targetColor2 = Color.blue;

    [Tooltip("Target color for the third material (if it exists).")]
    public Color targetColor3 = Color.green;

    [Tooltip("Duration of the color lerp (in seconds).")]
    public float lerpDuration = 2.0f;

    [Tooltip("Duration of the transparency lerp (in seconds).")]
    public float transparencyLerpDuration = 2.0f;

    private float lerpTime = 0.0f;
    private bool colorsLerped = false;
    private Color[][] initialColors;

    void Start()
    {
        if (parentObject == null)
        {
            Debug.LogError("No parent GameObject assigned!");
            return;
        }

        // Cache initial colors of all materials
        CacheInitialColors();
    }

    void Update()
    {
        if (parentObject == null) return;

        // Increment lerp time
        lerpTime += Time.deltaTime;

        if (!colorsLerped)
        {
            // Perform color lerp
            float lerpFactor = Mathf.Clamp01(lerpTime / lerpDuration);
            LerpColors(lerpFactor);

            if (lerpTime >= lerpDuration)
            {
                // Reset lerp time and prepare for transparency lerp
                lerpTime = 0.0f;
                colorsLerped = true;
            }
        }
        else
        {
            // Perform transparency lerp
            float lerpFactor = Mathf.Clamp01(lerpTime / transparencyLerpDuration);
            FadeToTransparent(lerpFactor);

            if (lerpTime >= transparencyLerpDuration)
            {
                // Destroy the parent object after full transparency
                Destroy(parentObject.transform.parent.gameObject, 0.05f);
            }
        }
    }

    void OnDestroy() {
        GameObject systemObject = GameObject.Find("System");
        SystemController system = systemObject.GetComponent<SystemController>();
        system.dandelionObjects.Remove(gameObject);
    }

    private void CacheInitialColors()
    {
        initialColors = new Color[parentObject.transform.childCount][];
        int index = 0;

        foreach (Transform child in parentObject.transform)
        {
            MeshRenderer renderer = child.GetComponent<MeshRenderer>();
            if (renderer == null) continue;

            Material[] materials = renderer.materials;
            initialColors[index] = new Color[materials.Length];

            for (int i = 0; i < materials.Length; i++)
            {
                initialColors[index][i] = materials[i].color;
            }

            index++;
        }
    }

    private void LerpColors(float lerpFactor)
    {
        int index = 0;

        foreach (Transform child in parentObject.transform)
        {
            MeshRenderer renderer = child.GetComponent<MeshRenderer>();
            if (renderer == null) continue;

            Material[] materials = renderer.materials;

            if (materials.Length > 0)
                materials[0].color = Color.Lerp(initialColors[index][0], targetColor1, lerpFactor);

            if (materials.Length > 1)
                materials[1].color = Color.Lerp(initialColors[index][1], targetColor2, lerpFactor);

            if (materials.Length > 2)
                materials[2].color = Color.Lerp(initialColors[index][2], targetColor3, lerpFactor);

            renderer.materials = materials;
            index++;
        }
    }

    private void FadeToTransparent(float lerpFactor)
    {
        foreach (Transform child in parentObject.transform)
        {
            MeshRenderer renderer = child.GetComponent<MeshRenderer>();
            if (renderer == null) continue;

            Material[] materials = renderer.materials;
            foreach (var material in materials)
            {
                // Set rendering mode to Transparent
                SetMaterialToTransparent(material);

                // Lerp the alpha value
                Color currentColor = material.color;
                currentColor.a = Mathf.Lerp(1.0f, 0.0f, lerpFactor); // Lerp alpha to 0
                material.color = currentColor;

                Debug.Log($"Material {material.name}: Color {material.color}");
            }

            renderer.materials = materials;
        }
    }

    private void SetMaterialToTransparent(Material material)
    {
        if (material.shader.name != "Standard")
        {
            Debug.LogWarning($"Material {material.name} is not using the Standard Shader.");
            return;
        }

        // Ensure the shader supports transparency
        material.SetFloat("_Mode", 3); // 3 = Transparent
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_ZWrite", 0);
        material.DisableKeyword("_ALPHATEST_ON");
        material.EnableKeyword("_ALPHABLEND_ON");
        material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        material.renderQueue = 3000; // Transparent queue
    }
}
