using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CharacterFader : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;
    public float maxCheckDistance = 200f;

    [Header("Fade Settings")]
    [Range(0f, 1f)] public float fadedAlpha = 0.15f;
    public float fadeSpeed = 8f;

    [Header("Hover / Raycast")]
    public bool fadeOnHover = true;          
    public bool requireFiring = false;       
    public bool useSphereCast = true;        
    public float hoverSphereRadius = 0.12f;  

    [Header("Raycast / Layers")]
    public LayerMask raycastLayerMask = ~0;

    [Header("Debug")]
    public bool enableDebugLogs = false;
    public bool forceFadeForTesting = false;

    private Renderer[] renderers;
    private Material[][] originalMaterialsPerRenderer;
    private Material[][] instanceMaterialsPerRenderer;
    private float currentAlpha = 1f;
    private Coroutine fadeCoroutine;
    private bool currentlyFaded = false;
    private bool isFiring = false;

    private void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(includeInactive: true);

        originalMaterialsPerRenderer = new Material[renderers.Length][];
        instanceMaterialsPerRenderer = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            var mats = renderers[i].sharedMaterials ?? new Material[0];
            originalMaterialsPerRenderer[i] = mats;
            Material[] instances = new Material[mats.Length];
            for (int j = 0; j < mats.Length; j++)
            {
                if (mats[j] == null) continue;
                instances[j] = new Material(mats[j]);
                SetupMaterialForFade(instances[j]);
            }
            instanceMaterialsPerRenderer[i] = instances;
            renderers[i].materials = instances;
        }
    }

    private void Start()
    {
        if (playerCamera == null)
        {
            if (PlayerController.Instance != null)
            {
                Camera c = PlayerController.Instance.gameObject.GetComponentInChildren<Camera>();
                if (c != null)
                {
                    playerCamera = c;
                    if (enableDebugLogs) Debug.Log("[CharacterFader] Found player camera on PlayerController.");
                }
            }

            if (playerCamera == null && Camera.main != null)
            {
                playerCamera = Camera.main;
                if (enableDebugLogs) Debug.Log("[CharacterFader] Using Camera.main as fallback.");
            }
        }

        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.OnFireStateChanged += OnFireStateChanged;
            if (enableDebugLogs) Debug.Log("[CharacterFader] Subscribed to PlayerController.OnFireStateChanged.");
        }
    }

    private void OnDestroy()
    {
        if (PlayerController.Instance != null)
            PlayerController.Instance.OnFireStateChanged -= OnFireStateChanged;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null && originalMaterialsPerRenderer[i] != null)
                renderers[i].materials = originalMaterialsPerRenderer[i];
        }
    }

    private void Update()
    {
        if (playerCamera == null)
        {
            if (enableDebugLogs) Debug.LogWarning("[CharacterFader] playerCamera not set.");
            return;
        }

        if (PlayerController.Instance != null)
        {
            isFiring = PlayerController.Instance.IsFiring;
        }

        bool shouldFade = false;

        if (forceFadeForTesting)
        {
            shouldFade = true;
        }
        else
        {
            // Fade wenn Maus über Drachen (Hover)
            if (fadeOnHover)
            {
                bool hoverHits = IsMouseOverSelf(out RaycastHit hit);
                if (enableDebugLogs) Debug.Log($"[CharacterFader] HoverHits={hoverHits}" + (hoverHits ? $", hit={hit.collider.name}" : ""));
                if (hoverHits)
                {
                    shouldFade = !requireFiring || (requireFiring && isFiring);
                }
                else
                {
                    shouldFade = false;
                }
            }
            else
            {
                if (isFiring)
                {
                    bool centerHits = IsCenterPointHittingSelf();
                    shouldFade = centerHits;
                    if (enableDebugLogs)
                        Debug.Log($"[CharacterFader] CenterHitsSelf={centerHits}");
                }
            }
        }

        if (shouldFade && !currentlyFaded)
        {
            currentAlpha = fadedAlpha;
            ApplyAlphaToInstances(currentAlpha);
            StartFadeTo(fadedAlpha);
            currentlyFaded = true;
        }
        else if (!shouldFade && currentlyFaded)
        {
            currentAlpha = 1f;
            ApplyAlphaToInstances(currentAlpha);
            StartFadeTo(1f);
            currentlyFaded = false;
        }
    }

    private void OnFireStateChanged(bool firing)
    {
        isFiring = firing;
        if (enableDebugLogs) Debug.Log($"[CharacterFader] OnFireStateChanged -> {firing}");
    }

    private bool IsMouseOverSelf(out RaycastHit outHit)
    {
        outHit = default;
        Vector3 mousePos = Input.mousePosition;

        if (mousePos.x < 0 || mousePos.y < 0 || mousePos.x > Screen.width || mousePos.y > Screen.height)
        {
            return IsCenterPointHittingSelf();
        }

        Ray ray = playerCamera.ScreenPointToRay(mousePos);

        if (useSphereCast)
        {
            if (Physics.SphereCast(ray, hoverSphereRadius, out RaycastHit hit, maxCheckDistance, raycastLayerMask))
            {
                if (hit.collider != null && hit.collider.transform.IsChildOf(transform))
                {
                    outHit = hit;
                    return true;
                }
                if (enableDebugLogs && hit.collider != null)
                    Debug.Log($"[CharacterFader] Mouse SphereCast hit {hit.collider.name}, isChild={hit.collider.transform.IsChildOf(transform)}");
            }
        }
        else
        {
            if (Physics.Raycast(ray, out RaycastHit hit, maxCheckDistance, raycastLayerMask))
            {
                if (hit.collider != null && hit.collider.transform.IsChildOf(transform))
                {
                    outHit = hit;
                    return true;
                }
                if (enableDebugLogs && hit.collider != null)
                    Debug.Log($"[CharacterFader] Mouse Raycast hit {hit.collider.name}, isChild={hit.collider.transform.IsChildOf(transform)}");
            }
        }

        return false;
    }

    private bool IsCenterPointHittingSelf()
    {
        Vector3 screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
        Ray ray = playerCamera.ScreenPointToRay(screenCenter);

        RaycastHit[] hits = Physics.RaycastAll(ray, maxCheckDistance);

        foreach (RaycastHit hit in hits)
        {
            if (hit.collider != null && hit.collider.transform.IsChildOf(transform))
            {
                if (enableDebugLogs)
                    Debug.Log($"[CharacterFader] MultiCollider hit: {hit.collider.name}");
                return true;
            }
        }

        return false;
    }


    private void StartFadeTo(float targetAlpha)
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeRoutine(targetAlpha));
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        float startAlpha = currentAlpha;
        float t = 0f;
        while (!Mathf.Approximately(currentAlpha, targetAlpha))
        {
            t += Time.deltaTime * fadeSpeed;
            currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            ApplyAlphaToInstances(currentAlpha);
            yield return null;
        }
        currentAlpha = targetAlpha;
        ApplyAlphaToInstances(currentAlpha);
        fadeCoroutine = null;
    }

    private void ApplyAlphaToInstances(float alpha)
    {
        for (int i = 0; i < instanceMaterialsPerRenderer.Length; i++)
        {
            var mats = instanceMaterialsPerRenderer[i];
            if (mats == null) continue;
            for (int j = 0; j < mats.Length; j++)
            {
                var m = mats[j];
                if (m == null) continue;

                bool applied = false;

                if (m.HasProperty("_Color"))
                {
                    Color c = m.color;
                    c.a = alpha;
                    m.color = c;
                    applied = true;
                }
                else if (m.HasProperty("_BaseColor"))
                {
                    Color c = m.GetColor("_BaseColor");
                    c.a = alpha;
                    m.SetColor("_BaseColor", c);
                    applied = true;
                }
                else if (m.HasProperty("_Surface"))
                {
                    m.SetFloat("_Surface", 1f);
                    if (m.HasProperty("_Blend")) m.SetFloat("_Blend", 0f);
                    if (m.HasProperty("_BaseColor"))
                    {
                        Color c = m.GetColor("_BaseColor");
                        c.a = alpha;
                        m.SetColor("_BaseColor", c);
                        applied = true;
                    }
                }

                if (!applied)
                {
                    if (m.HasProperty("_MainColor"))
                    {
                        Color c = m.GetColor("_MainColor");
                        c.a = alpha;
                        m.SetColor("_MainColor", c);
                        applied = true;
                    }
                }

                if (applied)
                {
                    m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    if (m.HasProperty("_ZWrite")) m.SetInt("_ZWrite", 0);
                    m.DisableKeyword("_ALPHATEST_ON");
                    m.EnableKeyword("_ALPHABLEND_ON");
                    m.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    m.renderQueue = 3000;
                }

                if (enableDebugLogs)
                {
                    Debug.Log($"[CharacterFader] Applied alpha={alpha} to material '{m.name}' (renderer {i} slot {j}) applied={applied}");
                }
            }
        }
    }

    private void SetupMaterialForFade(Material m)
    {
        if (m == null) return;

        if (m.HasProperty("_Mode"))
            m.SetFloat("_Mode", 2f);

        if (m.HasProperty("_Surface"))
        {
            m.SetFloat("_Surface", 1f);
            if (m.HasProperty("_Blend")) m.SetFloat("_Blend", 0f);
        }

        m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        if (m.HasProperty("_ZWrite")) m.SetInt("_ZWrite", 0);

        m.DisableKeyword("_ALPHATEST_ON");
        m.EnableKeyword("_ALPHABLEND_ON");
        m.DisableKeyword("_ALPHAPREMULTIPLY_ON");

        m.renderQueue = 3000;
    }
}
