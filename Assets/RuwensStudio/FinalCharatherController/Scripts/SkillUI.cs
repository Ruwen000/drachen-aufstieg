using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillUI : MonoBehaviour
{
    [Header("Texts")]
    public TMP_Text pointsText;

    public TMP_Text flightPointsText;
    public TMP_Text firePointsText;
    public TMP_Text speedPointsText;
    public TMP_Text damagePointsText;

    [Header("Buttons")]
    public Button btnFlight;
    public Button btnFire;
    public Button btnSpeed;
    public Button btnDamage;

    [Header("Cheat")]
    public KeyCode cheatKey = KeyCode.L;
    public int cheatAmount = 5;

    private void Start()
    {
        btnFlight.onClick.AddListener(() => Spend(StatType.FlightHeight));
        btnFire.onClick.AddListener(() => Spend(StatType.FirebeamDuration));
        btnSpeed.onClick.AddListener(() => Spend(StatType.Speed));
        btnDamage.onClick.AddListener(() => Spend(StatType.Damage));

        SkillSystem.Instance.OnPointsChanged += _ => UpdateUI();

        UpdateUI();
    }

    private void Update()
    {
        // Tastenkürzel
        if (Input.GetKeyDown(KeyCode.Alpha1)) Spend(StatType.FlightHeight);
        if (Input.GetKeyDown(KeyCode.Alpha2)) Spend(StatType.FirebeamDuration);
        if (Input.GetKeyDown(KeyCode.Alpha3)) Spend(StatType.Speed);
        if (Input.GetKeyDown(KeyCode.Alpha4)) Spend(StatType.Damage);

        // Cheat-Taste
        if (Input.GetKeyDown(cheatKey))
        {
            SkillSystem.Instance.AddSkillPoints(cheatAmount);
            Debug.Log($"CHEAT: +{cheatAmount} Skillpunkte");
            UpdateUI();
        }
    }

    private void Spend(StatType type)
    {
        SkillSystem.Instance.SpendPointOn(type);
        UpdateUI();
    }

    private void UpdateUI()
    {
        var skill = SkillSystem.Instance;

        pointsText.text = $"Skill-Punkte: {skill.unspentPoints}";

        flightPointsText.text = $"{skill.flightHeightPoints}";
        firePointsText.text = $"{skill.firebeamDurationPoints}";
        speedPointsText.text = $"{skill.speedPoints}";
        damagePointsText.text = $"{skill.damagePoints}";
    }
}
