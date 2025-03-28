using System.Collections.Generic;
using UnityEngine;

public class WorldStateManager : MonoBehaviour
{
    public static WorldStateManager Instance { get; private set; }
    public int Funds { get; internal set; } = 5000000;

    [Range(0, 100)]
    public int GlobalWarming { get; internal set; } = 0;
    public int WorldPopulation { get; internal set; } = 0;
    public List<Tile> Tiles { get; } = new();

    private WorldStateManager() { }

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
        }
    }

    public void IncreaseTension(int amount)
    {
        GlobalWarming = Mathf.Clamp(GlobalWarming + amount, 0, 100);
        Debug.Log($"Réchauffement climatique : {GlobalWarming}%");
    }

    public void DecreaseTension(int amount)
    {
        GlobalWarming = Mathf.Clamp(GlobalWarming - amount, 0, 100);
        Debug.Log($"Réchauffement climatique : {GlobalWarming}%");
    }
}
