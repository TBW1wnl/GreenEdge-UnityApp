using UnityEngine;
using System;
using UnityEngine.Networking;
using System.Collections;
using SlimUI.ModernMenu;
using System.Linq;

public class GameTimeManager : MonoBehaviour
{
    public static GameTimeManager Instance; // Singleton for easy access

    public int day = 1;
    public int month = 1;
    public int year = 1;
    private string apiUrl = "http://localhost:8000/api/";

    [SerializeField] public GameObject EventPrefab;
    [SerializeField] public GameObject EventContainer;


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
        int chance = UnityEngine.Random.Range(0, 10);
        if (chance == 1)
        {
            
        }
    }


    public void GetEvent()
    {
        StartCoroutine(GetEventCoroutine());
    }

    IEnumerator GetEventCoroutine()
    {
        System.Collections.Generic.IEnumerable<Tile> buildedTiles = WorldStateManager.Instance.Tiles.Where(t => t.tileData.IsBuild);
        int buildedTilesCount = buildedTiles.Count();
        Tile tile = buildedTiles.ToList()[UnityEngine.Random.Range(0, buildedTilesCount-1)];
        string jsonBody = $"\"tile\": {tile.tileData.id}";
        byte[] jsonToSend = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new(apiUrl + "get_random_event", "GET"))
        {
            request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Login Success: " + request.downloadHandler.text);

                string jsonResponse = request.downloadHandler.text;
                UIMenuManager.TokenResponse tokenData = JsonUtility.FromJson<UIMenuManager.TokenResponse>(jsonResponse);

                PlayerPrefs.SetString("auth_token", tokenData.token);
                PlayerPrefs.Save();
            }
            else
            {
                Debug.LogError("Login Failed: " + request.error);
            }
        }
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
