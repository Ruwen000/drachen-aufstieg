using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float maxHealth = 20f;
    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (amount <= 0f) return;
        currentHealth -= amount;
        Debug.Log($"Player.TakeDamage({amount:F2}) -> HP = {currentHealth:F2}");


        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Player gestorben!");
        gameObject.SetActive(false);
    }

    public float GetHealth() => currentHealth;
    public float GetMaxHealth() => maxHealth;
}
