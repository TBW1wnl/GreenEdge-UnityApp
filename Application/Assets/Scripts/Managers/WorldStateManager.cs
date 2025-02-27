using UnityEngine;

public class WorldStateManager : MonoBehaviour
{
    public static WorldStateManager Instance { get; private set; }

    [Range(0, 100)]
    public int GlobalWarming { get; internal set; } = 0;
    public int WorldPopulation { get; internal set; } = 0;

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

    public void ComputeWorldPopulation()
    {
        WorldPopulation = 0;
        foreach (Tile tile in FindObjectsByType<Tile>(FindObjectsSortMode.None))
        {
            WorldPopulation += tile.population;
        }
        Debug.Log($"Population mondiale : {WorldPopulation}");
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
