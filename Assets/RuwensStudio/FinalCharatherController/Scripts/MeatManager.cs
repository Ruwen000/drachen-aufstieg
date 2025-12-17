using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MeatManager : MonoBehaviour
{
    public static MeatManager Instance { get; private set; }


    public TMP_Text meatCountText;

    public Vector3 growPerMeat = new Vector3(0.1f, 0.1f, 0.1f);

    private int eatenCount = 0;


    private Vector3 initialScale = new Vector3(0.3f, 0.3f, 0.3f);
    private float initialCCHeight;
    private float initialCCRadius;
    private Vector3 initialCCCenter;



    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }


        var player = PlayerController.Instance;
        initialScale = player.transform.localScale;

    }

    private void Start()
    {
        UpdateUI();
    }

    public void AddMeat()
    {
        eatenCount++;
        UpdateUI();
        ScaleDragon();
        SkillSystem.Instance.AddSkillPoints(1);
    }

    private void ScaleDragon()
    {
        Vector3 newScale = initialScale + growPerMeat * eatenCount;
        PlayerController.Instance.transform.localScale = newScale;

    }

    private void UpdateUI()
    {
        if (meatCountText != null)
            meatCountText.text = $"Fleisch gegessen: {eatenCount}";
    }
}
