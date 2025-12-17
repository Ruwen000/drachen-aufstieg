using UnityEngine;

public class FoodInteractable : MonoBehaviour
{
    [Header("Einstellungen")]
    public float interactRange = 3f;
    public KeyCode interactKey = KeyCode.E;

    [Header("UI")]
    public GameObject promptUI;

    private Transform player;
    private bool isPlayerNearby = false;

    void Start()
    {
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO == null)
        {
            Debug.LogError($"[FoodInteractable] Kein GameObject mit Tag 'Player' gefunden auf {name}!");
        }
        else
        {
            player = playerGO.transform;
        }

        if (promptUI == null)
        {
            Debug.LogError($"[FoodInteractable] promptUI nicht gesetzt auf {name}! Ziehe dein 'Drücke E'-Text-Objekt in den Inspector.");
        }
        else
        {
            promptUI.SetActive(false);
        }
    }

    void Update()
    {
        if (player == null || promptUI == null)
            return;

        float dist = Vector3.Distance(player.position, transform.position);
        isPlayerNearby = dist <= interactRange;

        if (isPlayerNearby && !promptUI.activeSelf)
            promptUI.SetActive(true);
        else if (!isPlayerNearby && promptUI.activeSelf)
            promptUI.SetActive(false);

        if (isPlayerNearby && Input.GetKeyDown(interactKey))
            Eat();
    }

    private void Eat()
    {
        Debug.Log("Fleisch gegessen!");
        MeatManager.Instance.AddMeat();
        Destroy(gameObject);
        promptUI.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRange);
    }
}
