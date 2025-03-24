using UnityEngine;
using System;

public class GameTimeManager : MonoBehaviour
{
    public static GameTimeManager Instance; // Singleton for easy access

    public int day = 1;
    public int month = 1;
    public int year = 1;

    public float timeMultiplier = 3f; // Default to Speed 1 (3 seconds per day)

    private float timeCounter = 0f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (timeMultiplier > 0)
        {
            timeCounter += Time.deltaTime * timeMultiplier;

            if (timeCounter >= 1f)
            {
                timeCounter = 0f;
                AdvanceDay();
            }
        }
    }

    public void AdvanceDay()
    {
        day++;
        if (day > 30)
        {
            day = 1;
            month++;
            TriggerMounthly();
            if (month > 12)
            {
                month = 1;
                year++;
                TriggerYearly();
            }
        }
        TriggerDaily();
    }

    public void TriggerDaily()
    {
        
    }

    public void TriggerMounthly()
    {

    }

    public void TriggerYearly()
    {

    }

    public void SetTimeSpeed(int speed)
    {
        switch (speed)
        {
            case 1:
                timeMultiplier = 1f / 3f; // 3 sec per day
                break;
            case 2:
                timeMultiplier = 1f; // 1 sec per day
                break;
            case 3:
                timeMultiplier = 3f; // 3 days per sec
                break;
            case 0:
                timeMultiplier = 0f; // Pause time
                break;
            default:
                Debug.LogWarning("Invalid speed!");
                break;
        }
    }
}
