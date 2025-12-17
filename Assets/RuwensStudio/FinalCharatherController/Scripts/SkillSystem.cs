using UnityEngine;
using System;

public class SkillSystem : MonoBehaviour
{
    public static SkillSystem Instance { get; private set; }

    public int unspentPoints = 100;
    public int flightHeightPoints = 0;
    public int firebeamDurationPoints = 0;
    public int speedPoints = 0;
    public int damagePoints = 0; 


    public float baseMaxFlyHeight = 50f;
    public float baseFirebeamDuration = 2f;
    public float baseRunSpeed = 20f;
    public float baseFlySpeed = 15f;
    public float baseDamageMultiplier = 1.0f; 

 
    public event Action<int> OnPointsChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddSkillPoints(int amount)
    {
        unspentPoints += amount;
        OnPointsChanged?.Invoke(unspentPoints);
    }

    public bool SpendPointOn(StatType type)
    {
        if (unspentPoints <= 0) return false;
        unspentPoints--;

        switch (type)
        {
            case StatType.FlightHeight: flightHeightPoints++; break;
            case StatType.FirebeamDuration: firebeamDurationPoints++; break;
            case StatType.Speed: speedPoints++; break;
            case StatType.Damage: damagePoints++; break; 
        }

        OnPointsChanged?.Invoke(unspentPoints);
        return true;
    }

    public float GetMaxFlyHeight() => baseMaxFlyHeight + flightHeightPoints * 30f;
    public float GetFirebeamDuration() => baseFirebeamDuration + firebeamDurationPoints * 0.5f;
    public float GetRunSpeed() => baseRunSpeed + speedPoints * 1f;
    public float GetFlySpeed() => baseFlySpeed + speedPoints * 1f;
    public float GetDamageMultiplier() => baseDamageMultiplier + damagePoints * 0.2f; 
}

public enum StatType { FlightHeight, FirebeamDuration, Speed, Damage }