using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBarUI : MonoBehaviour
{
    [Header("References")]
    public PlayerHealth playerHealth;   
    public BossAI bossAI;               
    public Image fillImage;
    public TMP_Text hpText;
    public GameObject container;

    [Header("Options")]
    public bool smooth = true;
    public float smoothSpeed = 5f;
    public bool showAbsoluteValues = false;

    private float currentFill = 1f;

    void Update()
    {
        float cur = 0f;
        float max = 1f;
        bool valid = false;

        if (playerHealth != null)
        {
            cur = playerHealth.GetHealth();
            max = playerHealth.GetMaxHealth();
            valid = true;
        }
        else if (bossAI != null)
        {
            cur = bossAI.GetHealth();
            max = bossAI.GetMaxHealth();
            valid = true;
        }

        if (!valid)
        {
            if (container != null && container.activeSelf)
                container.SetActive(false);
            return;
        }

        if (container != null && !container.activeSelf)
            container.SetActive(true);

        float targetFill = Mathf.Clamp01(cur / max);
        if (!smooth)
            currentFill = targetFill;
        else
            currentFill = Mathf.MoveTowards(currentFill, targetFill, smoothSpeed * Time.deltaTime);

        if (fillImage != null)
            fillImage.fillAmount = currentFill;

        if (hpText != null)
        {
            if (showAbsoluteValues)
                hpText.text = $"{Mathf.CeilToInt(cur)} / {Mathf.CeilToInt(max)}";
            else
                hpText.text = $"{Mathf.RoundToInt(targetFill * 100f)} %";
        }
    }
}
