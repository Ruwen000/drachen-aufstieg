using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class FireBeamScript : MonoBehaviour
{
    [Header("References")]
    public GameObject firePrefab;
    public Transform firePoint;
    public LayerMask hitLayers;
    public float maxRange = 100f;
    public float firePointByFlying = 1f;

    [Header("UI - Beam Duration")]
    public Image durationImage;
    public TMP_Text durationText;

    [Header("UI - Cooldown")]
    public Image cooldownImage;
    public TMP_Text cooldownText;

    private GameObject spawnedFire;
    private LineRenderer lineRenderer;
    private PlayerController playerController;

    private bool isFiring = false;
    private float beamEndTime = 0f;

    private float cooldownStartTime = 0f;
    private float nextFireTime = 0f;
    private bool cooldownActive = false;

    void Start()
    {
        spawnedFire = Instantiate(firePrefab, firePoint.position, firePoint.rotation, firePoint);
        spawnedFire.SetActive(false);

        lineRenderer = spawnedFire.GetComponentInChildren<LineRenderer>();
        if (lineRenderer != null) lineRenderer.positionCount = 2;

        playerController = GetComponent<PlayerController>();

        ResetUI();
    }

    void Update()
    {
        float fireDuration = SkillSystem.Instance.GetFirebeamDuration();
        float fireCooldown = 5f;

        if (Input.GetMouseButtonDown(0) && Time.time >= nextFireTime && !isFiring)
            EnableFire(fireDuration, fireCooldown);

        if (isFiring && Time.time > beamEndTime)
            DisableFire();

        if (Input.GetMouseButtonUp(0) && isFiring)
            DisableFire();

        if (isFiring)
            UpdateFire();

        UpdateDurationUI(fireDuration);
        UpdateCooldownUI(fireCooldown);
    }

    private void EnableFire(float duration, float cooldown)
    {
        spawnedFire.SetActive(true);
        isFiring = true;

        beamEndTime = Time.time + duration;

        cooldownStartTime = Time.time;
        nextFireTime = cooldownStartTime + cooldown;
        cooldownActive = true;

        playerController?.SetFireCameraMode(true);
    }

    private void UpdateFire()
    {
        if (Camera.main == null) return;

        spawnedFire.transform.position = firePoint.position;
        Ray ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
        Vector3 target = ray.origin + ray.direction * maxRange;

        if (Physics.Raycast(ray, out RaycastHit hit, maxRange, hitLayers))
            target = hit.point;

        if (playerController._isFlying)
            spawnedFire.transform.position += Vector3.up * firePointByFlying;

        spawnedFire.transform.rotation = Quaternion.LookRotation((target - firePoint.position).normalized);

        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, Vector3.zero);
            lineRenderer.SetPosition(1, spawnedFire.transform.InverseTransformPoint(target));
        }
    }

    private void DisableFire()
    {
        spawnedFire.SetActive(false);
        isFiring = false;
        playerController?.SetFireCameraMode(false);
    }

    private void ResetUI()
    {
        if (durationImage != null) durationImage.fillAmount = 0f;
        if (durationText != null) durationText.text = "";
        if (cooldownImage != null) cooldownImage.fillAmount = 0f;
        if (cooldownText != null) cooldownText.text = "";
    }

    private void UpdateDurationUI(float fireDuration)
    {
        if (durationImage == null) return;

        float timeLeft = beamEndTime - Time.time;
        if (isFiring && timeLeft > 0f)
        {
            durationImage.fillAmount = timeLeft / fireDuration;
            if (durationText != null)
                durationText.text = Mathf.Ceil(timeLeft).ToString();
        }
        else
        {
            durationImage.fillAmount = 0f;
            if (durationText != null)
                durationText.text = "";
        }
    }

    private void UpdateCooldownUI(float fireCooldown)
    {
        if (!cooldownActive || cooldownImage == null)
            return;

        float elapsed = Time.time - cooldownStartTime;
        if (elapsed < fireCooldown)
        {
            cooldownImage.fillAmount = 1f - (elapsed / fireCooldown);
            if (cooldownText != null)
                cooldownText.text = Mathf.Ceil(fireCooldown - elapsed).ToString();
        }
        else
        {
            cooldownImage.fillAmount = 0f;
            if (cooldownText != null)
                cooldownText.text = "";
            cooldownActive = false;
        }
    }
}
