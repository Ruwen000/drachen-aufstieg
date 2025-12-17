using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillUI : MonoBehaviour
{
    public TMP_Text pointsText;
    public Button btnFlight;
    public Button btnFire;
    public Button btnSpeed;
    public Button btnDamage; 

    private void Start()
    {
        btnFlight.onClick.AddListener(() => SkillSystem.Instance.SpendPointOn(StatType.FlightHeight));
        btnFire.onClick.AddListener(() => SkillSystem.Instance.SpendPointOn(StatType.FirebeamDuration));
        btnSpeed.onClick.AddListener(() => SkillSystem.Instance.SpendPointOn(StatType.Speed));
        btnDamage.onClick.AddListener(() => SkillSystem.Instance.SpendPointOn(StatType.Damage)); 

        SkillSystem.Instance.OnPointsChanged += UpdateUI;

        UpdateUI(SkillSystem.Instance.unspentPoints);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            SkillSystem.Instance.SpendPointOn(StatType.FlightHeight);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            SkillSystem.Instance.SpendPointOn(StatType.FirebeamDuration);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            SkillSystem.Instance.SpendPointOn(StatType.Speed);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            SkillSystem.Instance.SpendPointOn(StatType.Damage); 
    }

    private void UpdateUI(int unspentPoints)
    {
        pointsText.text = $"Skill-Punkte: {unspentPoints}";
    }
}